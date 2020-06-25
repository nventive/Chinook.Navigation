# Chinook StackNavigation and SectionsNavigation
<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-2-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

This library provides unified cross-platform tools to perform ViewModel-based navigation using the [Frame](https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.frame) navigation model.
```csharp
// Navigate to the PersonDetailsPage.
await navigator.Navigate(ct, () => new PersonDetailsPageViewModel());
// Navigate back.
await navigator.NavigateBack(ct);
```

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

## Getting Started

### 1. Choose your Navigator

There are 2 types of navigators available:
- `IStackNavigator` - Use this if your app would use a single `Frame`.
- `ISectionsNavigator` - Use this if you want to use multiple frames (like sections tabs) or modals. Note that `IStackNavigator` is used as a building block by `ISectionsNavigator`. 

### 2. Create your Navigator

See how to get you instance for [StackNavigation](src/StackNavigation.Abstractions/StackNavigation.md#getting-started) or [SectionsNavigation](src/SectionsNavigation.Abstractions/SectionsNavigation.md#getting-started). Note that your code should always use the interface in order to be easily reused for integration tests.

### 3. Use your Navigator

There's a lot of things you can do. Here are some examples.

#### Navigate forward and back
```csharp
// Navigate to the PersonDetailsPage.
await navigator.Navigate(ct, () => new PersonDetailsPageViewModel());
// Navigate back.
await navigator.NavigateBack(ct);
```

#### Navigate forward, clearing the backstack
```csharp
// Navigate to the HomePage, clearing all other previous pages from the backstack.
await navigator.NavigateAndClear(ct, () => new HomePageViewModel());
```

#### Remove previous pages
```csharp
// Navigate to Step 1
await navigator.Navigate(ct, () => new Step1PageViewModel());
// Navigate to Step 2
await navigator.Navigate(ct, () => new Step2PageViewModel());
// Navigate to Step 2.1
await navigator.Navigate(ct, () => new Step21PageViewModel());
// Navigate to Step 3
await navigator.Navigate(ct, () => new Step3PageViewModel());

// Remove the previous page (Step 2.1) from the backstack. 
await navigator.RemovePrevious(ct);
// Navigate back to Step 2
await navigator.NavigateBack(ct);
```

The following examples only apply to `ISectionsNavigator`.

#### Change between sections
```csharp
// Go to Home section.
await sectionsNavigator.SetActiveSection(ct, "Home");
// Go to Messages section.
await sectionsNavigator.SetActiveSection(ct, "Messages");
// Go to Settings section.
await sectionsNavigator.SetActiveSection(ct, "Settings");
```

#### Return to root of section
```csharp
// Go to Home section.
await sectionsNavigator.SetActiveSection(ct, "Home", () => new HomePageViewModel());
// Navigate forward to some details page in the Home section.
await sectionsNavigator.Navigate(ct, () => new PersonDetailsPageViewModel());
// Go to Messages section.
await sectionsNavigator.SetActiveSection(ct, "Messages");

// Return to Home section on the Home page, not the PersonDetails page.
await sectionsNavigator.SetActiveSection(ct, "Home", () => new HomePageViewModel(), returnToRoot: true);
```

#### Open and close modals
```csharp
// Open LoginPage in a modal.
await sectionsNavigator.OpenModal(ct, () => new LoginPageViewModel());
// Close the modal.
await sectionsNavigator.CloseModal(ct);
```

#### Open modals behind other modals
```csharp
// Open LoginPage in a modal with a priority of 2.
await sectionsNavigator.OpenModal(ct, () => new LoginPageViewModel(), priority = 2);

// Open the SurveyPage in a modal behind the LoginPage page modal, using a lower priority of 1.
// Because the SurveyPage opens with a lower priority, you don't actually see this change happen.
await sectionsNavigator.OpenModal(ct, () => new SurveyPageViewModel(), priority = 1);

// Close the top-most modal (LoginPage) to reveal the SurveyPage modal behind it.
await sectionsNavigator.CloseModal(ct);
```

#### Change sections behind modals
```csharp
// Open LoginPage in a modal.
await sectionsNavigator.OpenModal(ct, () => new LoginPageViewModel());

// Change the section to Messages.
// Modals are displayed on top of sections, so you don't actually see this change happen.
await sectionsNavigator.SetActiveSection(ct, "Messages", () => new MessagesPageViewModel());

// Close the modal to reveal the Messages section.
await sectionsNavigator.CloseModal(ct);
```

#### Navigate in an inactive section
```csharp
// Go to Home section.
await sectionsNavigator.SetActiveSection(ct, "Home", () => new HomePageViewModel());
// Get the settings section navigator.
var settingsSection = sectionsNavigator.State.Sections["Settings"];

// Navigate forward to the SettingsPage, then the LicencePage in the Settings section.
// The Settings sections is not currently active, so you don't actually see this change happen.
await settingsSection.Navigate(ct, () => new SettingsPageViewModel());
await settingsSection.Navigate(ct, () => new LicencePageViewModel());

// Go to Settings section to see the Licence page.
await sectionsNavigator.SetActiveSection(ct, "Settings");
// Navigate back to SettingsPage.
await sectionsNavigator.NavigateBack(ct);
```

#### Navigate back or close modal
```csharp
// Check whether the navigator can navigate back or close a modal.
// This us useful when dealing with an hardware back button.
if (sectionsNavigator.CanNavigateBackOrCloseModal())
{
  // Navigates back within the modal if the modal has multiple pages in its stack
  // Or closes the modal if there's a modal that has an empty backstack
  // Or navigates back in the active section.
  await sectionsNavigator.NavigateBackOrCloseModal(ct);
}
```

## Features

### Ready for Dependency Injection
Navigators are made from simple interfaces. You can easily leverage containers such as Microsoft's [Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host).

### Ready for Integration Testing
Because this is ViewModel-based navigation and the navigator interfaces don't reference any UI type, you can use the navigators in **Test Projects** or **Console Applications** without changing your navigation logic. Just install the `Chinook.SectionsNavigation` or `Chinook.StackNavigation` packages and use the `BlindSectionsNavigator` or `BlindStackNavigator` implementations.

### No Double Navigation
If you invoke 2 operations simultaneously (double tap, press 2 buttons with 2 fingers, etc.), only the first will actually run.
This is because the request state (Processing, Processed or FailedToProcess) is part of the `ISectionsNavigator.State`.
If a request is made while another is processing, the second request is cancelled.

### Background Navigation
You can navigate in sections that are not active. This is useful if you want to _prepare_ a section before entering it.

### Customize Animations
When using `FrameSectionsNavigator`, you can customize or remove the animations of the `MultiFrame` using `MultiFrame.Animations`.

### Modals
`ISectionsNavigator` allows you to handle multiple stacks of navigation in your app, including modals. This means you can easily handle navigation with your modals, since the modals are just in another navigation stack. For instance, the user can navigate back and forth in the modals, and your app can navigate the pages behind the modals, without breaking the flow.

## Changelog

Please consult the [CHANGELOG](CHANGELOG.md) for more information about version
history.

## License

This project is licensed under the Apache 2.0 license - see the
[LICENSE](LICENSE) file for details.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on the process for
contributing to this project.

Be mindful of our [Code of Conduct](CODE_OF_CONDUCT.md).

## Contributors

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tr>
    <td align="center"><a href="https://github.com/MatFillion"><img src="https://avatars0.githubusercontent.com/u/7029537?v=4" width="100px;" alt=""/><br /><sub><b>Mathieu Fillion</b></sub></a><br /><a href="https://github.com/nventive/Chinook.Navigation/commits?author=MatFillion" title="Code">💻</a> <a href="#platform-MatFillion" title="Packaging/porting to new platform">📦</a></td>
    <td align="center"><a href="https://github.com/jeremiethibeault"><img src="https://avatars3.githubusercontent.com/u/5444226?v=4" width="100px;" alt=""/><br /><sub><b>Jérémie Thibeault</b></sub></a><br /><a href="https://github.com/nventive/Chinook.Navigation/commits?author=jeremiethibeault" title="Code">💻</a> <a href="https://github.com/nventive/Chinook.Navigation/commits?author=jeremiethibeault" title="Tests">⚠️</a></td>
    <td align="center"><a href="https://github.com/jeanplevesque"><img src="https://avatars3.githubusercontent.com/u/39710855?v=4" width="100px;" alt=""/><br /><sub><b>Jean-Philippe Lévesque</b></sub></a><br /><a href="https://github.com/nventive/Chinook.Navigation/commits?author=jeanplevesque" title="Code">💻</a> <a href="https://github.com/nventive/Chinook.Navigation/commits?author=jeanplevesque" title="Tests">⚠️</a></td>
  </tr>
</table>

<!-- markdownlint-enable -->
<!-- prettier-ignore-end -->
<!-- ALL-CONTRIBUTORS-LIST:END -->
<!-- ALL-CONTRIBUTORS-LIST:END -->
