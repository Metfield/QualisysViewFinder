using System;
using System.Collections.Generic;
using System.Text;
using QTMRealTimeSDK.Data;
using Arqus.Service;

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

            if (cameras != null)
            {
                uint id = 1;
                foreach (var camera in cameras)
                {
                    if (camera.MarkerData2D.Length > 0)
                    {
                        MessagingService.Send(this, MessageSubject.STREAM_DATA_SUCCESS + id, camera, track: false);
                    }

                    id++;
                }
            }

            SetNextFrame();
        }

        private void SetNextFrame()
        {
            if (currentFrame++ >= demoMode.GetFrameCount() - 1)
                currentFrame = 0;
        }
       
        public new void Dispose()
        {
            cameras.Clear();
            cameras = null;

            demoMode.Dispose();
            demoMode = null;

            base.Dispose();
        }
    }
}
