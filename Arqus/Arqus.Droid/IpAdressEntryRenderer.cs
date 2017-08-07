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

            var native = Control as EntryEditText;

            native.InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal | Android.Text.InputTypes.NumberFlagSigned | Android.Text.InputTypes.ClassText;
            native.KeyListener = DigitsKeyListener.GetInstance(false, true);
        }
    }
}