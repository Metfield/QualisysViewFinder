using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;
using Urho.Shapes;
using Urho.Actions;
using Urho.Gui;
using System;
using Arqus.Visualization;
using System.Diagnostics;
using Arqus.Components;

namespace Arqus
{
    public class Tracking2DView : Application
    {
        Camera camera;
        Scene scene;
        Octree octree;
        Node meshNode;

        Vector3 cameraOffset;

        float meshScale,
              markerSphereScale;

        Vector3 markerSphereScaleVector;

        List<QTMRealTimeSDK.Data.Camera> streamData;
        int cameraCount;
        MarkerSpherePool markerSpheres;

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
            cameraMovementSpeed = 0.01f;
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
            cameraNode.Rotation = new Quaternion(0f, 0f, 1f, 0f);

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

            // Initialize list with capacity for the total number of markers 
            markerSpheres = new MarkerSpherePool((int)streamData[0].MarkerCount, meshNode);

            // Create and Initialize cameras
            var frame = meshNode.CreateComponent<CameraScreen>();
            frame.Init(0);

            // Scale down mesh
            meshNode.Scale = new Vector3(meshScale, meshScale, meshScale);

            // Rotate it
            meshNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
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

            
            QTMRealTimeSDK.Data.Camera qCamera = streamData[0];
            QTMRealTimeSDK.Data.Q2D markerData;
            
            for (int i = 0; i < qCamera.MarkerCount; i++)
            {
                markerData = qCamera.MarkerData2D[i];

                tempPosition.X = markerData.X * 0.001f;
                tempPosition.Y = markerData.Y * 0.001f;

               markerSpheres.Get(i).Set2DPosition(tempPosition);
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
                cameraOffset.X = eventArgs.DX * cameraMovementSpeed;
                cameraOffset.Y = -eventArgs.DY * cameraMovementSpeed;
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
