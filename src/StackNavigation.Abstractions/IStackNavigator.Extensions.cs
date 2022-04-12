using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.StackNavigation
{
	/// <summary>
	/// This class provides extension methods on the <see cref="IStackNavigator"/> type.
	/// </summary>
	public static class StackNavigatorExtensions
	{
		/// <summary>
		/// Gets whether the navigator can navigate back.
		/// </summary>
		/// <param name="stackNavigator">The stack navigator.</param>
		public static bool CanNavigateBack(this IStackNavigator stackNavigator)
		{
			// Can't back if you only have 1 item in the stack because the stack contains the current item.
			return stackNavigator.State.Stack.Count >= 2;
		}

		/// <summary>
		/// Processes a <see cref="StackNavigatorRequest"/> based on its <see cref="StackNavigatorRequestType"/>.
		/// </summary>
		/// <param name="stackNavigator">The stack navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		/// <param name="request">The request to process.</param>
		public static Task ProcessRequest(this IStackNavigator stackNavigator, CancellationToken ct, StackNavigatorRequest request)
		{
			switch (request.RequestType)
			{
				case StackNavigatorRequestType.NavigateForward:
					return stackNavigator.Navigate(ct, request);
				case StackNavigatorRequestType.NavigateBack:
					return stackNavigator.NavigateBack(ct);
				case StackNavigatorRequestType.Clear:
					return stackNavigator.Clear(ct);
				case StackNavigatorRequestType.RemoveEntry:
					return stackNavigator.RemoveEntries(ct, request.EntryIndexesToRemove);
				default:
					throw new NotSupportedException($"The request type '{request.RequestType}' is not supported.");
			}
		}

		/// <summary>
		/// Navigates forward and clears the navigator's stack.
		/// </summary>
		/// <param name="navigator">The stack navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="viewModelProvider">The method to invoke to instanciate the ViewModel.</param>
		/// <param name="suppressTransition">Whether to suppress the navigation transition.</param>
		/// <returns>The instance of the newly</returns>
		public static async Task<INavigableViewModel> NavigateAndClear(this IStackNavigator navigator, CancellationToken ct, Type viewModelType, Func<INavigableViewModel> viewModelProvider, bool suppressTransition = false)
		{
			return await navigator.Navigate(ct, StackNavigatorRequest.GetNavigateRequest(viewModelType, viewModelProvider, suppressTransition, clearBackStack: true));
		}

		/// <summary>
		/// Navigates forward and clears the navigator's stack.
		/// </summary>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="navigator">The stack navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		/// <param name="viewModelProvider">The method to invoke to instanciate the ViewModel.</param>
		/// <param name="suppressTransition">Whether to suppress the navigation transition.</param>
		/// <returns>The instance of the newly</returns>
		public static async Task<TViewModel> NavigateAndClear<TViewModel>(this IStackNavigator navigator, CancellationToken ct, Func<TViewModel> viewModelProvider, bool suppressTransition = false)
			where TViewModel : INavigableViewModel
		{
			return (TViewModel)await navigator.Navigate(ct, StackNavigatorRequest.GetNavigateRequest(viewModelProvider, suppressTransition, clearBackStack: true));
		}

		/// <summary>
		/// Navigates forward.
		/// </summary>
		/// <param name="navigator">The stack navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="viewModelProvider">The method to invoke to instanciate the ViewModel.</param>
		/// <param name="suppressTransition">Whether to suppress the navigation transition.</param>
		/// <returns>The ViewModel instance of the active page after the navigation operation.</returns>
		public static async Task<INavigableViewModel> Navigate(this IStackNavigator navigator, CancellationToken ct, Type viewModelType, Func<INavigableViewModel> viewModelProvider, bool suppressTransition = false)
		{
			return await navigator.Navigate(ct, StackNavigatorRequest.GetNavigateRequest(viewModelType, viewModelProvider, suppressTransition));
		}

		/// <summary>
		/// Navigates forward.
		/// </summary>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="navigator">The stack navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		/// <param name="viewModelProvider">The method to invoke to instanciate the ViewModel.</param>
		/// <param name="suppressTransition">Whether to suppress the navigation transition.</param>
		/// <returns>The ViewModel instance of the active page after the navigation operation.</returns>
		public static async Task<TViewModel> Navigate<TViewModel>(this IStackNavigator navigator, CancellationToken ct, Func<TViewModel> viewModelProvider, bool suppressTransition = false)
			where TViewModel : INavigableViewModel
		{
			return (TViewModel)await navigator.Navigate(ct, StackNavigatorRequest.GetNavigateRequest(viewModelProvider, suppressTransition));
		}

		/// <summary>
		/// Removes the previous ViewModel from this <see cref="IStackNavigator"/>' stack.
		/// </summary>
		/// <param name="navigator">The stack navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		public static async Task RemovePrevious(this IStackNavigator navigator, CancellationToken ct)
		{
			await navigator.RemoveEntries(ct, new[] { navigator.State.Stack.Count - 2 });
		}

		/// <summary>
		/// Gets the ViewModel instance of the last item of this <see cref="IStackNavigator"/>'s stack.
		/// </summary>
		/// <param name="navigator">The stack navigator.</param>
		public static INavigableViewModel GetActiveViewModel(this IStackNavigator navigator)
		{
			return navigator.State.Stack.LastOrDefault()?.ViewModel;
		}

		/// <summary>
		/// Tries to navigate back to the ViewModel matching <paramref name="viewModelType"/>.
		/// </summary>
		/// <remarks>
		/// If no previous entry matches <paramref name="viewModelType"/>, nothing happens.
		/// </remarks>
		/// <param name="navigator">The stack navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		/// <param name="viewModelType">The ViewModel type.</param>
		/// <returns>True if the navigate back happened. False otherwise.</returns>
		public static async Task<bool> TryNavigateBackTo(this IStackNavigator navigator, CancellationToken ct, Type viewModelType)
		{
			var logger = typeof(StackNavigatorExtensions).Log();

			// Retrieve the current stack and its size
			var stack = navigator.State.Stack;
			var count = stack.Count;

			// Retrieve the instance of the target ViewModel
			var targetEntry = stack.FirstOrDefault(entry => entry.ViewModel.GetType() == viewModelType);
			if (targetEntry != null)
			{
				// Retrieve the index of the target viewModel
				var startIndex = stack.ToList().IndexOf(targetEntry);

				// Count how many items we should delete between the target viewModel and the current page viewmodel
				var itemsToDelete = count - 1 - startIndex - 1;
				if (itemsToDelete > 0)
				{
					var indexesToRemove = Enumerable.Range(startIndex + 1, itemsToDelete);
					await navigator.RemoveEntries(ct, indexesToRemove);
				}

				// Navigate back to the target view model
				await navigator.NavigateBack(ct);

				return true;
			}
			else
			{
				if (logger.IsEnabled(LogLevel.Warning))
				{
					logger.LogWarning($"Can't navigate back to '{viewModelType.Name}' because it's not in the navigation stack.");
				}

				return false;
			}
		}

		/// <summary>
		/// Tries to navigate back to the ViewModel matching <typeparamref name="TPageViewModel"/>.
		/// </summary>
		/// <remarks>
		/// If no previous entry matches <typeparamref name="TPageViewModel"/>, nothing happens.
		/// </remarks>
		/// <typeparam name="TPageViewModel">The ViewModel type.</typeparam>
		/// <param name="navigator">The stack navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		/// <returns>True if the navigate back happened. False otherwise.</returns>
		public static Task<bool> TryNavigateBackTo<TPageViewModel>(this IStackNavigator navigator, CancellationToken ct)
			=> TryNavigateBackTo(navigator, ct, typeof(TPageViewModel));

	}
}
