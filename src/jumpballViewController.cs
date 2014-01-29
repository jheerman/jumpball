using System;
using System.Linq;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.ObjCRuntime;

using ShinobiEssentials;
using MonoTouch.Dialog;

namespace jumpball
{
	public partial class jumpballViewController : UIViewController
	{
		public enum Arrow
		{
			Left = 1,
			Right = 2
		};

		UIScrollView _scroller;
		SEssentialsSlidingOverlay _slidingView;
		Arrow _position = Arrow.Right;
		UIImageView _arrow = new UIImageView (UIImage.FromFile ("1.png"));

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

		List<Arrow> _possessionHistory = new List<Arrow> ();

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
		}

		void alternatePossession (object sender, EventArgs args)
		{
			_scroller.AddSubview (
				new UILabel() {
					Frame = new RectangleF(20, (_possessionHistory.Count * 50), 200, 50),
					Text = Enum.GetName (typeof(Arrow), _position)
				}
			);
			_possessionHistory.Add (_position);

			if (_position == Arrow.Left) {
				//set the animation left to right
				_arrow.AnimationImages = left_to_right_arrows;
				_arrow.AnimationRepeatCount = 1;
				_arrow.AnimationDuration = .5;
				UIView.BeginAnimations ("rotateAnimation");
				UIView.SetAnimationDelegate (this);
				UIView.SetAnimationDidStopSelector (new Selector ("rotateAnimationFinished:"));
				UIView.CommitAnimations ();
				_arrow.StartAnimating ();
			}
			else {
				//set the animation right to left
				_arrow.AnimationImages = right_to_left_arrows;
				_arrow.AnimationRepeatCount = 1;
				_arrow.AnimationDuration = .5;
				UIView.BeginAnimations ("rotateAnimation");
				UIView.SetAnimationDelegate (this);
				UIView.SetAnimationDidStopSelector (new Selector ("rotateAnimationFinished:"));
				UIView.CommitAnimations ();
				_arrow.StartAnimating ();
			}
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			_slidingView = new SEssentialsSlidingOverlay (View.Frame, true);
			_slidingView.Overlay.BackgroundColor = UIColor.White;
			_slidingView.UnderlayRevealAmount = UserInterfaceIdiomIsPhone ? .66f : .33f;
			_slidingView.Underlay.AddShadowTop ();
			_slidingView.Toolbar.BackgroundColor = UIColor.Red;
			_slidingView.Overlay.AddSubview (_arrow);

			var title = new UILabel () { 
				Text = "Jumpball",
				TextAlignment = UITextAlignment.Center,
				Frame = new RectangleF(0,0,View.Frame.Width, 50),
				Font = UIFont.BoldSystemFontOfSize(20),
				TextColor = UIColor.Red
			};

			_slidingView.Overlay.AddSubview (title);

			_arrow.Center = View.Center;
			_arrow.AutosizesSubviews = true;
			_arrow.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;

			var alternate = new UIButton (new RectangleF (0, View.Frame.Bottom - 100, View.Frame.Width, 55));
			alternate.AutosizesSubviews = true;
			alternate.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
			alternate.SetTitleColor (UIColor.Blue, UIControlState.Normal);
			alternate.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
			alternate.SetTitle ("Alternate", UIControlState.Normal);
			alternate.TouchUpInside += alternatePossession;

			_slidingView.Overlay.AddSubview (alternate);
			_slidingView.Style.ButtonTintColor = UIColor.White;

			_scroller = new UIScrollView () {
				Frame = new RectangleF (0, 50, _slidingView.Underlay.Frame.Width, _slidingView.Underlay.Frame.Height+50)
			};

			_scroller.ScrollEnabled = true;
			_scroller.UserInteractionEnabled = true;
			_scroller.BackgroundColor = UIColor.Clear;
			_scroller.ContentSize = new SizeF (_slidingView.Underlay.Frame.Width,2000);

			_slidingView.Underlay.AddSubview (
				new UILabel () { 
					Frame = new RectangleF(0,0,_slidingView.Underlay.Frame.Width, 50),
					Text = "Possession History",
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

		[Export("rotateAnimationFinished:")]
		void RotateStopped ()
		{
			if (_position == Arrow.Right)
			{
				_arrow.Image = UIImage.FromFile("5.png");
				_position = Arrow.Left;
				return;
			}

			_arrow.Image = UIImage.FromFile("1.png");
			_position = Arrow.Right;

		}

	}
}

