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
        Task<bool> SetCameraMode(int id, CameraMode mode);
        Task<bool> SetCameraSettings(int id, string settingsParameter, int value);
    }
}
