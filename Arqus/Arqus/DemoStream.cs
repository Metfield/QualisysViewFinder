using System;
using System.Collections.Generic;
using System.Text;
using QTMRealTimeSDK.Data;
using Arqus.Service;
using System.Diagnostics;

namespace Arqus.Services
{
    class DemoStream : Stream<Camera>
    {
        DemoMode demoMode;
        List<Camera> cameras;

        private int currentFrame;

        public DemoStream(int frequency = 60) : base(ComponentType.Component2d, frequency, true) // True for demoMode
        {
            // Load demo file
            demoMode = new DemoMode("Running.qd");
            demoMode.LoadDemoFile();

            currentFrame = 0;
        }

        protected override void RetrieveDataAsync(RTPacket packet) // Ignore packet
        {
            cameras = demoMode.frames[currentFrame];

            Urho.Application.InvokeOnMainAsync(() => 
            {
                // NOTE: There is a chance that the packet does not contain data for the currently selected 
                // camera if that is the case simply catch the exception and log it then keep streaming as usual.
                try
                {                    
                    CameraStore.CurrentCamera.Parent.MarkerData = cameras[CameraStore.CurrentCamera.ID - 1];
                }
                catch (Exception e)
                {
                    Debug.WriteLine("DemoStream:" + e.Message);
                }
            });
            
            SetNextFrame();
        }

        private void SetNextFrame()
        {
            if (currentFrame++ >= demoMode.GetFrameCount() - 1)
                currentFrame = 0;
        }
       
        public void Clean()
        {
            cameras.Clear();
            cameras = null;

            demoMode.Dispose();
            demoMode = null;                        
        }
    }
}
