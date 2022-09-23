using System;
using System.Collections.Generic;
using System.Text;
#if __IOS__
using UIKit;
#endif

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This implementation of <see cref="FrameSectionsTransitionInfo"/> is used to animate frames using native iOS animations.
	/// This class allows you to leverage the various built-in options from UIKit.
	/// This class is only supported on iOS.
	/// </summary>
	public class UIViewControllerSectionsTransitionInfo : FrameSectionsTransitionInfo
	{
#if __IOS__
		/// <summary>
		/// Creates a new instance of <see cref="UIViewControllerSectionsTransitionInfo"/>.
		/// </summary>
		/// <param name="allowDismissFromGesture">See <see cref="AllowDismissFromGesture"/>.</param>
		/// <param name="modalPresentationStyle">See <see cref="ModalPresentationStyle"/>.</param>
		/// <param name="modalTransitionStyle">See <see cref="ModalTransitionStyle"/>.</param>
		public UIViewControllerSectionsTransitionInfo(bool allowDismissFromGesture = true, UIModalPresentationStyle modalPresentationStyle = UIModalPresentationStyle.PageSheet, UIModalTransitionStyle modalTransitionStyle = UIModalTransitionStyle.CoverVertical)
		{
			AllowDismissFromGesture = allowDismissFromGesture;
			ModalPresentationStyle = modalPresentationStyle;
			ModalTransitionStyle = modalTransitionStyle;
		}

		/// <summary>
		/// Gets whether the <see cref="UIViewController"/> should allow dismissal from native gestures.
		/// </summary>
		public bool AllowDismissFromGesture { get; }

		/// <inheritdoc cref="UIViewController.ModalPresentationStyle"/>
		public UIModalPresentationStyle ModalPresentationStyle { get; }

		/// <inheritdoc cref="UIViewController.ModalTransitionStyle"/>
		public UIModalTransitionStyle ModalTransitionStyle { get; }
#endif

		/// <inheritdoc/>
		public override FrameSectionsTransitionInfoTypes Type => FrameSectionsTransitionInfoTypes.UIViewControllerBased;
	}
}
