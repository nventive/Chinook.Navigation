using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.StackNavigation
{
	public interface INavigationProcessor
	{
		NavigationOperation GetOperation(string operationName);

		//IDisposable BeginOperationScope(NavigationOperation operation);

		bool TryBeginOperationScope(NavigationOperation operation, out IDisposable scope);
	}
}
