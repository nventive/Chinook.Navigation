using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Chinook.StackNavigation;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This class provides extension methods on the <see cref="ISectionsNavigator"/> type.
	/// </summary>
	public static class SectionsNavigatorExtensions
	{
		/// <summary>
		/// Sets the active section using the provided section name.
		/// </summary>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		/// <param name="sectionName">The name of the section to set as active.</param>
		/// <returns>The stack navigator of the active section.</returns>
		public static Task<ISectionStackNavigator> SetActiveSection(this ISectionsNavigator sectionsNavigator, CancellationToken ct, string sectionName)
		{
			return sectionsNavigator.SetActiveSection(ct, SectionsNavigatorRequest.GetSetActiveSectionRequest(sectionName));
		}

		/// <summary>
		/// Sets the active section using the provided section name and navigates.
		/// </summary>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		/// <param name="sectionName">The name of the section to set as active.</param>
		/// <param name="viewModelProvider">The method to make the view model instance. It will be invoked only if necessary.</param>
		/// <param name="returnToRoot">When this is true, the navigator will navigate back to the view model matching the type <typeparamref name="TViewModel"/>.</param>
		/// <returns>The stack navigator of the active section.</returns>
		public static async Task<ISectionStackNavigator> SetActiveSection<TViewModel>(this ISectionsNavigator sectionsNavigator,
			CancellationToken ct,
			string sectionName,
			Func<TViewModel> viewModelProvider,
			bool returnToRoot = false)
			where TViewModel : INavigableViewModel
		{
			if (ct.IsCancellationRequested)
			{
				typeof(SectionsNavigatorExtensions).Log().LogWarning($"Canceled 'SetActiveSection' operation to '{typeof(TViewModel).Name}' because of cancellation token.");

				return null;
			}

			// No cancellation beyond this point.
			ct = CancellationToken.None;

			var sectionNavigator = sectionsNavigator.State.Sections[sectionName];
			if (sectionNavigator.State.Stack.LastOrDefault() == null)
			{
				// Create the default page if there's nothing in the section.
				await sectionNavigator.Navigate(ct, StackNavigatorRequest.GetNavigateRequest(viewModelProvider, suppressTransition: true));
			}
			else if (returnToRoot && sectionNavigator.State.Stack.Last().ViewModel.GetType() != typeof(TViewModel))
			{
				if (sectionNavigator.State.Stack.Any(e => e.ViewModel.GetType() == typeof(TViewModel)))
				{
					// If the stack contains the root page of the section, remove all other entries and navigate back to it.
					var indexesToRemove = sectionNavigator.State.Stack
						.Select((entry, index) => (entry, index))
						.Where(t => t.entry.ViewModel.GetType() != typeof(TViewModel) && t.index < sectionNavigator.State.Stack.Count - 1)
						.Select(t => t.index)
						.ToList();

					await sectionNavigator.RemoveEntries(ct, indexesToRemove);
					await sectionNavigator.NavigateBack(ct);
				}
				else
				{
					// If the section root page isn't in the stack, clear everything and navigate to it.
					await sectionNavigator.Navigate(ct, StackNavigatorRequest.GetNavigateRequest(viewModelProvider, suppressTransition: true, clearBackStack: true));
				}
			}

			return await sectionsNavigator.SetActiveSection(ct, SectionsNavigatorRequest.GetSetActiveSectionRequest(sectionName));
		}

		/// <summary>
		/// Closes the top-most modal.
		/// </summary>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		public static async Task CloseModal(this ISectionsNavigator sectionsNavigator, CancellationToken ct)
		{
			await sectionsNavigator.CloseModal(ct, SectionsNavigatorRequest.GetCloseModalRequest(modalPriority: null));
		}

		/// <summary>
		/// Gets whether the <see cref="ISectionsNavigator"/> can navigate back or close a modal.
		/// </summary>
		/// <remarks>
		/// This is useful when dealing with hardware back buttons.
		/// </remarks>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		/// <returns>True if the navigator can navigate back or close a modal. False otherwise.</returns>
		public static bool CanNavigateBackOrCloseModal(this ISectionsNavigator sectionsNavigator)
		{
			return (sectionsNavigator.State.ActiveSection?.CanNavigateBack() ?? false) || sectionsNavigator.State.Modals.Any();
		}

		/// <summary>
		/// Executes back action depending on top-most frame state.
		/// </summary>
		/// <remarks>
		/// Priorities:
		/// <list type="number">
		/// <item>Navigates back within the modal, if possible.</item>
		/// <item>Closes the modal, if possible.</item>
		/// <item>Navigates back within a section.</item>
		/// </list>
		/// This is useful when dealing with hardware back buttons.
		/// </remarks>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		public static async Task NavigateBackOrCloseModal(this ISectionsNavigator sectionsNavigator, CancellationToken ct)
		{
			if (sectionsNavigator.State.ActiveModal?.CanNavigateBack() ?? false)
			{
				await sectionsNavigator.State.ActiveModal.NavigateBack(ct);
			}
			else if (sectionsNavigator.State.ActiveModal != null)
			{
				await sectionsNavigator.CloseModal(ct);
			}
			else if (sectionsNavigator.State.ActiveSection?.CanNavigateBack() ?? false)
			{
				await sectionsNavigator.State.ActiveSection.NavigateBack(ct);
			}
			else
			{
				throw new InvalidOperationException($"Failed to NavigateBack or CloseModal. The active section '{sectionsNavigator.State.ActiveSection?.Name ?? "null"}' can't currently navigate back and there are no modals to close.");
			}
		}

		/// <summary>
		/// Gets the active <see cref="IStackNavigator"/> of this <see cref="ISectionsNavigator"/>.
		/// </summary>
		/// <remarks>
		/// The result can be null if no navigator is active.
		/// </remarks>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		public static IStackNavigator GetActiveStackNavigator(this ISectionsNavigator sectionsNavigator)
		{
			return sectionsNavigator.State.GetActiveStackNavigator();
		}

		/// <summary>
		/// Calls <see cref="StackNavigatorExtensions.NavigateAndClear(IStackNavigator, CancellationToken, Type, Func{INavigableViewModel}, bool)"/> on this <see cref="ISectionsNavigator"/>'s active stack navigator.
		/// </summary>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		/// <param name="viewModelType">The ViewModel type.</param>
		/// <param name="viewModelProvider">The method to invoke to instanciate the ViewModel.</param>
		/// <param name="suppressTransition">Whether to suppress the navigation transition.</param>
		public static async Task<INavigableViewModel> NavigateAndClear(this ISectionsNavigator sectionsNavigator, CancellationToken ct, Type viewModelType, Func<INavigableViewModel> viewModelProvider, bool suppressTransition = false)
		{
			var navigator = GetActiveStackNavigator(sectionsNavigator);
			return await navigator.NavigateAndClear(ct, viewModelType, viewModelProvider, suppressTransition);
		}

		/// <summary>
		/// Calls <see cref="StackNavigatorExtensions.NavigateAndClear{TViewModel}(IStackNavigator, CancellationToken, Func{TViewModel}, bool)"/> on this <see cref="ISectionsNavigator"/>'s active stack navigator.
		/// </summary>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		/// <param name="viewModelProvider">The method to invoke to instanciate the ViewModel.</param>
		/// <param name="suppressTransition">Whether to suppress the navigation transition.</param>
		public static async Task<TViewModel> NavigateAndClear<TViewModel>(this ISectionsNavigator sectionsNavigator, CancellationToken ct, Func<TViewModel> viewModelProvider, bool suppressTransition = false)
			where TViewModel : INavigableViewModel
		{
			var navigator = GetActiveStackNavigator(sectionsNavigator);
			return await navigator.NavigateAndClear(ct, viewModelProvider, suppressTransition);
		}

		/// <summary>
		/// Calls <see cref="StackNavigatorExtensions.Navigate(IStackNavigator, CancellationToken, Type, Func{INavigableViewModel}, bool)"/> on this <see cref="ISectionsNavigator"/>'s active stack navigator.
		/// </summary>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		/// <param name="viewModelType">The ViewModel type.</param>
		/// <param name="viewModelProvider">The method to invoke to instanciate the ViewModel.</param>
		/// <param name="suppressTransition">Whether to suppress the navigation transition.</param>
		public static async Task<INavigableViewModel> Navigate(this ISectionsNavigator sectionsNavigator, CancellationToken ct, Type viewModelType, Func<INavigableViewModel> viewModelProvider, bool suppressTransition = false)
		{
			var navigator = GetActiveStackNavigator(sectionsNavigator);
			return await navigator.Navigate(ct, viewModelType, viewModelProvider, suppressTransition);
		}

		/// <summary>
		/// Calls <see cref="StackNavigatorExtensions.Navigate{TViewModel}(IStackNavigator, CancellationToken, Func{TViewModel}, bool)"/> on this <see cref="ISectionsNavigator"/>'s active stack navigator.
		/// </summary>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		/// <param name="viewModelProvider">The method to invoke to instanciate the ViewModel.</param>
		/// <param name="suppressTransition">Whether to suppress the navigation transition.</param>
		public static async Task<TViewModel> Navigate<TViewModel>(this ISectionsNavigator sectionsNavigator, CancellationToken ct, Func<TViewModel> viewModelProvider, bool suppressTransition = false)
			where TViewModel : INavigableViewModel
		{
			var navigator = GetActiveStackNavigator(sectionsNavigator);
			return await navigator.Navigate(ct, viewModelProvider, suppressTransition);
		}

		/// <summary>
		/// Calls <see cref="IStackNavigator.NavigateBack(CancellationToken)"/> on this <see cref="ISectionsNavigator"/>'s active stack navigator.
		/// </summary>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		public static async Task NavigateBack(this ISectionsNavigator sectionsNavigator, CancellationToken ct)
		{
			var navigator = GetActiveStackNavigator(sectionsNavigator);
			await navigator.NavigateBack(ct);
		}

		/// <summary>
		/// Calls <see cref="StackNavigatorExtensions.RemovePrevious(IStackNavigator, CancellationToken)"/> on this <see cref="ISectionsNavigator"/>'s active stack navigator.
		/// </summary>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		/// <param name="ct">The cancellation token.</param>
		public static async Task RemovePrevious(this ISectionsNavigator sectionsNavigator, CancellationToken ct)
		{
			var navigator = GetActiveStackNavigator(sectionsNavigator);
			await navigator.RemovePrevious(ct);
		}

		/// <summary>
		/// Calls <see cref="StackNavigatorExtensions.GetActiveViewModel(IStackNavigator)"/> on this <see cref="ISectionsNavigator"/>'s active stack navigator.
		/// </summary>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		public static INavigableViewModel GetActiveViewModel(this ISectionsNavigator sectionsNavigator)
		{
			var navigator = GetActiveStackNavigator(sectionsNavigator);
			return navigator.GetActiveViewModel();
		}

		/// <summary>
		/// Opens a new modal.
		/// </summary>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="sectionsNavigator">The sections navigator.</param>
		/// <param name="ct"></param>
		/// <param name="viewModelProvider">The method invoked to instanciate the new ViewModel.</param>
		/// <param name="priority">The modal's priority.</param>
		/// <param name="name">The modal's name.</param>
		/// <returns>The newly created ViewModel instance.</returns>
		public static async Task<TViewModel> OpenModal<TViewModel>(this ISectionsNavigator sectionsNavigator, CancellationToken ct, Func<TViewModel> viewModelProvider, int? priority = null, string name = null)
			where TViewModel : INavigableViewModel
		{
			var modalNavigator = await sectionsNavigator.OpenModal(ct, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(viewModelProvider, suppressTransition: true), name, priority));
			// Note that modalNavigator can be null if the OpenModal gets cancelled.
			return (TViewModel)modalNavigator?.GetActiveViewModel();
		}
	}
}
