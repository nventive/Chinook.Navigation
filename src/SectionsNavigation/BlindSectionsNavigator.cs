using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Chinook.StackNavigation;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This implementation of <see cref="ISectionsNavigator"/> doesn't deal with any view.
	/// It's useful for unit testing.
	/// </summary>
	public class BlindSectionsNavigator : SectionsNavigatorBase
	{
		public BlindSectionsNavigator(params string[] defaultControllerNames)
			:base(GetDefaultController(defaultControllerNames), new Dictionary<Type, Type>())
		{
		}

		private static IReadOnlyDictionary<string, ISectionStackNavigator> GetDefaultController(string[] defaultControllerNames)
		{
			return defaultControllerNames.ToDictionary<string, string, ISectionStackNavigator>(
				keySelector: name => name,
				elementSelector: name => new SectionStackNavigator(new BlindStackNavigator(), name, isModal: false, priority: 0)
			);
		}

		protected override ILogger GetLogger()
		{
			return this.Log();
		}

		protected override Task<IStackNavigator> CreateStackNavigator(string name, int priority, IReadOnlyDictionary<Type, Type> registrations)
		{
			return Task.FromResult<IStackNavigator>(new BlindStackNavigator());
		}

		protected override Task InnerOpenModal(IModalStackNavigator navigator, bool isTopModal)
		{
			// Don't do anything.
			return Task.CompletedTask;
		}

		protected override Task InnerCloseModal(IModalStackNavigator modaltoClose)
		{
			// Don't do anything.
			return Task.CompletedTask;
		}

		protected override Task InnerSetActiveSection(ISectionStackNavigator previousController, ISectionStackNavigator nextController)
		{
			// Don't do anything.
			return Task.CompletedTask;
		}		
	}
}
