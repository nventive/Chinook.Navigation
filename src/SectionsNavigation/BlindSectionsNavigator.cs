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
		/// <summary>
		/// Creates a new instance of <see cref="BlindSectionsNavigator"/>.
		/// </summary>
		/// <param name="defaultSectionNames">The default section names.</param>
		public BlindSectionsNavigator(params string[] defaultSectionNames)
			:base(GetDefaultSections(defaultSectionNames))
		{
		}

		/// <inheritdoc/>
		public override SectionsNavigatorTransitionInfo DefaultSetActiveSectionTransitionInfo { get; set; }
		
		/// <inheritdoc/>
		public override SectionsNavigatorTransitionInfo DefaultOpenModalTransitionInfo { get; set; }

		/// <inheritdoc/>
		public override SectionsNavigatorTransitionInfo DefaultCloseModalTransitionInfo { get; set; }

		private static IReadOnlyDictionary<string, ISectionStackNavigator> GetDefaultSections(string[] defaultSectionNames)
		{
			return defaultSectionNames.ToDictionary<string, string, ISectionStackNavigator>(
				keySelector: name => name,
				elementSelector: name => new SectionStackNavigator(new BlindStackNavigator(), name, isModal: false, priority: 0)
			);
		}

		/// <inheritdoc/>
		protected override ILogger GetLogger()
		{
			return this.Log();
		}

		/// <inheritdoc/>
		protected override Task<IStackNavigator> CreateStackNavigator(string name, int priority, SectionsNavigatorTransitionInfo transitionInfo)
		{
			return Task.FromResult<IStackNavigator>(new BlindStackNavigator());
		}

		/// <inheritdoc/>
		protected override Task InnerOpenModal(IModalStackNavigator navigator, bool isTopModal, SectionsNavigatorTransitionInfo transitionInfo)
		{
			// Don't do anything.
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		protected override Task InnerCloseModal(IModalStackNavigator modalToClose, SectionsNavigatorTransitionInfo transitionInfo)
		{
			// Don't do anything.
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		protected override Task InnerSetActiveSection(ISectionStackNavigator previousSection, ISectionStackNavigator nextsection, SectionsNavigatorTransitionInfo transitionInfo)
		{
			// Don't do anything.
			return Task.CompletedTask;
		}				
	}
}
