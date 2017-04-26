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

        List<Camera> cameras;
        protected override void RetrieveDataAsync(RTPacket packet)
        {
            cameras = packet.Get2DMarkerData();

            if(cameras != null)
            {
                uint id = 1;
                foreach (var camera in cameras)
                {
                    if(camera.MarkerData2D.Length > 0)
                    {
                        MessagingCenter.Send(this, MessageSubject.STREAM_DATA_SUCCESS.ToString() + id, camera);
                    }
                    id++;
                }
            }
        }
    }
}
