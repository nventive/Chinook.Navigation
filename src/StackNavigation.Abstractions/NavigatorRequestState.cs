using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.StackNavigation
{
	/// <summary>
	/// This represents the various states of a navigator's request.
	/// Navigators can only deal with 1 request at a time.
	/// While a request is <see cref="Processing"/>, the navigator will not execute new request.
	/// </summary>
	public enum NavigatorRequestState
	{
		/// <summary>
		/// The navigator is processing a request.
		/// The request has not changed the navigator's state yet.
		/// </summary>
		Processing,

		/// <summary>
		/// The navigator finished processing its most recent request.
		/// The request did change the navigator's state.
		/// </summary>
		Processed,

		/// <summary>
		/// The navigator failed to process its most recent request.
		/// When this happens, the navigator's state is restored to what is was before the request.
		/// However, side effects might have happened depending on the exception that occurred.
		/// </summary>
		FailedToProcess
	}
}
