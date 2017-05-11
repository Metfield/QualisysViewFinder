using System;

using System.Linq;

using System.Diagnostics;

using System.Threading.Tasks;

using System.Collections.Generic;



using Urho;

using Arqus.Visualization;

using Xamarin.Forms;

using Urho.Actions;

using Arqus.Helpers;

using Arqus.Service;



namespace Arqus

{

    public class CameraApplication : Urho.Application

    {

        private CameraStreamService cameraStreamService;

        private Camera camera;

        private Scene scene;

        private Octree octree;

        private Node meshNode;



        private Grid grid;

        private Carousel carousel;

        private CameraScreenLayout cameraScreenLayout;



        private Vector3 cameraOffset;

        

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

                    Debugger.Break();

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



            MessagingService.Subscribe<CameraPageViewModel>(this, MessageSubject.SET_CAMERA_SCREEN_LAYOUT, (sender) =>

            {

                if (cameraScreenLayout.Equals(carousel))

                {

                    camera.FarClip = 100.0f;

                    cameraScreenLayout = grid;

                }

                else

                {

                    camera.FarClip = 50.0f;

                    cameraScreenLayout = carousel;

                }

            });

            cameraStreamService = new CameraStreamService();
            cameraStreamService.Start();

        }

        protected override void OnDeleted()
        {
            cameraStreamService.Dispose();
            base.OnDeleted();
        }





        private bool updatingMarkerData;



        private void SetMarkerData(List<QTMRealTimeSDK.Data.Camera> data)

        {

            int count = 0;



            foreach (QTMRealTimeSDK.Data.Camera camera in data)

            {

                if (screenList.Count > count && !screenList[count].IsImageMode())

                    screenList[count].MarkerData = camera;

                count++;

            }

        }





        private async void CreateScene()

        {
            // Create carousel

            carouselInitialDistance = -80;

            // TODO: Fix number of camerascreens



            cameraMovementSpeed = 0.005f;      



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

            cameraNode.Position = new Vector3(0, 0, carouselInitialDistance);



            // Create light and attach to camera

            Node lightNode = cameraNode.CreateChild(name: "light");

            Light light = lightNode.CreateComponent<Light>();

            light.LightType = LightType.Point;

            light.Range = 10000;

            light.Brightness = 1.3f;



            // Initialize marker sphere meshes   

            InitializeCameras(cameraNode);



            grid = new Grid(screenList.Count, 2, camera);

            carousel = new Carousel(screenList.Count, camera);

            

            cameraScreenLayout = carousel;

            cameraScreenLayout.Select(CameraStore.CurrentCamera.ID);

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

        }

        



        // Called every frame

        protected override void OnUpdate(float timeStep)

        {

            base.OnUpdate(timeStep);



            // Update camera offset and reset 

            //UpdateCameraPosition();

            

            foreach (var screen in screenList)

            {

                cameraScreenLayout.SetCameraScreenPosition(screen);

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
                if (screenNode.Name == "screenNode")
                {

                    // Get selected camera ID 
                    int cameraID = screenNode.Parent.GetComponent<Visualization.CameraScreen>().Camera.ID;                    
                    
                    camera.FarClip = 50.0f;
                    cameraScreenLayout = carousel;
                    cameraScreenLayout.Select(cameraID);

                    MessagingService.Send(this, MessageSubject.SET_CAMERA_SELECTION, cameraScreenLayout.Selection, payload: new { });
                }

            }

        }



        /// <summary>

        /// Called every time a touch is moved

        /// </summary>

        /// <param name="eventArgs"></param>

        void OnTouched(TouchMoveEventArgs eventArgs)
        {

            if (Input.NumTouches == 1 && cameraScreenLayout != grid)
            {

                // Check if we are panning or just scrolling the carousel 

                // based on current zoom

                if (camera.Node.Position.Z == carouselInitialDistance)

                {

                    // We want to scroll 

                    cameraScreenLayout.Offset += eventArgs.DX * cameraMovementSpeed;

                }      
                else
                {
                    // We want to Pan
                    camera.Pan(eventArgs.DX, eventArgs.DY, cameraMovementSpeed * 5, carouselInitialDistance);

                }

            }



            if (Input.NumTouches >= 2 && cameraScreenLayout != grid)

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




            // Return if this is not our original tap finger

            if (eventArgs.TouchID != tapTouchID)

                return;



            // Get delta time

            float dt = Time.ElapsedTime - tapTimeStamp;



            // If it is lesser than our tap margin, it is a tap

            if (dt < tapTimeMargin && cameraScreenLayout == grid)
            {
                CastTouchRay(eventArgs.X, eventArgs.Y);
            }
            else if(cameraScreenLayout == carousel)
            {
                List<float> distance = screenList.Select((screen) => camera.GetDistance(screen.Node.WorldPosition)).ToList();
                CameraScreen focus = screenList[distance.IndexOf(distance.Min())];
                cameraScreenLayout.Select(focus.position);
                MessagingService.Send(this, MessageSubject.SET_CAMERA_SELECTION, cameraScreenLayout.Selection, payload: new { });
            }

        }    

    }

}

