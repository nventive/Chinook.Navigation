using Chinook.SectionsNavigation;
using Chinook.StackNavigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Contract
{
	public class ISectionStackNavigatorTests
	{
		[Fact]
		public async Task Interface_contract_changes_can_be_detected()
		{
			var ct = CancellationToken.None;
			var sectionsNavigator = new BlindSectionsNavigator("Section1", "Section2");

			// If the core contract changes, we get compilation errors here.
			ISectionStackNavigator navigator = await sectionsNavigator.SetActiveSection(ct, SectionsNavigatorRequest.GetSetActiveSectionRequest("Section1"));

			// Make sure ISectionStackNavigator inherits from IStackNavigator
			IStackNavigator stackNavigator = navigator;

			Assert.Equal("Section1", navigator.Name);
		}

		[Fact]
		public void Extensions_on_IStackNavigator_are_available_for_ISectionStackNavigator()
		{
			var assemblies = new Assembly[]
			{
				Assembly.GetAssembly(typeof(IStackNavigator)),
				Assembly.GetAssembly(typeof(ISectionStackNavigator)),
				Assembly.GetAssembly(typeof(StackNavigatorReactiveExtensions)),
				Assembly.GetAssembly(typeof(SectionsNavigatorReactiveExtensions)),

			};

			ReflectionHelper.MatchExtensions(assemblies, typeof(IStackNavigator), typeof(ISectionStackNavigator));
		}		
	}
}
