using QTMRealTimeSDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Services
{
    class MarkerStream: Stream
    {
        public MarkerStream(int frequency = 30) : base(ComponentType.Component2d, frequency){ }

        public List<Camera> GetMarkerData()
        {
            return currentPacket?.Get2DMarkerData();   
        }

        public Camera GetMarkerData(int id) { return currentPacket.Get2DMarkerData(id);}
    }
}
