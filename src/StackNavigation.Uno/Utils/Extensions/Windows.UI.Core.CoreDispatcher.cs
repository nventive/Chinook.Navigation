using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Windows.UI.Core
{
	/// <summary>
	/// Extension methods for classes in the Windows.UI.Core namespace.
	/// </summary>
	internal static class CoreDispatcherExtensions
	{
		/// <summary>
		/// This method allows for executing an async Task on the CoreDispatcher.
		/// </summary>
		/// <param name="coreDispatcher"></param>
		/// <param name="priority"></param>
		/// <param name="asyncAction">The async operation.</param>
		internal static async Task RunTaskAsync(this CoreDispatcher coreDispatcher, CoreDispatcherPriority priority, Func<Task> asyncAction)
		{
			var completion = new TaskCompletionSource<bool>();
			await coreDispatcher.RunAsync(priority, RunActionUI);
			await completion.Task;

			async void RunActionUI()
			{
				try
				{
					await asyncAction();
					completion.SetResult(true);
				}
				catch (Exception exception)
				{
					completion.SetException(exception);
				}
			}
		}

		/// <summary>
		/// This method allows for executing an async Task with result on the CoreDispatcher.
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="coreDispatcher"></param>
		/// <param name="priority"></param>
		/// <param name="asyncFunc">The async operation.</param>
		internal static async Task<TResult> RunTaskAsync<TResult>(this CoreDispatcher coreDispatcher, CoreDispatcherPriority priority, Func<Task<TResult>> asyncFunc)
		{
			var completion = new TaskCompletionSource<TResult>();
			await coreDispatcher.RunAsync(priority, RunActionUI);
			return await completion.Task;

			async void RunActionUI()
			{
				try
				{
					var result = await asyncFunc();
					completion.SetResult(result);
				}
				catch (Exception exception)
				{
					completion.SetException(exception);
				}
			}
		}
	}
}
