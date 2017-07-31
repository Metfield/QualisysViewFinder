using Android.Content;
using Android.Runtime;
using Android.Views;
using Arqus.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(Orientation))]
public class Orientation : IOrientation
{
    public DeviceOrientations GetOrientation()
    {
        IWindowManager windowManager = Android.App.Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

        var rotation = windowManager.DefaultDisplay.Rotation;
        bool isLandscape = rotation == SurfaceOrientation.Rotation90 || rotation == SurfaceOrientation.Rotation270;
        return isLandscape ? DeviceOrientations.Landscape : DeviceOrientations.Portrait;
    }
}
