using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QTMRealTimeSDK.Settings;
using QTMRealTimeSDK;

namespace Arqus.Services
{
    public interface IRESTfulQTMService
    {
        void GetSettings();
        void SetCameraMode(uint id, CameraMode mode);
    }
}
