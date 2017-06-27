﻿using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Arqus.Service;
using Arqus.Visualization;

using Xamarin.Forms;

using Urho;
using Arqus.Services;

namespace Arqus
{
    /// <summary>
    /// The camera application displays a set of cameras inside a 3D view
    /// </summary>
    public class CameraApplication : Urho.Application
    {
        private Scene scene;
        private Octree octree;
        private Node meshNode;

        private Camera camera;
        private Vector3 cameraOffset;
        private CameraStreamService cameraStreamService = null;

        public DeviceOrientations Orientation { get; set; }

        public enum ScreenLayoutType
        {
            Grid,
            Carousel
        }

        private Dictionary<ScreenLayoutType, CameraScreenLayout> screenLayout;
        private CameraScreenLayout currentScreenLayout;

        private float carouselInitialDistance;

        private float meshScale;
        private float markerSphereScale;
        
        Vector3 markerSphereScaleVector;        

        List<Node> cameraScreens;

        int cameraCount;
        bool updateScreens;

        int currentFrame = 0;
        float cameraMovementSpeed;

        bool demoModeActive;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        [Preserve]
        public CameraApplication(ApplicationOptions options) : base(options)
        {
            // Listen to start stream event (from CameraPageviewModel)
            MessagingService.Subscribe<CameraPageViewModel, bool>(this, MessageSubject.STREAM_START, (sender, demoMode) => StartStream(demoMode));
        }
      
        static CameraApplication()
        {
            UnhandledException += (s, e) =>
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                e.Handled = true;
            };
        }

        
        protected override void Start()
        {
            base.Start();
            
            CreateScene();
            SetupViewport();
            
            // Update the application when a new screen layout is set in the view model
            MessagingService.Subscribe<CameraPageViewModel, ScreenLayoutType>
            (
                this, 
                MessageSubject.SET_CAMERA_SCREEN_LAYOUT, 
                (sender, type) => currentScreenLayout = screenLayout[type]
            );
        }

        // Starts streaming based on input
        private void StartStream(bool demoMode = false)
        {
            demoModeActive = demoMode;

            cameraStreamService = new CameraStreamService(demoMode);
            cameraStreamService.Start();
        }            

        protected override void OnDeleted()
        {
            cameraStreamService.Dispose();
            cameraStreamService = null;

            // Update the application when a new screen layout is set in the view model
            MessagingCenter.Unsubscribe<CameraPageViewModel, string>(this, MessageSubject.SET_CAMERA_SCREEN_LAYOUT);
            MessagingCenter.Unsubscribe<CameraPageViewModel, bool>(this, MessageSubject.STREAM_START);
            
            base.OnDeleted();
        }
        
        private async void CreateScene()
        {

            // Subscribe to touch event
            Input.TouchMove += OnTouched;
            Input.TouchBegin += OnTouchBegan;
            Input.TouchEnd += OnTouchReleased;
            
            // Create new scene
            scene = new Scene();
            scene.Clear(true, true);

            // Create default octree (-1000:1000)
            octree = scene.CreateComponent<Octree>();
            
            // Create camera 
            Node cameraNode = scene.CreateChild("camera");
            camera = cameraNode.CreateComponent<Camera>();
            
            // Arbitrary far clipping plane
            camera.FarClip = 50.0f;
            
            // Reposition it..
            cameraNode.Position = new Vector3(0, 0, 0);
            
            // Create light and attach to camera
            Node lightNode = cameraNode.CreateChild(name: "light");
            Light light = lightNode.CreateComponent<Light>();
            light.LightType = LightType.Point;
            light.Range = 10000;
            light.Brightness = 1.3f;

            // Initialize marker sphere meshes   
            InitializeCameras(cameraNode);

            // Set the default layout
            screenLayout = new Dictionary<ScreenLayoutType, CameraScreenLayout>()
            {
                { ScreenLayoutType.Grid,  new GridScreenLayout(screenList.Count, 2, camera)},
                { ScreenLayoutType.Carousel, new CarouselScreenLayout(screenList.Count, camera)}
            };

            currentScreenLayout = screenLayout[ScreenLayoutType.Carousel];
            currentScreenLayout.Select(CameraStore.CurrentCamera.ID);
        }

        
        List<CameraScreen> screenList;
        /// <summary>
        /// Creates the cameras and attaches markersphers to them
        /// </summary>
        private void InitializeCameras(Node cameraNode)
        {
            // Create mesh node that will hold every marker
            meshNode = scene.CreateChild();
            screenList = CameraStore.GenerateCameraScreens(cameraNode);
            
            foreach (CameraScreen screen in screenList)
            {
                Node screenNode = meshNode.CreateChild();
                // Create and Initialize cameras, order matters here so make sure to attach children AFTER creation
                screen.Scale = 10;
                screenNode.AddComponent(screen);
            }            
        }

        
        int tapTouchID;
        float tapTimeStamp;
        
