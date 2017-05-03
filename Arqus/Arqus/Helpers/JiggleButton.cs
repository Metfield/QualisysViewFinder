using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Arqus.UI
{
    public class JiggleButton : Button
    {
        bool isJiggling;

        public JiggleButton()
        {
            Clicked += async (sender, args) => {
                if (isJiggling) return;

                isJiggling = true;

                await this.ScaleTo(1.1, 150, new Easing(t => Math.Sin(t)));
                await this.ScaleTo(1, 150, new Easing(t => Math.Sin(t)));
                isJiggling = false;
            };
        }
    }
}
