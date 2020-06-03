using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This class provides reactive extension methods on the <see cref="ISectionStackNavigator"/> type.
	/// </summary>
	public static class SectionsNavigatorReactiveExtensions
	{
		/// <summary>
		/// Gets an observable sequence that produces values whenever <see cref="ISectionsNavigator.StateChanged"/> is raised.
		/// </summary>
		/// <param name="navigator">The sections navigator.</param>
		/// <returns>An observable sequence of <see cref="EventPattern{SectionsNavigatorEventArgs}"/>.</returns>
		public static IObservable<EventPattern<SectionsNavigatorEventArgs>> ObserveStateChanged(this ISectionsNavigator navigator)
		{
			return Observable
				.FromEventPattern<SectionsNavigatorStateChangedEventHandler, SectionsNavigatorEventArgs>(
					handler => navigator.StateChanged += handler,
					handler => navigator.StateChanged -= handler
				);
		}

		/// <summary>
		/// Gets an observable sequence that produces values whenever <see cref="ISectionsNavigator.StateChanged"/> is raised, pushing only the <see cref="SectionsNavigatorEventArgs.CurrentState"/> value.
		/// </summary>
		/// <param name="navigator">The sections navigator.</param>
		/// <returns>An observable sequence of <see cref="SectionsNavigatorState"/>.</returns>
		public static IObservable<SectionsNavigatorState> ObserveCurrentState(this ISectionsNavigator navigator)
		{
			return navigator
				.ObserveStateChanged()
				.Select(pattern => pattern.EventArgs.CurrentState);
		}
	}
}
