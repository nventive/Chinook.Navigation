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
	/// This class decorates a <see cref="IStackNavigator"/> to implement both <see cref="ISectionsNavigator"/> and <see cref="IModalStackNavigator"/>.
	/// </summary>
	public class SectionStackNavigator : ISectionStackNavigator, IModalStackNavigator
	{
		private readonly IStackNavigator _inner;

		/// <summary>
		/// Creates a new instance of <see cref="SectionStackNavigator"/>.
		/// </summary>
		/// <param name="inner">The decorated stack navigator.</param>
		/// <param name="name">The name of this section.</param>
		/// <param name="isModal">Flag indicating whether this section is a modal.</param>
		/// <param name="priority">The priority of the section.</param>
		public SectionStackNavigator(IStackNavigator inner, string name, bool isModal, int priority)
		{
			_inner = inner;
			Name = name;
			IsModal = isModal;
			Priority = priority;

			_inner.StateChanged += OnInnerStateChanged;
		}

		private void OnInnerStateChanged(object sender, StackNavigatorEventArgs args)
		{
			StateChanged?.Invoke(this, args);
		}

		/// <inheritdoc/>
		public string Name { get; }

		/// <summary>
		/// Gets whether this stack is a modal.
		/// </summary>
		public bool IsModal { get; }

		/// <inheritdoc/>
		public int Priority { get; }

		/// <inheritdoc/>
		public StackNavigatorState State => _inner.State;

		/// <inheritdoc/>
		/// <remarks>
		/// We don't forward <see cref="StateChanged"/> to <see cref="_inner"/> because we want to clear the subscription in <see cref="Dispose"/>.
		/// </remarks>
		public event StackNavigatorStateChangedEventHandler StateChanged;

		/// <inheritdoc/>
		public Task Clear(CancellationToken ct)
		{
			return _inner.Clear(ct);
		}

		/// <inheritdoc/>
		public Task<INavigableViewModel> Navigate(CancellationToken ct, StackNavigatorRequest request)
		{
			return _inner.Navigate(ct, request);
		}

		/// <inheritdoc/>
		public Task<INavigableViewModel> NavigateBack(CancellationToken ct)
		{
			return _inner.NavigateBack(ct);
		}

		/// <inheritdoc/>
		public Task RemoveEntries(CancellationToken ct, IEnumerable<int> indexes)
		{
			return _inner.RemoveEntries(ct, indexes);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_inner.StateChanged -= OnInnerStateChanged;
			StateChanged = null;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Name}, Stack count: {State.Stack.Count}, Active ViewModel: {State.Stack.LastOrDefault()?.ViewModel.GetType().Name ?? "null"}";
		}
	}

}
