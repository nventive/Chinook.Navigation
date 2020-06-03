using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.StackNavigation
{
	/// <summary>
	/// The exception thrown when navigation gets stopped.
	/// </summary>
	public class NavigationCanceledException : OperationCanceledException
	{
		/// <summary>
		/// Creates a new instance of <see cref="NavigationCanceledException"/>.
		/// </summary>
		public NavigationCanceledException() : base(message: "The navigation was stopped.")
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="NavigationCanceledException"/>.
		/// </summary>
		/// <param name="message"><inheritdoc cref="OperationCanceledException.OperationCanceledException(string)"/></param>
		public NavigationCanceledException(string message) : base(message)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="NavigationCanceledException"/>.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The inner exception.</param>
		public NavigationCanceledException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
