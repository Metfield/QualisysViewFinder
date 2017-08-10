using Android.App;
using Android.Support.Design.Widget;
using Arqus.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(Notification_Android))]
public class Notification_Android : INotification
{
    public void Show(string messageTitle, string message)
    {
        var activity = (Activity)Forms.Context;
        var view = activity.FindViewById(Android.Resource.Id.Content);
        Snackbar.Make(view, messageTitle + ": " + message, 2500).Show();
    }
}

