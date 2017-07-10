using Arqus.Droid;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly:ResolutionGroupName("Arqus")]
[assembly:ExportEffect (typeof(FlatButtonEffect), "FlatButtonEffect")]
namespace Arqus.Droid
{
    public class FlatButtonEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                Control.StateListAnimator = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
