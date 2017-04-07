using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QTMRealTimeSDK.Settings;
using QTMRealTimeSDK;

namespace Arqus
{
    public interface ISettingsService
    {
        void GetSettings();
        Task<bool> SetCameraMode(uint id, CameraMode mode);
    }
}
