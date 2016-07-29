﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using Foundation;
using Toggl.Phoebe.Helpers;
using Toggl.Ross.Theme;
using UIKit;

namespace Toggl.Ross.ViewControllers
{
    public sealed class LeftViewController : UIViewController
    {
        private static string DefaultUserName = "Loading...";
        private static string DefaultUserEmail = "Loading...";
        private static string DefaultImage = "profile.png";
        private static string DefaultRemoteImage = "https://assets.toggl.com/images/profile.png";

        public enum MenuOption
        {
            Timer = 0,
            Reports = 1,
            Settings = 2,
            Feedback = 3,
            Logout = 4,
            Login = 5,
            SignUp = 6
        }

        private UIButton logButton;
        private UIButton reportsButton;
        private UIButton settingsButton;
        private UIButton feedbackButton;
        private UIButton signOutButton;
        private UIButton loginButton;
        private UIButton signUpButton;
        private UIButton[] menuButtons;
        private UILabel usernameLabel;
        private UILabel emailLabel;

        private UIImageView userAvatarImage;
        private UIImageView separatorLineImage;
        private const int horizMargin = 15;
        private const int menuOffset = 60;
        private Action<MenuOption> buttonSelector;

        public LeftViewController(Action<MenuOption> buttonSelector)
        {
            this.buttonSelector = buttonSelector;
        }

        public override void LoadView()
        {
            base.LoadView();
            View.BackgroundColor = UIColor.White;

            menuButtons = new[]
            {
                logButton = CreateDrawerButton("LeftPanelMenuLog", Image.TimerButton, Image.TimerButtonPressed),
                reportsButton = CreateDrawerButton("LeftPanelMenuReports", Image.ReportsButton, Image.ReportsButtonPressed),
                settingsButton = CreateDrawerButton("LeftPanelMenuSettings", Image.SettingsButton, Image.SettingsButtonPressed),
                feedbackButton = CreateDrawerButton("LeftPanelMenuFeedback", Image.FeedbackButton, Image.FeedbackButtonPressed),
                signOutButton = CreateDrawerButton("LeftPanelMenuSignOut", Image.SignoutButton, Image.SignoutButtonPressed, false),
                loginButton = CreateDrawerButton("LeftPanelMenuLogin", Image.LoginButton, Image.LoginButtonPressed, false),
                signUpButton = CreateDrawerButton("LeftPanelMenuSignUp", Image.SignUpButton, Image.SignUpButtonPressed, false),
            };

            logButton.SetImage(Image.TimerButtonPressed, UIControlState.Normal);
            logButton.SetTitleColor(Color.LightishGreen, UIControlState.Normal);

            UpdateLayoutIfNeeded();
        }

        private void UpdateLayoutIfNeeded()
        {
            if (NoUserHelper.IsLoggedIn)
            {
                View.AddSubview(signOutButton);
            }
            else
            {
                View.AddSubview(loginButton);
                View.AddSubview(signUpButton);
            }

            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            View.AddConstraints(MakeConstraints(View));
        }

