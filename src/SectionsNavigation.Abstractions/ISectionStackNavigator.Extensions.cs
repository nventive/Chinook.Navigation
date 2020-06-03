using Chinook.StackNavigation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.SectionsNavigation
{
    /// <summary>
	/// This class provides extension methods on the <see cref="ISectionStackNavigator"/> type.
	/// </summary>
    /// <remarks>
    /// This class forwards all method calls to <see cref="StackNavigatorExtensions"/>.
    /// The purpose of this class is to offer the same extensions on <see cref="IStackNavigator"/>, but in the <see cref="Chinook.SectionsNavigation"/> namespace.
    /// </remarks>
    public static class SectionStackNavigatorExtensions
    {
        /// <inheritdoc cref="StackNavigatorExtensions.CanNavigateBack(IStackNavigator)"/>
        public static bool CanNavigateBack(this ISectionStackNavigator stackNavigator)
        {
            return StackNavigatorExtensions.CanNavigateBack(stackNavigator);
        }

        /// <inheritdoc cref="StackNavigatorExtensions.ProcessRequest(IStackNavigator, CancellationToken, StackNavigatorRequest)"/>
        public static Task ProcessRequest(this ISectionStackNavigator stackNavigator, CancellationToken ct, StackNavigatorRequest request)
        {
            return StackNavigatorExtensions.ProcessRequest(stackNavigator, ct, request);
        }

        /// <inheritdoc cref="StackNavigatorExtensions.NavigateAndClear{TViewModel}(IStackNavigator, CancellationToken, Func{TViewModel}, bool)"/>
        public static Task<TViewModel> NavigateAndClear<TViewModel>(this ISectionStackNavigator stackNavigator, CancellationToken ct, Func<TViewModel> viewModelProvider, bool suppressTransition = false)
            where TViewModel : INavigableViewModel
        {
            return StackNavigatorExtensions.NavigateAndClear(stackNavigator, ct, viewModelProvider, suppressTransition);
        }

        /// <inheritdoc cref="StackNavigatorExtensions.Navigate{TViewModel}(IStackNavigator, CancellationToken, Func{TViewModel}, bool)"/>
        public static Task<TViewModel> Navigate<TViewModel>(this ISectionStackNavigator stackNavigator, CancellationToken ct, Func<TViewModel> viewModelProvider, bool suppressTransition = false)
            where TViewModel : INavigableViewModel
        {
            return StackNavigatorExtensions.Navigate(stackNavigator, ct, viewModelProvider, suppressTransition);
        }

        /// <inheritdoc cref="StackNavigatorExtensions.RemovePrevious(IStackNavigator, CancellationToken)"/>
        public static Task RemovePrevious(this ISectionStackNavigator stackNavigator, CancellationToken ct)
        {
            return StackNavigatorExtensions.RemovePrevious(stackNavigator, ct);
        }

        /// <inheritdoc cref="StackNavigatorExtensions.GetActiveViewModel(IStackNavigator)"/>
		public static INavigableViewModel GetActiveViewModel(this ISectionStackNavigator stackNavigator)
		{
            return StackNavigatorExtensions.GetActiveViewModel(stackNavigator);
        }

        /// <inheritdoc cref="StackNavigatorExtensions.TryNavigateBackTo{TPageViewModel}(IStackNavigator, CancellationToken)"/>
		public static Task<bool> TryNavigateBackTo<TPageViewModel>(this ISectionStackNavigator stackNavigator, CancellationToken ct)
		{
            return StackNavigatorExtensions.TryNavigateBackTo<TPageViewModel>(stackNavigator, ct);
        }
	}
}