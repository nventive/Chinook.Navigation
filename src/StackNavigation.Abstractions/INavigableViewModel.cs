using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.StackNavigation
{
	/// <summary>
	/// This represents a ViewModel in the context of navigation.
	/// </summary>
	public interface INavigableViewModel : IDisposable
	{
		/// <summary>
		/// This method is called by <see cref="IStackNavigator"/> implementations after forward navigations.
		/// </summary>
		/// <remarks>
		/// The <paramref name="view"/> is typed as object to allow this interface to be available even in non-UI dependent librairies.
		/// </remarks>
		/// <param name="view">The view associated with the navigation. This is typically something inheriting from FrameworkElement.</param>
		void SetView(object view);
	}
}
