using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Services.MobileCenterService
{
    enum EventName
    {

    }

    interface IMobileCenterService
    {
        void Init();

        void TrackEvent();
    }
}
