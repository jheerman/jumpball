using System;
using System.Linq;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.ObjCRuntime;

using ShinobiEssentials;
using MonoTouch.Dialog;
using System.Threading.Tasks;

namespace jumpball
{
	public partial class jumpballViewController : UIViewController
	{
		public enum ArrowPosition
		{
			Left = 1,
			Right = 2
		};

		UIScrollView _scroller;
		SEssentialsSlidingOverlay _slidingView;
		ArrowPosition _position = ArrowPosition.Right;
		UIButton _arrowButton = new UIButton ();

		static readonly UIImage[] right_to_left_arrows = new UIImage [] {
			UIImage.FromFile("1.png"),
			UIImage.FromFile("2.png"),
			UIImage.FromFile("3.png"),
			UIImage.FromFile("4.png"),
			UIImage.FromFile("5.png")
		};

		static readonly UIImage[] left_to_right_arrows = new UIImage [] {
			UIImage.FromFile("5.png"),
			UIImage.FromFile("6.png"),
			UIImage.FromFile("7.png"),
			UIImage.FromFile("8.png"),
			UIImage.FromFile("1.png")
		};

		List<ArrowPosition> _possessionHistory = new List<ArrowPosition> ();

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public jumpballViewController ()
			: base (UserInterfaceIdiomIsPhone ? "jumpballViewController_iPhone" : "jumpballViewController_iPad", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
			ResetHistory ();
		}

		void alternatePossession (object sender, EventArgs args)
		{
			_scroller.AddSubview (
				new UILabel() {
					Frame = new RectangleF(10, (_possessionHistory.Count * 50), 200, 50),
					TextAlignment = UITextAlignment.Left,
					AutosizesSubviews = true,
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
					Font = UIFont.SystemFontOfSize(13),
					Text = String.Format("{0} - {1}", 
						Enum.GetName (typeof(ArrowPosition), _position),
						DateTime.Now.ToString("G"))
				}
			);

			_possessionHistory.Add (_position);
			_scroller.ContentSize = new SizeF (_slidingView.Underlay.Frame.Width, (50 * _possessionHistory.Count));

			_arrowButton.ImageView.AnimationImages = _position == ArrowPosition.Left ? left_to_right_arrows : right_to_left_arrows;
			_arrowButton.ImageView.AnimationRepeatCount = 1;
			_arrowButton.ImageView.AnimationDuration = .5;
			UIView.BeginAnimations ("rotateAnimation");
			UIView.SetAnimationDelegate (this);
			UIView.SetAnimationDidStopSelector (new Selector ("rotateAnimationFinished:"));
			UIView.CommitAnimations ();
			_arrowButton.ImageView.StartAnimating ();

		}

		void ResetHistory ()
		{
			foreach (var subview in _scroller.Subviews) {
				// ignore header label but remove all other subviews
				if (subview.Tag == 1001)
					continue;
				subview.RemoveFromSuperview ();
			}
			_possessionHistory.Clear ();
			_scroller.ContentSize = new SizeF (0f, 0f);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			_arrowButton.Frame = new RectangleF (0, 0, 400, 400);
			_arrowButton.SetImage (UIImage.FromFile("1.png"), UIControlState.Normal);
			_arrowButton.TouchUpInside += alternatePossession;
			_arrowButton.Center = View.Center;
			_arrowButton.AdjustsImageWhenHighlighted = false;
			_arrowButton.AutosizesSubviews = true;
			_arrowButton.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;

			_slidingView = new SEssentialsSlidingOverlay (View.Frame, true);
			_slidingView.Overlay.BackgroundColor = UIColor.White;
			_slidingView.Overlay.AddSubview (_arrowButton);

			_slidingView.UnderlayRevealAmount = UserInterfaceIdiomIsPhone ? .66f : .33f;
			_slidingView.Underlay.AddShadowTop ();
			_slidingView.Toolbar.BackgroundColor = UIColor.Red;

			var title = new UILabel () { 
				Text = "Jumpball",
				TextAlignment = UITextAlignment.Center,
				Frame = new RectangleF(0,0,View.Frame.Width, 50),
				Font = UIFont.BoldSystemFontOfSize(20),
				TextColor = UIColor.Red,
				AutosizesSubviews = true,
				AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin
			};

			_slidingView.Overlay.AddSubview (title);

			var resetButton = new UIButton () { 
				Frame = new RectangleF (0, View.Frame.Bottom - 150, View.Frame.Width, 55),
				AutosizesSubviews = true,
				HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
				AutoresizingMask = UIViewAutoresizing.FlexibleMargins
			};

			resetButton.SetTitleColor (UIColor.Blue, UIControlState.Normal);
			resetButton.SetTitle ("Reset", UIControlState.Normal);
			resetButton.TouchUpInside += async delegate {
				await YesNoPrompt("Clear History", "Are you sure you want to continue?");
			}; 


			_slidingView.Overlay.AddSubview (resetButton);
			_slidingView.Style.ButtonTintColor = UIColor.White;

			_scroller = new UIScrollView () {
				Frame = new RectangleF (0, 50, _slidingView.Underlay.Frame.Width, _slidingView.Underlay.Frame.Height - 50),
				ScrollEnabled = true,
				UserInteractionEnabled = true,
				BackgroundColor = UIColor.Clear,
				AutosizesSubviews = true,
				AutoresizingMask = UIViewAutoresizing.All,
				ContentSize = new SizeF (_slidingView.Underlay.Frame.Width,_slidingView.Underlay.Frame.Height)
			};

			_slidingView.Underlay.AddSubview (
				new UILabel () { 
					Frame = new RectangleF(0,0,_slidingView.Underlay.Frame.Width, 50),
					Text = "Possession History",
					Tag = 1001,
					Font = UIFont.BoldSystemFontOfSize(20),
					TextAlignment = UITextAlignment.Center
				}
			);

			_slidingView.Underlay.AddSubview (_scroller);
			View.AddSubview (_slidingView);
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		async Task YesNoPrompt(string title, string message) {
			var result = await ShowModalAlertViewAsync (title, message, "Yes", "No");
			if (result)
				ResetHistory ();
		}

		Task<bool> ShowModalAlertViewAsync (string title, string message, params string[] buttons)
		{
			var alertView = new UIAlertView (title, message,  null, null, buttons);
			alertView.Show ();
			var tsc = new TaskCompletionSource<bool> ();

			alertView.Clicked += (sender, buttonArgs) => {
				tsc.TrySetResult(buttonArgs.ButtonIndex == 0);
			};    
			return tsc.Task;
		}		

		[Export("rotateAnimationFinished:")]
		void RotateStopped ()
		{
			if (_position == ArrowPosition.Right)
			{
				_arrowButton.SetImage (UIImage.FromFile("5.png"), UIControlState.Normal);
				_position = ArrowPosition.Left;
				return;
			}

			_arrowButton.SetImage (UIImage.FromFile("1.png"), UIControlState.Normal);
			_position = ArrowPosition.Right;

		}

	}
}

