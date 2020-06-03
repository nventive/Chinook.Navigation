using Chinook.SectionsNavigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Chinook.Navigation.Tests.Contract
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
			IModalStackNavigator modalNavigator = await navigator.OpenModal(ct, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigation.StackNavigatorRequest.GetNavigateRequest(() => new TestVM()), "modalName", 0));
			await navigator.CloseModal(ct, SectionsNavigatorRequest.GetCloseModalRequest("modalName", 0));
		}

		[Fact]
		public async Task Extended_interface_contract_changes_can_be_detected()
		{
			var ct = CancellationToken.None;
			var navigator = new BlindSectionsNavigator("Section1", "Section2");

			// If the extension methods available in the abstraction package change their signatures, we get compilation errors here.

			// Only test extensions specific to ISectionStackNavigator

			ISectionStackNavigator sectionNavigator1 = await navigator.SetActiveSection(ct, "Section1");
			ISectionStackNavigator sectionNavigator2 = await navigator.SetActiveSection(ct, "Section1", () => new TestVM(), true);

			StackNavigation.IStackNavigator stackNavigator = navigator.GetActiveStackNavigator();
			await navigator.OpenModal(ct, () => new TestVM(), 0, "modalName");
			bool canNavigateBackOrCloseModal = navigator.CanNavigateBackOrCloseModal();
			await navigator.NavigateBackOrCloseModal(ct);

			IObservable<EventPattern<SectionsNavigatorEventArgs>> ob1 = navigator.ObserveStateChanged();
			IObservable<SectionsNavigatorState> ob2 = navigator.ObserveCurrentState();
		}

		[Fact]
		public void Extensions_on_IStackNavigator_are_available_for_ISectionsNavigator()
		{
			var assemblies = new Assembly[]
			{
				Assembly.GetAssembly(typeof(StackNavigation.IStackNavigator)),
				Assembly.GetAssembly(typeof(ISectionsNavigator)),
				Assembly.GetAssembly(typeof(StackNavigation.StackNavigatorReactiveExtensions)),
				Assembly.GetAssembly(typeof(SectionsNavigatorReactiveExtensions)),
			};

			ReflectionHelper.MatchExtensions(assemblies, typeof(StackNavigation.IStackNavigator), typeof(ISectionsNavigator),
				exceptions: new string[]
				{
					// Those StackNavigation extensions don't make sense to re-expose in the SectionsNavigator because they could be confusing.
					nameof(StackNavigation.StackNavigatorExtensions.CanNavigateBack),
					nameof(StackNavigation.StackNavigatorExtensions.TryNavigateBackTo),
					nameof(StackNavigation.StackNavigatorExtensions.ProcessRequest),
					nameof(StackNavigation.StackNavigatorReactiveExtensions.ObserveCurrentState),
					nameof(StackNavigation.StackNavigatorReactiveExtensions.ObserveStateChanged),
				});
		}

		private class TestVM : StackNavigation.INavigableViewModel
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
