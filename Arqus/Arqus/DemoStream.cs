using System;
using System.Collections.Generic;
using System.Text;
using QTMRealTimeSDK.Data;
using Arqus.Service;
using System.Diagnostics;
using Arqus.Visualization;

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

        CameraScreen cameraScreen;

        protected override void RetrieveDataAsync() // Ignore packet
        {
            cameras = demoMode.frames[currentFrame];
            int id;

            for (int i = 0; i < cameras.Count; i++)
            {
                id = i + 1;

                try
                {
                    CameraStore.Cameras[id].Screen.MarkerData = cameras[i];
                }
                catch (Exception e)
                {
                    Debug.WriteLine("DemoStream:" + e.Message);
                }
            }

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
