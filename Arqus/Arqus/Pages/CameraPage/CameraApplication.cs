using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Urho;
using Arqus.Helpers;
using Arqus.Visualization;

using QTMRealTimeSDK;
using QTMRealTimeSDK.Settings;

using Xamarin.Forms;

namespace Arqus
{
    public class CameraApplication : Urho.Application
    {
        private Camera camera;
        private Scene scene;
        private Octree octree;
        private Node meshNode;

        private Carousel carousel;
        private Vector3 cameraOffset;
        private Position cameraMinPosition;

        float meshScale,
              markerSphereScale;

        Vector3 markerSphereScaleVector;
        
        List<Node> cameraScreens;
        int cameraCount;
        bool updateScreens;

        int currentFrame = 0;

        float cameraMovementSpeed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        [Preserve]
        public CameraApplication(ApplicationOptions options) : base(options){ }

        static CameraApplication()
        {
            UnhandledException += (s, e) =>
            {
                if (Debugger.IsAttached)
                {
                    // NOTE: This will always activate when switching mode
                    //Debugger.Break();
                }
                e.Handled = true;
            };
        }
        

        protected override void Start()
        {
            base.Start();
            
            // Setup messaging wíth the view model to retrieve data
            CreateScene();
            SetupViewport();


            // Every time we recieve new data we invoke it on the main thread to update the graphics accordingly
            MessagingCenter.Subscribe<CameraService, List<QTMRealTimeSDK.Data.Camera>>(this, MessageSubject.STREAM_DATA_SUCCESS.ToString(), (sender, cameras) =>
            {
                SetMarkerData(cameras);
            });

            // Every time we recieve new data we invoke it on the main thread to update the graphics accordingly
            MessagingCenter.Subscribe<CameraService, List<ImageSharp.Color[]>>(this, MessageSubject.STREAM_DATA_SUCCESS.ToString(), (sender, imageData) =>
            {
                SetImageData(imageData);
            });

        }

        private void SetMode(int id, CameraMode mode)
        {
            if(screenList.Count > id)
                screenList[id].SetMode(mode);
        }

        private void CreateScene()
        {
            cameraMovementSpeed = 0.001f;
            // Create carousel
            carousel = new Carousel(300, 8, 0, 0);

            // Subscribe to touch event
            Input.SubscribeToTouchMove(OnTouched);
            Input.SubscribeToTouchEnd(OnTouchReleased);


            // Create new scene
            scene = new Scene();

            // Create default octree (-1000:1000)
            octree = scene.CreateComponent<Octree>();

            // Create camera 
            Node cameraNode = scene.CreateChild("camera");
            camera = cameraNode.CreateComponent<Camera>();
            // TODO: Change this to max when that has been implemented
            cameraNode.Position = new Vector3(0, 0, carousel.Min);

            // Create light and attach to camera
            Node lightNode = cameraNode.CreateChild(name: "light");
            Light light = lightNode.CreateComponent<Light>();
            light.LightType = LightType.Point;
            light.Range = 10000;
            light.Brightness = 1.3f;


            // Initialize marker sphere meshes   
            InitializeCameras();
        }


        List<CameraScreen> screenList;
        /// <summary>
        /// Creates the cameras and attaches markersphers to them
        /// </summary>
        private void InitializeCameras()
        {
            screenList = new List<CameraScreen>();
            // Create mesh node that will hold every marker
            meshNode = scene.CreateChild();

            QTMNetworkConnection connection = new QTMNetworkConnection();

            List<ImageCamera> cameras = connection.GetImageSettings();

            foreach (ImageCamera camera in cameras)
            {
                Node screenNode = meshNode.CreateChild();

                // TODO: Handle Camera settings individually
                ImageResolution resolution = new ImageResolution(camera.Width, camera.Height);
                float frameHeight = 10;
                float frameWidth = frameHeight * resolution.PixelAspectRatio;

                // Create and Initialize cameras, order matters here so make sure to attach children AFTER creation
                CameraScreen screen = new CameraScreen(camera.CameraID, resolution, frameHeight, frameWidth, carousel.Min);
                
                screenNode.AddComponent(screen);
                screenList.Add(screen);
            }
            
        }


