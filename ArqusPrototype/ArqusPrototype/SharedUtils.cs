using System;
using System.Collections.Generic;
using System.Text;

namespace ArqusPrototype
{
    class SharedUtils
    {
        static string logTag = "QTMTestUtil";

        // Try AppPrintHelper?
        public static void Log(string printString)
        {
#if __ANDROID__
            Android.Util.Log.Info(logTag, printString);
#endif
            return;
        }

        // This method is implemented separately in each platform
        public static void ShowNotification(string message)
        {           
            SharedProjects.Notification.Show("Attention", message);
        }
    }
}
