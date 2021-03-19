using System;
using System.Collections.Generic;
using System.Text;
using Chinook.StackNavigation;

namespace Chinook.SectionsNavigation
{
	public enum SectionsNavigatorRequestType
	{
		/// <summary>
		/// This is the request type when opening a new modal.
		/// <list type="bullet">
		///		<listheader><term>Relevant <see cref="SectionsNavigatorRequest"/> properties</term></listheader>
		///		<item><see cref="SectionsNavigatorRequest.ModalName"/> (Optional)</item>
		///		<item><see cref="SectionsNavigatorRequest.ModalPriority"/> (Optional)</item>
		///		<item><see cref="SectionsNavigatorRequest.NewModalStackNavigationRequest"/></item>
		/// </list>
		/// </summary>
		OpenModal,

		/// <summary>
		/// This is the request type when changing the active section.
		/// <list type="bullet">
		///		<listheader><term>Relevant <see cref="SectionsNavigatorRequest"/> properties</term></listheader>
		///		<item><see cref="SectionsNavigatorRequest.SectionName"/></item>
		/// </list>
		/// </summary>
		SetActiveSection,

		/// <summary>
		/// This is the request type when closing a modal.
		/// <list type="bullet">
		///		<listheader><term>Relevant <see cref="SectionsNavigatorRequest"/> properties</term></listheader>
		///		<item><see cref="SectionsNavigatorRequest.ModalName"/> (Optional)</item>
		///		<item><see cref="SectionsNavigatorRequest.ModalPriority"/> (Optional)</item>
		/// </list>
		/// </summary>
		CloseModal,

		/// <summary>
		/// This is the request type when a <see cref="ISectionsNavigator"/> reacts to a request from one of its <see cref="ISectionStackNavigator"/> or <see cref="IModalStackNavigator"/>.
		/// The only thing the <see cref="ISectionsNavigator"/> does with this is report the event using its <see cref="ISectionsNavigator.State"/> and <see cref="ISectionsNavigator.StateChanged"/> members.
		/// <list type="bullet">
		///		<listheader><term>Relevant <see cref="SectionsNavigatorRequest"/> properties</term></listheader>
		///		<item><see cref="SectionsNavigatorRequest.SectionName"/></item>
		/// </list>
		/// </summary>
		ReportSectionStateChanged
	}

	public class SectionsNavigatorRequest
	{
		public static SectionsNavigatorRequest GetSetActiveSectionRequest(string controllerName, SectionsNavigatorTransitionInfo transitionInfo = null) => new SectionsNavigatorRequest(
			SectionsNavigatorRequestType.SetActiveSection,
			sectionName: controllerName,
			modalName: null,
			modalPriority: null,
			newModalStackNavigationRequest: null,
			transitionInfo: transitionInfo
		);

		public static SectionsNavigatorRequest GetOpenModalRequest(StackNavigatorRequest newModalStackNavigationRequest, string modalName = null, int? modalPriority = null, SectionsNavigatorTransitionInfo transitionInfo = null) => new SectionsNavigatorRequest(
			SectionsNavigatorRequestType.OpenModal,
			sectionName: null,
			modalName: modalName,
			modalPriority: modalPriority,
			newModalStackNavigationRequest: newModalStackNavigationRequest,
			transitionInfo: transitionInfo
		);

		public static SectionsNavigatorRequest GetCloseModalRequest(string modalName = null, int? modalPriority = null, SectionsNavigatorTransitionInfo transitionInfo = null) => new SectionsNavigatorRequest(
			SectionsNavigatorRequestType.CloseModal,
			sectionName: null,
			modalName: modalName,
			modalPriority: modalPriority,
			newModalStackNavigationRequest: null,
			transitionInfo: transitionInfo
		);

		public SectionsNavigatorRequest(SectionsNavigatorRequestType requestType, string sectionName, string modalName, int? modalPriority, StackNavigatorRequest newModalStackNavigationRequest, SectionsNavigatorTransitionInfo transitionInfo)
		{
			RequestType = requestType;
			SectionName = sectionName;
			ModalName = modalName;
			ModalPriority = modalPriority;
			NewModalStackNavigationRequest = newModalStackNavigationRequest;
			TransitionInfo = transitionInfo;
		}

		/// <summary>
		/// Gets the type of section navigator request that this instance represents.
		/// This value indicates which other properties are meaningful for this instance.
		/// </summary>
		public SectionsNavigatorRequestType RequestType { get; }

		/// <summary>
		/// Gets the name of the <see cref="ISectionStackNavigator"/> associated with this request.
		/// This is null when this request is assocated with a modal.
		/// </summary>
		public string SectionName { get; }

		/// <summary>
		/// Gets the name of the <see cref="IModalStackNavigator"/> associated with this request.
		/// <br></br>When setting this to null and opening a modal, a generated name will be used.
		/// <br></br>When setting this to null and closing a modal, the <see cref="ModalPriority"/> will be used to find the modal.
		/// <br></br>This is null when this request is associated with a non-modal section.
		/// </summary>
		public string ModalName { get; }

		/// <summary>
		/// Gets the priority of the <see cref="IModalStackNavigator"/> associated with this request.
		/// The higher priority modals will mask lower priority ones.
		/// <br></br>When setting this to null and opening a modal, the current highest modal's priority + 1 will be used.
		/// <br></br>When setting this to null and closing a modal, the current highest modal's priority will be used.
		/// <br></br>This is null when this request is associated with a non-modal section.
		/// </summary>
		public int? ModalPriority { get; }

		/// <summary>
		/// Gets the <see cref="StackNavigatorRequest"/> for the newly created modal.
		/// This request is processed before the new modal becomes active.
		/// </summary>
		public StackNavigatorRequest NewModalStackNavigationRequest { get; }

		/// <summary>
		/// Gets the <see cref="TransitionInfo"/> for the operation.
		/// </summary>
		public SectionsNavigatorTransitionInfo TransitionInfo {get;}

		public override string ToString()
		{
			var builder = new StringBuilder($"{RequestType}");
			if (SectionName != null)
			{
				builder.Append(", ");
				builder.Append(nameof(SectionName));
				builder.Append($": {SectionName}");
			}

			if (ModalName != null)
			{
				builder.Append(", ");
				builder.Append(nameof(ModalName));
				builder.Append($": {ModalName}");
			}

			if (ModalPriority != null)
			{
				builder.Append(", ");
				builder.Append(nameof(ModalPriority));
				builder.Append($": {ModalPriority}");
			}

			if (NewModalStackNavigationRequest != null)
			{
				builder.Append(", ");
				builder.Append(nameof(NewModalStackNavigationRequest));
				builder.Append($": ({NewModalStackNavigationRequest})");
			}

			return builder.ToString();
		}
	}
}
