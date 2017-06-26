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
        public MarkerStream(int frequency = 30) : base(ComponentType.Component2d, frequency, false){}

        List<Camera> cameras;
        protected override void RetrieveDataAsync(RTPacket packet)
        {
            Urho.Application.InvokeOnMainAsync(() => {
                // NOTE: There is a chance that the packet does not contain data for the currently selected 
                // camera if that is the case simply catch the exception and log it then keep streaming as usual.
                try
                {
                    CameraStore.CurrentCamera.Parent?.OnMarkerUpdate(packet.Get2DMarkerData(CameraStore.CurrentCamera.ID - 1));
                }
                catch (Exception e)
                {
                    Debug.WriteLine("MarkerStream:" + e.Message);
                }
            });
        }
    }
}
