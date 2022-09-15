using System.Runtime.CompilerServices;

// What is internal in StackNavigation.Abstractions will be accessible in Chinook.StackNavigation, etc.
[assembly: InternalsVisibleTo("Chinook.StackNavigation")]
[assembly: InternalsVisibleTo("Chinook.StackNavigation.Uno")]
[assembly: InternalsVisibleTo("Chinook.StackNavigation.Uno.WinUI")]
