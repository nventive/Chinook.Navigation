using Chinook.SectionsNavigation;
using Chinook.StackNavigation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
	public class SectionsNavigatorStateExtensionsTests
	{
		public SectionsNavigatorStateExtensionsTests(ITestOutputHelper outputHelper)
		{				
			var loggerFactory = LoggerFactory.Create(builder => builder
				.AddSerilog(new LoggerConfiguration()
					.WriteTo.TestOutput(outputHelper)
					.MinimumLevel.Verbose()
					.CreateLogger()
				)
			);

			SectionsNavigationConfiguration.LoggerFactory = loggerFactory;
			StackNavigationConfiguration.LoggerFactory = loggerFactory;
		}

		[Theory]
		[MemberData(nameof(NavigationOperations))]
		public async Task Prevision_from_GetNextViewModelType_matches_result_from_GetLastViewModelType(Func<CancellationToken, ISectionsNavigator, Task> navigationOperations, string testName)
		{
			// testName exists for debugging purposes.
			testName.Should().NotBeNullOrEmpty();
			var states = new List<SectionsNavigatorState>();
			var navigator = new BlindSectionsNavigator("Home", "Settings");
			await navigator.SetActiveSection(CancellationToken.None, "Home");

			var processingState = default(SectionsNavigatorState);
			var nextVMType = default(Type);
			
			navigator.StateChanged += Navigator_StateChanged;

			void Navigator_StateChanged(object sender, SectionsNavigatorEventArgs args)
			{
				if (args.CurrentState.LastRequestState == NavigatorRequestState.Processing)
				{
					processingState = args.CurrentState;
					nextVMType = processingState.GetNextViewModelType();					
				}
				
				if (args.CurrentState.LastRequestState == NavigatorRequestState.Processed)
				{
					var processedState = args.CurrentState;
					var currentVMType = processedState.GetLastViewModelType();
					
					nextVMType.Should().Be(currentVMType);

					processingState = null;
					nextVMType = null;
				}
			}

			await navigationOperations(CancellationToken.None, navigator);
		}

		public static IEnumerable<object[]> NavigationOperations { get; } = new object[][]
		{
			new object[]{ (Func<CancellationToken, ISectionsNavigator, Task>)NavigateForwardAndBack, nameof(NavigateForwardAndBack) },
			new object[]{ (Func<CancellationToken, ISectionsNavigator, Task>)NavigateAndClear, nameof(NavigateAndClear) },
			new object[]{ (Func<CancellationToken, ISectionsNavigator, Task>)OpenAndCloseModal, nameof(OpenAndCloseModal) },
			new object[]{ (Func<CancellationToken, ISectionsNavigator, Task>)OpenAndCloseModalWithNavigation, nameof(OpenAndCloseModalWithNavigation) },
			new object[]{ (Func<CancellationToken, ISectionsNavigator, Task>)OpenAndCloseModals, nameof(OpenAndCloseModals) },
			new object[]{ (Func<CancellationToken, ISectionsNavigator, Task>)OpenAndCloseModalsWithPriority, nameof(OpenAndCloseModalsWithPriority) },
			new object[]{ (Func<CancellationToken, ISectionsNavigator, Task>)NavigateInInactiveSection, nameof(NavigateInInactiveSection) },
			new object[]{ (Func<CancellationToken, ISectionsNavigator, Task>)ChangeSectionBehindModal, nameof(ChangeSectionBehindModal) },
			new object[]{ (Func<CancellationToken, ISectionsNavigator, Task>)Clear, nameof(Clear) },
			new object[]{ (Func<CancellationToken, ISectionsNavigator, Task>)NavigateBackTo, nameof(NavigateBackTo) },
			new object[]{ (Func<CancellationToken, ISectionsNavigator, Task>)NavigateBackToMiddle, nameof(NavigateBackToMiddle) },
		};

		private static async Task NavigateForwardAndBack(CancellationToken ct, ISectionsNavigator navigator)
		{
			await navigator.Navigate(ct, () => new HomePageVM());
			await navigator.Navigate(ct, () => new DetailsPageVM());
			await navigator.NavigateBack(ct);
		}

		private static async Task NavigateAndClear(CancellationToken ct, ISectionsNavigator navigator)
		{
			await navigator.NavigateAndClear(ct, () => new HomePageVM());
			await navigator.NavigateAndClear(ct, () => new DetailsPageVM());
		}

		private static async Task OpenAndCloseModal(CancellationToken ct, ISectionsNavigator navigator)
		{
			await navigator.OpenModal(ct, () => new HomePageVM());
			await navigator.CloseModal(ct);
		}

		private static async Task OpenAndCloseModalWithNavigation(CancellationToken ct, ISectionsNavigator navigator)
		{
			await navigator.OpenModal(ct, () => new HomePageVM());
			await navigator.Navigate(ct, () => new DetailsPageVM());
			await navigator.CloseModal(ct);
		}

		private static async Task OpenAndCloseModals(CancellationToken ct, ISectionsNavigator navigator)
		{
			await navigator.OpenModal(ct, () => new HomePageVM());
			await navigator.OpenModal(ct, () => new DetailsPageVM());
			await navigator.CloseModal(ct);
			await navigator.CloseModal(ct);
		}

		private static async Task OpenAndCloseModalsWithPriority(CancellationToken ct, ISectionsNavigator navigator)
		{
			await navigator.OpenModal(ct, () => new HomePageVM(), priority: 2);
			await navigator.OpenModal(ct, () => new DetailsPageVM(), priority: 1);
			await navigator.CloseModal(ct);
			await navigator.CloseModal(ct);
		}

		private static async Task NavigateInInactiveSection(CancellationToken ct, ISectionsNavigator navigator)
		{
			await navigator.Navigate(ct, () => new HomePageVM());
			var settingsNavigator = navigator.State.Sections["Settings"];
			await settingsNavigator.Navigate(ct, () => new SettingsPageVM());
			await settingsNavigator.Navigate(ct, () => new LanguagePageVM());
			await navigator.SetActiveSection(ct, "Settings");
		}

		private static async Task ChangeSectionBehindModal(CancellationToken ct, ISectionsNavigator navigator)
		{
			await navigator.Navigate(ct, () => new HomePageVM());
			await navigator.OpenModal(ct, () => new DetailsPageVM());
			await navigator.SetActiveSection(ct, "Settings", () => new SettingsPageVM());
			await navigator.CloseModal(ct);
		}

		private static async Task Clear(CancellationToken ct, ISectionsNavigator navigator)
		{
			await navigator.Navigate(ct, () => new HomePageVM());
			await navigator.GetActiveStackNavigator().Clear(ct);
		}
		
		private static async Task NavigateBackTo(CancellationToken ct, ISectionsNavigator navigator)
		{
			await navigator.Navigate(ct, () => new HomePageVM());
			await navigator.Navigate(ct, () => new DetailsPageVM());
			await navigator.Navigate(ct, () => new SettingsPageVM());
			await navigator.Navigate(ct, () => new LanguagePageVM());
			var result = await navigator.GetActiveStackNavigator().TryNavigateBackTo<HomePageVM>(ct);
			result.Should().BeTrue();
		}

		private static async Task NavigateBackToMiddle(CancellationToken ct, ISectionsNavigator navigator)
		{
			await navigator.Navigate(ct, () => new HomePageVM());
			await navigator.Navigate(ct, () => new DetailsPageVM());
			await navigator.Navigate(ct, () => new SettingsPageVM());
			await navigator.Navigate(ct, () => new LanguagePageVM());
			var result = await navigator.GetActiveStackNavigator().TryNavigateBackTo<DetailsPageVM>(ct);
			result.Should().BeTrue();
		}

		private class TestVMBase : INavigableViewModel
		{
			public void Dispose()
			{
			}

			public void SetView(object view)
			{
			}
		}

		private class HomePageVM : TestVMBase { }
		private class DetailsPageVM : TestVMBase { }
		private class SettingsPageVM : TestVMBase { }
		private class LanguagePageVM : TestVMBase { }

	}
}
