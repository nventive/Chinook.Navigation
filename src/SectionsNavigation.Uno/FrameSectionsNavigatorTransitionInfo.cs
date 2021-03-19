using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// Represents the transition information for a <see cref="FrameSectionsNavigator"/>.
	/// </summary>
	public abstract class FrameSectionsNavigatorTransitionInfo : SectionsNavigatorTransitionInfo
	{
		/// <summary>
		/// The type of <see cref="FrameSectionsNavigatorTransitionInfo"/>.
		/// </summary>
		public abstract FrameSectionsNavigatorTransitionInfoTypes Type { get; }

		/// <summary>
		/// Gets the transition info for a suppressed transition. There is not visual animation when using this transition info.
		/// </summary>
		public static FrameTransitionInfo SuppressTransition { get; } = new FrameTransitionInfo(ExecuteSuppressTransition);

		/// <summary>
		/// The new frame fades in or the previous frame fades out, depending on the layering.
		/// </summary>
		public static FrameTransitionInfo FadeInOrFadeOut { get; } = new FrameTransitionInfo(ExecuteFadeInOrFadeOut);

		/// <summary>
		/// The new frame slides up, hiding the previous frame.
		/// </summary>
		public static FrameTransitionInfo SlideUp { get; } = new FrameTransitionInfo(ExecuteSlideUp);

		/// <summary>
		/// The previous frame slides down, revealing the new frame.
		/// </summary>
		public static FrameTransitionInfo SlideDown { get; } = new FrameTransitionInfo(ExecuteSlideDown);

		/// <summary>
		/// The frames are animated using a UIViewController with the default configuration.
		/// </summary>
		public static UIViewControllerTransitionInfo NativeiOSModal { get; } = new UIViewControllerTransitionInfo();

		private static Task ExecuteSlideDown(Frame frameToHide, Frame frameToShow, bool frameToShowIsAboveFrameToHide)
		{
			return Animations.SlideFrame1DownToRevealFrame2(frameToHide, frameToShow);
		}

		private static Task ExecuteSlideUp(Frame frameToHide, Frame frameToShow, bool frameToShowIsAboveFrameToHide)
		{
			return Animations.SlideFrame2UpwardsToHideFrame1(frameToHide, frameToShow);
		}

		private static Task ExecuteFadeInOrFadeOut(Frame frameToHide, Frame frameToShow, bool frameToShowIsAboveFrameToHide)
		{
			if (frameToShowIsAboveFrameToHide)
			{
				return Animations.FadeInFrame2ToHideFrame1(frameToHide, frameToShow);
			}
			else
			{
				return Animations.FadeOutFrame1ToRevealFrame2(frameToHide, frameToShow);
			}
		}

		private static Task ExecuteSuppressTransition(Frame frameToHide, Frame frameToShow, bool frameToShowIsAboveFrameToHide)
		{
			return Animations.CollapseFrame1AndShowFrame2(frameToHide, frameToShow);
		}
	}

	/// <summary>
	/// Represents the supported types of <see cref="FrameSectionsNavigatorTransitionInfo"/>.
	/// </summary>
	public enum FrameSectionsNavigatorTransitionInfoTypes
	{
		/// <summary>
		/// The transition is applied by changing properties or animating properties of <see cref="Frame"/> objects.
		/// This is associated with the <see cref="FrameTransitionInfo"/> class.
		/// </summary>
		FrameBased,

		/// <summary>
		/// The transition is applied by using the native iOS transitions offered by UIKit.
		/// This is associated with the <see cref="UIViewControllerTransitionInfo"/> class.
		/// </summary>
		UIViewControllerBased
	}
}
