# Getting started with Stack Navigation

1. Install the latest version of `Chinook.StackNavigation.Uno.WinUI` in your project.

1. Create a base class that implements `INavigableViewModel` for your ViewModels.

   You can use the MVVM framework of your choice.
   In this sample, we use `ViewModelBase` from [`Chinook.DynamicMvvm`](https://github.com/nventive/Chinook.DynamicMvvm).
   ```csharp
   using Chinook.DynamicMvvm;
   using Chinook.StackNavigation;
   using Windows.UI.Xaml.Controls;
   namespace ChinookSample
   {
     public class ViewModel : ViewModelBase, INavigableViewModel
     {
       public void SetView(object view)
       {
         // For Chinook.DynamicMvvm, we want to create an MVVM dispatcher using the CoreDispatcher of the Page.
         Dispatcher = new CoreDispatcherDispatcher((Page)view);
       }
     }
   }
   ```

1. Map your ViewModels to Pages.
   
   Create a method returning a dictionary associating the types.
   You can put that method in `App.xaml.cs`.
   ```csharp
   private static IReadOnlyDictionary<Type, Type> GetPageRegistrations() => new Dictionary<Type, Type>()
   {
     // Assuming that MainPageViewModel is a class that inherits from the ViewModel class of the previous step.
     { typeof(MainPageViewModel), typeof(MainPage) }
   };
   ```
1. From a blank WinUI application, create a `FrameStackNavigator` using the predefined `rootFrame` in the `OnLaunched` method of `App.xaml.cs`.

   Overral, `OnLaunched` should look like this after the changes.
   ```csharp
   protected override void OnLaunched(LaunchActivatedEventArgs e)
   {
      Frame rootFrame = Window.Current.Content as Frame;
      IStackNavigator navigator = null;

      // Do not repeat app initialization when the Window already has content,
      // just ensure that the window is active
      if (rootFrame == null)
      {
         // Create a Frame to act as the navigation context and navigate to the first page
         rootFrame = new Frame();
         rootFrame.NavigationFailed += OnNavigationFailed;

         navigator = new FrameStackNavigator(rootFrame, GetPageRegistrations());

         if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
         {
			//TODO: Load state from previously suspended application
         }

		// Place the frame in the current Window
         Window.Current.Content = rootFrame;
      }

      if (e.PrelaunchActivated == false)
      {
         if (rootFrame.Content == null)
         {
			// When the navigation stack isn't restored navigate to the first page,
			// configuring the new page by passing required information as a navigation
			// parameter
			navigator.Navigate(CancellationToken.None, () => new MainPageViewModel());
         }
         // Ensure the current window is active
         Window.Current.Activate();
      }
   }
   ```

1. Start your application!

   From there you probably want to publicly expose the your `IStackNavigator` instance to your ViewModels so that they can manipulate the navigation.
   You can do that using the pattern of your choice.
