using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Chinook.StackNavigation
{
	/// <summary>
	/// This is a base class for <see cref="IStackNavigator"/> implementations.
	/// This class does all the business logic to respect the <see cref="IStackNavigator"/> contract. 
	/// </summary>
	public abstract class StackNavigatorBase : IStackNavigator
	{
		/// <summary>The mutex to use when setting the <see cref="State"/> property to Processing.</summary>
		private readonly object _processingStateMutex = new object();

		protected readonly ILogger _logger;
		protected readonly IReadOnlyDictionary<Type, Type> _registrations;

		/// <summary>
		/// Creates a new instance of <see cref="StackNavigatorBase"/>.
		/// </summary>
		/// <param name="registrations">Mapping of view model types to their view types.</param>
		public StackNavigatorBase(IReadOnlyDictionary<Type, Type> registrations)
		{
			_registrations = registrations;
			_logger = GetLogger();

			State = new StackNavigatorState(
				stack: new List<NavigationStackEntry>(),
				lastRequestState: NavigatorRequestState.Processed,
				lastRequest: null);
		}

		/// <summary>
		/// Gets the logger for this class.
		/// This method is abstract because we want the actual (not abstract) implementation to show up in the logs instead of <see cref="StackNavigatorBase"/>.
		/// </summary>
		protected abstract ILogger GetLogger();

		private StackNavigatorState _state;

		/// <inheritdoc/>
		public StackNavigatorState State
		{
			get => _state;
			private set
			{
				var previous = _state;
				_state = value;
				StateChanged?.Invoke(this, new StackNavigatorEventArgs(previous, _state));
			}
		}

		protected IReadOnlyList<NavigationStackEntry> Stack => State.Stack;

		/// <inheritdoc/>
		public event StackNavigatorStateChangedEventHandler StateChanged;

		// Here the same pattern is used for the 3 main operations of the class (Clear, Navigate, NavigateBack):
		//
		// 1. Check for cancellation with the ct parameter.
		//    We cancel the operation if the method is invoked with an already "canceled" token.
		//    We don't use ct.Register because all navigation operations are non-cancellable.
		//
		// 2. Set the State to Processing.
		//    We use a lock to make sure only 1 call can change the State property to Processing.
		//    We don't need another lock at the end of each method when setting it back to Processed because invocations other than the first are discarded.
		//
		// 3. Process the request.
		//    This class deals with its own state correctly, but each method uses an async abstract method to allow implementors to handle view processing.
		//
		// 4. Set the State to Processed.
		//    Doing so allows future requests for processing.

		/// <inheritdoc/>
		public async Task Clear(CancellationToken ct)
		{
			if (_logger.IsEnabled(LogLevel.Debug))
			{
				_logger.LogDebug("Starting 'Clear' operation.");
			}

			if (ct.IsCancellationRequested)
			{
				if (_logger.IsEnabled(LogLevel.Warning))
				{
					_logger.LogWarning("Canceled 'Clear' operation because of cancellation token.");
				}

				return;
			}

			// No cancellation beyond this point.

			var request = StackNavigatorRequest.GetClearRequest();
			lock (_processingStateMutex)
			{
				if (State.LastRequestState == NavigatorRequestState.Processing)
				{
					if (_logger.IsEnabled(LogLevel.Warning))
					{
						_logger.LogWarning("Canceled 'Clear' operation because another request is processing.");
					}

					return;
				}

				State = new StackNavigatorState(Stack, NavigatorRequestState.Processing, request);
			}

			try
			{
				foreach (var entry in Stack)
				{
					entry.ViewModel.Dispose();
				}

				await InnerClear();

				State = new StackNavigatorState(
					stack: new List<NavigationStackEntry>(),
					lastRequestState: NavigatorRequestState.Processed,
					lastRequest: request);

				if (_logger.IsEnabled(LogLevel.Information))
				{
					_logger.LogInformation("Finished 'Clear' operation.");
				}
			}
			catch
			{
				// In the case of an error, this makes sure to set the state to FailedToProcess instead of Processing.
				State = new StackNavigatorState(Stack, NavigatorRequestState.FailedToProcess, request);
				throw;
			}
		}

		/// <summary>
		/// Implementors must override this method to typically update the View layer.
		/// </summary>
		protected abstract Task InnerClear();

		/// <inheritdoc/>
		public async Task RemoveEntries(CancellationToken ct, IEnumerable<int> indexes)
		{
			var logIndexes = string.Join(", ", indexes);
			if (_logger.IsEnabled(LogLevel.Debug))
			{
				_logger.LogDebug($"Starting 'RemoveEntry' operation for items [{logIndexes}].");
			}

			if (ct.IsCancellationRequested)
			{
				if (_logger.IsEnabled(LogLevel.Warning))
				{
					_logger.LogWarning($"Canceled 'RemoveEntry' operation for items [{logIndexes}] because of cancellation token.");
				}

				return;
			}

			// No cancellation beyond this point.

			var request = StackNavigatorRequest.GetRemoveEntryRequest(indexes);
			lock (_processingStateMutex)
			{
				if (State.LastRequestState == NavigatorRequestState.Processing)
				{
					if (_logger.IsEnabled(LogLevel.Warning))
					{
						_logger.LogWarning($"Canceled 'RemoveEntry' operation for items [{logIndexes}] because another request is processing.");
					}

					return;
				}

				State = new StackNavigatorState(Stack, NavigatorRequestState.Processing, request);
			}

			try
			{
				var orderedIndexes = indexes.OrderByDescending(i => i).ToList();
				var stack = Stack;

				// Start with the last item so that the indexes stay valid as we iterate.
				foreach (var index in orderedIndexes)
				{
					stack[index].ViewModel.Dispose();
					stack = stack.ImmutableRemoveAt(index);
				}

				await InnerRemoveEntries(orderedIndexes);

				State = new StackNavigatorState(
					stack: stack,
					lastRequestState: NavigatorRequestState.Processed,
					lastRequest: request);

				if (_logger.IsEnabled(LogLevel.Information))
				{
					_logger.LogInformation($"Finished 'RemoveEntry' operation for items [{logIndexes}].");
				}
			}
			catch
			{
				// In the case of an error, this makes sure to set the state to FailedToProcess instead of Processing.
				State = new StackNavigatorState(Stack, NavigatorRequestState.FailedToProcess, request);
				throw;
			}
		}

		/// <summary>
		/// Implementors must override this method to typically update the View layer.
		/// </summary>
		/// <param name="orderedIndexes">The list of indexes at which entries are being removed.</param>
		protected abstract Task InnerRemoveEntries(IEnumerable<int> orderedIndexes);

		/// <inheritdoc/>
		public async Task<INavigableViewModel> Navigate(CancellationToken ct, StackNavigatorRequest request)
		{
			if (_logger.IsEnabled(LogLevel.Debug))
			{
				_logger.LogDebug($"Starting 'Navigate' operation to '{request.ViewModelType.FullName}'.");
			}

			if (ct.IsCancellationRequested)
			{
				if (_logger.IsEnabled(LogLevel.Warning))
				{
					_logger.LogWarning($"Canceled 'Navigate' operation to '{request.ViewModelType.FullName}' because of cancellation token.");
				}

				return null;
			}

			// No cancellation beyond this point.

			lock (_processingStateMutex)
			{
				if (State.LastRequestState == NavigatorRequestState.Processing)
				{
					if (_logger.IsEnabled(LogLevel.Warning))
					{
						_logger.LogWarning($"Canceled 'Clear' operation to '{request.ViewModelType.FullName}' because another request is processing.");
					}

					return null;
				}

				State = new StackNavigatorState(Stack, NavigatorRequestState.Processing, request);
			}

			try
			{
				var viewModel = request.ViewModelProvider();
				var view = await InnerNavigateAndGetView(viewModel);

				var entry = new NavigationStackEntry(request, viewModel, view);
				var stack = request.ClearBackStack ?? false ? new List<NavigationStackEntry>() { entry } : Stack.ImmutableAdd(entry);

				State = new StackNavigatorState(
					stack: stack,
					lastRequestState: NavigatorRequestState.Processed,
					lastRequest: request);

				if (_logger.IsEnabled(LogLevel.Information))
				{
					_logger.LogInformation($"Finished 'Navigate' operation to '{request.ViewModelType.FullName}'.");
				}

				return viewModel;
			}
			catch
			{
				// In the case of an error, this makes sure to set the state to FailedToProcess instead of Processing.
				State = new StackNavigatorState(Stack, NavigatorRequestState.FailedToProcess, request);
				throw;
			}
		}

		/// <summary>
		/// Implementors must override this method to typically update the View layer.
		/// </summary>
		/// <param name="viewModel">The view model instance of the navigation target.</param>
		/// <returns>The view instance.</returns>
		protected abstract Task<object> InnerNavigateAndGetView(INavigableViewModel viewModel);

		/// <inheritdoc/>
		public async Task<INavigableViewModel> NavigateBack(CancellationToken ct)
		{
			if (_logger.IsEnabled(LogLevel.Debug))
			{
				_logger.LogDebug("Starting 'NavigateBack' operation.");
			}

			if (ct.IsCancellationRequested)
			{
				if (_logger.IsEnabled(LogLevel.Warning))
				{
					_logger.LogWarning("Canceled 'NavigateBack' operation because of cancellation token.");
				}

				return null;
			}

			// No cancellation beyond this point.

			var request = StackNavigatorRequest.GetNavigateBackRequest();
			lock (_processingStateMutex)
			{
				if (State.LastRequestState == NavigatorRequestState.Processing)
				{
					if (_logger.IsEnabled(LogLevel.Warning))
					{
						_logger.LogWarning("Canceled 'NavigateBack' operation because another request is processing.");
					}

					return null;
				}

				State = new StackNavigatorState(Stack, NavigatorRequestState.Processing, request);
			}

			try
			{
				if (Stack.Count < 2)
				{
					if (_logger.IsEnabled(LogLevel.Error))
					{
						_logger.LogError($"Failed 'NavigateBack' operation because there are fewer than 2 items in the stack ({Stack.Count}).");
					}

					State = new StackNavigatorState(Stack, NavigatorRequestState.Processed, request);

					return null;
				}
				else
				{
					var entryToRemove = Stack.Last();
					var activeEntry = Stack[Stack.Count - 2];

					entryToRemove.ViewModel.Dispose();

					await InnerNavigateBack(entryToRemove, activeEntry);

					State = new StackNavigatorState(Stack.ImmutableRemove(entryToRemove), NavigatorRequestState.Processed, request);

					if (_logger.IsEnabled(LogLevel.Information))
					{
						_logger.LogInformation($"Finished 'NavigateBack' operation to '{activeEntry.Request.ViewModelType.FullName}'.");
					}

					// Return the new active ViewModel.
					return activeEntry.ViewModel;
				}
			}
			catch
			{
				// In the case of an error, this makes sure to set the state to FailedToProcess instead of Processing.
				State = new StackNavigatorState(Stack, NavigatorRequestState.FailedToProcess, request);
				throw;
			}
		}

		/// <summary>
		/// Implementors must override this method to typically update the View layer.
		/// </summary>
		/// <param name="entryToRemove">The entry being removed.</param>
		/// <param name="activeEntry">The entry becoming active.</param>
		protected abstract Task InnerNavigateBack(NavigationStackEntry entryToRemove, NavigationStackEntry activeEntry);

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Stack count: {State.Stack.Count}, Active ViewModel: {State.Stack.LastOrDefault()?.ViewModel.GetType().Name ?? "null"}";
		}
	}
}
