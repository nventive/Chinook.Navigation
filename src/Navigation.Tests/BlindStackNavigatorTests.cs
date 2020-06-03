using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chinook.StackNavigation;
using Xunit;
using Xunit.Abstractions;

namespace Chinook.Navigation.Tests
{
	public class BlindStackNavigatorTests
	{
		[Fact]
		public void It_has_a_default_state()
		{
			var sut = new BlindStackNavigator();

			Assert.NotNull(sut.State);
			Assert.Empty(sut.State.Stack);
		}

		[Fact]
		public async Task It_adds_entries_to_stack_when_it_navigates_forward()
		{
			var sut = new BlindStackNavigator();

			var vm = await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM()));

			Assert.NotNull(vm);
			Assert.Single(sut.State.Stack);
		}

		[Fact]
		public async Task It_removes_all_stack_entries_when_it_clears()
		{
			var sut = new BlindStackNavigator();

			// Add an item to the stack.
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM()));

			// Clear the stack.
			await sut.Clear(CancellationToken.None);

			// Stack must be empty.
			Assert.Empty(sut.State.Stack);
		}

		[Fact]
		public async Task It_navigates_back()
		{
			var sut = new BlindStackNavigator();

			// Add 2 items to the stack.
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(0)));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1)));

			// The last entry must match the last navigation.
			Assert.Equal(1, ((TestVM)sut.State.Stack.Last().ViewModel).Id);

			var vmFromNavigation = await sut.NavigateBack(CancellationToken.None);
			var vmFromStack = (TestVM)sut.State.Stack.Last().ViewModel;

			// The ViewModels from the navigation and stack must be the same.
			Assert.Equal(vmFromStack, vmFromNavigation);

			// The final ViewModel must be the first one we navigated to.
			Assert.Equal(0, vmFromStack.Id);
		}

		[Fact]
		public async Task It_fails_to_navigate_back_when_there_arent_enough_entries()
		{
			var sut = new BlindStackNavigator();

			// Add 1 item to the stack.
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM()));

			var stackBeforeNavigateBack = sut.State.Stack;

			// Navigating back must 'fail' if there is only 1 entry.
			// There is no exception, but the result vm must be null.
			var vm = await sut.NavigateBack(CancellationToken.None);

			Assert.Null(vm);

			var stackAfterNavigateBack = sut.State.Stack;

			// Because the back didn't do anything, the stack must not change.
			Assert.Equal(stackAfterNavigateBack, stackBeforeNavigateBack);
		}

		[Fact]
		public async Task It_removes_entries_correctly()
		{
			var sut = new BlindStackNavigator();

			// Add 2 items to the stack.
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(0)));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1)));

			// Remove the first item
			await sut.RemoveEntries(CancellationToken.None, new int[] { 0 });

			// There must be only 1 item left in the stack because we just removed one.
			Assert.Single(sut.State.Stack);

			// The remaining item must be the second navigation.
			Assert.Equal(1, ((TestVM)sut.State.Stack.Last().ViewModel).Id);
		}

		[Fact]
		public async Task It_removes_one_entry_and_navigate_back_correctly()
		{
			var sut = new BlindStackNavigator();

			// Add 4 items to the stack.
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(0)));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1)));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(2)));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(3)));

			var indexes = Enumerable.Range(2, 1);

			// Remove the third item
			await sut.RemoveEntries(CancellationToken.None, indexes);

			// There must be 3 items left
			Assert.Equal(3, sut.State.Stack.Count);

			// Navigates back
			await sut.NavigateBack(CancellationToken.None);

			// The viewmodel count should be 2 and the current viewmodel id should be 1
			Assert.Equal(2, sut.State.Stack.Count);
			Assert.Equal(1, ((TestVM)sut.State.Stack.Last().ViewModel).Id);
		}

		[Fact]
		public async Task It_removes_multiple_entries_and_navigate_back_correctly()
		{
			var sut = new BlindStackNavigator();

			// Add 6 items to the stack.
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(0)));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1)));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(2)));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(3)));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(4)));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(5)));

			var indexes = Enumerable.Range(2, 3);

			// Remove the third, fourth and fifth items
			await sut.RemoveEntries(CancellationToken.None, indexes);

			// There must be 3 items left
			Assert.Equal(3, sut.State.Stack.Count);

			// Navigates back
			await sut.NavigateBack(CancellationToken.None);

			// The viewmodel count should be 2 and the current viewmodel id should be 1
			Assert.Equal(2, sut.State.Stack.Count);
			Assert.Equal(1, ((TestVM)sut.State.Stack.Last().ViewModel).Id);
		}

		[Fact]
		public async Task It_disposes_VM_when_removing_entries()
		{
			var sut = new BlindStackNavigator();

			// Add 2 items to the stack.
			var vm = (TestVM)await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(0)));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1)));

			// Remove the first item.
			await sut.RemoveEntries(CancellationToken.None, new int[] { 0 });

			// The removed entry must be disposed.
			Assert.True(vm.IsDisposed);
		}

		[Fact]
		public async Task It_disposes_VM_when_navigating_back()
		{
			var sut = new BlindStackNavigator();

			// Add 2 items to the stack.
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(0)));
			var vm = (TestVM)await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1)));

			// Navigate back to remove the last item.
			await sut.NavigateBack(CancellationToken.None);

			// The removed entry must be disposed.
			Assert.True(vm.IsDisposed);
		}

		[Fact]
		public async Task It_disposes_VMs_when_clearing()
		{
			var sut = new BlindStackNavigator();

			// Add 2 items to the stack.
			var vm0 = (TestVM)await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(0)));
			var vm1 = (TestVM)await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(1)));

			// Clear to remove all items.
			await sut.Clear(CancellationToken.None);

			// All entries must be disposed.
			Assert.True(vm0.IsDisposed);
			Assert.True(vm1.IsDisposed);
		}

		[Fact]
		public async Task It_produces_2_notifications_for_Navigate()
		{
			var sut = new BlindStackNavigator();

			var events = new List<StackNavigatorEventArgs>();
			sut.StateChanged += Sut_StateChanged;

			void Sut_StateChanged(object sender, StackNavigatorEventArgs args)
			{
				events.Add(args);
			}

			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM()));

			sut.StateChanged -= Sut_StateChanged;

			// There must be 2 notifications: 1 Processing when the navigation starts and 1 Processed when it ends.
			Assert.Equal(2, events.Count);
			Assert.Equal(NavigatorRequestState.Processing, events[0].CurrentState.LastRequestState);
			Assert.Equal(NavigatorRequestState.Processed, events[1].CurrentState.LastRequestState);
		}

		[Fact]
		public async Task It_produces_failed_notifications_for_Navigate()
		{
			var sut = new BlindStackNavigator();

			var events = new List<StackNavigatorEventArgs>();
			sut.StateChanged += Sut_StateChanged;

			void Sut_StateChanged(object sender, StackNavigatorEventArgs args)
			{
				events.Add(args);
			}

			await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => TestVM.Throw())));

			sut.StateChanged -= Sut_StateChanged;

			// There must be 2 notifications: 1 Processing when the navigation starts and 1 FailedToProcess when it ends.
			Assert.Equal(2, events.Count);
			Assert.Equal(NavigatorRequestState.Processing, events[0].CurrentState.LastRequestState);
			Assert.Equal(NavigatorRequestState.FailedToProcess, events[1].CurrentState.LastRequestState);
		}

		[Fact]
		public async Task It_produces_2_notifications_for_Back()
		{
			var sut = new BlindStackNavigator();

			// Add 2 items to be able to perform a back operation.
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM()));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM()));

			var events = new List<StackNavigatorEventArgs>();
			sut.StateChanged += Sut_StateChanged;

			void Sut_StateChanged(object sender, StackNavigatorEventArgs args)
			{
				events.Add(args);
			}

			await sut.NavigateBack(CancellationToken.None);

			sut.StateChanged -= Sut_StateChanged;

			// There must be 2 notifications: 1 Processing when the back starts and 1 Processed when it ends.
			Assert.Equal(2, events.Count);
			Assert.Equal(NavigatorRequestState.Processing, events[0].CurrentState.LastRequestState);
			Assert.Equal(NavigatorRequestState.Processed, events[1].CurrentState.LastRequestState);
		}

		[Fact]
		public async Task It_produces_failed_notifications_for_Back()
		{
			var sut = new BlindStackNavigator();

			// Add 2 items to be able to perform a back operation.
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM()));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(throwOnDispose: true)));

			var events = new List<StackNavigatorEventArgs>();
			sut.StateChanged += Sut_StateChanged;

			void Sut_StateChanged(object sender, StackNavigatorEventArgs args)
			{
				events.Add(args);
			}

			await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.NavigateBack(CancellationToken.None));

			sut.StateChanged -= Sut_StateChanged;

			// There must be 2 notifications: 1 Processing when the back starts and 1 FailedToProcess when it ends.
			Assert.Equal(2, events.Count);
			Assert.Equal(NavigatorRequestState.Processing, events[0].CurrentState.LastRequestState);
			Assert.Equal(NavigatorRequestState.FailedToProcess, events[1].CurrentState.LastRequestState);
		}

		[Fact]
		public async Task It_produces_2_notifications_for_Clear()
		{
			var sut = new BlindStackNavigator();

			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM()));

			var events = new List<StackNavigatorEventArgs>();
			sut.StateChanged += Sut_StateChanged;

			void Sut_StateChanged(object sender, StackNavigatorEventArgs args)
			{
				events.Add(args);
			}

			await sut.Clear(CancellationToken.None);

			sut.StateChanged -= Sut_StateChanged;

			// There must be 2 notifications: 1 Processing when the clear starts and 1 Processed when it ends.
			Assert.Equal(2, events.Count);
			Assert.Equal(NavigatorRequestState.Processing, events[0].CurrentState.LastRequestState);
			Assert.Equal(NavigatorRequestState.Processed, events[1].CurrentState.LastRequestState);
		}

		[Fact]
		public async Task It_produces_failed_notifications_for_Clear()
		{
			var sut = new BlindStackNavigator();

			 await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(throwOnDispose: true)));

			var events = new List<StackNavigatorEventArgs>();
			sut.StateChanged += Sut_StateChanged;

			void Sut_StateChanged(object sender, StackNavigatorEventArgs args)
			{
				events.Add(args);
			}

			await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.Clear(CancellationToken.None));

			sut.StateChanged -= Sut_StateChanged;

			// There must be 2 notifications: 1 Processing when the clear starts and 1 FailedToProcess when it ends.
			Assert.Equal(2, events.Count);
			Assert.Equal(NavigatorRequestState.Processing, events[0].CurrentState.LastRequestState);
			Assert.Equal(NavigatorRequestState.FailedToProcess, events[1].CurrentState.LastRequestState);
		}		

		[Fact]
		public async Task It_produces_2_notifications_for_RemoveEntry()
		{
			var sut = new BlindStackNavigator();

			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM()));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM()));

			var events = new List<StackNavigatorEventArgs>();
			sut.StateChanged += Sut_StateChanged;

			void Sut_StateChanged(object sender, StackNavigatorEventArgs args)
			{
				events.Add(args);
			}

			await sut.RemoveEntries(CancellationToken.None, new int[] { 0 });

			sut.StateChanged -= Sut_StateChanged;

			// There must be 2 notifications: 1 Processing when the remove starts and 1 Processed when it ends.
			Assert.Equal(2, events.Count);
			Assert.Equal(NavigatorRequestState.Processing, events[0].CurrentState.LastRequestState);
			Assert.Equal(NavigatorRequestState.Processed, events[1].CurrentState.LastRequestState);
		}

		[Fact]
		public async Task It_produces_failed_notifications_for_RemoveEntry()
		{
			var sut = new BlindStackNavigator();

			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM(throwOnDispose: true)));
			await sut.Navigate(CancellationToken.None, StackNavigatorRequest.GetNavigateRequest(() => new TestVM()));

			var events = new List<StackNavigatorEventArgs>();
			sut.StateChanged += Sut_StateChanged;

			void Sut_StateChanged(object sender, StackNavigatorEventArgs args)
			{
				events.Add(args);
			}

			await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.RemoveEntries(CancellationToken.None, new int[] { 0 }));

			sut.StateChanged -= Sut_StateChanged;

			// There must be 2 notifications: 1 Processing when the remove starts and 1 FailedToProcess when it ends.
			Assert.Equal(2, events.Count);
			Assert.Equal(NavigatorRequestState.Processing, events[0].CurrentState.LastRequestState);
			Assert.Equal(NavigatorRequestState.FailedToProcess, events[1].CurrentState.LastRequestState);
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
