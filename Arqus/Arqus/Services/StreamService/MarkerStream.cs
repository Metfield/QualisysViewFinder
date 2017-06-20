using Arqus.Helpers;
using Arqus.Service;
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
        public MarkerStream(int frequency = 10) : base(ComponentType.Component2d, frequency){}

        List<Camera> cameras;
        protected override void RetrieveDataAsync(RTPacket packet)
        {
            Urho.Application.InvokeOnMainAsync(() => CameraStore.CurrentCamera.Parent?.OnMarkerUpdate(packet.Get2DMarkerData(CameraStore.CurrentCamera.ID - 1)));
        }
    }
}
