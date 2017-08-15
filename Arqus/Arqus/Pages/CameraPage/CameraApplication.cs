using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Urho;
using Xamarin.Forms;

using Arqus.Services;
using Arqus.Visualization;

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
        bool gridImageStreamEnabled = false;

        bool streamHasStarted = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        [Preserve]
        public CameraApplication(ApplicationOptions options) : base(options)
        {
            // Listen to start stream event (from CameraPageviewModel)
            MessagingCenter.Subscribe<CameraPageViewModel, bool>(this, Messages.Subject.STREAM_START, (sender, demoMode) => StartStream(demoMode));
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
            MessagingCenter.Subscribe<CameraPageViewModel, ScreenLayoutType>(this, Messages.Subject.SET_CAMERA_SCREEN_LAYOUT, (sender, type) =>
            {
                // Switch to new screen layout
                InvokeOnMainAsync(() => SwitchScreenLayout(type));
            });

            // Subscribe to touch event
            Input.TouchMove += OnTouched;
            Input.TouchBegin += OnTouchBegan;
            Input.TouchEnd += OnTouchReleased;
            Input.KeyDown += OnKeyDown;
        }

        // Starts streaming based on input
        private void StartStream(bool demoMode = false)
        {
            demoModeActive = demoMode;

            cameraStreamService = new CameraStreamService(demoMode);
            cameraStreamService.Start();

            streamHasStarted = true;
        }

        private async void CreateScene()
        {
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

            List<DataModels.Camera> cameras = CameraManager.GetCameras();

            // Set the default layout
            screenLayout = new Dictionary<ScreenLayoutType, CameraScreenLayout>()
            {
                { ScreenLayoutType.Grid,  new GridScreenLayout(cameras.Count, 2, camera)},
                { ScreenLayoutType.Carousel, new CarouselScreenLayout(cameras.Count, camera)}
            };

            SwitchScreenLayout(ScreenLayoutType.Carousel);
            currentScreenLayout.Select(CameraManager.CurrentCamera.ID);
        }
        
        /// <summary>
        /// Creates the cameras and attaches markersphers to them
        /// </summary>
        private void InitializeCameras(Node cameraNode)
        {
            // Create mesh node that will hold every marker
            meshNode = scene.CreateChild();
            CameraManager.GenerateCameraScreens(cameraNode);
            
            foreach(DataModels.Camera camera in CameraManager.GetCameras())
            {
                Node screenNode = meshNode.CreateChild();
                // Create and Initialize cameras, order matters here so make sure to attach children AFTER creation
                camera.Screen.Scale = 10;
                screenNode.AddComponent(camera.Screen);
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

            List<DataModels.Camera> cameras = CameraManager.GetCameras();

            for (int i = 0; i < cameras.Count; i++)
            {
                if (i < currentScreenLayout.Selection + 1|| i > currentScreenLayout.Selection  - 1)
                {
                    cameras[i].Screen.Enabled = true;
                    currentScreenLayout.SetCameraScreenPosition(cameras[i].Screen, Orientation);
                }
                else
                {
                    cameras[i].Screen.Enabled = false;
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

        // Handles camera info depending on view mode
        private void ToggleCameraUIInfo(ScreenLayoutType screenLayoutType)
        {
            List<DataModels.Camera> cameras = CameraManager.GetCameras();

            // Iterate over screens and toggle the info
            foreach (DataModels.Camera camera in cameras)
            {
                camera.Screen.ToggleUIInfo(screenLayoutType);
            }
        }

        // Switches screen layout from carousel to grid and viceversa
        private void SwitchScreenLayout(ScreenLayoutType layoutType)
        {
            currentScreenLayout = screenLayout[layoutType];
            ToggleCameraUIInfo(layoutType);

            // Enable screen frames if going into grid mode
            // --------------------------------------------
            // Enable grid mode image streaming (streams all cameras at once) if going to grid mode,
            // otherwise turn it off
            if (layoutType == ScreenLayoutType.Grid)
            {
                ToggleCameraScreenFrame(true);
                EnableGridModeImageStreaming();
            }
            else
            {
                ToggleCameraScreenFrame(false);

                if (cameraStreamService == null)
                    return;
                
                DisableGridModeImageStreaming();
            }
        }

        // Asynchronous task; runs for as long as grid layout is selected
        // Streams every image-enabled camera in the system at a lower resolution
        private async void UpdateGridCameras()
        {
            // Update while this mode is enabled
            while (gridImageStreamEnabled)
            {
                if (currentScreenLayout.GetType() == typeof(GridScreenLayout))
                {
                    cameraStreamService.UpdateGridCameras();
                }
            }
        }

        // Sets everything up for grid mode every-camera-streaming
        private void EnableGridModeImageStreaming()
        {
            // Don't try to stream if demo mode is active or if cameras
            // are not yet streaming
            if (demoModeActive || !streamHasStarted)
                return;

            // Deselect a camera when going into grid mode
            // Technically all are selected/not-selected
            CameraManager.CurrentCamera.Deselect();

            cameraStreamService.StartGridCamerasUpdate();
            EnableImageStreamOnAllCameras();

            gridImageStreamEnabled = true;

            // Create asynchronous task that will run for as long as grid mode is active
            Task.Factory.StartNew(() => UpdateGridCameras(), TaskCreationOptions.LongRunning);
        }

        // Disable grid mode streaming
        private void DisableGridModeImageStreaming()
        {
            // Don't try to stream if demo mode is active or if cameras
            // are not yet streaming
            if (demoModeActive || !streamHasStarted)
                return;

            gridImageStreamEnabled = false;

            cameraStreamService.StopGridCamerasUpdate();
            DisableImageStreamOnAllCameras();
        }

        // Enables image streaming for every camera in the system
        private void EnableImageStreamOnAllCameras()
        {
            for (int i = 0; i < CameraManager.Cameras.Count; i++)
            {
                CameraManager.Cameras[i + 1].EnableImageMode(true);
            }
        }

        // Disables image streaming for every camera
        private void DisableImageStreamOnAllCameras()
        {
            for (int i = 0; i < CameraManager.Cameras.Count; i++)
            {
                CameraManager.Cameras[i + 1].DisableImageMode();
            }
        }

        // Turns on/off the frame around a camera screen in the 3D scene
        private void ToggleCameraScreenFrame(bool flag)
        {
            List<DataModels.Camera> cameras = CameraManager.GetCameras();

            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].Screen.Node.ResetDeepEnabled();
                cameras[i].Screen.ToggleFrame(flag);

            }
        }

        /// <summary>
        /// Casts camera ray from the viewport's coordinates
        /// </summary>
        /// <param name="x">Touch X Coordinate - raw (not normalized)</param>
        /// <param name="y">Touch Y Coordinate - raw (not normalized)</param>
        void CastTouchRay(int x, int y)
        {
            // Shoot from camera to grid view
            float rayDistance = camera.FarClip + 1;
            
            // Create ray with normalized screen coordinates
            Ray camRay = camera.GetScreenRay((float)x / Graphics.Width, (float)y / Graphics.Height);

            // Cast the ray looking for 3D geometry (our grid screen planes) and store result in variable
            RayQueryResult? rayResult = octree.RaycastSingle(camRay);
            
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

                    // Reset the camera position when going to the carousel mode
                    if (camera.Node.Position.Y != 0 || camera.Node.Position.X != 0)
                        Urho.Application.InvokeOnMain(() => camera.Node.SetPosition2D(0, 0));

                    SwitchScreenLayout(ScreenLayoutType.Carousel);
                    currentScreenLayout.Select(cameraID);

                    MessagingCenter.Send(this, Messages.Subject.SET_CAMERA_SELECTION, currentScreenLayout.Selection);
                }
            }
        }

        // Used to catch Android's back button
        private void OnKeyDown(KeyDownEventArgs args)
        {
            switch(args.Key)
            {
                case Key.Esc:
                    MessagingCenter.Send(this, Messages.Subject.URHO_ANDROID_BACK_BUTTON_PRESSED);
                    break;

                default:
                    // ¯\_(ツ)_/¯ 
                    break;
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
            
            // If it is lesser than our tap margin, it is a tap!
            if (dt < tapTimeMargin)
            {
                // Notify CameraPageViewModel of a tap. This is used to hide
                // the settings drawer
                MessagingCenter.Send(this, Messages.Subject.URHO_SURFACE_TAPPED);

                // Are we in grid mode? Test for camera selection
                if (currentScreenLayout.GetType() == typeof(GridScreenLayout))
                    CastTouchRay(eventArgs.X, eventArgs.Y);
            }
        }

        // Called before dispose
        protected override void Stop()
        {
            DisableImageStreamOnAllCameras();

            gridImageStreamEnabled = false;
            streamHasStarted = false;

            CameraManager.CurrentCamera.Deselect();

            base.Stop();
        }

        protected override void Dispose(bool disposing)
        {
            cameraStreamService.Dispose();
            cameraStreamService = null;

            // Update the application when a new screen layout is set in the view model
            MessagingCenter.Unsubscribe<CameraPageViewModel, string>(this, Messages.Subject.SET_CAMERA_SCREEN_LAYOUT);
            MessagingCenter.Unsubscribe<CameraPageViewModel, bool>(this, Messages.Subject.STREAM_START);

            base.Dispose(disposing);
        }
    }
}

