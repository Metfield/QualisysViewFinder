using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;
using Urho.Shapes;
using Urho.Actions;
using Urho.Gui;
using System;
using Arqus.Visualization;
using Arqus.Camera2D;
using System.Diagnostics;
using Arqus.Components;
using QTMRealTimeSDK.Settings;
using static Arqus.Visualization.Carousel;

namespace Arqus
{
    public class Tracking2DView : Application
    {
        Camera camera;
        Scene scene;
        Octree octree;
        CameraScreen screen;
        Node meshNode;

        Carousel carousel;
        Vector3 cameraOffset;

        float meshScale,
              markerSphereScale;

        Vector3 markerSphereScaleVector;

        List<QTMRealTimeSDK.Data.Camera> streamData;
        List<Node> cameraScreens;
        int cameraCount;
        

        float cameraMovementSpeed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        public Tracking2DView(ApplicationOptions options) : base(options)
        {
            cameraOffset = new Vector3(0, 0, 0);
            meshScale = 0.1f;
            markerSphereScale = 1.0f;
            markerSphereScaleVector = new Vector3(markerSphereScale, markerSphereScale, markerSphereScale);
            cameraMovementSpeed = 0.001f;
        }

        protected override void Start()
        {
            base.Start();

            UpdateStreamData();
            CreateScene();
            SetupViewport();
        }

        private void CreateScene()
        {
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
            cameraNode.Rotate(new Quaternion(0, 0, 0), TransformSpace.Local);

            // Create light and attach to camera
            Node lightNode = cameraNode.CreateChild(name: "light");
            Light light = lightNode.CreateComponent<Light>();
            light.LightType = LightType.Point;
            light.Range = 100;
            light.Brightness = 1.3f;

            // Initialize marker sphere meshes   
            InitializeCameras();               
        }

        /// <summary>
        /// Creates the cameras and attaches markersphers to them
        /// </summary>
        private void InitializeCameras()
        {
            // Create mesh node that will hold every marker
            meshNode = scene.CreateChild();

            QTMNetworkConnection connection = QTMNetworkConnection.Instance;

            List<ImageCamera> cameras = connection.GetImageSettings();

            foreach(ImageCamera camera in cameras)
            {
                Node screenNode = meshNode.CreateChild();

                // Create and Initialize cameras, order matters here so make sure to attach children AFTER creation
                screen = new CameraScreen(camera.CameraID, new ImageResolution(camera.Width, camera.Height));
                screenNode.AddComponent(screen);
            }
            
            // Scale down mesh
           // markerSpheresNode.Scale = new Vector3(meshScale, meshScale, meshScale);
        }

        
        // Called every frame
        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            // Update stream data before rendering
            UpdateStreamData();

            // If by some reason we don't have data yet, return!
            if(streamData == null)
                return;

            // Create a dummy position vector 
            Vector2 tempPosition = Vector2.Zero;

            Node[] cameraScreenNodes = meshNode.GetChildrenWithComponent<CameraScreen>();

            int count = 0;
           foreach(QTMRealTimeSDK.Data.Camera camera in streamData)
            {
                CameraScreen screen = cameraScreenNodes[count].GetComponent<CameraScreen>();

                Position coordinates = carousel.GetCoordinates(screen.position);
                screen.CenterX = coordinates.X;
                screen.CenterY = coordinates.Y;

                // Update marker positions
                for (int i = 0; i < camera.MarkerCount; i++)
                {
                    screen.Pool.Get(i).MarkerData = camera.MarkerData2D[i];
                }

                count++;
            }

            // Update camera offset and reset 
            camera.Node.Position += cameraOffset;
            cameraOffset = Vector3.Zero;     
        }

        /// <summary>
        /// Fetches new stream data and updates the local structure
        /// </summary>
        private void UpdateStreamData()
        {
            streamData = CameraStream.Instance.GetStreamMarkerData();
            cameraCount = streamData.Count;

            // TODO: Handle markerCount change
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
                Offset += eventArgs.DX * cameraMovementSpeed;
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
