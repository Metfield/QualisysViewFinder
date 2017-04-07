using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;
using Urho.Shapes;
using Urho.Actions;
using Urho.Gui;
using System;
using Arqus.Visualization;
using Arqus.Helpers;
using System.Diagnostics;
using Arqus.Components;
using QTMRealTimeSDK.Settings;
using Xamarin.Forms;
using QTMRealTimeSDK;
using Arqus.Services;

namespace Arqus
{
    public class CameraApplication : Urho.Application
    {
        Camera camera;
        Scene scene;
        Octree octree;
        Node meshNode;

        Carousel carousel;
        Vector3 cameraOffset;

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
                    Debugger.Break();
                e.Handled = true;
            };
        }
        

        protected override void Start()
        {
            base.Start();
            
            // Setup messaging wíth the view model to retrieve data

            // TODO: Do we need to handle this when multiple modes are avaible?
           /* MessagingCenter.Subscribe<CameraPageViewModel, CameraMode>(this, MessageSubject.STREAM_MODE_CHANGED.ToString(), (sender, mode) =>
            {
                SetMode(0, mode);
            });
            */

            // Every time we recieve new data we invoke it on the main thread to update the graphics accordingly
            MessagingCenter.Subscribe<CameraPageViewModel, List<ImageSharp.Color[]>>(this, MessageSubject.STREAM_DATA_SUCCESS.ToString(), (sender, imageData) =>
            {
                SetImageData(imageData);
            });

            // Every time we recieve new data we invoke it on the main thread to update the graphics accordingly
            MessagingCenter.Subscribe<CameraPageViewModel, List<QTMRealTimeSDK.Data.Camera>>(this, MessageSubject.STREAM_DATA_SUCCESS.ToString(), (sender, cameras) =>
            {
                SetMarkerData(cameras);
            });

            // Make sure that the stream update loop runs in its own thread to keep interactions responsive
            Task.Run(() => UpdateDataTask(1, () => InvokeOnMain(() => MessagingCenter.Send(this, MessageSubject.FETCH_IMAGE_DATA.ToString()) ) ) );
            Task.Run(() => UpdateDataTask(30, () => InvokeOnMain(() => MessagingCenter.Send(this, MessageSubject.FETCH_MARKER_DATA.ToString()) ) ) );

            CreateScene();
            SetupViewport();
            
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
            carousel = new Carousel(500, 8, 0, 0);

            // Subscribe to touch event
            Input.SubscribeToTouchMove(OnTouched);

            // Create new scene
            scene = new Scene();

            // Create default octree (-1000:1000)
            octree = scene.CreateComponent<Octree>();

            // Create camera 
            Node cameraNode = scene.CreateChild("camera");
            camera = cameraNode.CreateComponent<Camera>();
            cameraNode.Position = new Vector3(0, 0, -10);

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
                CameraScreen screen = new CameraScreen(camera.CameraID, resolution, frameHeight, frameWidth, Urho.Color.Cyan);

                screenNode.AddComponent(screen);
                screenList.Add(screen);
            }
            
        }


        // Called every frame
        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            // Create a dummy position vector 
            //Vector2 tempPosition = Vector2.Zero;
            // Update camera offset and reset 
            camera.Node.Position += cameraOffset;
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
                if (screenList.Count > count && screenList[count].CurrentCameraMode == CameraMode.ModeMarkerIntensity)
                    screenList[count].ImageData = image;
                count++;
            }
        }

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
        /// Fetches new stream data and updates the local structure
        /// </summary>
        public async void UpdateDataTask(int frequency, Action onUpdate)
        {
            while (!Engine.Exiting && !IsDeleted)
            {
                DateTime time = DateTime.UtcNow;
                onUpdate();
                
                if (frequency > 0)
                {
                    var deltaTime = (DateTime.UtcNow - time).TotalMilliseconds;
                    var timeToWait = 1000d / frequency - deltaTime;

                    if (timeToWait >= 0)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(timeToWait));
                    }
                }
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
                //cameraOffset.X = -eventArgs.DX * cameraMovementSpeed;
                //cameraOffset.Y = eventArgs.DY * cameraMovementSpeed;
                carousel.Offset += eventArgs.DX * cameraMovementSpeed;
            }
            else if (Input.NumTouches >= 2)
            {
                var precision = 0.1;
                var touch1 = Input.GetTouch(0);
                var touch2 = Input.GetTouch(1);

                double oldDistance = getDistance2D(touch1.LastPosition.X, touch2.LastPosition.X, touch1.LastPosition.Y, touch2.LastPosition.Y);
                double newDistance = getDistance2D(touch1.Position.X, touch2.Position.X, touch1.Position.Y, touch2.Position.Y);
                double deltaDistance = oldDistance - newDistance;

                if (Math.Abs(deltaDistance) > precision)
                {
                    float scale = (float)(newDistance / oldDistance);
                    float zoom = (newDistance > oldDistance) ? scale : -scale;

                    camera.Node.SetWorldPosition(new Vector3(camera.Node.Position.X, camera.Node.Position.Y, camera.Node.Position.Z + zoom));
                }
            }
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
