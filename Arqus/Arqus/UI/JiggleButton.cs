using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace Arqus.UI
{
    public class JiggleButton : Button
    {
        bool isJiggling;
        Color disabledColor = new Color(0.062, 0.4, 0.69);
        Color enabledColor = Color.FromHex("2196F3");

        public JiggleButton()
        {
            Clicked += async (sender, args) => 
            {
#if __IOS__
                BackgroundColor = disabledColor;
#endif

                if (isJiggling)
                    return;

                isJiggling = true;

                await this.ScaleTo(1.1, 150, new Easing(t => Math.Sin(t)));
                await this.ScaleTo(1, 150, new Easing(t => Math.Sin(t)));

                isJiggling = false;
            };

            // iOS specific button tweaks
#if __IOS__
            BorderRadius = 2;
            HeightRequest = 40;
#endif
        }

        // More iOS-specific stuff because it sucks
#if __IOS__
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (Text == "CONNECT")
                return;

            if ((propertyName == "IsEnabled") && IsEnabled)
                BackgroundColor = enabledColor;
        }
#endif
    }
}
