using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.StackNavigation
{
	/// <summary>
	/// ­The event handler for the <see cref="IStackNavigator.StateChanged"/> event.
	/// </summary>
	/// <param name="sender">The sender that raises the event.</param>
	/// <param name="args">The event arguments.</param>
	public delegate void StackNavigatorStateChangedEventHandler(object sender, StackNavigatorEventArgs args);

	/// <summary>
	/// The event arguments for the <see cref="IStackNavigator.StateChanged"/> event.
	/// </summary>
	public class StackNavigatorEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StackNavigatorEventArgs"/> class.
		/// </summary>
		/// <param name="previousState">The previous state of the <see cref="IStackNavigator"/>.</param>
		/// <param name="currentState">The current state of the <see cref="IStackNavigator"/>.</param>
		public StackNavigatorEventArgs(StackNavigatorState previousState, StackNavigatorState currentState)
		{
			PreviousState = previousState;
			CurrentState = currentState;
		}

		/// <summary>
		/// Gets the previous state of the <see cref="IStackNavigator"/>.
		/// </summary>
		public StackNavigatorState PreviousState { get; }

		/// <summary>
		/// Gets the previous state of the <see cref="IStackNavigator"/>.
		/// </summary>
		public StackNavigatorState CurrentState { get; }
	}
}
