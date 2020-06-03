using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.StackNavigation
{
	/// <summary>
	/// Represents an entry in a <see cref="IStackNavigator"/>'s stack.
	/// </summary>
	public class NavigationStackEntry
	{
		/// <summary>
		/// Creates a new instance of <see cref="NavigationStackEntry"/>.
		/// </summary>
		/// <param name="request">The navigation request.</param>
		/// <param name="viewModel">The ViewModel.</param>
		/// <param name="view">The view.</param>
		public NavigationStackEntry(StackNavigatorRequest request, INavigableViewModel viewModel, object view)
		{
			Request = request;
			ViewModel = viewModel;
			View = view;
		}

		/// <summary>
		/// Gets the request from which this instance was created.
		/// </summary>
		public StackNavigatorRequest Request { get; }

		/// <summary>
		/// Gets the ViewModel instance created from <see cref="Request"/>.
		/// </summary>
		public INavigableViewModel ViewModel { get; }

		/// <summary>
		/// Gets the view instance created from <see cref="Request"/>.
		/// </summary>
		public object View { get; }
	}

}
