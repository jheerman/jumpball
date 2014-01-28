using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.ObjCRuntime;

using ShinobiEssentials;

namespace jumpball
{
	public partial class jumpballViewController : UIViewController
	{
		public enum Arrow
		{
			Left = 1,
			Right = 2
		};

		Arrow _position = Arrow.Right;
		UIImageView _arrow = new UIImageView (UIImage.FromFile ("1.png"));
		UIButton _alternate = new UIButton();

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

		List<Arrow> possessionHistory = new List<Arrow> ();

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

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			SEssentialsSlidingOverlay slidingView = new SEssentialsSlidingOverlay (View.Frame, true);
			slidingView.Overlay.BackgroundColor = UIColor.White;
			slidingView.UnderlayRevealAmount = UserInterfaceIdiomIsPhone ? .66f : .33f;
			slidingView.Underlay.AddShadowTop ();
			slidingView.Overlay.AddSubview (_arrow);

			_arrow.Center = View.Center;
			_arrow.AutosizesSubviews = true;
			_arrow.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;

			_alternate = new UIButton (new RectangleF (0, View.Frame.Bottom - 100, View.Frame.Width, 55));
			_alternate.AutosizesSubviews = true;
			_alternate.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
			_alternate.SetTitleColor (UIColor.Blue, UIControlState.Normal);
			_alternate.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
			_alternate.SetTitle ("Alternate", UIControlState.Normal);
			_alternate.TouchUpInside += (object sender, EventArgs e) => {
				possessionHistory.Add(_position);
				if (_position == Arrow.Left)
				{
					//set the animation left to right
					_arrow.AnimationImages = left_to_right_arrows;
					_arrow.AnimationRepeatCount = 1;
					_arrow.AnimationDuration = .5;

					UIView.BeginAnimations("rotateAnimation");
					UIView.SetAnimationDelegate (this);
					UIView.SetAnimationDidStopSelector (
						new Selector ("rotateAnimationFinished:"));
					UIView.CommitAnimations();

					_arrow.StartAnimating();
				}
				else
				{
					//set the animation right to left
					_arrow.AnimationImages = right_to_left_arrows;
					_arrow.AnimationRepeatCount = 1;
					_arrow.AnimationDuration = .5;

					UIView.BeginAnimations("rotateAnimation");
					UIView.SetAnimationDelegate (this);
					UIView.SetAnimationDidStopSelector (
						new Selector ("rotateAnimationFinished:"));
					UIView.CommitAnimations();

					_arrow.StartAnimating();

				}
			};

			slidingView.Overlay.AddSubview (_alternate);
			slidingView.Style.ButtonTintColor = UIColor.Black;
			View.AddSubview (slidingView);
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

