# Stack Navigation

The `StackNavigation` namespace offers classes to allow **ViewModel based navigation** using a single navigation stack. (i.e. You can navigate forward or back.)

The central point of this namespace is the [IStackNavigator](IStackNavigator.cs) interface.
It is reponsible for both 
- the **state** of the navigation stack (what ViewModel is active and what are the previous ones),
- the **operations** that can change that state (such as _Navigate_ or _NavigateBack_).

## Getting started

### 1. Get an instance of `IStackNavigator`
There are 2 classes available that implement `IStackNavigator`:
- `BlindStackNavigator`
  - Use this implementation for projects that don't use views (tests projects).
- `FrameStackNavigator`
  - Use this implementation for your launchable apps (UWP, Android, iOS, etc.).
  - It requires a `Frame` that you should put at the root of your application.
  - It accepts a mapping of your ViewModel types to your Page types. This is useful if you register all your vm and pages in one place.

### 2. Navigate to ViewModels
This navigation is ViewModel based. That means that ViewModels are mainly what defines the navigation.
You can use the various overloads of `Navigate` or `NavigateAndClear` to add ViewModels in the stack.

```csharp
// The Navigate operation returns the newly created ViewModel.
var vm = await stackNavigator.Navigate(ct, () => new EditPostPageViewModel);
```

You can use `NavigateBack` to return on the previous page.
```csharp
// The NavigateBack operation returns the previous ViewModel.
var vm = await stackNavigator.NavigateBack(ct);
```

### 3. Use the state
You can access the stack navigator's state using `IStackNavigator.State`.
It gives you access to the navigation stack as well as the last request.
You can observe the state with the `IStackNavigator.StateChanged` event.

## Features

### No double navigation
If you invoke 2 operations simultaneously (double tap, press 2 buttons with 2 fingers, etc.), only the first will actually run.
This is because the the request state (either processing or processed) is part of the `IStackNavigator.State`.
If a request is made while another is processing, the second request is cancelled.

### Clear back stack on forward navigation
If you navigate forward, you can set `StackNavigatorRequest.ClearBackStack` to true to remove all previous pages from the navigate stack.
This will prevent going back once you're on your new page.

### Suppress transtions
When navigating forward using `FrameStackNavigator`, transitions can be suppressed by setting `StackNavigatorRequest.SuppressTransitions` to true.

### Remove previous entries
You can use `IStackNavigator.RemoveEntries` to remove previous entries from the navigation stack.
You can use this to avoid going back to specific pages while still keeping other items of your back stack.

### iOS back gesture
The back swipe gesture is supported on iOS devices when using `FrameStackNavigator`.

### Navigation testing
Because `BlindStackNavigator` doesn't require any view, you can use it in tests projects.
This allows for unit testing or integration testing of navigation scenarios.
e.g. You could assert that the state of the navigator contains specific view models after some specific actions.