        // TODO: Get paper or something to justify this time
        float tapTimeMargin = 0.1f;

        public void OnTouchBegan(TouchBeginEventArgs eventArgs)
        {
            // Fill out variables for tap gesture
            tapTouchID = eventArgs.TouchID;
            tapTimeStamp = Time.ElapsedTime;
            currentScreenLayout.OnTouchBegan(eventArgs);
        }
        

        // Called every frame
        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
            
            // Update camera offset and reset 
            //UpdateCameraPosition();
            
            for (int i = 0; i < screenList.Count; i++)
            {
                if (i < currentScreenLayout.Selection + 1|| i > currentScreenLayout.Selection  - 1)
                {
                   screenList[i].Enabled = true;
                   currentScreenLayout.SetCameraScreenPosition(screenList[i], Orientation);
                }
                else
                {
                   screenList[i].Enabled = false;
                }
            }
            
        }

        /// <summary>
        /// Sets up viewport
        /// </summary>
        void SetupViewport()
        {
            Renderer renderer = Renderer;
            Viewport viewport = new Viewport(Context, scene, camera, null);
            viewport.SetClearColor(Urho.Color.FromHex("#303030"));

            renderer.SetViewport(0, viewport);
        }        

        /// <summary>
        /// Casts camera ray from the viewport's coordinates
        /// </summary>
        /// <param name="x">Touch X Coordinate - raw (not normalized)</param>
        /// <param name="y">Touch Y Coordinate - raw (not normalized)</param>
        void CastTouchRay(int x, int y)
        {
            // Shoot from camera to grid view
            float rayDistance = camera.FarClip;
            
            // Create ray with normalized screen coordinates
            Ray camRay = camera.GetScreenRay((float)x / Graphics.Width, (float)y / Graphics.Height);

            // Cast the ray looking for 3D geometry (our grid screen planes) and store result in variable
            RayQueryResult? rayResult = octree.RaycastSingle(camRay, RayQueryLevel.Triangle, rayDistance, DrawableFlags.Geometry, uint.MaxValue);
            
            // Check if there was a hit
            if (rayResult.HasValue)
            {
                // Get node
                Node screenNode = rayResult.Value.Node;
                
                // Check if it's a screen node
                // TODO: Add a public dictionary or query class for name?
                if (screenNode.Name == "backdrop")
                {

                    // Get selected camera ID 
                    int cameraID = screenNode.Parent.GetComponent<Visualization.CameraScreen>().Camera.ID;                    
                    
                    camera.FarClip = 150.0f;
                    currentScreenLayout = screenLayout[ScreenLayoutType.Carousel];
                    currentScreenLayout.Select(cameraID);

                    MessagingService.Send(this, MessageSubject.SET_CAMERA_SELECTION, currentScreenLayout.Selection, payload: new { });
                }
            }
        }

        /// <summary>
        /// Called every time a touch is moved
        /// </summary>
        /// <param name="eventArgs"></param>
        void OnTouched(TouchMoveEventArgs eventArgs)
        {
            currentScreenLayout.OnTouch(Input, eventArgs);
        }

        void OnTouchReleased(TouchEndEventArgs eventArgs)
        {
            currentScreenLayout.OnTouchReleased(Input, eventArgs);

            // Return if this is not our original tap finger
            if (eventArgs.TouchID != tapTouchID)
                return;
            
            // Get delta time
            float dt = Time.ElapsedTime - tapTimeStamp;
            
            // If it is lesser than our tap margin, it is a tap
            if (dt < tapTimeMargin && currentScreenLayout.GetType() == typeof(GridScreenLayout))
            {
                CastTouchRay(eventArgs.X, eventArgs.Y);
            }
            else if(currentScreenLayout.GetType() == typeof(CarouselScreenLayout))
            {
                //List<float> distance = screenList.Select((screen) => camera.GetDistance(screen.Node.WorldPosition)).ToList();
                //CameraScreen focus = screenList[distance.IndexOf(distance.Min())];
                //currentScreenLayout.Select(focus.position);
            }
        }    
    }
}
