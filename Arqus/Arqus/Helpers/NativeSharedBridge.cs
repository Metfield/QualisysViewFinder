using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus
{
    // Ugly ugly ugly... but Xamarin did not want to cooperate in any other way
    // This is a gross container for a public, global variable :'c
    public static class NativeSharedBridge
    {
        // This flag is used by CameraPage.xaml.cs::OnDisappear()
        // Used only by Android to avoid skit crash
        public static bool applicationIsEnteringBackground;
    }
}
