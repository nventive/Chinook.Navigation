using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.UI.Xaml;
#if __IOS__
using _UIViewController = UIKit.UIViewController;
#else
using _UIViewController = System.Object;
#endif

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// Represents a UIViewController that wraps a <see cref="Frame"/> in the context of modal sections.
	/// </summary>
	public class ModalViewController : _UIViewController, IDisposable
	{
#if __IOS__
		private readonly _UIViewController _parent;
		private bool _isClosingProgrammatically;
		private bool _wasClosedNatively;
#endif

		/// <summary>
		/// Creates a new instance of <see cref="ModalViewController"/>.
		/// </summary>
		/// <param name="modalName">The modal name.</param>
		/// <param name="frame">The frame to wrap.</param>
		/// <param name="parent">The parent UIViewController to use to present this controller.</param>
		public ModalViewController(string modalName, Frame frame, _UIViewController parent)
		{
			ModalName = modalName;
			frame.Visibility = Visibility.Visible;
			frame.Opacity = 1;
			frame.IsHitTestVisible = true;
#if __IOS__
			View = frame;
			_parent = parent;
#endif
		}

		/// <summary>
		/// The modal name associated with this UIViewController.
		/// </summary>
		public string ModalName { get; }

		/// <summary>
		/// The <see cref="UIViewControllerTransitionInfo"/> used when opening this modal.
		/// This is reused for the closing transition info when closing natively.
		/// </summary>
		public UIViewControllerTransitionInfo OpeningTransitionInfo { get; private set; }

		/// <summary>
		/// This event is raised when the UIViewController was closed natively, meaning that the <see cref="ISectionsNavigator"/> was not responsible for the close operation.
		/// This can happen when the user uses a native gesture.
		/// </summary>
		public event EventHandler ClosedNatively;

		/// <summary>
		/// Opens this UIViewController.
		/// </summary>
		/// <param name="transitionInfo">The transition info affecting the native animation.</param>
		public async Task Open(UIViewControllerTransitionInfo transitionInfo)
		{
			OpeningTransitionInfo = transitionInfo;
#if __IOS__
			SetTransitionInfo(transitionInfo);

			await _parent.PresentViewControllerAsync(this, animated: true);
#else
			await Task.CompletedTask;
#endif
		}

		/// <summary>
		/// Closes this UIViewController.
		/// </summary>
		/// <param name="transitionInfo">The transition info affecting the native animation.</param>
		public async Task Close(UIViewControllerTransitionInfo transitionInfo)
		{
#if __IOS__
			if (_wasClosedNatively)
			{
				return;
			}

			_isClosingProgrammatically = true;
			SetTransitionInfo(transitionInfo);

			await DismissViewControllerAsync(animated: true);
#else
			await Task.CompletedTask;
#endif
		}

#if __IOS__
		private void SetTransitionInfo(UIViewControllerTransitionInfo transitionInfo)
		{
			ModalInPresentation = !transitionInfo.AllowDismissFromGesture;
			ModalPresentationStyle = transitionInfo.ModalPresentationStyle;
			ModalTransitionStyle = transitionInfo.ModalTransitionStyle;
		}

		///<inheritdoc/>
		public override void ViewDidDisappear(bool animated)
		{
			if (!_isClosingProgrammatically)
			{
				_wasClosedNatively = true;

				ClosedNatively?.Invoke(this, EventArgs.Empty);
			}
		}

		///<inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			ClosedNatively = null;
		}
#else
		///<inheritdoc/>
		public void Dispose()
		{
			ClosedNatively = null;
		}
#endif
	}
}
