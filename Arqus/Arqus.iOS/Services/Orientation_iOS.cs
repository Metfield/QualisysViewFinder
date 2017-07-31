using System;
using UIKit;
using Foundation;
using Arqus.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(Orientation))]
public class Orientation : IOrientation
{
    public DeviceOrientations GetOrientation()
    {
        var currentOrientation = UIApplication.SharedApplication.StatusBarOrientation;
        bool isPortrait = currentOrientation == UIInterfaceOrientation.Portrait
            || currentOrientation == UIInterfaceOrientation.PortraitUpsideDown;

        return isPortrait ? DeviceOrientations.Portrait : DeviceOrientations.Landscape;
    }
}
