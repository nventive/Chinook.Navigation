#if __IOS__
using MediaPlayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Uno.UI.Controls;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This subclass of <see cref="RootViewController"/> delegates the presentation of new <see cref="UIViewController"/> to the <see cref="TopMostPresentedController"/>.
	/// <br/>With this implementation, it is possible to repeatedly call <see cref="PresentViewController(UIViewController, bool, Action)"/> and effectively stack <see cref="UIViewController"/>s automatically.
	/// </summary>
	public class MostPresentedRootViewController : RootViewController
	{
		/// <summary>
		/// Gets the top-most presented <see cref="UIViewController"/>. This iterates using <see cref="UIViewController.PresentedViewController"/> to get the top-most non-null value.
		/// </summary>
		public UIViewController TopMostPresentedController
		{
			get
			{
				UIViewController iterator = this;
				while (iterator.PresentedViewController != null)
				{
					iterator = iterator.PresentedViewController;
				}

				if (iterator == this)
				{
					return null;
				}

				return iterator;
			}
		}

		/// <inheritdoc/>
		public override Task PresentViewControllerAsync(UIViewController viewControllerToPresent, bool animated)
		{
			if (TopMostPresentedController == null)
			{
				return base.PresentViewControllerAsync(viewControllerToPresent, animated);
			}
			else
			{
				return TopMostPresentedController.PresentViewControllerAsync(viewControllerToPresent, animated);
			}
		}

		/// <inheritdoc/>
		public override void PresentViewController(UIViewController viewControllerToPresent, bool animated, Action completionHandler)
		{
			if (TopMostPresentedController == null)
			{
				base.PresentViewController(viewControllerToPresent, animated, completionHandler);
			}
			else
			{
				TopMostPresentedController.PresentViewController(viewControllerToPresent, animated, completionHandler);
			}
		}

		/// <inheritdoc/>
		public override void PresentModalViewController(UIViewController modalViewController, bool animated)
		{
			if (TopMostPresentedController == null)
			{
				base.PresentModalViewController(modalViewController, animated);
			}
			else
			{
				TopMostPresentedController.PresentModalViewController(modalViewController, animated);
			}
		}

		/// <inheritdoc/>
		public override void PresentMoviePlayerViewController(MPMoviePlayerViewController moviePlayerViewController)
		{
			if (TopMostPresentedController == null)
			{
				base.PresentMoviePlayerViewController(moviePlayerViewController);
			}
			else
			{
				TopMostPresentedController.PresentMoviePlayerViewController(moviePlayerViewController);
			}
		}

	}
}
#endif