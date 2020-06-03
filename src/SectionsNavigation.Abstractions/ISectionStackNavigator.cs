using System;
using System.Collections.Generic;
using System.Text;
using Chinook.StackNavigation;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This represents a navigation stack in the context of a sections navigator.
	/// It's a section of an <see cref="ISectionsNavigator"/> (out of multiple).
	/// </summary>
	public interface ISectionStackNavigator : IStackNavigator
	{
		/// <summary>
		/// Gets the name of the section.
		/// This name can be used to retrieve this instance from <see cref="SectionsNavigatorState.Sections"/> mapping.
		/// </summary>
		string Name { get; }
	}
}
