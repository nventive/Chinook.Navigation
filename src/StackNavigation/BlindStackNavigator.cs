using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Chinook.StackNavigation
{
	/// <summary>
	/// This implementation of <see cref="IStackNavigator"/> doesn't deal with any view.
	/// It's useful for unit testing.
	/// </summary>
	public class BlindStackNavigator : StackNavigatorBase
	{
		public BlindStackNavigator() : base(new Dictionary<Type, Type>())
		{
		}

		protected override ILogger GetLogger() => this.Log();

		protected override Task InnerClear()
		{
			// Don't do anything.
			return Task.CompletedTask;
		}

		protected override Task InnerRemoveEntries(IEnumerable<int> orderedIndexes)
		{
			// Don't do anything.
			return Task.CompletedTask;
		}

		protected override Task<object> InnerNavigateAndGetView(INavigableViewModel viewModel)
		{
			// Don't do anything.
			return Task.FromResult<object>(null);
		}

		protected override Task InnerNavigateBack(NavigationStackEntry entryToRemove, NavigationStackEntry activeEntry)
		{
			// Don't do anything.
			return Task.CompletedTask;
		}
	}
}
