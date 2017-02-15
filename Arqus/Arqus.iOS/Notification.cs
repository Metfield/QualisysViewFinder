using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Why are these not found?
//using MonoTouch.Foundation;
//using MonoTouch.UIKit;

using Foundation;
using UIKit;

namespace SharedProjects
{
    internal class Notification
    {
        internal static void Show(string title, string message)
        {
            new UIAlertView(title, message, null, "OK").Show();
        }
    }
}