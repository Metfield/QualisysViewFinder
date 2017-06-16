using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus.UI
{
    class BottomSheet : StackLayout
    {
        Button ToggleButton;
        Frame ContentFrame;
        public StackLayout Content { get; set; }

        public BottomSheet()
        {
            ToggleButton = new Button
            {
                Image = "drawable-hdpi/ic_settings_white_24dp",
                HorizontalOptions = LayoutOptions.Center,
                BorderWidth = 0,
                BorderColor = Color.Transparent,
                Command = new DelegateCommand(() => ToggleDrawer())
            };

            Content = new StackLayout();

            ContentFrame = new Frame
            {
                Style = (Style) Application.Current.Resources["Card"],
                HorizontalOptions = LayoutOptions.Fill,
                Margin = 0,
                CornerRadius = 0,
                Content = Content
            };

            Children.Add(ToggleButton);
            Children.Add(ContentFrame);
        }

        /*
        public static readonly BindableProperty OpenProperty = BindableProperty.Create(nameof(Open), typeof(bool), typeof(BottomSheet), false,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var control = bindable as BottomSheet;
                    var changingFrom = (bool) oldValue;
                    var changingTo = (bool) newValue;

                    UpdateDrawer(control, (bool)newValue);
                });

        */

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            base.LayoutChildren(x, y, width, height);

            foreach ( View child in Children)
            {
                if (!child.IsVisible)
                    continue;

                if (child.GetType() != typeof(StackLayout))
                {

                }
            }
        }

        protected override void OnAdded(View view)
        {
            //base.OnAdded(view);
        }
        private bool open = false;
        
        private async void ToggleDrawer()
        {
            open = !open;

            if (open)
            {

                await OpenDrawer();
            }
            else
            {
                await CloseDrawer();
            }


        }

        /*
        private static async void UpdateDrawer(BottomSheet control, bool open)
        {
            if (open)
            {
                
                await control.OpenDrawer();
            }
            else
            {
                await control.CloseDrawer();
            }
        }*/
        
        private async Task<Action> CloseDrawer(uint length = 250)
        {

            /*
            double buttonHeight = ToggleButton.Height;
            await Task.Run(() => this.TranslateTo(0, Height - buttonHeight, length, Easing.SpringOut));
            return () => this.FindByName<Frame>("settingsFrame").IsVisible = true;
            */
            return () => this.IsVisible = false;
        }

        private async Task<Action>OpenDrawer(uint length = 250)
        {
            /*
            await Task.Run(() => this.TranslateTo(0, 0, length, Easing.SpringIn));
            return () => this.FindByName<Frame>("settingsFrame").IsVisible = false;
            */
            return () => this.IsVisible = true;
        }
    }
}
