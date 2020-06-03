using Chinook.StackNavigation;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This class provides reactive extension methods on the <see cref="ISectionStackNavigator"/> type.
	/// </summary>
	/// <remarks>
	/// This class forwards all method calls to <see cref="StackNavigatorReactiveExtensions"/>.
	/// The purpose of this class is to offer the same extensions on <see cref="IStackNavigator"/>, but in the <see cref="Chinook.SectionsNavigation"/> namespace.
	/// </remarks>
	public static class SectionStackNavigatorReactiveExtensions
    {
		/// <inheritdoc cref="StackNavigatorReactiveExtensions.ObserveStateChanged(IStackNavigator)"/>
		public static IObservable<EventPattern<StackNavigatorEventArgs>> ObserveStateChanged(this ISectionStackNavigator navigator)
		{
			return StackNavigatorReactiveExtensions.ObserveStateChanged(navigator);
		}

		/// <inheritdoc cref="StackNavigatorReactiveExtensions.ObserveCurrentState(IStackNavigator)"/>
		public static IObservable<StackNavigatorState> ObserveCurrentState(this ISectionStackNavigator navigator)
		{
			return StackNavigatorReactiveExtensions.ObserveCurrentState(navigator);
		}
	}
}