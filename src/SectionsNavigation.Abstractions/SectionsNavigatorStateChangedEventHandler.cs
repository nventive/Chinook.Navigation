using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// ­The event handler for the <see cref="ISectionsNavigator.StateChanged"/> event.
	/// </summary>
	/// <param name="sender">The sender that raises the event.</param>
	/// <param name="args">The event arguments.</param>
	public delegate void SectionsNavigatorStateChangedEventHandler(object sender, SectionsNavigatorEventArgs args);

	/// <summary>
	/// The event arguments for the <see cref="ISectionsNavigator.StateChanged"/> event.
	/// </summary>
	public class SectionsNavigatorEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SectionsNavigatorEventArgs"/> class.
		/// </summary>
		/// <param name="previousState">The previous state of the <see cref="ISectionsNavigator"/>.</param>
		/// <param name="currentState">The current state of the <see cref="ISectionsNavigator"/>.</param>
		public SectionsNavigatorEventArgs(SectionsNavigatorState previousState, SectionsNavigatorState currentState)
		{
			PreviousState = previousState;
			CurrentState = currentState;
		}

		/// <summary>
		/// Gets the previous state of the <see cref="ISectionsNavigator"/>.
		/// </summary>
		public SectionsNavigatorState PreviousState { get; }

		/// <summary>
		/// Gets the current state of the <see cref="ISectionsNavigator"/>.
		/// </summary>
		public SectionsNavigatorState CurrentState { get; }
	}
}
