using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.StackNavigation
{
	public enum StackNavigatorRequestType
	{
		NavigateForward,
		NavigateBack,
		RemoveEntry,
		Clear
	}

	public class StackNavigatorRequest
	{
		public static StackNavigatorRequest GetNavigateRequest(Type viewModelType, Func<INavigableViewModel> viewModelProvider, bool suppressTransition = false, bool clearBackStack = false) => new StackNavigatorRequest(
			StackNavigatorRequestType.NavigateForward,
			viewModelType: viewModelType,
			viewType: null,
			viewModelProvider: () => viewModelProvider(),
			suppressTransitions: suppressTransition,
			clearBackStack: clearBackStack,
			entryIndexesToRemove: null);

		public static StackNavigatorRequest GetNavigateRequest<TViewModel>(Func<TViewModel> viewModelProvider, bool suppressTransition = false, bool clearBackStack = false) where TViewModel : INavigableViewModel => new StackNavigatorRequest(
			StackNavigatorRequestType.NavigateForward,
			viewModelType: typeof(TViewModel),
			viewType: null,
			viewModelProvider: () => viewModelProvider(),
			suppressTransitions: suppressTransition,
			clearBackStack: clearBackStack,
			entryIndexesToRemove: null);

		public static StackNavigatorRequest GetRemoveEntryRequest(IEnumerable<int> entryIndexesToRemove) => new StackNavigatorRequest(
			StackNavigatorRequestType.RemoveEntry,
			viewModelType: null,
			viewType: null,
			viewModelProvider: null,
			suppressTransitions: null,
			clearBackStack: null,
			entryIndexesToRemove: entryIndexesToRemove);

		public static StackNavigatorRequest GetClearRequest() => new StackNavigatorRequest(
			StackNavigatorRequestType.Clear,
			viewModelType: null,
			viewType: null,
			viewModelProvider: null,
			suppressTransitions: null,
			clearBackStack: null,
			entryIndexesToRemove: null);

		public static StackNavigatorRequest GetNavigateBackRequest() => new StackNavigatorRequest(
			StackNavigatorRequestType.NavigateBack,
			viewModelType: null,
			viewType: null,
			viewModelProvider: null,
			suppressTransitions: null,
			clearBackStack: null,
			entryIndexesToRemove: null);

		public StackNavigatorRequest(
			StackNavigatorRequestType requestType,
			Type viewModelType,
			Type viewType,
			Func<INavigableViewModel> viewModelProvider,
			bool? suppressTransitions,
			bool? clearBackStack,
			IEnumerable<int> entryIndexesToRemove)
		{
			RequestType = requestType;
			ViewModelType = viewModelType;
			ViewType = viewType;
			ViewModelProvider = viewModelProvider;
			SuppressTransitions = suppressTransitions;
			ClearBackStack = clearBackStack;
			EntryIndexesToRemove = entryIndexesToRemove;
		}

		/// <summary>
		/// Gets the type of navigation request that this instance represents.
		/// This value indicates which other properties are meaningful for this instance.
		/// </summary>
		public StackNavigatorRequestType RequestType { get; }

		/// <summary>
		/// Gets the type of ViewModel that will be instanciated by this request when <see cref="RequestType"/> is <see cref="StackNavigatorRequestType.NavigateForward"/>.
		/// Null otherwise.
		/// </summary>
		public Type ViewModelType { get; }

		/// <summary>
		/// Gets the type of view that will be instanciated by this request when <see cref="RequestType"/> is <see cref="StackNavigatorRequestType.NavigateForward"/>.
		/// Null otherwise.
		/// </summary>
		public Type ViewType { get; internal set; }

		/// <summary>
		/// Gets the method to invoke to instanciate the ViewModel when <see cref="RequestType"/> is <see cref="StackNavigatorRequestType.NavigateForward"/>.
		/// Null otherwise.
		/// </summary>
		internal Func<INavigableViewModel> ViewModelProvider { get; }

		/// <summary>
		/// Gets whether to suppress the view transition when <see cref="RequestType"/> is <see cref="StackNavigatorRequestType.NavigateForward"/>.
		/// Null otherwise.
		/// </summary>
		public bool? SuppressTransitions { get; } = false;

		// TODO: Replace by a list of entries to remove?
		/// <summary>
		/// Gets whether to clear the navigation stack when <see cref="RequestType"/> is <see cref="StackNavigatorRequestType.NavigateForward"/>.
		/// Null otherwise.
		/// </summary>
		public bool? ClearBackStack { get; } = false;

		/// <summary>
		/// Gets a list of <see cref="NavigationStackEntry"/> index to remove when <see cref="RequestType"/> is <see cref="StackNavigatorRequestType.RemoveEntry"/>.
		/// Null otherwise.
		/// </summary>
		public IEnumerable<int> EntryIndexesToRemove { get; }

		public override string ToString()
		{
			return $"{RequestType}, {nameof(ViewModelType)}: {ViewModelType?.Name}";
		}
	}

}
