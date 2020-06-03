using Chinook.SectionsNavigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Chinook.Navigation.Tests.Contract
{
	public class IModalStackNavigatorTests
	{
		[Fact]
		public async Task Interface_contract_changes_can_be_detected()
		{
			var ct = CancellationToken.None;
			var sectionsNavigator = new BlindSectionsNavigator("Section1", "Section2");

			// If the core contract changes, we get compilation errors here.
			IModalStackNavigator navigator = await sectionsNavigator.OpenModal(ct, SectionsNavigatorRequest.GetOpenModalRequest(StackNavigation.StackNavigatorRequest.GetNavigateRequest(() => new TestVM()), "modalName", 0));

			// Make sure IModalStackNavigator inherits from IStackNavigator
			StackNavigation.IStackNavigator stackNavigator = navigator;

			Assert.Equal("modalName", navigator.Name);
			Assert.Equal(0, navigator.Priority);
		}

		[Fact]
		public void Extensions_on_IStackNavigator_are_available_for_IModalStackNavigator()
		{
			var assemblies = new Assembly[]
			{
				Assembly.GetAssembly(typeof(StackNavigation.IStackNavigator)),
				Assembly.GetAssembly(typeof(IModalStackNavigator)),
				Assembly.GetAssembly(typeof(StackNavigation.StackNavigatorReactiveExtensions)),
				Assembly.GetAssembly(typeof(SectionsNavigatorReactiveExtensions)),
			};

			ReflectionHelper.MatchExtensions(assemblies, typeof(StackNavigation.IStackNavigator), typeof(IModalStackNavigator));
		}

		private class TestVM : StackNavigation.INavigableViewModel
		{
			public void Dispose()
			{
			}

			public void SetView(object view)
			{
			}
		}
	}
}
