using Chinook.SectionsNavigation;
using Chinook.StackNavigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Contract
{
	public class ISectionsNavigatorTests
	{
		[Fact]
		public async Task Interface_contract_changes_can_be_detected()
		{
			var ct = CancellationToken.None;
			var navigator = new BlindSectionsNavigator("Section1", "Section2");

			// If the core contract changes, we get compilation errors here.
			ISectionStackNavigator sectionNavigator = await navigator.SetActiveSection(ct, SectionsNavigatorRequest.GetSetActiveSectionRequest("Section1"));
			IModalStackNavigator modalNavigator = await navigator.OpenModal(ct, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigatorRequest.GetNavigateRequest(() => new TestVM()), "modalName", 0));
			await navigator.CloseModal(ct, SectionsNavigatorRequest.GetCloseModalRequest("modalName", 0));
		}

		[Fact]
		public async Task Extended_interface_contract_changes_can_be_detected()
		{
			var ct = CancellationToken.None;
			var navigator = new BlindSectionsNavigator("Section1", "Section2");

			// If the extension methods available in the abstraction package change their signatures, we get compilation errors here.

			// Only test extensions specific to ISectionsNavigator

			ISectionStackNavigator sectionNavigator1 = await navigator.SetActiveSection(ct, "Section1");
			ISectionStackNavigator sectionNavigator2 = await navigator.SetActiveSection(ct, "Section1", () => new TestVM(), true);
			ISectionStackNavigator sectionNavigator3 = await navigator.SetActiveSection(ct, "Section1", typeof(TestVM), ProvideViewModel, true);

			IStackNavigator stackNavigator = navigator.GetActiveStackNavigator();
			TestVM modalVM = await navigator.OpenModal(ct, () => new TestVM(), 0, "modalName");
			bool canNavigateBackOrCloseModal = navigator.CanNavigateBackOrCloseModal();
			await navigator.NavigateBackOrCloseModal(ct);
			INavigableViewModel modalVMUntyped = await navigator.OpenModal(ct, typeof(TestVM), ProvideViewModel, 0, "modalName");

			IObservable<EventPattern<SectionsNavigatorEventArgs>> ob1 = navigator.ObserveStateChanged();
			IObservable<SectionsNavigatorState> ob2 = navigator.ObserveCurrentState();

			INavigableViewModel ProvideViewModel()
			{
				return new TestVM();
			}
		}

		[Fact]
		public void Extensions_on_IStackNavigator_are_available_for_ISectionsNavigator()
		{
			var assemblies = new Assembly[]
			{
				Assembly.GetAssembly(typeof(IStackNavigator)),
				Assembly.GetAssembly(typeof(ISectionsNavigator)),
				Assembly.GetAssembly(typeof(StackNavigatorReactiveExtensions)),
				Assembly.GetAssembly(typeof(SectionsNavigatorReactiveExtensions)),
			};

			ReflectionHelper.MatchExtensions(assemblies, typeof(IStackNavigator), typeof(ISectionsNavigator),
				exceptions: new string[]
				{
					// Those StackNavigation extensions don't make sense to re-expose in the SectionsNavigator because they could be confusing.
					nameof(StackNavigatorExtensions.CanNavigateBack),
					nameof(StackNavigatorExtensions.TryNavigateBackTo),
					nameof(StackNavigatorExtensions.ProcessRequest),
					nameof(StackNavigatorReactiveExtensions.ObserveCurrentState),
					nameof(StackNavigatorReactiveExtensions.ObserveStateChanged),
				});
		}

		private class TestVM : INavigableViewModel
		{
			public void Dispose()
			{
			}

			public void SetView(object view)
			{
			}
		}
	}
}
