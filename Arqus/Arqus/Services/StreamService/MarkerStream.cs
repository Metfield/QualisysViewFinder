﻿using Arqus.Helpers;
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
        protected override void RetrieveDataAsync(RTPacket packet)
        {
            int id = 1;
            cameras = packet.Get2DMarkerData();

            if (cameras != null)
            {
                for (int i = 0; i < cameras.Count; i++)
                {
                    CameraScreen cameraScreen = CameraStore.Cameras[i + 1]?.Parent;
                    Camera camera = cameras[i];

                    Urho.Application.InvokeOnMain(() =>
                    {
                    // NOTE: There is a chance that the packet does not contain data for the currently selected 
                    // camera if that is the case simply catch the exception and log it then keep streaming as usual.
                    try
                        {
                            cameraScreen.MarkerData = camera;
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("MarkerStream:" + e.Message);
                        }
                    });
                }
            }
            
        }
    }
}
