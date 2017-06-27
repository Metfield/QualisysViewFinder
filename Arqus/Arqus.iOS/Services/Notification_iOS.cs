
using Arqus.Services;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Notification_iOS))]
public class Notification_iOS : INotification
{
    public void Show(string message)
    {
        new UIAlertView("hello", message, null, "OK").Show();
    }
}

