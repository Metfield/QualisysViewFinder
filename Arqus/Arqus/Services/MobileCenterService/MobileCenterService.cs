using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using Nest;

namespace Arqus.Services.MobileCenterService
{
    static class MobileCenterService
    {
        /*static readonly string ANDROID_KEY = "9b95d7e1-82d4-4b8c-9b85-0b9a4e1cdffd";
        static readonly string IOS_KEY = "0076b52f-37a1-4a4d-9a7e-a0dd257838ba";*/
        
        public static void Init()
        {
            /*MobileCenter.Start("android=9b95d7e1-82d4-4b8c-9b85-0b9a4e1cdffd;" +
                   "ios=0076b52f-37a1-4a4d-9a7e-a0dd257838ba;",
                   typeof(Analytics), typeof(Crashes));*/
        }

        public static void TrackEvent(string caller, string eventName, [CallerMemberNameAttribute] string callerMethod = "")
        {
           /* Analytics.TrackEvent(string.Format("{0}: {1}", caller, eventName), new Dictionary<string, string>
            {
                { "Caller", caller },
                { "CallerMethod", callerMethod }
            });*/
        }
    }
}
