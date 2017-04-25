using Arqus.Helpers;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Arqus.Services
{
    class MarkerStream: Stream<Camera>
    {
        public MarkerStream(int frequency = 30) : base(ComponentType.Component2d, frequency){}

        protected override void EnqueueDataAsync(RTPacket packet)
        {
            var data = packet.Get2DMarkerData();

            if(data != null)
            {
                uint id = 1;
                foreach (var camera in data)
                {
                    if(camera.MarkerData2D.Length > 0)
                    {
                        Enqueue(dataQueue, id, camera, DateTimeOffset.Now.ToUnixTimeMilliseconds());
                    }
                    id++;
                }
            }
        }

        /*
        protected override bool GetCurrentFrame()
        {
            return QTMNetworkConnection.GetCurrentCameraFrame();
        }
        */
    }
}
