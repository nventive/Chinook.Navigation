using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Chinook.SectionsNavigation
{
	public partial class MultiFrame : Grid
	{
		private readonly Dictionary<string, FrameInfo> _frames = new Dictionary<string, FrameInfo>();
		private readonly TaskCompletionSource<bool> _isReady = new TaskCompletionSource<bool>();

		public MultiFrame()
		{
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			_isReady.TrySetResult(true);
		}

		public string CommaSeparatedSectionsFrameNames
		{
			get { return (string)GetValue(CommaSeparatedSectionsFrameNamesProperty); }
			set { SetValue(CommaSeparatedSectionsFrameNamesProperty, value); }
		}

		public static readonly DependencyProperty CommaSeparatedSectionsFrameNamesProperty =
			DependencyProperty.Register("CommaSeparatedSectionsFrameNames", typeof(string), typeof(MultiFrame), new PropertyMetadata(null, OnCommaSeparatedSectionsFrameNamesChanged));

		private static void OnCommaSeparatedSectionsFrameNamesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var that = (MultiFrame)d;
			var commaSeparatedNames = e.NewValue as string;

			if (!string.IsNullOrEmpty(commaSeparatedNames))
			{
				var names = commaSeparatedNames.Split(',');
				foreach (var name in names)
				{
					that.GetOrCreateFrame(name, priority: 0);
				}

				if (that.SectionsFrameNames == null)
				{
					that.SectionsFrameNames = names.ToList();
				}
				else
				{
					throw new NotSupportedException($"Setting {nameof(MultiFrame)}.{nameof(CommaSeparatedSectionsFrameNames)} more than once is not supported.");
				}
			}
		}

		public IReadOnlyList<string> SectionsFrameNames { get; private set; }

		/// <summary>
		/// Gets an existing Frame with the specified name.
		/// </summary>
		/// <param name="name">The name of the frame to retrieve.</param>
		public Frame GetSectionFrame(string name)
		{
			return _frames[name].Frame;
		}

		/// <summary>
		/// Gets or creates a Frame with the specified name and priority.
		/// </summary>
		/// <param name="name">The name of the Frame to retrieve or create.</param>
		/// <param name="priority">The priority determines the Z order of the view. It's only used when creating a new Frame.</param>
		public Frame GetOrCreateFrame(string name, int priority) // Must run on UI thread
		{
			if (_frames.TryGetValue(name, out var existingFrame))
			{
				return existingFrame.Frame;
			}
			else
			{
				var frame = new Frame()
				{
					Name = name,
					Opacity = 0,
					IsHitTestVisible = false,
					RenderTransform = new TranslateTransform()
				};

				var framesAbove = _frames.Values.Where(fi => fi.Priority > priority).ToList();
				var index = _frames.Count - framesAbove.Count;
				foreach (var frameAbove in framesAbove)
				{
					++frameAbove.Index;
				}

				var frameState = new FrameInfo(frame, index, priority);

				_frames.Add(name, frameState);
				Children.Insert(index, frame);

				return frame;
			}
		}

		/// <summary>
		/// Removes a Frame with the specified name.
		/// </summary>
		/// <param name="name">The name of the Frame to remove.</param>
		public async Task RemoveFrame(string name)
		{
			var frame = _frames[name];
			_frames.Remove(name);

			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, RemoveFrameUI);

			void RemoveFrameUI() // Runs on UI thread
			{
				Children.Remove(frame.Frame);
			}
		}

		/// <summary>
		/// Shows a frame instantly (there would never be any animation for this).
		/// </summary>
		/// <param name="frameName">The name of the frame to show.</param>
		public async Task ShowInstantly(string frameName) // Runs on background thread.
		{
			await _isReady.Task;

			var frame = _frames[frameName];

			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, UpdateFrameUI);

			void UpdateFrameUI() // Runs on UI thread
			{
				frame.State = FrameState.Shown;
				frame.Frame.Opacity = 1;
				frame.Frame.Visibility = Visibility.Visible;
				frame.Frame.IsHitTestVisible = true;
			}
		}

		/// <summary>
		/// Changes the active section.
		/// The animation can be customized using <see cref="Animations.ChangeSectionAnimation_HideFrame1ToRevealFrame2"/> and <see cref="Animations.ChangeSectionAnimation_ShowFrame2ToHideFrame1"/>.
		/// </summary>
		/// <param name="previousFrameName">The name of the frame that will no longer be active.</param>
		/// <param name="nextFrameName">The name of the frame that will be active.</param>
		public async Task ChangeActiveSection(string previousFrameName, string nextFrameName) // Runs on background thread.
		{
			await HidePreviousFrameAndShowNextFrame(previousFrameName, nextFrameName, UpdateView);

			async Task UpdateView(FrameInfo previousFrame, FrameInfo nextFrame) // Runs on the UI thread
			{
				if (previousFrame.Index > nextFrame.Index)
				{
					// If the previous frame is on top of the next one, we hide the previous frame to reveal the next one underneath.

					// 1.  We must first hide all frames that would be visible between our two target frames.
					var framesToHideInstantly = _frames.Values
						.Where(f => f.State == FrameState.Shown && f.Index > nextFrame.Index && f.Index < previousFrame.Index);

					foreach (var f in framesToHideInstantly)
					{
						f.Frame.Visibility = Visibility.Collapsed;
						f.State = FrameState.Hidden;
					}

					// 2. Animate the previous frame
					await Animations.ChangeSectionAnimation_HideFrame1ToRevealFrame2(previousFrame.Frame, nextFrame.Frame);

					// 3. Once faded out, collapse the previous frame.
					previousFrame.Frame.Visibility = Visibility.Collapsed;
				}
				else
				{
					// Otherwise, the next frame is displayed on top of the previous one.
					await Animations.ChangeSectionAnimation_ShowFrame2ToHideFrame1(previousFrame.Frame, nextFrame.Frame);

					// 5. Collapse the previous frame that is no longer visible.
					previousFrame.Frame.Visibility = Visibility.Collapsed;
				}
			}
		}

		/// <summary>
		/// Shows a modal frame.
		/// </summary>
		/// <param name="previousFrameName">The name of the frame that will be hidden by the modal.</param>
		/// <param name="nextFrameName">The name of the modal frame.</param>
		public async Task OpenModal(string previousFrameName, string nextFrameName) // Runs on background thread.
		{
			await HidePreviousFrameAndShowNextFrame(previousFrameName, nextFrameName, UpdateView);

			async Task UpdateView(FrameInfo previousFrame, FrameInfo nextFrame) // Runs on the UI thread
			{
				await Animations.OpenModalAnimation(previousFrame.Frame, nextFrame.Frame);
			}
		}

		/// <summary>
		/// Hides a modal frame.
		/// </summary>
		/// <param name="previousFrameName">The name of the modal frame to close.</param>
		/// <param name="nextFrameName">The name of the frame that will be revealed as a result of closing the modal.</param>
		public async Task CloseModal(string previousFrameName, string nextFrameName) // Runs on background thread.
		{
			await HidePreviousFrameAndShowNextFrame(previousFrameName, nextFrameName, UpdateView);

			async Task UpdateView(FrameInfo previousFrame, FrameInfo nextFrame) // Runs on the UI thread
			{
				await Animations.CloseModalAnimation(previousFrame.Frame, nextFrame.Frame);
			}
		}

		/// <summary>
		/// Helper method to abstract the wait for the control, try-catch, threading, and changing the state of the FrameInfo instances.
		/// </summary>
		private async Task HidePreviousFrameAndShowNextFrame(string previousFrameName, string nextFrameName, Func<FrameInfo, FrameInfo, Task> innerAction)
		{
			await _isReady.Task;

			var previousFrame = _frames[previousFrameName];
			var nextFrame = _frames[nextFrameName];

			await Dispatcher.RunTaskAsync(CoreDispatcherPriority.Normal, UpdateView);

			async Task UpdateView() // Runs on UI thread
			{
				try
				{
					nextFrame.State = FrameState.Showing;
					previousFrame.State = FrameState.Hiding;

					await innerAction(previousFrame, nextFrame);

					nextFrame.State = FrameState.Shown;
					previousFrame.State = FrameState.Hidden;
				}
				catch (Exception exception)
				{
					this.Log().LogError("Error in animation", exception);
				}
			}
		}

		private enum FrameState
		{
			Hidden,
			Showing,
			Shown,
			Hiding
		}

		/// <summary>
		/// This class is used to associate properties with a <see cref="Frame"/> instance.
		/// </summary>
		private class FrameInfo
		{
			public FrameInfo(Frame frame, int index, int priority)
			{
				Frame = frame;
				Index = index;
				Priority = priority;
				State = FrameState.Hidden;
			}

			/// <summary>
			/// The frame associated with the other properties.
			/// </summary>
			public Frame Frame { get; }

			/// <summary>
			/// The state of the frame visually.
			/// This is used to cleanup frames when changing states.
			/// </summary>
			public FrameState State { get; set; }

			/// <summary>
			/// The index of the frame in the its parent <see cref="Panel.Children"/> collection.
			/// This is used to chose which animation to use when changing sections.
			/// </summary>
			public int Index { get; set; }

			/// <summary>
			/// The priority used to order frames in their parent <see cref="Panel.Children"/> collection.
			/// This ensures that modal are on top of sections and that modals are ordered even when opened with custom priorities.
			/// </summary>
			public int Priority { get; }
		}
	}
}
