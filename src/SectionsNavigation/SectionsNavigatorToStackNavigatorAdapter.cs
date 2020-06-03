using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chinook.StackNavigation;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This adapts an <see cref="ISectionsNavigator"/> into an <see cref="IStackNavigator"/>.
	/// To strategy to transform a MultiStackNavigation into a SingleStackNavigation is to forward the member implementation to the active stack navigator.
	/// </summary>
	public class SectionsNavigatorToStackNavigatorAdapter : IStackNavigator
	{
		private readonly ISectionsNavigator _sectionsNavigator;

		/// <summary>
		/// Creates a new instance of <see cref="SectionsNavigatorToStackNavigatorAdapter"/> from the specified adaptee.
		/// </summary>
		/// <param name="sectionsNavigator">The sections navigator to adapt into a <see cref="IStackNavigator"/>.</param>
		public SectionsNavigatorToStackNavigatorAdapter(ISectionsNavigator sectionsNavigator)
		{
			_sectionsNavigator = sectionsNavigator;
		}

		private IStackNavigator ActiveStackNavigator => (IStackNavigator)_sectionsNavigator.State.ActiveModal ?? _sectionsNavigator.State.ActiveSection;

		/// <inheritdoc/>
		public StackNavigatorState State => ActiveStackNavigator.State;

		/// <inheritdoc/>
		public event StackNavigatorStateChangedEventHandler StateChanged
		{
			add => ActiveStackNavigator.StateChanged += value;
			remove => ActiveStackNavigator.StateChanged -= value;
		}

		/// <inheritdoc/>
		public Task Clear(CancellationToken ct)
		{
			return ActiveStackNavigator.Clear(ct);
		}

		/// <inheritdoc/>
		public Task<INavigableViewModel> Navigate(CancellationToken ct, StackNavigatorRequest request)
		{
			return ActiveStackNavigator.Navigate(ct, request);
		}

		/// <inheritdoc/>
		public async Task<INavigableViewModel> NavigateBack(CancellationToken ct)
		{
			// NavigateBack is a special case where we prefer to go with NavigateBackOrCloseModal because it's more convenient.
			await _sectionsNavigator.NavigateBackOrCloseModal(ct);
			return ActiveStackNavigator.State.Stack.LastOrDefault().ViewModel;
		}

		/// <inheritdoc/>
		public Task RemoveEntries(CancellationToken ct, IEnumerable<int> indexes)
		{
			return ActiveStackNavigator.RemoveEntries(ct, indexes);
		}
	}
}
