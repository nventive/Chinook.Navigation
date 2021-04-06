using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This <see cref="FrameSectionsTransitionInfo"/> implementation is used to create a custom transition between frames.
	/// This class allows you to build your own animation as an async Task using <see cref="FrameSectionsTransitionDelegate"/>.
	/// </summary>
	public class DelegatingFrameSectionsTransitionInfo : FrameSectionsTransitionInfo
	{
		private readonly FrameSectionsTransitionDelegate _frameTranstion;

		/// <summary>
		/// Creates a new instance of <see cref="DelegatingFrameSectionsTransitionInfo"/>.
		/// </summary>
		/// <param name="frameTranstion">The method describing the transition.</param>
		public DelegatingFrameSectionsTransitionInfo(FrameSectionsTransitionDelegate frameTranstion)
		{
			_frameTranstion = frameTranstion;
		}

		///<inheritdoc/>
		public override FrameSectionsTransitionInfoTypes Type => FrameSectionsTransitionInfoTypes.FrameBased;

		/// <summary>
		/// Runs the transition.
		/// </summary>
		/// <param name="frameToHide">The <see cref="Frame"/> that must be hidden after the transition.</param>
		/// <param name="frameToShow">The <see cref="Frame"/> that must be visible after the transition.</param>
		/// <param name="frameToShowIsAboveFrameToHide">Flag indicating whether the frame to show is above the frame to hide in their parent container.</param>
		/// <returns>Task running the transition operation.</returns>
		public Task Run(Frame frameToHide, Frame frameToShow, bool frameToShowIsAboveFrameToHide)
		{
			return _frameTranstion(frameToHide, frameToShow, frameToShowIsAboveFrameToHide);
		}
	}

	/// <summary>
	/// Delegate describing a <see cref="Frame"/> transition animation.
	/// We recommend that the animation doesn't go over 300ms.
	/// </summary>
	/// <param name="frameToHide">The <see cref="Frame"/> that must be hidden after the transition.</param>
	/// <param name="frameToShow">The <see cref="Frame"/> that must be visible after the transition.</param>
	/// <param name="frameToShowIsAboveFrameToHide">Flag indicating whether the frame to show is above the frame to hide in their parent container.</param>
	/// <returns>Task running the transition operation.</returns>
	public delegate Task FrameSectionsTransitionDelegate(Frame frameToHide, Frame frameToShow, bool frameToShowIsAboveFrameToHide);
}
