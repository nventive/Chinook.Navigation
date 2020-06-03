using Chinook.StackNavigation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.SectionsNavigation
{
    /// <summary>
	/// This class provides extension methods on the <see cref="IModalStackNavigator"/> type.
	/// </summary>
    /// <remarks>
    /// This class forwards all method calls to <see cref="StackNavigatorExtensions"/>.
    /// The purpose of this class is to offer the same extensions on <see cref="IStackNavigator"/>, but in the <see cref="Chinook.SectionsNavigation"/> namespace.
    /// </remarks>
    public static class ModalStackNavigatorExtensions
    {
        /// <inheritdoc cref="StackNavigatorExtensions.CanNavigateBack(IStackNavigator)"/>
        public static bool CanNavigateBack(this IModalStackNavigator stackNavigator)
        {
            return StackNavigatorExtensions.CanNavigateBack(stackNavigator);
        }

        /// <inheritdoc cref="StackNavigatorExtensions.ProcessRequest(IStackNavigator, CancellationToken, StackNavigatorRequest)"/>
        public static Task ProcessRequest(this IModalStackNavigator stackNavigator, CancellationToken ct, StackNavigatorRequest request)
        {
            return StackNavigatorExtensions.ProcessRequest(stackNavigator, ct, request);
        }

        /// <inheritdoc cref="StackNavigatorExtensions.NavigateAndClear{TViewModel}(IStackNavigator, CancellationToken, Func{TViewModel}, bool)"/>
        public static Task<TViewModel> NavigateAndClear<TViewModel>(this IModalStackNavigator stackNavigator, CancellationToken ct, Func<TViewModel> viewModelProvider, bool suppressTransition = false)
            where TViewModel : INavigableViewModel
        {
            return StackNavigatorExtensions.NavigateAndClear(stackNavigator, ct, viewModelProvider, suppressTransition);
        }

        /// <inheritdoc cref="StackNavigatorExtensions.Navigate{TViewModel}(IStackNavigator, CancellationToken, Func{TViewModel}, bool)"/>
        public static Task<TViewModel> Navigate<TViewModel>(this IModalStackNavigator stackNavigator, CancellationToken ct, Func<TViewModel> viewModelProvider, bool suppressTransition = false)
            where TViewModel : INavigableViewModel
        {
            return StackNavigatorExtensions.Navigate(stackNavigator, ct, viewModelProvider, suppressTransition);
        }

        /// <inheritdoc cref="StackNavigatorExtensions.RemovePrevious(IStackNavigator, CancellationToken)"/>
        public static Task RemovePrevious(this IModalStackNavigator stackNavigator, CancellationToken ct)
        {
            return StackNavigatorExtensions.RemovePrevious(stackNavigator, ct);
        }

        /// <inheritdoc cref="StackNavigatorExtensions.GetActiveViewModel(IStackNavigator)"/>
		public static INavigableViewModel GetActiveViewModel(this IModalStackNavigator stackNavigator)
		{
            return StackNavigatorExtensions.GetActiveViewModel(stackNavigator);
        }

        /// <inheritdoc cref="StackNavigatorExtensions.TryNavigateBackTo{TPageViewModel}(IStackNavigator, CancellationToken)"/>
		public static Task<bool> TryNavigateBackTo<TPageViewModel>(this IModalStackNavigator stackNavigator, CancellationToken ct)
		{
            return StackNavigatorExtensions.TryNavigateBackTo<TPageViewModel>(stackNavigator, ct);
        }
	}
}