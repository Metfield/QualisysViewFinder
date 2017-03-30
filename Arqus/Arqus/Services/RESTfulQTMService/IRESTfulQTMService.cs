using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QTMRealTimeSDK.Settings;

namespace Arqus.Services
{
    public enum CameraMode
    {
        MARKER,
        VIDEO,
        INTENSITY
    }
    public interface IRESTfulQTMService
    {
        void GetSettings();
        void SetCameraMode(CameraMode mode);
    }
}
