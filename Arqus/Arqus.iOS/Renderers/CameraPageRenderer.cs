using Arqus;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly:ExportRenderer(typeof(CameraPage), typeof(CameraPageRenderer))]
namespace Arqus
{
    // iOS-specific renderer for CameraPage
    class CameraPageRenderer : PageRenderer
    {
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // Disable swipe-to-go-back gesture
            var navigationController = this.ViewController.NavigationController;
            navigationController.InteractivePopGestureRecognizer.Enabled = false;
        }
    }
}
