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
using Urho.Actions;

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

        private float carouselInitialDistance;

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
        public CameraApplication(ApplicationOptions options) : base(options){}

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
        }

        

        private bool updatingMarkerData;

        private void SetMarkerData(List<QTMRealTimeSDK.Data.Camera> data)
        {
            int count = 0;

            foreach (QTMRealTimeSDK.Data.Camera camera in data)
            {
                if (screenList.Count > count && !screenList[count].IsImageMode)
                    screenList[count].MarkerData = camera;
                count++;
            }
        }


        private async void CreateScene()
        {
            cameraMovementSpeed = 0.001f;
            // Create carousel
            carouselInitialDistance = -70;
            carousel = new Carousel(300, 8, 0, 0);            

            // Subscribe to touch event
            Input.SubscribeToTouchMove(OnTouched);
            Input.SubscribeToTouchEnd(OnTouchReleased);
            
            // Create new scene
            scene = new Scene();
            scene.Clear(true, true);

            // Create default octree (-1000:1000)
            octree = scene.CreateComponent<Octree>();

            // Create camera 
            Node cameraNode = scene.CreateChild("camera");
            camera = cameraNode.CreateComponent<Camera>();

            // Arbitrary far clipping plane
            camera.FarClip = 30.0f;
            
            // Reposition it..
            cameraNode.Position = new Vector3(0, 0, carouselInitialDistance);

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
            // Create mesh node that will hold every marker
            meshNode = scene.CreateChild();
            screenList = CameraStore.GenerateCameraScreens();
           
            foreach (CameraScreen screen in screenList)
            {
                Node screenNode = meshNode.CreateChild();
                // Create and Initialize cameras, order matters here so make sure to attach children AFTER creation
                screen.Scale = 10;
                screenNode.AddComponent(screen);

                if(screen.CameraID == CameraStore.State.ID)
                    carousel.SetFocus(screen.position);
            }

        }


        // Called every frame
        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            // Update camera offset and reset 
            //UpdateCameraPosition();
            
            foreach (var screen in screenList)
            {
                Position coordinates = carousel.GetCoordinatesForPosition(screen.position);
                screen.Node.SetWorldPosition(new Vector3((float)coordinates.X, screen.Node.Position.Y, (float)coordinates.Y));
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
                // Check if we are panning or just scrolling the carousel 
                // based on current zoom
                if (camera.Node.Position.Z == carouselInitialDistance)
                {
                    // We want to scroll 
                    carousel.Offset += eventArgs.DX * cameraMovementSpeed;
                }                
                else
                {
                    // We want to Pan
                    camera.Pan(eventArgs.DX, eventArgs.DY, cameraMovementSpeed * 5, carouselInitialDistance);
                }
            }

            if (Input.NumTouches >= 2)
            {
                // Get Touchstates
                TouchState fingerOne = Input.GetTouch(0);
                TouchState fingerTwo = Input.GetTouch(1);
                
                // HACK: Current max is not calculated, this should be fixed to more closesly corelate to
                // what a full screen actually is...
                camera.PinchAndZoom(fingerOne, fingerTwo, carouselInitialDistance, carouselInitialDistance + 20);
            }
        }

        void OnTouchReleased(TouchEndEventArgs eventArgs)
        {
            List<float> distance = screenList.Select((screen) => camera.GetDistance(screen.Node.WorldPosition)).ToList();
            CameraScreen focus = screenList[distance.IndexOf(distance.Min())];
            Debug.WriteLine(focus.CameraID);
            carousel.SetFocus(focus.position);

            // Make an ease in during snapping
            /*foreach (var screen in screenList)
            {
                Position coordinates = carousel.GetCoordinatesForPosition(screen.position);
                screen.Node.RunActionsAsync(new EaseElasticIn( new MoveTo( 1000, new Vector3((float)coordinates.X, screen.Node.Position.Y, (float)coordinates.Y))));
            }
            */

            MessagingCenter.Send(this, MessageSubject.SET_CAMERA_SELECTION.ToString(), focus.CameraID);
        }    

    }
}
