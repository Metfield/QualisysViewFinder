using Arqus.Helpers;
using Arqus.Visualization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus
{
    class CameraStore
    {
        
        private static CameraStore instance = null;
        private static readonly object padlock = new object();

        CameraStore()
        {
            Init();
        }

        public static CameraStore Instance
        {
            get
            {
                lock(padlock)
                {
                    if(instance == null)
                    {
                        instance = new CameraStore();
                    }
                    return instance;
                }
            }
        }
        
        private int currentID;
        public Dictionary<int, CameraScreen> Screens { get; private set; }
        QTMNetworkConnection connection = new QTMNetworkConnection();


        void Init()
        {
            Screens = new Dictionary<int, CameraScreen>();
            var cameras = connection.GetImageSettings();

            foreach(var camera in cameras)
            {
                // Create resolution object and calculate frame size
                // TODO: Discuss whether we want different sizes for screens..                    
                ImageResolution imageResolution = new ImageResolution(camera.Width, camera.Height);
                
                // Create screen component and node. Add it to parent node (scene)
                // TODO: handle 
                CameraScreen screen = new CameraScreen(camera.CameraID, imageResolution);
                Screens.Add(camera.CameraID, screen);
            }
        }

        public CameraScreen GetCurrentCamera()
        {
            return Screens[currentID];
        }


    }
}
