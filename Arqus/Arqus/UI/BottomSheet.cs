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
        double initialHeight;

        public BottomSheet()
        {
            initialHeight = TranslationY;
        }
        

        protected async override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(propertyName == "IsVisible")
            {
                if (!IsVisible){
                    Button suckIt = this.FindByName<Button>("toggleButton");

                    await Task.Run(() => this.TranslateTo(0, Height - suckIt.Height, 250, Easing.SpringOut));
                    
                }
                else {
                    await Task.Run(() => this.TranslateTo(0, 0, 250, Easing.SpringIn));
                }
                    
            }
            else
                base.OnPropertyChanged(propertyName);

        }
        
        
    }
}
