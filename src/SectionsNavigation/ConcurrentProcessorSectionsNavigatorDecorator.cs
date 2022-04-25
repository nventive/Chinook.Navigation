using Chinook.StackNavigation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.SectionsNavigation
{
	public class ConcurrentProcessorSectionsNavigatorDecorator : ConcurrentNavigationProcessor, ISectionsNavigator, ISectionsNavigatorProcessor
	{
		public ConcurrentProcessorSectionsNavigatorDecorator(ISectionsNavigator sectionsNavigator)
		{
			SectionsNavigator = sectionsNavigator;
		}

		public ISectionsNavigator SectionsNavigator { get; }

		/// <inheritdoc/>
		public SectionsNavigatorState State => SectionsNavigator.State;

		/// <inheritdoc/>
		public event SectionsNavigatorStateChangedEventHandler StateChanged
		{
			add => SectionsNavigator.StateChanged += value;
			remove => SectionsNavigator.StateChanged -= value;
		}

		/// <inheritdoc/>
		public Task CloseModal(CancellationToken ct, SectionsNavigatorRequest request)
		{
			var operation = GetOperation(nameof(CloseModal));
			return CloseModal(ct, request, operation);
		}

		/// <inheritdoc/>
		public async Task CloseModal(CancellationToken ct, SectionsNavigatorRequest request, NavigationOperation operation)
		{
			if (TryBeginOperationScope(operation, out var scope))
			{
				using (scope)
				{
					await SectionsNavigator.CloseModal(ct, request);
				}
			}
			else
			{

			}
		}

		/// <inheritdoc/>
		public Task<IModalStackNavigator> OpenModal(CancellationToken ct, SectionsNavigatorRequest request)
		{
			var operation = GetOperation(nameof(OpenModal));
			return OpenModal(ct, request, operation);
		}

		/// <inheritdoc/>
		public async Task<IModalStackNavigator> OpenModal(CancellationToken ct, SectionsNavigatorRequest request, NavigationOperation operation)
		{
			if (TryBeginOperationScope(operation, out var scope))
			{
				using (scope)
				{
					return await SectionsNavigator.OpenModal(ct, request);
				}
			}
			else
			{
				return null;
			}
		}

		/// <inheritdoc/>
		public Task<ISectionStackNavigator> SetActiveSection(CancellationToken ct, SectionsNavigatorRequest request)
		{
			var operation = GetOperation(nameof(SetActiveSection));
			return SetActiveSection(ct, request, operation);
		}

		/// <inheritdoc/>
		public async Task<ISectionStackNavigator> SetActiveSection(CancellationToken ct, SectionsNavigatorRequest request, NavigationOperation operation)
		{
			if (TryBeginOperationScope(operation, out var scope))
			{
				using (scope)
				{
					return await SectionsNavigator.SetActiveSection(ct, request);
				}
			}
			else
			{
				return null;
			}
		}
	}
}
