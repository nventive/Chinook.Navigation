using System;
using System.Collections.Generic;
using System.Text;
using Chinook.StackNavigation;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// Represents the type of <see cref="SectionsNavigatorRequest"/>.
	/// </summary>
	public enum SectionsNavigatorRequestType
	{
		/// <summary>
		/// This is the request type when opening a new modal.
		/// <list type="bullet">
		///		<listheader><term>Relevant <see cref="SectionsNavigatorRequest"/> properties</term></listheader>
		///		<item><see cref="SectionsNavigatorRequest.ModalName"/> (Optional)</item>
		///		<item><see cref="SectionsNavigatorRequest.ModalPriority"/> (Optional)</item>
		///		<item><see cref="SectionsNavigatorRequest.NewModalStackNavigationRequest"/></item>
		///		<item><see cref="SectionsNavigatorRequest.TransitionInfo"/> (Optional)</item>
		///		<item><see cref="SectionsNavigatorRequest.NewModalClosingTransitionInfo"/> (Optional)</item>
		/// </list>
		/// </summary>
		OpenModal,

		/// <summary>
		/// This is the request type when changing the active section.
		/// <list type="bullet">
		///		<listheader><term>Relevant <see cref="SectionsNavigatorRequest"/> properties</term></listheader>
		///		<item><see cref="SectionsNavigatorRequest.SectionName"/></item>
		///		<item><see cref="SectionsNavigatorRequest.TransitionInfo"/> (Optional)</item>
		/// </list>
		/// </summary>
		SetActiveSection,

		/// <summary>
		/// This is the request type when closing a modal.
		/// <list type="bullet">
		///		<listheader><term>Relevant <see cref="SectionsNavigatorRequest"/> properties</term></listheader>
		///		<item><see cref="SectionsNavigatorRequest.ModalName"/> (Optional)</item>
		///		<item><see cref="SectionsNavigatorRequest.ModalPriority"/> (Optional)</item>
		///		<item><see cref="SectionsNavigatorRequest.TransitionInfo"/> (Optional)</item>
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

	/// <summary>
	/// Represents the request to be executed by a <see cref="ISectionsNavigator"/>.
	/// </summary>
	public class SectionsNavigatorRequest
	{
		/// <summary>
		/// Create a new instance of <see cref="SectionsNavigatorRequest"/> of type <see cref="SectionsNavigatorRequestType.SetActiveSection"/>.
		/// </summary>
		/// <param name="sectionName">The section name.</param>
		/// <param name="transitionInfo">The optional transition info.</param>
		/// <returns>The newly created request.</returns>
		public static SectionsNavigatorRequest GetSetActiveSectionRequest(string sectionName, SectionsTransitionInfo transitionInfo = null) => new SectionsNavigatorRequest(
			SectionsNavigatorRequestType.SetActiveSection,
			sectionName: sectionName,
			modalName: null,
			modalPriority: null,
			newModalStackNavigationRequest: null,
			transitionInfo: transitionInfo,
			newModalClosingTransitionInfo: null
		);

		/// <summary>
		/// Create a new instance of <see cref="SectionsNavigatorRequest"/> of type <see cref="SectionsNavigatorRequestType.OpenModal"/>.
		/// </summary>
		/// <param name="newModalStackNavigationRequest">The initial <see cref="StackNavigatorRequest"/> for the new <see cref="IModalStackNavigator"/>.</param>
		/// <param name="modalName">The optional modal name.</param>
		/// <param name="modalPriority">The optional modal priority.</param>
		/// <param name="transitionInfo">The optional transition info.</param>
		/// <param name="newModalClosingTransitionInfo">The optional transition info for the future close modal request.</param>
		/// <returns>The newly created request.</returns>
		public static SectionsNavigatorRequest GetOpenModalRequest(StackNavigatorRequest newModalStackNavigationRequest, string modalName = null, int? modalPriority = null, SectionsTransitionInfo transitionInfo = null, SectionsTransitionInfo newModalClosingTransitionInfo = null) => new SectionsNavigatorRequest(
			SectionsNavigatorRequestType.OpenModal,
			sectionName: null,
			modalName: modalName,
			modalPriority: modalPriority,
			newModalStackNavigationRequest: newModalStackNavigationRequest,
			transitionInfo: transitionInfo,
			newModalClosingTransitionInfo: newModalClosingTransitionInfo
		);

		/// <summary>
		/// Create a new instance of <see cref="SectionsNavigatorRequest"/> of type <see cref="SectionsNavigatorRequestType.CloseModal"/>.
		/// </summary>
		/// <param name="modalName">The optional modal name.</param>
		/// <param name="modalPriority">The optional modal priority.</param>
		/// <param name="transitionInfo">The optional transition info.</param>
		/// <returns>The newly created request.</returns>
		public static SectionsNavigatorRequest GetCloseModalRequest(string modalName = null, int? modalPriority = null, SectionsTransitionInfo transitionInfo = null) => new SectionsNavigatorRequest(
			SectionsNavigatorRequestType.CloseModal,
			sectionName: null,
			modalName: modalName,
			modalPriority: modalPriority,
			newModalStackNavigationRequest: null,
			transitionInfo: transitionInfo,
			newModalClosingTransitionInfo: null
		);

		/// <summary>
		/// Creates a new instance of <see cref="SectionsNavigatorRequest"/>.
		/// </summary>
		/// <param name="requestType">The type of request.</param>
		/// <param name="sectionName">The sections name associated to this request.</param>
		/// <param name="modalName">The modal name associated to this request.</param>
		/// <param name="modalPriority">The modal priority associated to this request.</param>
		/// <param name="newModalStackNavigationRequest">The <see cref="StackNavigatorRequest"/> associated to this request.</param>
		/// <param name="transitionInfo">The transition info of this request.</param>
		/// <param name="newModalClosingTransitionInfo">The default transiton info of the future close modal request.</param>
		public SectionsNavigatorRequest(SectionsNavigatorRequestType requestType, string sectionName, string modalName, int? modalPriority, StackNavigatorRequest newModalStackNavigationRequest, SectionsTransitionInfo transitionInfo, SectionsTransitionInfo newModalClosingTransitionInfo)
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
		public SectionsTransitionInfo TransitionInfo { get; }

		/// <summary>
		/// Gets the <see cref="TransitionInfo"/> for the future close modal operation of the newly created modal.
		/// </summary>
		/// <remarks>
		/// The open and close transitions should be correlated so it's very useful to define the close transition as you open the modal. 
		/// </remarks>
		public SectionsTransitionInfo NewModalClosingTransitionInfo { get; }

		/// <inheritdoc/>
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
