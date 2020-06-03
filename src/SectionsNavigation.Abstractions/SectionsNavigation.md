# Sections Navigation

The `SectionsNavigation` namespace offers classes to allow **ViewModel based navigation** using multiple stacks.
Those stacks can either be a _section_ or a _modal_.

The central point of this namespace is the [ISectionsNavigator](ISectionsNavigator.cs) interface.
It is reponsible for both
- the **state** of the navigation stacks (which section is active, what is the modal stack),
- the **operations** that can change that state (such as _SetActiveSection_ or _OpenModal_).

Here is how navigation around sections and modals are related to stack navigation:
- 1 Dimension: **Stack Navigator**
  - It's linear. You can go forward or back.
- 2 Dimensions: **Sections**
  - Each section is a stack navigator.
  - You can have multiple sections next to each other.
  - You can change the active section freely (without limitations).
- 3 Dimensions: **Modals**
  - Each modal is a stack navigator.
  - You can have multiple modals on top of each other.
  - You can't change the modal freely; you can only open a new modal and close existing modals.
  - The active modal is always the one with the highest priority (set when opening).

Axis | Navigator            | Operations
:---:|----------------------|-----------
X    | `IStackNavigator`    | Navigate, NavigateBack
Y    | `ISectionsNavigator` | SetActiveSection
Z    | `ISectionsNavigator` | OpenModal, CloseModal

## Getting started

### 1. Get an instance of `ISectionsNavigator`
There are 2 classes available that implement `ISectionsNavigator`:
- `BlindSectionsNavigator`
  - Use this implementation for projects that don't use views (tests projects).
- `FrameSectionsNavigator`
  - Use this implementation for your launchable apps (UWP, Android, iOS, etc.).
  - It requires a `MultiFrame` that you should put at the root of your application.
  - It accepts a mapping of your ViewModel types to your Page types. This is useful if you register all your vm and pages in one place.

```xml
<!-- Here we create the MultiFrame with 3 sections named "Home", "Posts" and "Settings". -->
<nav:MultiFrame x:Name="RootNavigationMultiFrame"
                CommaSeparatedSectionsFrameNames="Home,Posts,Settings" />
```

### 2. Change active sections
The `ISectionsNavigator` deals with multiple `IStackNavigator` that either implement `ISectionStackNavigator` or `IModalStackNavigator`.
You can set the active section using `ISectionsNavigator.SetActiveSection`.

```csharp
// The SetActiveSection operation returns the active section.
var section = await sectionsNavigator.SetActiveSection(ct, "Home");

// You can then use it to navigate.
await section.Navigate(ct, () => new HomePageViewModel());
```

### 3. Open and close modals
You can set the active modal using `ISectionsNavigator.OpenModal`.

```csharp
// When you don't specify a modal priority, one is chosen automatically.
await sectionsNavigator.OpenModal(ct, () => new DiagnosticsPageViewModel());

// The same rule applies when closing modals.
await sectionsNavigator.CloseModal(ct);
```

Modals take precedence over sections.
That means that calling `SetActiveSection` when a modal is open will not close that modal.
You'll see the change of section once you close the modal.

### 4. Use the state
You can access the sections navigator's state using `ISectionsNavigator.State`.
It gives you access to all section stacks and modal stacks as well as the last request.
You can observe the state with the `ISectionsNavigator.StateChanged` event.

## Features

### No double navigation
If you invoke 2 operations simultaneously (double tap, press 2 buttons with 2 fingers, etc.), only the first will actually run.
This is because the the request state (either processing or processed) is part of the `ISectionsNavigator.State`.
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