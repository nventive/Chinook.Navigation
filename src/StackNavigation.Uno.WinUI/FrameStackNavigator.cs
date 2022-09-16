using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Windows.UI.Dispatching;

namespace Chinook.StackNavigation
{
    /// <summary>
    /// This implementation of <see cref="IStackNavigator"/> uses a <see cref="Frame"/> to manage the navigation.
    /// </summary>
    public class FrameStackNavigator : StackNavigatorBase
    {
        private readonly Frame _frame;

        private bool _isProcessingFrameInitiatedBack;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameStackNavigator"/> class.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="registrations">The type registrations mapping ViewModel types to View types.</param>
        public FrameStackNavigator(Frame frame, IReadOnlyDictionary<Type, Type> registrations)
            : base(registrations)
        {
            _frame = frame;

            HandleFrameInitiatedBackNavigations();
        }

        private DispatcherQueue Dispatcher => _frame.DispatcherQueue;

        /// <inheritdoc/>
        protected override ILogger GetLogger() => this.Log();

        /// <summary>
        /// Calls <see cref="IStackNavigator.NavigateBack(CancellationToken)"/> when the frame initiates a back navigation on its own.
        /// </summary>
        /// <remarks>
        /// This can happen when the user swipes to go back on iOS.
        /// </remarks>
        private void HandleFrameInitiatedBackNavigations()
        {
            _ = Dispatcher.EnqueueAsync(UIHandleFrameInitiatedBackNavigations, DispatcherQueuePriority.High);

            Task<bool> UIHandleFrameInitiatedBackNavigations()
            {
                _frame.Navigated += async (s, e) =>
                {
                    if (e.NavigationMode == NavigationMode.Back && State.LastRequestState != NavigatorRequestState.Processing)
                    {
                        try
                        {
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                _logger.LogDebug($"Processing frame initiated back.");
                            }

                            // When the LastRequestState isn't Processing, the navigator didn't cause the Navigated event.
                            // The Frame must have initiated a back navigation on its own (i.e., swipe to go back on iOS).
                            // When that happens, we simulate a back navigation without re-triggering a back navigation on the Frame.
                            _isProcessingFrameInitiatedBack = true;

                            // We schedule on a background thread because all the work doesn't require any work on the UI thread.
                            await Task.Run(async () => await NavigateBack(CancellationToken.None));
                        }
                        catch (Exception exception)
                        {
                            if (_logger.IsEnabled(LogLevel.Error))
                            {
                                _logger.LogError($"Failed to process frame initiated back.", exception);
                            }
                        }
                        finally
                        {
                            _isProcessingFrameInitiatedBack = false;
                        }
                    }
                };

                return Task.FromResult(true);
            }
        }

        /// <inheritdoc/>
        protected override async Task InnerClear()
        {
            var entriesToRemove = Stack.ToArray();

            await Dispatcher.EnqueueAsync(UIClear, DispatcherQueuePriority.High);

            _ = Dispatcher.EnqueueAsync(() => UIResetDataContext(entriesToRemove), DispatcherQueuePriority.Low);

            Task<bool> UIClear()
            {
                _frame.Content = null;
                _frame.BackStack?.Clear();
                return Task.FromResult(true);
            }
        }

        /// <inheritdoc/>
        protected override async Task InnerRemoveEntries(IEnumerable<int> orderedIndexes)
        {
            var entriesToRemove = orderedIndexes.Select(s => Stack.ElementAt(s)).ToArray();

            await Dispatcher.EnqueueAsync(UIRemoveEntries, DispatcherQueuePriority.High);

            _ = Dispatcher.EnqueueAsync(() => UIResetDataContext(entriesToRemove), DispatcherQueuePriority.Low);

            Task<bool> UIRemoveEntries()
            {
                // Start with the last item so that the indexes stay valid as we iterate.
                foreach (var index in orderedIndexes)
                {
                    if (index == _frame.BackStack.Count)
                    {
                        // If the index points to the current page, we clear the content.
                        _frame.Content = null;
                    }
                    else
                    {
                        _frame.BackStack.RemoveAt(index);
                    }
                }

                return Task.FromResult(true);
            }
        }

