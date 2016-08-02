using System.Collections.Generic;
using Cirrious.FluentLayouts.Touch;
using GalaSoft.MvvmLight.Helpers;
using Toggl.Phoebe.Reactive;
using Toggl.Phoebe.ViewModels;
using Toggl.Ross.Theme;
using Toggl.Ross.Views;
using UIKit;

namespace Toggl.Ross.ViewControllers
{
    public class SettingsViewController : UIViewController
    {
        private SettingsVM viewModel { get; set; }

        private LabelSwitchView askProjectView { get; set; }
        private LabelSwitchView mobileTagView { get; set; }

        private Binding<bool, bool> askProjectBinding;
        private Binding<bool, bool> mobileTagBinding;

        public SettingsViewController()
        {
            Title = "SettingsTitle".Tr();
            EdgesForExtendedLayout = UIRectEdge.None;
        }

        public override void LoadView()
        {
            View = new UIView().Apply(Style.Screen);

            Add(new SeparatorView().Apply(Style.Settings.Separator));
            Add(askProjectView = new LabelSwitchView().Apply(Style.Settings.RowBackground));
            askProjectView.Label.Apply(Style.Settings.SettingLabel);
            askProjectView.Label.Text = "SettingsAskProject".Tr();
            askProjectView.Switch.ValueChanged += (s, e) => viewModel.SetChooseProjectForNew(askProjectView.Switch.On);

            Add(new SeparatorView().Apply(Style.Settings.Separator));
            Add(new UILabel() { Text = "SettingsAskProjectDesc".Tr() } .Apply(Style.Settings.DescriptionLabel));

            Add(new SeparatorView().Apply(Style.Settings.Separator));
            Add(mobileTagView = new LabelSwitchView().Apply(Style.Settings.RowBackground));
            mobileTagView.Label.Apply(Style.Settings.SettingLabel);
            mobileTagView.Label.Text = "SettingsMobileTag".Tr();
            mobileTagView.Switch.ValueChanged += (s, e) => viewModel.SetUseDefaultTag(mobileTagView.Switch.On);

            Add(new SeparatorView().Apply(Style.Settings.Separator));
            Add(new UILabel() { Text = "SettingsMobileTagDesc".Tr() } .Apply(Style.Settings.DescriptionLabel));

            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();

            View.AddConstraints(MakeConstraints(View));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            viewModel = new SettingsVM(StoreManager.Singleton.AppState);

            mobileTagBinding = this.SetBinding(() => viewModel.UseDefaultTag)
                               .WhenSourceChanges(() => mobileTagView.Switch.On = viewModel.UseDefaultTag);
            askProjectBinding = this.SetBinding(() => viewModel.ChooseProjectForNew)
                                .WhenSourceChanges(() => askProjectView.Switch.On = viewModel.ChooseProjectForNew);
        }

        public override void ViewWillDisappear(bool animated)
        {
            this.askProjectBinding.Detach();
            this.mobileTagBinding.Detach();
            viewModel.Dispose();

            base.ViewWillDisappear(animated);
        }

        private static IEnumerable<FluentLayout> MakeConstraints(UIView container)
        {
            UIView prev = null;

            foreach (var view in container.Subviews)
            {
                var topMargin = 0f;
                var horizMargin = 0f;

                if (view is UILabel)
                {
                    topMargin = 7f;
                    horizMargin = 15f;
                }
                else if (view is SeparatorView && !(prev is LabelSwitchView))
                {
                    topMargin = 20f;
                    horizMargin = 0f;
                }

                if (prev == null)
                {
                    yield return view.AtTopOf(container, topMargin);
                }
                else
                {
                    yield return view.Below(prev, topMargin);
                }

                yield return view.AtLeftOf(container, horizMargin);
                yield return view.AtRightOf(container, horizMargin);

                if (view is LabelSwitchView)
                {
                    yield return view.Height().EqualTo(42f);
                }
                else if (view is SeparatorView)
                {
                    yield return view.Height().EqualTo(1f);
                }

                prev = view;
            }
        }

        private class SeparatorView : UIView
        {
            public SeparatorView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false;
            }
        }
    }
}