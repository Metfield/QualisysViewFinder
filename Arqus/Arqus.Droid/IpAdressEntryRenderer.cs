using Android.Text.Method;
using Arqus.Droid.Renderers;
using Arqus.UI;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(IpAdressEntryRenderer), typeof(CustomIpAdressEntryRenderer))]
namespace Arqus.Droid.Renderers
{
    public class CustomIpAdressEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            // EntryEditText has been deprecated and FormsEditText must be used
            // starting with Xamarin.forms 2.4
            (Control as FormsEditText).InputType = Android.Text.InputTypes.ClassPhone;
        }
    }
}