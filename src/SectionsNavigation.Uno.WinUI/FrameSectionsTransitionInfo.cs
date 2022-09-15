using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Chinook.SectionsNavigation
{
    /// <summary>
    /// Represents the transition information for a <see cref="FrameSectionsNavigator"/>.
    /// </summary>
    public abstract class FrameSectionsTransitionInfo : SectionsTransitionInfo
    {
        /// <summary>
        /// The type of <see cref="FrameSectionsTransitionInfo"/>.
        /// </summary>
        public abstract FrameSectionsTransitionInfoTypes Type { get; }

        /// <summary>
        /// Gets the transition info for a suppressed transition. There is not visual animation when using this transition info.
        /// </summary>
        public static DelegatingFrameSectionsTransitionInfo SuppressTransition { get; } = new DelegatingFrameSectionsTransitionInfo(ExecuteSuppressTransition);

        /// <summary>
        /// The new frame fades in or the previous frame fades out, depending on the layering.
        /// </summary>
        public static DelegatingFrameSectionsTransitionInfo FadeInOrFadeOut { get; } = new DelegatingFrameSectionsTransitionInfo(ExecuteFadeInOrFadeOut);

        /// <summary>
        /// The new frame slides up, hiding the previous frame.
        /// </summary>
        public static DelegatingFrameSectionsTransitionInfo SlideUp { get; } = new DelegatingFrameSectionsTransitionInfo(ExecuteSlideUp);

        /// <summary>
        /// The previous frame slides down, revealing the new frame.
        /// </summary>
        public static DelegatingFrameSectionsTransitionInfo SlideDown { get; } = new DelegatingFrameSectionsTransitionInfo(ExecuteSlideDown);

        /// <summary>
        /// The frames are animated using a UIViewController with the default configuration.
        /// </summary>
        public static UIViewControllerSectionsTransitionInfo NativeiOSModal { get; } = new UIViewControllerSectionsTransitionInfo();

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
    /// Represents the supported types of <see cref="FrameSectionsTransitionInfo"/>.
    /// </summary>
    public enum FrameSectionsTransitionInfoTypes
    {
        /// <summary>
        /// The transition is applied by changing properties or animating properties of <see cref="Frame"/> objects.
        /// This is associated with the <see cref="DelegatingFrameSectionsTransitionInfo"/> class.
        /// </summary>
        FrameBased,

        /// <summary>
        /// The transition is applied by using the native iOS transitions offered by UIKit.
        /// This is associated with the <see cref="UIViewControllerSectionsTransitionInfo"/> class.
        /// </summary>
        UIViewControllerBased
    }
}
