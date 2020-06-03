using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Chinook.StackNavigation;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Chinook.SectionsNavigation
{
	public class FrameSectionsNavigator : SectionsNavigatorBase
	{
		private readonly MultiFrame _multiFrame;

		public FrameSectionsNavigator(MultiFrame multiFrame, IReadOnlyDictionary<Type, Type> globalRegistrations)
			: base(GetDefaultControllers(multiFrame, globalRegistrations), globalRegistrations)
		{
			_multiFrame = multiFrame;
		}

		private static IReadOnlyDictionary<string, ISectionStackNavigator> GetDefaultControllers(MultiFrame multiFrame, IReadOnlyDictionary<Type, Type> globalRegistrations)
		{
			return multiFrame.SectionsFrameNames.ToDictionary<string, string, ISectionStackNavigator>(
				keySelector: n => n,
				elementSelector: n => new SectionStackNavigator(new FrameStackNavigator(multiFrame.GetSectionFrame(n), globalRegistrations), n, isModal: false, priority: 0)
			);
		}

		private CoreDispatcher Dispatcher => _multiFrame.Dispatcher;

		protected override ILogger GetLogger()
		{
			return this.Log();
		}

		protected override async Task<IStackNavigator> CreateStackNavigator(string name, int priority, IReadOnlyDictionary<Type, Type> registrations) // Runs on background thread
		{
			var frame = default(Frame);
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, CreateFrameUI);

			return new FrameStackNavigator(frame, _globalRegistrations);

			void CreateFrameUI() // Runs on UI thread
			{
				frame = _multiFrame.GetOrCreateFrame(name, priority);
			}
		}

		protected override async Task InnerOpenModal(IModalStackNavigator navigator, bool isTopModal)
		{
			if (isTopModal)
			{
				var previousNavigatorName = State.ActiveModal?.Name ?? State.ActiveSection?.Name;
				await _multiFrame.OpenModal(previousNavigatorName, navigator.Name);
			}
			else
			{
				// The view doesn't change.
			}
		}

		protected override async Task InnerSetActiveSection(ISectionStackNavigator previousSection, ISectionStackNavigator nextSection)
		{
			if (previousSection == null)
			{
				await _multiFrame.ShowInstantly(nextSection.Name);
			}
			else
			{
				await _multiFrame.ChangeActiveSection(previousSection.Name, nextSection.Name);				
			}
		}

		protected override async Task InnerCloseModal(IModalStackNavigator modalToClose)
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
				await _multiFrame.CloseModal(modalToClose.Name, navigatorToRevealName);
				await _multiFrame.RemoveFrame(modalToClose.Name);
			}
		}		
	}
}
