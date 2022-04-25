using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Chinook.StackNavigation
{
	public class ConcurrentNavigationProcessor : INavigationProcessor
	{
		private readonly object _mutex = new object();
		private long _operationSequenceIdProvider = -1;
		private NavigationOperation _currentOperation;

		/// <inheritdoc/>
		public NavigationOperation GetOperation(string operationName)
		{
			return new NavigationOperation(operationName, Interlocked.Increment(ref _operationSequenceIdProvider));
		}

		/// <inheritdoc/>
		public bool TryBeginOperationScope(NavigationOperation operation, out IDisposable scope)
		{
			lock (_mutex)
			{
				if (_currentOperation is null)
				{
					_currentOperation = operation;
					scope = new ActionDisposable(() =>
					{
						lock (_mutex)
						{
							_currentOperation = null;
						}
					});
					return true;
				}
				else
				{
					scope = null;
					return false;
				}
			}
		}

		private class ActionDisposable : IDisposable
		{
			private readonly Action _action;
			private bool _isDisposed;

			public ActionDisposable(Action action)
			{
				_action = action;
			}

			public void Dispose()
			{
				if (!_isDisposed)
				{
					_isDisposed = true;
					_action();
				}
			}
		}
	}
}
