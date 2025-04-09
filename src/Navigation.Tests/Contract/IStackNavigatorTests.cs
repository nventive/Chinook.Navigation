using Chinook.StackNavigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Contract
{
	public class IStackNavigatorTests
	{
		[Fact]
		public async Task Interface_contract_changes_can_be_detected()
		{
			var ct = CancellationToken.None;
			IStackNavigator navigator = new BlindStackNavigator();

			// If the core contract changes, we get compilation errors here.
			INavigableViewModel vmAfterNavigate = await navigator.Navigate(ct, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(), false, false));
			INavigableViewModel vmAfterBack = await navigator.NavigateBack(ct);
			await navigator.Clear(ct);

			// Navigate twice so that RemoveEntries works.
			await navigator.Navigate(ct, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(), false, false));
			await navigator.Navigate(ct, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(), false, false));
			await navigator.RemoveEntries(ct, new int[] { 0 }.AsEnumerable());
		}

		[Fact]
		public async Task Extended_interface_contract_changes_can_be_detected()
		{
			var ct = CancellationToken.None;
			IStackNavigator navigator = new BlindStackNavigator();

			// If the extension methods available in the abstraction package change their signatures, we get compilation errors here.

			TestVM vmAfterNavigate = await navigator.Navigate(ct, () => new TestVM(), suppressTransition: false);
			TestVM vmAfterNavigateAndClear = await navigator.NavigateAndClear(ct, () => new TestVM(), suppressTransition: false);

			INavigableViewModel vmAfterNavigateUntyped = await navigator.Navigate(ct, ProvideVM, suppressTransition: false);
			INavigableViewModel vmAfterNavigateAndClearUntyped = await navigator.NavigateAndClear(ct, ProvideVM, suppressTransition: false);

			INavigableViewModel vm = navigator.GetActiveViewModel();
			bool canGoBack = navigator.CanNavigateBack();

			// Navigate twice so that RemovePrevious works.
			await navigator.Navigate(ct, () => new TestVM(), suppressTransition: false);
			await navigator.Navigate(ct, () => new TestVM(), suppressTransition: false);
			await navigator.RemovePrevious(ct);

			bool didNavigateBack = await navigator.TryNavigateBackTo<TestVM>(ct);
			bool didNavigateBackUntyped = await navigator.TryNavigateBackTo(ct, typeof(TestVM));

			await navigator.ProcessRequest(ct, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(), false, false));

			IObservable<EventPattern<StackNavigatorEventArgs>> ob1 = navigator.ObserveStateChanged();
			IObservable<StackNavigatorState> ob2 = navigator.ObserveCurrentState();

			INavigableViewModel ProvideVM()
			{
				return new TestVM();
			}
		}

		private class TestVM : INavigableViewModel
		{
			public void Dispose()
			{
			}

			public void SetView(object view)
			{
			}

			public void WillDispose()
			{
			}
		}
	}
}
