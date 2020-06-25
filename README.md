# Chinook SectionsNavigation and StackNavigation
<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-4-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

Chinook.Navigation

This library offers a simple method of navigating through an app. [StackNavigation](src/StackNavigation.Abstractions/StackNavigation.md) and [SectionsNavigation](src/SectionsNavigation.Abstractions/SectionsNavigation.md) are used together to provide a simple interface to navigate forward, backward and between discrete sections.

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

## Getting Started

### Uno project

To get started in a project which uses the [Uno platform](https://platform.uno/):

* Add to your project the `Chinook.SectionsNavigation.Uno` nuget package and its dependencies.
* Create a mapping of View Model - View. For more information on how ViewModels are used in the navigation, please look at the documentation for [StackNavigation](src/StackNavigation.Abstractions/StackNavigation.md).
* With these registrations, create a singleton instance of `ISectionsNavigator` which will be used throughout the app:

```
var shell = Shell.Instance;
// mapping of View Model - View
var registrations = GetRegistrations();

// This is the instance to be used throughout the app
 var navigator = new FrameSectionsNavigator(shell.NavigationFrame, registrations);
```

### Unit tests

To support navigation in your unit tests:

* Add to your project the `Chinook.SectionsNavigation` nuget package and its dependencies.
* For each test which involves navigation, create an instance of BlindSectionsNavigator:

```
var navController = new BlindSectionsNavigator("section 1", "section 2");
```

For additional information on how to use these navigators, please take a look at these documents:

[StackNavigation](src/StackNavigation.Abstractions/StackNavigation.md)

[SectionsNavigation](src/SectionsNavigation.Abstractions/SectionsNavigation.md)

## Features

### No double navigation
If you invoke 2 operations simultaneously (double tap, press 2 buttons with 2 fingers, etc.), only the first will actually run.
This is because the request state (Processing, Processed or FailedToProcess) is part of the `ISectionsNavigator.State`.
If a request is made while another is processing, the second request is cancelled.

### Background navigation
You can navigate in sections that are not active. This is useful if you want to _prepare_ a section before entering it.

### Stack navigator adapter
You can use `SectionsNavigatorToStackNavigatorAdapter` to adapt a `ISectionsNavigator` into a `IStackNavigator`. This is useful if your app mostly use the single stack pattern.

### Customize animations
When using `FrameSectionsNavigator`, you can customize or remove the animations of the MultiFrame using `MultiFrame.Animations`.

### Navigation testing
Because `BlindSectionsNavigator` doesn't require any view, you can use it in tests projects.
This allows for unit testing or integration testing of navigation scenarios.
e.g. You could assert that the state of the navigator contains specific view models after some specific actions.

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
    <td align="center"><a href="https://github.com/jcantin-nventive"><img src="https://avatars1.githubusercontent.com/u/43351943?v=4" width="100px;" alt=""/><br /><sub><b>jcantin-nventive</b></sub></a><br /><a href="https://github.com/nventive/Chinook.Navigation/commits?author=jcantin-nventive" title="Documentation">📖</a></td>
  </tr>
</table>

<!-- markdownlint-enable -->
<!-- prettier-ignore-end -->
<!-- ALL-CONTRIBUTORS-LIST:END -->
<!-- ALL-CONTRIBUTORS-LIST:END -->
