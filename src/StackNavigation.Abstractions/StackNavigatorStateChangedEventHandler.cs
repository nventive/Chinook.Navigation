using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.StackNavigation
{
	public delegate void StackNavigatorStateChangedEventHandler(object sender, StackNavigatorEventArgs args);

	public class StackNavigatorEventArgs : EventArgs
	{
		public StackNavigatorEventArgs(StackNavigatorState previousState, StackNavigatorState currentState)
		{
			PreviousState = previousState;
			CurrentState = currentState;
		}

		public StackNavigatorState PreviousState { get; }

		public StackNavigatorState CurrentState { get; }
	}
}
