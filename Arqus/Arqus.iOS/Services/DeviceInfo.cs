using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Arqus.Services
{
    internal class DeviceInfo
    {
        internal static float GetAspectRatio()
        {
            return (float)(UIScreen.MainScreen.Bounds.Height / UIScreen.MainScreen.Bounds.Width);
        }
    }
}