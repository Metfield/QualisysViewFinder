using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Arqus.Services.MobileCenterService
{
    enum EventName
    {

    }

    interface IMobileCenterService
    {
        void TrackEvent(string eventName, [CallerMemberNameAttribute]string callerMethod = "");
    }
}
