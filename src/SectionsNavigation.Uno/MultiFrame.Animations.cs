using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Chinook.SectionsNavigation
{
	public partial class MultiFrame
	{
		public static class Animations
		{
			public static Func<Frame, Frame, Task> ChangeSectionAnimation_HideFrame1ToRevealFrame2 { get; set; } = FadeOutFrame1ToRevealFrame2;

			public static Func<Frame, Frame, Task> ChangeSectionAnimation_ShowFrame2ToHideFrame1 { get; set; } = FadeInFrame2ToHideFrame1;

			public static Func<Frame, Frame, Task> OpenModalAnimation { get; set; } = SlideFrame2UpwardsToHideFrame1;

			public static Func<Frame, Frame, Task> CloseModalAnimation { get; set; } = SlideFrame1DownToRevealFrame2;

			public static async Task FadeOutFrame1ToRevealFrame2(Frame frame1, Frame frame2)
			{
				// 1. Disable the currently visible frame during the animation.
				frame1.IsHitTestVisible = false;

				// 2. Make the next frame visible so that we see it as the previous frame fades out.
				frame2.Opacity = 1;
#if __IOS__ || __ANDROID__
				// TODO: Fix this workaround
				frame2.SetValue(UIElement.OpacityProperty, 1d, DependencyPropertyValuePrecedences.Animations);
#endif
				frame2.Visibility = Visibility.Visible;
				frame2.IsHitTestVisible = true;

				// 3. Fade out the frame.
				var storyboard = new Storyboard();
				AddFadeOut(storyboard, frame1);
				await storyboard.Run();
			}

			public static async Task FadeInFrame2ToHideFrame1(Frame frame1, Frame frame2)
			{
				// 1. Disable the currently visible frame during the animation.
				frame1.IsHitTestVisible = false;

				// 2. Make the next frame visible, but transparent.
				frame2.Opacity = 0;
#if __IOS__ || __ANDROID__
				// TODO: Fix this workaround
				frame2.SetValue(UIElement.OpacityProperty, 0d, DependencyPropertyValuePrecedences.Animations);
#endif
				frame2.Visibility = Visibility.Visible;

				// 3. Fade in the frame.
				var storyboard = new Storyboard();
				AddFadeIn(storyboard, frame2);
				await storyboard.Run();

				// 4. Once the next frame is visible, enable it.
				frame2.IsHitTestVisible = true;
			}

			public static async Task SlideFrame2UpwardsToHideFrame1(Frame frame1, Frame frame2)
			{
				frame1.IsHitTestVisible = false;
				((TranslateTransform)frame2.RenderTransform).Y = frame1.ActualHeight;
				frame2.Opacity = 1;
				frame2.Visibility = Visibility.Visible;

				var storyboard = new Storyboard();
				AddSlideInFromBottom(storyboard, (TranslateTransform)frame2.RenderTransform);
				await storyboard.Run();

				frame2.IsHitTestVisible = true;
			}

			public static async Task SlideFrame1DownToRevealFrame2(Frame frame1, Frame frame2)
			{
				frame1.IsHitTestVisible = false;
				frame2.Opacity = 1;
				frame2.Visibility = Visibility.Visible;

				var storyboard = new Storyboard();
				AddSlideBackToBottom(storyboard, (TranslateTransform)frame1.RenderTransform, frame2.ActualHeight);
				await storyboard.Run();

				frame2.IsHitTestVisible = true;
			}

			public static Task CollapseFrame1AndShowFrame2(Frame frame1, Frame frame2)
			{
				frame1.Visibility = Visibility.Collapsed;
				frame2.IsHitTestVisible = false;

				frame2.Visibility = Visibility.Visible;
				frame2.Opacity = 1;
				frame2.IsHitTestVisible = true;

				return Task.CompletedTask;
			}

			private static void AddFadeIn(Storyboard storyboard, DependencyObject target)
			{
				var animation = new DoubleAnimation()
				{
					To = 1,
					Duration = new Duration(TimeSpan.FromSeconds(0.250)),
					EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut }
				};

				Storyboard.SetTarget(animation, target);
				Storyboard.SetTargetProperty(animation, "Opacity");

				storyboard.Children.Add(animation);
			}

			private static void AddFadeOut(Storyboard storyboard, DependencyObject target)
			{
				var animation = new DoubleAnimation()
				{
					To = 0,
					Duration = new Duration(TimeSpan.FromSeconds(0.250)),
					EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut }
				};

				Storyboard.SetTarget(animation, target);
				Storyboard.SetTargetProperty(animation, "Opacity");

				storyboard.Children.Add(animation);
			}

			private static void AddSlideInFromBottom(Storyboard storyboard, TranslateTransform target)
			{
				var animation = new DoubleAnimation()
				{
					To = 0,
					Duration = new Duration(TimeSpan.FromSeconds(0.250)),
					EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
				};

				Storyboard.SetTarget(animation, target);
				Storyboard.SetTargetProperty(animation, "Y");

				storyboard.Children.Add(animation);
			}

			private static void AddSlideBackToBottom(Storyboard storyboard, TranslateTransform target, double translation)
			{
				var animation = new DoubleAnimation()
				{
					To = translation,
					Duration = new Duration(TimeSpan.FromSeconds(0.250)),
					EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
				};

				Storyboard.SetTarget(animation, target);
				Storyboard.SetTargetProperty(animation, "Y");

				storyboard.Children.Add(animation);
			}
		}
	}
}
