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
            (Control as EntryEditText).InputType = Android.Text.InputTypes.ClassPhone;
        } 
    }
}