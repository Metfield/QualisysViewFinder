using System;
using Android.Widget;

namespace SharedProjects
{    
    internal class Notification
    {
        internal static void Show(string title, string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
        }
    }
}