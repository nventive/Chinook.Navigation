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
		/// To navigate back, the navigator requires at least 2 entries in its stack (including the current page).
		/// </summary>
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
		/// <typeparam name="TViewModel"></typeparam>
		/// <param name="controller"></param>
		/// <param name="ct"></param>
		/// <param name="viewModelProvider">The method to invoke to instanciate the ViewModel.</param>
		/// <param name="suppressTransition">Whether to suppress the navigation transition.</param>
		/// <returns>The instance of the newly</returns>
		public static async Task<TViewModel> NavigateAndClear<TViewModel>(this IStackNavigator controller, CancellationToken ct, Func<TViewModel> viewModelProvider, bool suppressTransition = false)
			where TViewModel : INavigableViewModel
		{
			return (TViewModel)await controller.Navigate(ct, StackNavigatorRequest.GetNavigateRequest(viewModelProvider, suppressTransition, clearBackStack: true));
		}

		/// <summary>
		/// Navigates forward.
		/// </summary>
		/// <typeparam name="TViewModel"></typeparam>
		/// <param name="controller"></param>
		/// <param name="ct"></param>
		/// <param name="viewModelProvider">The method to invoke to instanciate the ViewModel.</param>
		/// <param name="suppressTransition">Whether to suppress the navigation transition.</param>
		/// <returns>The instance of the newly</returns>
		public static async Task<TViewModel> Navigate<TViewModel>(this IStackNavigator controller, CancellationToken ct, Func<TViewModel> viewModelProvider, bool suppressTransition = false)
			where TViewModel : INavigableViewModel
		{
			return (TViewModel)await controller.Navigate(ct, StackNavigatorRequest.GetNavigateRequest(viewModelProvider, suppressTransition));
		}

		/// <summary>
		/// Removes the previous ViewModel from this <see cref="IStackNavigator"/>' stack.
		/// </summary>
		public static async Task RemovePrevious(this IStackNavigator controller, CancellationToken ct)
		{
			await controller.RemoveEntries(ct, new[] { controller.State.Stack.Count - 2 });
		}

		/// <summary>
		/// Gets the ViewModel instance of the last item of this <see cref="IStackNavigator"/>'s stack.
		/// </summary>
		public static INavigableViewModel GetActiveViewModel(this IStackNavigator stackNavigator)
		{
			return stackNavigator.State.Stack.LastOrDefault()?.ViewModel;
		}

		/// <summary>
		/// Navigates back to the ViewModel matching <typeparamref name="TPageViewModel"/>.
		/// If no previous entry matches <typeparamref name="TPageViewModel"/>, nothing happens.
		/// </summary>
		/// <typeparam name="TPageViewModel">The ViewModel type.</typeparam>
		/// <param name="stackNavigator"></param>
		/// <param name="ct"></param>
		/// <returns>True if the navigate back happened. False otherwise.</returns>
		public static async Task<bool> TryNavigateBackTo<TPageViewModel>(this IStackNavigator stackNavigator, CancellationToken ct)
		{
			var logger = typeof(StackNavigatorExtensions).Log();

			// Retrieve the current stack and its size
			var stack = stackNavigator.State.Stack;
			var count = stack.Count;

			// Retrieve the instance of the target ViewModel
			var targetEntry = stack.FirstOrDefault(entry => entry.ViewModel.GetType() == typeof(TPageViewModel));
			if (targetEntry != null)
			{
				// Retrieve the index of the target viewModel
				var startIndex = stack.ToList().IndexOf(targetEntry);

				// Count how many items we should delete between the target viewModel and the current page viewmodel
				var itemsToDelete = count - 1 - startIndex - 1;
				if (itemsToDelete > 0)
				{
					var indexesToRemove = Enumerable.Range(startIndex + 1, itemsToDelete);
					await stackNavigator.RemoveEntries(ct, indexesToRemove);
				}

				// Navigate back to the target view model
				await stackNavigator.NavigateBack(ct);

				return true;
			}
			else
			{
				if (logger.IsEnabled(LogLevel.Warning))
				{
					logger.LogWarning($"Can't navigate back to '{typeof(TPageViewModel).Name}' because it's not in the navigation stack.");
				}

				return false;
			}
		}
	}
}