        // Called every frame
        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
            
            foreach(var screen in screenList)
            {
                Position coordinates = carousel.GetCoordinatesForPosition(screen.position);
                screen.Node.SetWorldPosition(new Vector3((float)coordinates.X, screen.Node.Position.Y, (float)coordinates.Y));
            }
            
            // Update camera offset and reset 
            //UpdateCameraPosition();
            
        }

        void UpdateCameraPosition()
        {
            // Update camera offset and reset 
            camera.Node.Position += (camera.Node.Right * cameraOffset.X) +
                                    (camera.Node.Up * cameraOffset.Y) +
                                    (camera.Node.Direction * cameraOffset.Z);

            cameraOffset = Vector3.Zero;
        }

        private void SetImageData(List<ImageSharp.Color[]> data)
        {
            int count = 0;
        
            foreach (ImageSharp.Color[] image in data)
            {
                if (image == null)
                    break;

                // TODO: Handle video as well
                if (screenList.Count > count && screenList[count].CurrentCameraMode != CameraMode.ModeMarker)
                    screenList[count].ImageData = image;

                count++;
            }
        }

        private bool updatingMarkerData;

        private void SetMarkerData(List<QTMRealTimeSDK.Data.Camera> data)
        {
            int count = 0;

            foreach (QTMRealTimeSDK.Data.Camera camera in data)
            {
                if(screenList.Count > count && screenList[count].CurrentCameraMode == CameraMode.ModeMarker)
                    screenList[count].MarkerData = camera;
                count++;
            }
            
        }


        /// <summary>
        /// Sets up viewport
        /// </summary>
        void SetupViewport()
        {
            Renderer renderer = Renderer;
            renderer.SetViewport(0, new Viewport(Context, scene, camera, null));
        }

        /// <summary>
        /// Called every time a touch is moved
        /// </summary>
        /// <param name="eventArgs"></param>
        void OnTouched(TouchMoveEventArgs eventArgs)
        {
            if (Input.NumTouches == 1)
            {
                carousel.Offset += eventArgs.DX * cameraMovementSpeed;
            }

            if (Input.NumTouches >= 2)
            {
                // Get Touchstates
                TouchState fingerOne = Input.GetTouch(0);
                TouchState fingerTwo = Input.GetTouch(1);
                
                // HACK: Current max is not calculated, this should be fixed to more closesly corelate to
                // what a full screen actually is...
                camera.PinchAndZoom(fingerOne, fingerTwo, carousel.Min, carousel.Min - 30);
            }

        }

        void OnTouchReleased(TouchEndEventArgs eventArgs)
        {
            List<float> distance = screenList.Select((screen) => camera.GetDistance(screen.Node.WorldPosition)).ToList();
            CameraScreen focus = screenList[distance.IndexOf(distance.Min())];
            Debug.WriteLine(focus.CameraID);
            carousel.SetFocus(focus.position);

            MessagingCenter.Send(this, MessageSubject.SET_CAMERA_SELECTION.ToString(), focus.CameraID);
        }
        

        /// <summary>
        /// Returns the distance between two 2D-points
        /// </summary>
        /// <param name="x1">First X value</param>
        /// <param name="x2">Second X value</param>
        /// <param name="y1">First Y value</param>
        /// <param name="y2">Second Y value</param>
        /// <returns></returns>
        private double getDistance2D(float x1, float x2, float y1, float y2)
        {
            float deltaX = Math.Abs(x1 - x2);
            float deltaY = Math.Abs(y1 - y2);
            return Math.Sqrt(Math.Pow(deltaX, 2.0f) * Math.Pow(deltaY, 2));
        }



    }
}
