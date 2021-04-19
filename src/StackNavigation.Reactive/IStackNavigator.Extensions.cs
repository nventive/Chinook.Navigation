using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace Chinook.StackNavigation
{
	/// <summary>
	/// This class provides reactive extension methods on the <see cref="IStackNavigator"/> type.
	/// </summary>
	public static class StackNavigatorReactiveExtensions
	{
		/// <summary>
		/// Gets an observable sequence that produces values whenever <see cref="IStackNavigator.StateChanged"/> is raised.
		/// </summary>
		/// <param name="navigator">The stack navigator.</param>
		/// <returns>An observable sequence of <see cref="EventPattern{SectionsNavigatorEventArgs}"/>.</returns>
		public static IObservable<EventPattern<StackNavigatorEventArgs>> ObserveStateChanged(this IStackNavigator navigator)
		{
			return Observable
				.FromEventPattern<StackNavigatorStateChangedEventHandler, StackNavigatorEventArgs>(
					handler => navigator.StateChanged += handler,
					handler => navigator.StateChanged -= handler
				);
		}

		/// <summary>
		/// Gets an observable sequence that produces values whenever <see cref="IStackNavigator.StateChanged"/> is raised, pushing only the <see cref="StackNavigatorEventArgs.CurrentState"/> value.
		/// </summary>
		/// <param name="navigator">The stack navigator.</param>
		/// <returns>An observable sequence of <see cref="StackNavigatorState"/>.</returns>
		public static IObservable<StackNavigatorState> ObserveCurrentState(this IStackNavigator navigator)
		{
			return navigator
				.ObserveStateChanged()
				.Select(pattern => pattern.EventArgs.CurrentState);
		}
	}
}
