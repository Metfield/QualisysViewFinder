using UIKit;
using Xamarin.Forms;
using Arqus.UI;
using Arqus.iOS.Renderers;
using Xamarin.Forms.Platform.iOS;
using System.ComponentModel;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

[assembly: ExportRenderer(typeof(IpAdressEntryRenderer), typeof(CustomIpAdressEntryRenderer))]
namespace Arqus.iOS.Renderers
{
    public class CustomIpAdressEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Entry> e)
        {
            base.OnElementChanged(e);

            UITextField nativeField = Control as UITextField;
            nativeField.KeyboardType = UIKeyboardType.DecimalPad;
            nativeField.ReloadInputViews();
        }
    }
}