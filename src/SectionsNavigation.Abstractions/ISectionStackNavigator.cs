using System;
using System.Collections.Generic;
using System.Text;
using Chinook.StackNavigation;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This represents a navigation stack in the context of a sections navigator.
	/// It's a section of an <see cref="ISectionsNavigator"/>.
	/// </summary>
	public interface ISectionStackNavigator : IStackNavigator
	{
		/// <summary>
		/// Gets the name of the section.
		/// </summary>
		/// <remarks>
		/// This name can be used to retrieve this instance from <see cref="SectionsNavigatorState.Sections"/> mapping.
		/// </remarks>
		string Name { get; }
	}
}
