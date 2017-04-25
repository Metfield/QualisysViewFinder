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
        ImageSharp.PixelFormats.Rgba32[] GetImageData(int id);
    }
}