        private UIButton CreateDrawerButton(string text, UIImage normalImage, UIImage pressedImage, bool addToView = true)
        {
            var button = new UIButton();
            button.SetTitle(text.Tr(), UIControlState.Normal);
            button.SetImage(normalImage, UIControlState.Normal);
            button.SetImage(pressedImage, UIControlState.Highlighted);
            button.SetTitleColor(Color.LightishGreen, UIControlState.Highlighted);
            button.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            button.Apply(Style.LeftView.Button);
            button.TouchUpInside += OnMenuButtonTouchUpInside;

            if (!addToView) return button;

            View.AddSubview(button);
            return button;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            usernameLabel = new UILabel().Apply(Style.LeftView.UserLabel);
            var imageStartingPoint = View.Frame.Width - menuOffset - 90f;
            usernameLabel.Frame = new CGRect(40, View.Frame.Height - 110f, height: 50f, width: imageStartingPoint - 40f);
            View.AddSubview(usernameLabel);
            emailLabel = new UILabel().Apply(Style.LeftView.EmailLabel);
            emailLabel.Frame = new CGRect(40f, View.Frame.Height - 80f, height: 50f, width: imageStartingPoint - 40f);
            View.AddSubview(emailLabel);

            userAvatarImage = new UIImageView(
                new CGRect(
                    imageStartingPoint,
                    View.Frame.Height - 100f,
                    60f,
                    60f
                ));

            userAvatarImage.Layer.CornerRadius = 30f;
            userAvatarImage.Layer.MasksToBounds = true;
            View.AddSubview(userAvatarImage);

            separatorLineImage = new UIImageView(UIImage.FromFile("line.png"));
            separatorLineImage.Frame = new CGRect(0f, View.Frame.Height - 140f, height: 1f, width: View.Frame.Width - menuOffset);
            if (View.Frame.Height > 480)
            {
                View.AddSubview(separatorLineImage);
            }

            if (NoUserHelper.IsLoggedIn)
            {
                userAvatarImage.Hidden = false;
                separatorLineImage.Hidden = false;
                // Set default values
                ConfigureUserData(DefaultUserName, DefaultUserEmail, DefaultImage);
            }
            else
            {
                userAvatarImage.Hidden = true;
                separatorLineImage.Hidden = true;
            }
        }

        public async void ConfigureUserData(string name, string email, string imageUrl)
        {
            usernameLabel.Text = name;
            emailLabel.Text = email;
            UIImage image;

            if (imageUrl == DefaultImage || imageUrl == DefaultRemoteImage)
            {
                userAvatarImage.Image = UIImage.FromFile(DefaultImage);
                return;
            }

            // Try to download the image from server
            // if user doesn't have image configured or
            // there is not connection, use a local image.
            try
            {
                image = await LoadImage(imageUrl);
            }
            catch
            {
                image = UIImage.FromFile(DefaultImage);
            }

            userAvatarImage.Image = image;
        }

        private void OnMenuButtonTouchUpInside(object sender, EventArgs e)
        {
            if (buttonSelector == null)
                return;

            if (sender == logButton)
            {
                buttonSelector.Invoke(MenuOption.Timer);
            }
            else if (sender == reportsButton)
            {
                buttonSelector.Invoke(MenuOption.Reports);
            }
            else if (sender == settingsButton)
            {
                buttonSelector.Invoke(MenuOption.Settings);
            }
            else if (sender == feedbackButton)
            {
                buttonSelector.Invoke(MenuOption.Feedback);
            }
            else if (sender == loginButton)
            {
                buttonSelector.Invoke(MenuOption.Login);
            }
            else if (sender == signUpButton)
            {
                buttonSelector.Invoke(MenuOption.SignUp);
            }
            else
            {
                buttonSelector.Invoke(MenuOption.Logout);
            }
        }

        public nfloat MinDraggingX => 0;

        public nfloat MaxDraggingX => View.Frame.Width - menuOffset;

        private static IEnumerable<FluentLayout> MakeConstraints(UIView container)
        {
            UIView prev = null;
            const float startTopMargin = 60.0f;
            const float topMargin = 7f;

            foreach (var view in container.Subviews)
            {
                if (!(view is UIButton))
                {
                    continue;
                }

                if (prev == null)
                {
                    yield return view.AtTopOf(container, topMargin + startTopMargin);
                }
                else
                {
                    yield return view.Below(prev, topMargin);
                }

                yield return view.AtLeftOf(container, horizMargin);
                yield return view.AtRightOf(container, horizMargin + 20);

                prev = view;
            }
        }

        private async Task<UIImage> LoadImage(string imageUrl)
        {
            var httpClient = new HttpClient();

            Task<byte[]> contentsTask = httpClient.GetByteArrayAsync(imageUrl);

            // await! control returns to the caller and the task continues to run on another thread
            var contents = await contentsTask;

            // load from bytes
            return UIImage.LoadFromData(NSData.FromArray(contents));
        }
    }
}
