using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.StackNavigation
{
	/// <summary>
	/// Represents the type of a <see cref="StackNavigatorRequest"/>.
	/// </summary>
	public enum StackNavigatorRequestType
	{
		/// <summary>
		/// Represents a forward navigation.
		/// </summary>
		NavigateForward,
		
		/// <summary>
		/// Represents a back navigation.
		/// </summary>
		NavigateBack,

		/// <summary>
		/// Represents a removal of existing navigation entries.
		/// </summary>
		RemoveEntry,

		/// <summary>
		/// Represents a removal of all existing navigations entries.
		/// </summary>
		Clear
	}

	/// <summary>
	/// Represents a navigation request for a <see cref="IStackNavigator"/>.
	/// </summary>
	public class StackNavigatorRequest
	{
		/// <summary>
		/// Gets a new <see cref="StackNavigatorRequest"/> for forward navigation.
		/// </summary>
		/// <inheritdoc cref="StackNavigatorRequest.StackNavigatorRequest(StackNavigatorRequestType, Type, Type, Func{INavigableViewModel}, bool?, bool?, IEnumerable{int})"/>
		public static StackNavigatorRequest GetNavigateRequest(Type viewModelType, Func<INavigableViewModel> viewModelProvider, bool suppressTransitions = false, bool clearBackStack = false) => new StackNavigatorRequest(
			StackNavigatorRequestType.NavigateForward,
			viewModelType: viewModelType,
			viewType: null,
			viewModelProvider: () => viewModelProvider(),
			suppressTransitions: suppressTransitions,
			clearBackStack: clearBackStack,
			entryIndexesToRemove: null);

		/// <summary>
		/// Gets a new <see cref="StackNavigatorRequest"/> for forward navigation.
		/// </summary>
		/// <typeparam name="TViewModel">The type of ViewModel.</typeparam>
		/// <inheritdoc cref="StackNavigatorRequest.StackNavigatorRequest(StackNavigatorRequestType, Type, Type, Func{INavigableViewModel}, bool?, bool?, IEnumerable{int})"/>
		public static StackNavigatorRequest GetNavigateRequest<TViewModel>(Func<TViewModel> viewModelProvider, bool suppressTransitions = false, bool clearBackStack = false) where TViewModel : INavigableViewModel => new StackNavigatorRequest(
			StackNavigatorRequestType.NavigateForward,
			viewModelType: typeof(TViewModel),
			viewType: null,
			viewModelProvider: () => viewModelProvider(),
			suppressTransitions: suppressTransitions,
			clearBackStack: clearBackStack,
			entryIndexesToRemove: null);

		/// <summary>
		/// Gets a new <see cref="StackNavigatorRequest"/> for a <see cref="StackNavigatorRequestType.RemoveEntry"/> operation.
		/// </summary>
		/// <inheritdoc cref="StackNavigatorRequest.StackNavigatorRequest(StackNavigatorRequestType, Type, Type, Func{INavigableViewModel}, bool?, bool?, IEnumerable{int})"/>
		public static StackNavigatorRequest GetRemoveEntryRequest(IEnumerable<int> entryIndexesToRemove) => new StackNavigatorRequest(
			StackNavigatorRequestType.RemoveEntry,
			viewModelType: null,
			viewType: null,
			viewModelProvider: null,
			suppressTransitions: null,
			clearBackStack: null,
			entryIndexesToRemove: entryIndexesToRemove);

		/// <summary>
		/// Gets a new <see cref="StackNavigatorRequest"/> for a <see cref="StackNavigatorRequestType.Clear"/> operation.
		/// </summary>
		/// <inheritdoc cref="StackNavigatorRequest.StackNavigatorRequest(StackNavigatorRequestType, Type, Type, Func{INavigableViewModel}, bool?, bool?, IEnumerable{int})"/>
		public static StackNavigatorRequest GetClearRequest() => new StackNavigatorRequest(
			StackNavigatorRequestType.Clear,
			viewModelType: null,
			viewType: null,
			viewModelProvider: null,
			suppressTransitions: null,
			clearBackStack: null,
			entryIndexesToRemove: null);

		/// <summary>
		/// Gets a new <see cref="StackNavigatorRequest"/> for back navigation.
		/// </summary>
		/// <inheritdoc cref="StackNavigatorRequest.StackNavigatorRequest(StackNavigatorRequestType, Type, Type, Func{INavigableViewModel}, bool?, bool?, IEnumerable{int})"/>
		public static StackNavigatorRequest GetNavigateBackRequest() => new StackNavigatorRequest(
			StackNavigatorRequestType.NavigateBack,
			viewModelType: null,
			viewType: null,
			viewModelProvider: null,
			suppressTransitions: null,
			clearBackStack: null,
			entryIndexesToRemove: null);

		/// <summary>
		/// ­Initializes a new instance of the <see cref="StackNavigatorRequest"/> class.
		/// </summary>
		/// <param name="requestType">The type of request.</param>
		/// <param name="viewModelType">The type of the ViewModel.</param>
		/// <param name="viewType">The type of the view.</param>
		/// <param name="viewModelProvider">The function providing the ViewModel instance.</param>
		/// <param name="suppressTransitions">Flag indicating whether to suppress the navigation transitions.</param>
		/// <param name="clearBackStack">Flag indicating whether to clear the back stack.</param>
		/// <param name="entryIndexesToRemove">The list of index to remove.</param>
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

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{RequestType}, {nameof(ViewModelType)}: {ViewModelType?.Name}";
		}
	}
}
