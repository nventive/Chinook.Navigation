using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.SectionsNavigation
{
	public delegate void SectionsNavigatorStateChangedEventHandler(object sender, SectionsNavigatorEventArgs args);

	public class SectionsNavigatorEventArgs : EventArgs
	{
		public SectionsNavigatorEventArgs(SectionsNavigatorState previousState, SectionsNavigatorState currentState)
		{
			PreviousState = previousState;
			CurrentState = currentState;
		}

		public SectionsNavigatorState PreviousState { get; }

		public SectionsNavigatorState CurrentState { get; }
	}
}
