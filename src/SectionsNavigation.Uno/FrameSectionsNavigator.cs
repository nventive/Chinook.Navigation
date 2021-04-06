using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Chinook.StackNavigation;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using System.Threading;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// The <see cref="ISectionsNavigator"/> implementation for <see cref="Frame"/>-based views.
	/// This implementation requires a <see cref="MultiFrame"/> to handle the views.
	/// </summary>
	public class FrameSectionsNavigator : SectionsNavigatorBase
	{
		private readonly MultiFrame _multiFrame;
		private readonly IReadOnlyDictionary<Type, Type> _globalRegistrations;

		/// <summary>
		/// Creates a new instance of <see cref="FrameSectionsNavigator"/>.
		/// </summary>
		/// <param name="multiFrame">The <see cref="MultiFrame"/> hosting the views.</param>
		/// <param name="globalRegistrations">The dictionary of view model types mapping to their page type.</param>
		public FrameSectionsNavigator(MultiFrame multiFrame, IReadOnlyDictionary<Type, Type> globalRegistrations)
			: base(GetDefaultSections(multiFrame, globalRegistrations))
		{
			_multiFrame = multiFrame;
			_globalRegistrations = globalRegistrations;

			multiFrame.ModalClosedNatively += OnModalClosedNatively;
		}

		private async void OnModalClosedNatively(object sender, ModalClosedEventArgs e)
		{
			try
			{
				if (_logger.IsEnabled(LogLevel.Debug))
				{
					_logger.LogDebug($"Processing native close modal.");
				}

				// We schedule on a background thread because most of the work doesn't require any work on the UI thread.
				await Task.Run(async () => await CloseModal(CancellationToken.None, SectionsNavigatorRequest.GetCloseModalRequest(e.ModalName, transitionInfo: e.TransitionInfo)));
			}
			catch (Exception exception)
			{
				if (_logger.IsEnabled(LogLevel.Error))
				{
					_logger.LogError($"Failed to process native close modal.", exception);
				}
			}
			finally
			{
				if (_logger.IsEnabled(LogLevel.Debug))
				{
					_logger.LogDebug($"Processed native close modal.");
				}
			}
		}

		private static IReadOnlyDictionary<string, ISectionStackNavigator> GetDefaultSections(MultiFrame multiFrame, IReadOnlyDictionary<Type, Type> globalRegistrations)
		{
			return multiFrame.SectionsFrameNames.ToDictionary<string, string, ISectionStackNavigator>(
				keySelector: n => n,
				elementSelector: n => new SectionStackNavigator(new FrameStackNavigator(multiFrame.GetSectionFrame(n), globalRegistrations), n, isModal: false, priority: 0)
			);
		}

		private CoreDispatcher Dispatcher => _multiFrame.Dispatcher;

		/// <inheritdoc/>
		public override SectionsTransitionInfo DefaultSetActiveSectionTransitionInfo { get; set; } = FrameSectionsTransitionInfo.FadeInOrFadeOut;
		
		/// <inheritdoc/>
		public override SectionsTransitionInfo DefaultOpenModalTransitionInfo { get; set; } =
#if __IOS__
			FrameSectionsTransitionInfo.NativeiOSModal;
#else
			FrameSectionsTransitionInfo.SlideUp;
#endif
		
		/// <inheritdoc/>
		public override SectionsTransitionInfo DefaultCloseModalTransitionInfo { get; set; } =
#if __IOS__
			FrameSectionsTransitionInfo.NativeiOSModal;
#else
			FrameSectionsTransitionInfo.SlideDown;
#endif

		/// <inheritdoc/>
		protected override ILogger GetLogger()
		{
			return this.Log();
		}

		/// <inheritdoc/>
		protected override async Task<IStackNavigator> CreateStackNavigator(string name, int priority, SectionsTransitionInfo transitionInfo) // Runs on background thread
		{
			var frame = default(Frame);
			var transitionInfoType = ((FrameSectionsTransitionInfo)transitionInfo).Type;
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, CreateFrameUI);

			return new FrameStackNavigator(frame, _globalRegistrations);

			void CreateFrameUI() // Runs on UI thread
			{
				frame = _multiFrame.GetOrCreateFrame(name, priority, transitionInfoType);
			}
		}

		/// <inheritdoc/>
		protected override async Task InnerOpenModal(IModalStackNavigator navigator, bool isTopModal, SectionsTransitionInfo transitionInfo)
		{
			if (isTopModal)
			{
				var previousNavigatorName = State.ActiveModal?.Name ?? State.ActiveSection?.Name;
				await _multiFrame.OpenModal(previousNavigatorName, navigator.Name, (FrameSectionsTransitionInfo)transitionInfo);
			}
			else
			{
				// The view doesn't change.
			}
		}

		/// <inheritdoc/>
		protected override async Task InnerSetActiveSection(ISectionStackNavigator previousSection, ISectionStackNavigator nextSection, SectionsTransitionInfo transitionInfo)
		{
			if (previousSection == null)
			{
				await _multiFrame.ShowInstantly(nextSection.Name);
			}
			else
			{
				await _multiFrame.ChangeActiveSection(previousSection.Name, nextSection.Name, (FrameSectionsTransitionInfo)transitionInfo);				
			}
		}

		/// <inheritdoc/>
		protected override async Task InnerCloseModal(IModalStackNavigator modalToClose, SectionsTransitionInfo transitionInfo)
		{
			var request = State.LastRequest;
			var isClosingHiddenModal = request.ModalPriority.HasValue
				? State.Modals.Count >= 2 && State.Modals.Max(m => m.Priority) > request.ModalPriority.Value
				: false; // If the modal priority isn't specified, we automatically close the top-most modal.
			var navigatorToRevealName = State.Modals.LastOrDefault(m => m.Priority < modalToClose.Priority)?.Name ?? State.ActiveSection.Name;

			if (isClosingHiddenModal)
			{
				// When closing a hidden modal (one behind the active one), we simply remove the frame.
				await _multiFrame.RemoveFrame(modalToClose.Name);
			}
			else
			{
				await _multiFrame.CloseModal(modalToClose.Name, navigatorToRevealName, (FrameSectionsTransitionInfo)transitionInfo);
				await _multiFrame.RemoveFrame(modalToClose.Name);
			}
		}		
	}
}