        /// <inheritdoc/>
        protected override async Task<object> InnerNavigateAndGetView(INavigableViewModel viewModel)
        {
            var request = State.LastRequest;

            var viewType = request.ViewType;
            if (viewType == null)
            {
                if (_registrations.TryGetValue(request.ViewModelType, out var registeredViewType))
                {
                    viewType = registeredViewType;
                }
                else
                {
                    throw new KeyNotFoundException($"Can't process navigation because no view is registered with '{request.ViewModelType.FullName}'. Provide a view type in the NavigationRequest or provide a registration in the constructor.");
                }
            }

            var view = await Dispatcher.EnqueueAsync(UINavigate, DispatcherQueuePriority.High);

            if (view != null && !(request.SuppressTransitions ?? false))
            {
                // There's no way to know when the animation ends. So, we assume it runs for 250ms.
                await Task.Delay(millisecondsDelay: 250);
            }

            // The returned view is stored in the Stack.
            return view;

            async Task<Page> UINavigate()
            {
                try
                {
                    var overrideInfo = request.SuppressTransitions ?? false
                        ? (NavigationTransitionInfo)new SuppressNavigationTransitionInfo()
                        : (NavigationTransitionInfo)new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight };

                    var navigationResult = await WaitForNavigationResult(() =>
                    {
                        var isNavigationSuccessful = _frame.Navigate(viewType, parameter: null, overrideInfo);
                        if (!isNavigationSuccessful)
                        {
                            if (_logger.IsEnabled(LogLevel.Error))
                            {
                                _logger.LogError($"Failed frame navigation to view '{viewType.FullName}'.");
                            }
                        }
                    });

                    if (request.ClearBackStack ?? false)
                    {
                        _frame.BackStack?.Clear();
                    }

                    if (navigationResult.Content is Page page)
                    {
                        var viewTcs = new TaskCompletionSource<Page>();
                        page.Loaded += OnPageLoaded;

                        void OnPageLoaded(object sender, RoutedEventArgs e)
                        {
                            page.Loaded -= OnPageLoaded;
                            viewTcs.SetResult(page);
                        }

                        page.NavigationCacheMode = NavigationCacheMode.Required; // TODO: Investigate why we need this.
                        viewModel.SetView(page);
                        page.DataContext = viewModel;

                        if (page.IsLoaded || _frame.Visibility == Visibility.Collapsed)
                        {
                            // If the page is loaded, we return it directly.
                            // If the page is not loaded, but the _frame is collapsed, the page will not load, so we don't wait for it to load.
                            page.Loaded -= OnPageLoaded;
                            return page;
                        }
                        else
                        {
#if __IOS__
							// The event sequence doesn't seem to work properly on iOS.
							// The Loaded event doesn't seem to be raised so we don't wait and simply return the page.
							page.Loaded -= OnPageLoaded;
							return page;
#else
                            // If the _frame is visible and the page is not loaded, we wait for the page to load.
                            return await viewTcs.Task;
#endif
                        }
                    }
                    else
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                        {
                            _logger.LogError($"Failed frame navigation to view '{viewType.FullName}' because the this type is not a Page.");
                        }

                        throw new InvalidOperationException($"Failed frame navigation to view '{viewType.FullName}' because the this type is not a Page.");
                    }
                }
                catch (NavigationCanceledException e)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError($"Failed frame navigation to view '{viewType.FullName}' because it was canceled.", e);
                    }

                    throw;
                }
                catch (Exception e)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError($"Failed frame navigation to view '{viewType.FullName}'.", e);
                    }

                    throw;
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task InnerNavigateBack(NavigationStackEntry entryToRemove, NavigationStackEntry activeEntry)
        {
            // When this flag is true, the Frame already navigated back (e.g. using the back button),
            // so we don't need to ask the Frame to navigate back. The only thing we need to make
            // sure is to reset the DataContext of the view.
            if (!_isProcessingFrameInitiatedBack)
            {
                await Dispatcher.EnqueueAsync(UIBack, DispatcherQueuePriority.High);

                if (!(entryToRemove.Request.SuppressTransitions ?? false))
                {
                    // There's no way to know when the animation ends. So, we assume it runs for 250ms.
                    await Task.Delay(millisecondsDelay: 250);
                }
            }

            _ = Dispatcher.EnqueueAsync(() => UIResetDataContext(entryToRemove), DispatcherQueuePriority.Low);

            async Task<bool> UIBack()
            {
                try
                {
                    var overrideInfo = entryToRemove.Request.SuppressTransitions ?? false
                        ? (NavigationTransitionInfo)new SuppressNavigationTransitionInfo()
                        : (NavigationTransitionInfo)new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight };

                    var result = await WaitForNavigationResult(() => _frame.GoBack(overrideInfo));

#if NETFX_CORE
					// On Windows, Frame re-creates a new Page instance for every navigation (whether through Navigate or GoBack),
					// unless NavigationCacheMode is set to Required (in which case a single Page instance is recycled).
					if (result.Content != activeEntry.View)
					{
						// A new Page instance has been created and set as the content of the Frame.
						// However, this new instance doesn't reflect the state or DataContext of the actual previous page.
						// Therefore, we replace it with the previous page instance we saved earlier.
						_frame.Content = activeEntry.View;
					}
#endif
                }
                catch (Exception e)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError($"Failed frame navigate back.", e);
                    }
                }

                return true;
            }
        }

        private async Task<NavigationEventArgs> WaitForNavigationResult(Action frameOperation)
        {
            var tcs = new TaskCompletionSource<NavigationEventArgs>();

            void OnNavigated(object sender, NavigationEventArgs e)
                => tcs.TrySetResult(e);

            void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
                => tcs.TrySetException(e.Exception);

            void OnNavigationStopped(object sender, NavigationEventArgs e)
                => tcs.TrySetException(new NavigationCanceledException());

            try
            {
                _frame.Navigated += OnNavigated;
                _frame.NavigationFailed += OnNavigationFailed;
                _frame.NavigationStopped += OnNavigationStopped;

                frameOperation();

                return await tcs.Task;
            }
            finally
            {
                _frame.Navigated -= OnNavigated;
                _frame.NavigationFailed -= OnNavigationFailed;
                _frame.NavigationStopped -= OnNavigationStopped;
            }
        }

        private Task<bool> UIResetDataContext(params NavigationStackEntry[] entries)
        {
            foreach (var entry in entries)
            {
                try
                {
                    if (entry.View is FrameworkElement frameworkElement)
                    {
                        // This is important to ensure that the view doesn't keep a reference
                        // to its ViewModel and create potential memory leaks.
                        frameworkElement.DataContext = null;
                    }
                }
                catch (Exception e)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError($"Failed to reset the data context.", e);
                    }
                }
            }

            return Task.FromResult(true);
        }
    }
}
