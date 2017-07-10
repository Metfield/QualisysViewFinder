using Arqus.Helpers;
using Arqus.Service;
using Arqus.Visualization;
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
        protected override void RetrieveDataAsync()
        {
            RTPacket packet = connection.Protocol.GetRTPacket();
            cameras = packet.Get2DMarkerData();

            if (cameras != null)
            {
                for (int i = 0; i < cameras.Count; i++)
                {
                    int id = i + 1;
                    if (CameraStore.Cameras.ContainsKey(id))
                    {
                        Camera camera = cameras[i];
                        CameraScreen cameraScreen = CameraStore.Cameras[id]?.Screen;

                        // NOTE: There is a chance that the packet does not contain data for the currently selected 
                        // camera if that is the case simply catch the exception and log it then keep streaming as usual.
                        try
                        {
                            if (cameraScreen != null)
                            {
                                Urho.Application.InvokeOnMain(() => cameraScreen.MarkerData = camera);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("MarkerStream:" + e.Message);
                            Debugger.Break();
                        }
                    }
                }
            }
        }
    }
}
