using Chinook.StackNavigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This class provides extension methods on the <see cref="SectionsNavigatorState"/> type.
	/// </summary>
	public static class SectionsNavigatorStateExtensions
	{
		/// <summary>
		/// Gets the active <see cref="IStackNavigator"/> of this <see cref="SectionsNavigatorState"/>.
		/// The result can be null is no navigator is active.
		/// </summary>
		public static IStackNavigator GetActiveStackNavigator(this SectionsNavigatorState sectionsNavigatorState)
		{
			return (IStackNavigator)sectionsNavigatorState.ActiveModal ?? sectionsNavigatorState.ActiveSection;
		}

		/// <summary>
		/// Gets the ViewModel type of the last item of the active <see cref="IStackNavigator"/> of this <see cref="SectionsNavigatorState"/>.
		/// </summary>
		public static Type GetLastViewModelType(this SectionsNavigatorState state)
		{
			return state.GetActiveStackNavigator()?.State.Stack.LastOrDefault()?.ViewModel.GetType();
		}

		/// <summary>
		/// Gets whether there is an active ViewModel in a <see cref="SectionsNavigatorState"/>.
		/// </summary>
		/// <param name="sectionsNavigatorState"></param>
		/// <returns>True if there is an active ViewModel. False otherwise.</returns>
		public static bool HasActiveViewModel(this SectionsNavigatorState sectionsNavigatorState)
		{
			return (sectionsNavigatorState.ActiveModal?.State.Stack.Any() ?? false) || (sectionsNavigatorState.ActiveSection?.State.Stack.Any() ?? false);
		}

		/// <summary>
		/// Gets the type of the next page ViewModel assuming the current <paramref name="sectionsNavigatorState"/> has a <see cref="NavigatorRequestState.Processing"/> request.
		/// Since the next page doesn't exist at that time, it's calculated using the request's type, parameters, and the current state.
		/// </summary>
		public static Type GetNextViewModelType(this SectionsNavigatorState sectionsNavigatorState)
		{
			if (sectionsNavigatorState.LastRequestState != NavigatorRequestState.Processing)
			{
				throw new InvalidOperationException("Can't get the next ViewModel on a processed request.");
			}

			var currentVM = sectionsNavigatorState.GetActiveStackNavigator()?.State.Stack.LastOrDefault()?.Request.ViewModelType;
			var sectionsRequest = sectionsNavigatorState.LastRequest;
			switch (sectionsRequest.RequestType)
			{
				case SectionsNavigatorRequestType.ReportSectionStateChanged:
					return GetNextFromReportSectionStateChanged();
				case SectionsNavigatorRequestType.OpenModal:
					return GetNextFromOpenModal();
				case SectionsNavigatorRequestType.CloseModal:
					return GetNextFromCloseModal();
				case SectionsNavigatorRequestType.SetActiveSection:
					return GetNextFromSetActiveSection();
				default:
					throw new NotSupportedException($"The request type {sectionsRequest.RequestType} is not supported.");
			}

			Type GetNextFromReportSectionStateChanged()
			{
				if (sectionsNavigatorState.IsStackNavigatorFromSectionsRequestActive(out var stackNavigator))
				{
					// If the current request is on the active frame, we know that the request will change the next page.
					var stackRequest = stackNavigator.State.LastRequest;
					switch (stackRequest.RequestType)
					{
						case StackNavigatorRequestType.NavigateForward:
							return stackRequest.ViewModelType;
						case StackNavigatorRequestType.NavigateBack:
							return stackNavigator.State.Stack[stackNavigator.State.Stack.Count - 2].Request.ViewModelType;
						case StackNavigatorRequestType.Clear:
							return null;
						case StackNavigatorRequestType.RemoveEntry:
							// RemoveEntry is only used to remove previous pages (not the current page), so it doesn't change the current VM.
							return currentVM;
						default:
							throw new NotSupportedException($"The request type {stackRequest.RequestType} is not supported.");
					}
				}
				else
				{
					// If the current request isn't on the active frame, then it's done in the background and doesn't affect the active page
					return currentVM;
				}
			}

			Type GetNextFromOpenModal()
			{
				if (sectionsRequest.ModalPriority.HasValue)
				{
					// If the priority is specified, the new modal could be opened behind an existing one.
					if ((sectionsNavigatorState.ActiveModal?.Priority ?? 0) >= sectionsRequest.ModalPriority.Value)
					{
						// If the priority of the new modal is above the active one's, the new modal will be the active one.
						return sectionsRequest.NewModalStackNavigationRequest.ViewModelType;
					}
					else
					{
						// If not, the new modal opens in background.
						return currentVM;
					}
				}
				else
				{
					// If the priority isn't specified, the new modal will be the active one.
					return sectionsRequest.NewModalStackNavigationRequest.ViewModelType;
				}
			}

			Type GetNextFromCloseModal()
			{
				var closingModal = sectionsNavigatorState.Modals.FirstOrDefault(m => m.Name == sectionsRequest.ModalName || m.Priority == sectionsRequest.ModalPriority) ?? sectionsNavigatorState.ActiveModal;
				if (closingModal == sectionsNavigatorState.ActiveModal)
				{
					// If the active modal is closing, the next stack is either another modal or a section.
					var modalUnderTheClosingOne = sectionsNavigatorState.Modals.FirstOrDefault(m => m.Priority < closingModal.Priority);
					if (modalUnderTheClosingOne == null)
					{
						return sectionsNavigatorState.ActiveSection.State.Stack.LastOrDefault()?.Request.ViewModelType;
					}
					else
					{
						return modalUnderTheClosingOne.State.Stack.LastOrDefault()?.Request.ViewModelType;
					}
				}
				else
				{
					// If not, the modal closes in background and the next ViewModel doesn't change.
					return currentVM;
				}
			}

			Type GetNextFromSetActiveSection()
			{
				if (sectionsNavigatorState.Modals.Any())
				{
					// If there are modals, changing sections doesn't change the active VM.
					return currentVM;
				}
				else
				{
					if (sectionsNavigatorState.Sections.TryGetValue(sectionsRequest.SectionName, out var sectionNavigator))
					{
						// If there is a section associated to the request, return that section's current VM.
						return sectionNavigator.State.Stack.LastOrDefault()?.Request.ViewModelType;
					}
					else
					{
						// If not, the request will fail. We fallback to the current VM.
						return currentVM;
					}
				}
			}
		}

		/// <summary>
		/// Gets whether the navigator from a <see cref="SectionsNavigatorRequestType.ReportSectionStateChanged"/> request is active.
		/// </summary>
		/// <param name="sectionsNavigatorState"></param>
		/// <param name="stackNavigator">The stack navigator that caused the <see cref="SectionsNavigatorRequestType.ReportSectionStateChanged"/> request if it's active. Null otherwhise.</param>
		/// <returns>Returns true when the <see cref="IStackNavigator"/> that caused the current <see cref="SectionsNavigatorRequestType.ReportSectionStateChanged"/> is currently active. False otherwise.</returns>
		public static bool IsStackNavigatorFromSectionsRequestActive(this SectionsNavigatorState sectionsNavigatorState, out IStackNavigator stackNavigator)
		{
			var sectionsNavigatorRequest = sectionsNavigatorState.LastRequest;
			var modalName = sectionsNavigatorRequest.ModalName;
			if (modalName != null)
			{
				stackNavigator = sectionsNavigatorState.Modals.FirstOrDefault(m => m.Name == modalName);
				if (stackNavigator == null)
				{
					return false;
				}
				else
				{
					return stackNavigator == sectionsNavigatorState.ActiveModal;
				}
			}
			else
			{
				var sectionName = sectionsNavigatorRequest.SectionName;
				stackNavigator = sectionsNavigatorState.Sections[sectionName];

				// Don't consider the section active if a Modal is active.
				return stackNavigator == sectionsNavigatorState.ActiveSection && sectionsNavigatorState.ActiveModal == null;
			}
		}
	}
}
