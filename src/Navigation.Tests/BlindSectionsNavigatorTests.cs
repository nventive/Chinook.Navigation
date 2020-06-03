using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chinook.SectionsNavigation;
using Chinook.StackNavigation;
using Xunit;
using Xunit.Abstractions;

namespace Chinook.Navigation.Tests
{
	public class BlindSectionsNavigatorTests
	{
		[Fact]
		public void It_has_a_default_state()
		{
			var sut = new BlindSectionsNavigator();

			Assert.NotNull(sut.State);
			Assert.Null(sut.State.ActiveSection);
			Assert.NotNull(sut.State.Sections);
			Assert.Empty(sut.State.Sections);
		}

		[Fact]
		public void It_has_a_default_state_with_default_sections()
		{
			var sut = new BlindSectionsNavigator("defaultSection1", "defaultSection2");

			Assert.NotNull(sut.State);
			Assert.Null(sut.State.ActiveSection);
			Assert.NotNull(sut.State.Sections);
			Assert.Equal(2, sut.State.Sections.Count);
		}

		[Fact]
		public async Task It_stacks_modals_navigators()
		{
			var sut = new BlindSectionsNavigator();

			var modalNavigator = await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1), suppressTransition: true)));

			Assert.NotNull(modalNavigator);
			Assert.Equal(1, modalNavigator.Priority);
			Assert.Single(modalNavigator.State.Stack);

			Assert.Single(sut.State.Modals);
			Assert.NotNull(sut.State.ActiveModal);
			Assert.Equal(modalNavigator, sut.State.ActiveModal);
			Assert.Equal(modalNavigator, sut.State.Modals[0]);

			var modalNavigator2 = await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM(2), suppressTransition: true)));

			Assert.NotNull(modalNavigator2);
			Assert.Equal(2, modalNavigator2.Priority);
			Assert.Single(modalNavigator2.State.Stack);

			Assert.Equal(2, sut.State.Modals.Count);
			Assert.NotNull(sut.State.ActiveModal);
			Assert.Equal(modalNavigator2, sut.State.ActiveModal);
			Assert.Equal(modalNavigator, sut.State.Modals[0]);
			Assert.Equal(modalNavigator2, sut.State.Modals[1]);
		}

		[Fact]
		public async Task It_disposes_modal_VM_when_closing_modals()
		{
			var sut = new BlindSectionsNavigator();

			var modalNavigator = await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1), suppressTransition: true)));
			var vm = (TestVM)modalNavigator.State.Stack.Last().ViewModel;

			await sut.CloseModal(CancellationToken.None, SectionsNavigatorRequest.GetCloseModalRequest(null));

			Assert.True(vm.IsDisposed);
		}

		[Fact]
		public async Task It_orders_modals()
		{
			var sut = new BlindSectionsNavigator();

			var modalNavigator3 = await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM(3), suppressTransition: true), modalPriority: 3));
			var modalNavigator1 = await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1), suppressTransition: true), modalPriority: 1));

			Assert.Equal(sut.State.Modals[0], modalNavigator1);
			Assert.Equal(sut.State.Modals[1], modalNavigator3);

			var modalNavigator2 = await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM(2), suppressTransition: true), modalPriority: 2));

			Assert.Equal(sut.State.Modals[0], modalNavigator1);
			Assert.Equal(sut.State.Modals[1], modalNavigator2);
			Assert.Equal(sut.State.Modals[2], modalNavigator3);
		}

		[Fact]
		public async Task It_fails_to_open_2_modals_with_same_name()
		{
			var sut = new BlindSectionsNavigator();

			await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM(), suppressTransition: true), modalName: "modalName"));
			await Assert.ThrowsAsync<ArgumentException>(async () =>
			{
				await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM(), suppressTransition: true), modalName: "modalName"));
			});
		}

		[Fact]
		public async Task It_fails_to_open_2_modals_with_same_priority()
		{
			var sut = new BlindSectionsNavigator();

			await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM(), suppressTransition: true), modalPriority: 1));
			await Assert.ThrowsAsync<ArgumentException>(async () =>
			{
				await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM(), suppressTransition: true), modalPriority: 1));
			});
		}

		[Fact]
		public async Task It_produces_2_notifications_when_a_section_navigates()
		{
			var sut = new BlindSectionsNavigator("defaultSection1", "defaultSection2");
			var eventList = new List<(object sender, SectionsNavigatorEventArgs args)>();

			var section = await sut.SetActiveSection(CancellationToken.None, SectionsNavigatorRequest.GetSetActiveSectionRequest("defaultSection1"));

			sut.StateChanged += OnStateChanged;
			void OnStateChanged(object sender, SectionsNavigatorEventArgs args)
			{
				eventList.Add((sender, args));
			}

			await section.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1), suppressTransition: true));

			Assert.Equal(2, eventList.Count);
			Assert.Equal(NavigatorRequestState.Processing, eventList[0].args.CurrentState.LastRequestState);
			Assert.Equal(SectionsNavigatorRequestType.ReportSectionStateChanged, eventList[0].args.CurrentState.LastRequest.RequestType);
			Assert.Equal(NavigatorRequestState.Processed, eventList[1].args.CurrentState.LastRequestState);
			Assert.Equal(SectionsNavigatorRequestType.ReportSectionStateChanged, eventList[1].args.CurrentState.LastRequest.RequestType);
		}

		[Fact]
		public async Task It_produces_failed_notifications_when_a_section_navigates()
		{
			var sut = new BlindSectionsNavigator("defaultSection1", "defaultSection2");
			var eventList = new List<(object sender, SectionsNavigatorEventArgs args)>();

			var section = await sut.SetActiveSection(CancellationToken.None, SectionsNavigatorRequest.GetSetActiveSectionRequest("defaultSection1"));

			sut.StateChanged += OnStateChanged;
			void OnStateChanged(object sender, SectionsNavigatorEventArgs args)
			{
				eventList.Add((sender, args));
			}

			await Assert.ThrowsAsync<InvalidOperationException>(async () => await section.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => TestVM.Throw(), suppressTransition: true)));

			Assert.Equal(2, eventList.Count);
			Assert.Equal(NavigatorRequestState.Processing, eventList[0].args.CurrentState.LastRequestState);
			Assert.Equal(SectionsNavigatorRequestType.ReportSectionStateChanged, eventList[0].args.CurrentState.LastRequest.RequestType);
			Assert.Equal(NavigatorRequestState.FailedToProcess, eventList[1].args.CurrentState.LastRequestState);
			Assert.Equal(SectionsNavigatorRequestType.ReportSectionStateChanged, eventList[1].args.CurrentState.LastRequest.RequestType);
		}

		[Fact]
		public async Task It_produces_2_notifications_for_OpenModal()
		{
			var sut = new BlindSectionsNavigator();
			var eventList = new List<(object sender, SectionsNavigatorEventArgs args)>();

			sut.StateChanged += OnStateChanged;
			void OnStateChanged(object sender, SectionsNavigatorEventArgs args)
			{
				eventList.Add((sender, args));
			}

			var modalNavigator = await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1), suppressTransition: true)));
			sut.StateChanged -= OnStateChanged;

			Assert.Equal(2, eventList.Count);
			Assert.Equal(NavigatorRequestState.Processing, eventList[0].args.CurrentState.LastRequestState);
			Assert.Equal(SectionsNavigatorRequestType.OpenModal, eventList[0].args.CurrentState.LastRequest.RequestType);

			Assert.Equal(NavigatorRequestState.Processed, eventList[1].args.CurrentState.LastRequestState);
			Assert.Equal(SectionsNavigatorRequestType.OpenModal, eventList[1].args.CurrentState.LastRequest.RequestType);
		}

		[Fact]
		public async Task It_produces_failed_notifications_for_OpenModal()
		{
			var sut = new BlindSectionsNavigator();
			var eventList = new List<(object sender, SectionsNavigatorEventArgs args)>();

			sut.StateChanged += OnStateChanged;
			void OnStateChanged(object sender, SectionsNavigatorEventArgs args)
			{
				eventList.Add((sender, args));
			}

			await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => TestVM.Throw(), suppressTransition: true))));
			sut.StateChanged -= OnStateChanged;

			Assert.Equal(2, eventList.Count);
			Assert.Equal(NavigatorRequestState.Processing, eventList[0].args.CurrentState.LastRequestState);
			Assert.Equal(SectionsNavigatorRequestType.OpenModal, eventList[0].args.CurrentState.LastRequest.RequestType);

			Assert.Equal(NavigatorRequestState.FailedToProcess, eventList[1].args.CurrentState.LastRequestState);
			Assert.Equal(SectionsNavigatorRequestType.OpenModal, eventList[1].args.CurrentState.LastRequest.RequestType);
		}

		[Fact]
		public async Task It_produces_2_notifications_for_SetActiveSection()
		{
			var sut = new BlindSectionsNavigator("defaultSection1", "defaultSection2");

			var eventList = new List<(object sender, SectionsNavigatorEventArgs args)>();

			sut.StateChanged += OnStateChanged;
			void OnStateChanged(object sender, SectionsNavigatorEventArgs args)
			{
				eventList.Add((sender, args));
			}

			Assert.Null(sut.State.ActiveSection);

			const string sectionName = "defaultSection1";
			var activeSection = await sut.SetActiveSection(CancellationToken.None, SectionsNavigatorRequest.GetSetActiveSectionRequest(sectionName));
			sut.StateChanged -= OnStateChanged;

			Assert.NotNull(sut.State.ActiveSection);
			Assert.Equal(sut.State.ActiveSection, activeSection);

			Assert.Equal(2, eventList.Count);
			Assert.Equal(NavigatorRequestState.Processing, eventList[0].args.CurrentState.LastRequestState);
			Assert.Equal(NavigatorRequestState.Processed, eventList[1].args.CurrentState.LastRequestState);
		}

		[Fact]
		public async Task It_produces_failed_notifications_for_SetActiveSection()
		{
			var sut = new BlindSectionsNavigator("defaultSection1", "defaultSection2");

			var eventList = new List<(object sender, SectionsNavigatorEventArgs args)>();

			sut.StateChanged += OnStateChanged;
			void OnStateChanged(object sender, SectionsNavigatorEventArgs args)
			{
				eventList.Add((sender, args));
			}

			Assert.Null(sut.State.ActiveSection);

			await Assert.ThrowsAsync<KeyNotFoundException>(async () => await sut.SetActiveSection(CancellationToken.None, SectionsNavigatorRequest.GetSetActiveSectionRequest("Invalid section name")));
			sut.StateChanged -= OnStateChanged;

			Assert.Null(sut.State.ActiveSection);

			Assert.Equal(2, eventList.Count);
			Assert.Equal(NavigatorRequestState.Processing, eventList[0].args.CurrentState.LastRequestState);
			Assert.Equal(NavigatorRequestState.FailedToProcess, eventList[1].args.CurrentState.LastRequestState);
		}

		[Fact]
		public async Task It_produces_2_notifications_for_CloseModal()
		{
			var sut = new BlindSectionsNavigator();

			var eventList = new List<(object sender, SectionsNavigatorEventArgs args)>();

			var modalNavigator = await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1), suppressTransition: true)));

			sut.StateChanged += OnStateChanged;
			void OnStateChanged(object sender, SectionsNavigatorEventArgs args)
			{
				eventList.Add((sender, args));
			}

			await sut.CloseModal(CancellationToken.None, SectionsNavigatorRequest.GetCloseModalRequest(null));
			sut.StateChanged -= OnStateChanged;

			Assert.Null(sut.State.ActiveSection);

			Assert.Equal(2, eventList.Count);
			Assert.Equal(NavigatorRequestState.Processing, eventList[0].args.CurrentState.LastRequestState);
			Assert.Equal(NavigatorRequestState.Processed, eventList[1].args.CurrentState.LastRequestState);
		}

		[Fact]
		public async Task It_produces_failed_notifications_for_CloseModal()
		{
			var sut = new BlindSectionsNavigator();

			var eventList = new List<(object sender, SectionsNavigatorEventArgs args)>();

			var modalNavigator = await sut.OpenModal(CancellationToken.None, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1, throwOnDispose: true), suppressTransition: true)));

			sut.StateChanged += OnStateChanged;
			void OnStateChanged(object sender, SectionsNavigatorEventArgs args)
			{
				eventList.Add((sender, args));
			}

			await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.CloseModal(CancellationToken.None, SectionsNavigatorRequest.GetCloseModalRequest(null)));
			sut.StateChanged -= OnStateChanged;

			Assert.Null(sut.State.ActiveSection);

			Assert.Equal(2, eventList.Count);
			Assert.Equal(NavigatorRequestState.Processing, eventList[0].args.CurrentState.LastRequestState);
			Assert.Equal(NavigatorRequestState.FailedToProcess, eventList[1].args.CurrentState.LastRequestState);
		}

		private class TestVM : INavigableViewModel
		{
			public static TestVM Throw()
			{
				throw new InvalidOperationException();
			}

			private readonly bool _throwOnDispose;

			public TestVM(int id = 0, bool throwOnDispose = false)
			{
				Id = id;
				_throwOnDispose = throwOnDispose;
			}

			public int Id { get; set; }

			public bool IsDisposed { get; private set; }

			public void Dispose()
			{
				if (_throwOnDispose)
				{
					throw new InvalidOperationException();
				}

				IsDisposed = true;
			}

			public void SetView(object view)
			{
				throw new NotImplementedException();
			}
		}
	}
}
