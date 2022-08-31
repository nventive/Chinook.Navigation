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
		/// <summary>
		/// Initializes a new instance of the <see cref="BlindStackNavigator"/> class.
		/// </summary>
		public BlindStackNavigator() : base(new Dictionary<Type, Type>())
		{
		}

		/// <inheritdoc/>
		protected override ILogger GetLogger() => this.Log();

		/// <inheritdoc/>
		protected override Task InnerClear()
		{
			// Don't do anything.
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		protected override Task InnerRemoveEntries(IEnumerable<int> orderedIndexes)
		{
			// Don't do anything.
			return Task.CompletedTask;
		}

		/// <inheritdoc/>		
		protected override Task<object> InnerNavigateAndGetView(INavigableViewModel viewModel)
		{
			// Don't do anything.
			return Task.FromResult<object>(null);
		}

		/// <inheritdoc/>
		protected override Task InnerNavigateBack(NavigationStackEntry entryToRemove, NavigationStackEntry activeEntry)
		{
			// Don't do anything.
			return Task.CompletedTask;
		}
	}
}
