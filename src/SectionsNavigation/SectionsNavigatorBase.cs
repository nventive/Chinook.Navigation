using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Chinook.StackNavigation;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This is a base class for <see cref="ISectionsNavigator"/> implementations.
	/// This class does all the business logic to respect the <see cref="ISectionsNavigator"/> contract. 
	/// </summary>
	public abstract class SectionsNavigatorBase : ISectionsNavigator
	{
		/// <summary>The mutex to use when setting the <see cref="State"/> property to Processing.</summary>
		private readonly object _processingStateMutex = new object();

		protected readonly ILogger _logger;
		protected readonly IReadOnlyDictionary<Type, Type> _globalRegistrations;

		/// <summary>
		/// Creates a new instance of <see cref="SectionsNavigatorBase"/>.
		/// </summary>
		/// <param name="defaultSections">The sections reachable by <see cref="SetActiveSection(CancellationToken, SectionsNavigatorRequest)"/> mapped by their name.</param>
		/// <param name="globalRegistrations">Mapping of view model types to their view types.</param>
		public SectionsNavigatorBase(IReadOnlyDictionary<string, ISectionStackNavigator> defaultSections, IReadOnlyDictionary<Type, Type> globalRegistrations)
		{
			_globalRegistrations = globalRegistrations;
			_logger = GetLogger();

			State = new SectionsNavigatorState(
				sections: defaultSections ?? new Dictionary<string, ISectionStackNavigator>(),
				activeSection: null,
				modals: new List<IModalStackNavigator>(),
				lastRequestState: NavigatorRequestState.Processed,
				lastRequest: null);

			foreach (var section in defaultSections.Values)
			{
				section.StateChanged += OnSectionStateChanged;
			}
		}

		/// <summary>
		/// Gets the logger for this class.
		/// This method is abstract because we want the actual (not abstract) implementation to show up in the logs instead of <see cref="SectionsNavigatorBase"/>.
		/// </summary>
		protected abstract ILogger GetLogger();

		private SectionsNavigatorState _state;

		/// <inheritdoc/>
		public SectionsNavigatorState State
		{
			get => _state;
			private set
			{
				var previous = _state;
				_state = value;
				StateChanged?.Invoke(this, new SectionsNavigatorEventArgs(previous, _state));
			}
		}

		/// <inheritdoc/>
		public event SectionsNavigatorStateChangedEventHandler StateChanged;

		/// <inheritdoc/>
		public async Task<IModalStackNavigator> OpenModal(CancellationToken ct, SectionsNavigatorRequest request)
		{
			if (_logger.IsEnabled(LogLevel.Debug))
			{
				_logger.LogDebug("Starting 'OpenModal' operation.");
			}

			if (ct.IsCancellationRequested)
			{
				if (_logger.IsEnabled(LogLevel.Warning))
				{
					_logger.LogWarning("Canceled 'OpenModal' operation because of cancellation token.");
				}

				return null;
			}

			// No cancellation beyond this point.

			lock (_processingStateMutex)
			{
				if (State.LastRequestState == NavigatorRequestState.Processing)
				{
					if (_logger.IsEnabled(LogLevel.Warning))
					{
						_logger.LogWarning($"Canceled 'OpenModal' operation because another request is processing. (Processing request: '{State.LastRequest}')");
					}

					return null;
				}

				State = new SectionsNavigatorState(State, NavigatorRequestState.Processing, request);
			}

			try
			{
				var modalPriority = request.ModalPriority ?? (State.Modals.LastOrDefault()?.Priority ?? 0) + 1;
				var modalName = request.ModalName ?? "Modal" + modalPriority;

				if (State.Modals.Any(m => m.Priority == modalPriority))
				{
					throw new ArgumentException($"Can't open new modal with priority '{modalPriority}' because another modal already exists with that priority.", paramName: $"{nameof(request)}.{nameof(SectionsNavigatorRequest.ModalPriority)}");
				}
				if (State.Modals.Any(m => m.Name == modalName))
				{
					throw new ArgumentException($"Can't open new modal named '{modalName}' because another modal already exists with that name.", paramName: $"{nameof(request)}.{nameof(SectionsNavigatorRequest.ModalName)}");
				}

				var modalNavigator = await CreateModalNavigator(modalPriority, modalName);
				await modalNavigator.Navigate(CancellationToken.None, request.NewModalStackNavigationRequest);

				var isTopModal = !State.Modals.Any() || modalPriority > State.Modals.Max(m => m.Priority);

				await InnerOpenModal(modalNavigator, isTopModal);

				State = new SectionsNavigatorState(
					sections: State.Sections,
					activeSection: State.ActiveSection,
					modals: ImmutableOrderedAdd(State.Modals, modalNavigator),
					lastRequestState: NavigatorRequestState.Processed,
					lastRequest: request);

				if (_logger.IsEnabled(LogLevel.Information))
				{
					_logger.LogInformation("Finished 'OpenModal' operation.");
				}

				return modalNavigator;
			}
			catch
			{
				// In the case of an error, this makes sure to set the state to FailedToProcess instead of Processing.
				State = new SectionsNavigatorState(
					state: State,
					lastRequestState: NavigatorRequestState.FailedToProcess,
					lastRequest: request);

				throw;
			}

			async Task<IModalStackNavigator> CreateModalNavigator(int priority, string name)
			{
				var singleStackNavigator = await CreateStackNavigator(name, priority, _globalRegistrations);
				var decoratedNavigator = new SectionStackNavigator(singleStackNavigator, name, isModal: true, priority);

				decoratedNavigator.StateChanged += OnSectionStateChanged;

				return decoratedNavigator;
			}
		}

		private void OnSectionStateChanged(object sender, StackNavigatorEventArgs args)
		{
			var navigator = (SectionStackNavigator)sender;

			if (!navigator.IsModal || State.Modals.Contains(navigator))
			{
				// We don't report stack navigator events for modals that are not yet in the Modals list.
				// The reason is that we don't want to report changes on something that we don't have yet.

				var request = new SectionsNavigatorRequest(
					requestType: SectionsNavigatorRequestType.ReportSectionStateChanged,
					sectionName: navigator.IsModal ? null : navigator.Name,
					modalName: navigator.IsModal ? navigator.Name : null,
					modalPriority: navigator.Priority,
					newModalStackNavigationRequest: null
				);

				// We don't use the lock for HandleSectionRequest requests because the effect is instantaneous.
				State = new SectionsNavigatorState(
					state: State,
					lastRequestState: args.CurrentState.LastRequestState,
					lastRequest: request);
			}
		}

		/// <summary>
		/// This method is reponsible to create a new instance implementing <see cref="IStackNavigator"/>, adding views to UI tree, etc.
		/// </summary>
		protected abstract Task<IStackNavigator> CreateStackNavigator(string name, int priority, IReadOnlyDictionary<Type, Type> registrations);

		/// <summary>
		/// Implementors must override this method to typically update the View layer.
		/// </summary>
		/// <param name="navigator">The modal navigator being opened.</param>
		/// <param name="isTopModal">Whether the new modal is the top-most one. This is false when a modal opens with a lower priority than the active modal.</param>
		protected abstract Task InnerOpenModal(IModalStackNavigator navigator, bool isTopModal);

		/// <inheritdoc/>
		public async Task<ISectionStackNavigator> SetActiveSection(CancellationToken ct, SectionsNavigatorRequest request)
		{
			if (_logger.IsEnabled(LogLevel.Debug))
			{
				_logger.LogDebug($"Starting 'SetActiveSection' operation for section '{request.SectionName}'.");
			}

			if (ct.IsCancellationRequested)
			{
				if (_logger.IsEnabled(LogLevel.Warning))
				{
					_logger.LogWarning("Canceled 'SetActiveSection' operation because of cancellation token.");
				}

				return null;
			}

			// No cancellation beyond this point.

			lock (_processingStateMutex)
			{
				if (State.LastRequestState == NavigatorRequestState.Processing)
				{
					if (_logger.IsEnabled(LogLevel.Warning))
					{
						_logger.LogWarning($"Canceled 'SetActiveSection' operation because another request is processing.  (Processing request: '{State.LastRequest}')");
					}

					return null;
				}

				if (State.ActiveSection?.Name == request.SectionName)
				{
					if (_logger.IsEnabled(LogLevel.Warning))
					{
						_logger.LogWarning($"Canceled 'SetActiveSection' operation because the section '{request.SectionName}' is already active.");
					}

					return null;
				}

				State = new SectionsNavigatorState(State, NavigatorRequestState.Processing, request);
			}

			try
			{
				if (State.Sections.TryGetValue(request.SectionName, out var nextSection))
				{
					var previousSection = State.ActiveSection;
					await InnerSetActiveSection(previousSection, nextSection);

					State = new SectionsNavigatorState(
						sections: State.Sections,
						activeSection: nextSection,
						modals: State.Modals,
						lastRequestState: NavigatorRequestState.Processed,
						lastRequest: request);

					if (_logger.IsEnabled(LogLevel.Information))
					{
						_logger.LogInformation($"Finished 'SetActiveSection' operation for section '{request.SectionName}'.");
					}

					return nextSection;
				}
				else
				{
					throw new KeyNotFoundException($"Failed to set active secton '{request.SectionName}' because no section was found with that name.");
				}
			}
			catch
			{
				// In the case of an error, this makes sure to set the state to FailedToProcess instead of Processing.
				State = new SectionsNavigatorState(
					state: State,
					lastRequestState: NavigatorRequestState.FailedToProcess,
					lastRequest: request);

				throw;
			}
		}

		/// <summary>
		/// Implementors must override this method to typically update the View layer. 
		/// </summary>
		/// <param name="previousSection">The section that was previously active.</param>
		/// <param name="nextsection">The section will be the active one.</param>
		protected abstract Task InnerSetActiveSection(ISectionStackNavigator previousSection, ISectionStackNavigator nextsection);

		/// <inheritdoc/>
		public async Task CloseModal(CancellationToken ct, SectionsNavigatorRequest request)
		{
			if (_logger.IsEnabled(LogLevel.Debug))
			{
				_logger.LogDebug($"Starting 'CloseModal' operation on modal at priority '{request.ModalPriority}'.");
			}

			if (ct.IsCancellationRequested)
			{
				if (_logger.IsEnabled(LogLevel.Warning))
				{
					_logger.LogWarning("Canceled 'CloseModal' operation because of cancellation token.");
				}

				return;
			}

			// No cancellation beyond this point.

			lock (_processingStateMutex)
			{
				if (State.LastRequestState == NavigatorRequestState.Processing)
				{
					if (_logger.IsEnabled(LogLevel.Warning))
					{
						_logger.LogWarning($"Canceled 'CloseModal' operation because another request is processing. (Processing request: '{State.LastRequest}')");
					}

					return;
				}

				State = new SectionsNavigatorState(State, NavigatorRequestState.Processing, request);
			}

			try
			{
				if (!State.Modals.Any())
				{
					throw new InvalidOperationException($"Can't close modal because there are no modals.");
				}

				var modalNavigator = State.Modals.FirstOrDefault(m => m.Name == request.ModalName) ?? State.Modals.LastOrDefault(m => m.Priority == (request.ModalPriority ?? m.Priority));
				if (modalNavigator != null)
				{
					await InnerCloseModal(modalNavigator);

					// We dispose the navigator before calling clear, so that we don't report the clear event.
					modalNavigator.Dispose();
					// We clear the navigator to dispose all ViewModels.
					await modalNavigator.Clear(CancellationToken.None);

					State = new SectionsNavigatorState(
						sections: State.Sections,
						activeSection: State.ActiveSection,
						modals: ImmutableRemove(State.Modals, modalNavigator),
						lastRequestState: NavigatorRequestState.Processed,
						lastRequest: request);

					if (_logger.IsEnabled(LogLevel.Information))
					{
						_logger.LogInformation($"Finished 'CloseModal' operation on for modal at priority '{request.ModalPriority}'.");
					}

				}
				else
				{
					throw new KeyNotFoundException($"Failed to close modal at priority '{request.ModalPriority}' because no modal was found with that priority.");
				}
			}
			catch
			{
				// In the case of an error, this makes sure to set the state to FailedToProcess instead of Processing.
				State = new SectionsNavigatorState(
					state: State,
					lastRequestState: NavigatorRequestState.FailedToProcess,
					lastRequest: request);

				throw;
			}
		}

		/// <summary>
		/// This method is responsible for changing any UI, play animations, release View references, etc.
		/// </summary>
		/// <param name="modalToClose">The modal section to close.</param>
		protected abstract Task InnerCloseModal(IModalStackNavigator modalToClose);

		private static IReadOnlyList<IModalStackNavigator> ImmutableOrderedAdd(IReadOnlyList<IModalStackNavigator> list, IModalStackNavigator item)
		{
			var result = new List<IModalStackNavigator>(capacity: list.Count + 1);
			result.AddRange(list);
			result.Add(item);
			result.Sort((left, right) => left.Priority.CompareTo(right.Priority));
			return result;
		}

		private static IReadOnlyList<T> ImmutableRemove<T>(IReadOnlyList<T> list, T item)
		{
			var result = new List<T>(capacity: list.Count);
			result.AddRange(list);
			result.Remove(item);
			return result;
		}
	}
}
