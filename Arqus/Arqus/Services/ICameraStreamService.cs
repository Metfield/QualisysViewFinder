using Arqus.Visualization;
using QTMRealTimeSDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus
{
    public interface ICameraStreamService
    {
        Camera? GetMarkerData(int id);
        ImageSharp.Color[] GetImageData(int id);
    }
}
