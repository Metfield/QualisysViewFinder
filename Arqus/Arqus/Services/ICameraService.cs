using QTMRealTimeSDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus
{
    public interface ICameraService
    {
        Camera? GetMarkerData(int id);
        ImageSharp.Color[] GetImageData(int id);
    }
}
