
using Arqus.Services;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Notification_iOS))]
public class Notification_iOS : INotification
{
    public void Show(string messageTitle, string message)
    {
        // iOS' notifications need to run on main thread, otherwise welcome to exception town
        Device.BeginInvokeOnMainThread( () => new UIAlertView(messageTitle, message, null, "OK").Show());
    }
}

