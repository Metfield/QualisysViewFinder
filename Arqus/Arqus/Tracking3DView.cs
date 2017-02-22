using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;
using Urho.Shapes;
using Urho.Actions;
using Urho.Gui;
using System;

namespace Arqus
{
    public class Tracking3DView : Application
    {
        Camera camera;
        Scene scene;
        Octree octree;
        Node meshNode;
        
        float meshScale,
              markerSphereScale;

        Vector3 markerSphereScaleVector;

        List<QTMRealTimeSDK.Data.Q3D> streamData;
        int markerCount;
        List<Sphere> markerSpheres;

        // Camera-related members
        Vector3 cameraPositionOffset;
        float cameraMovementSpeed,
              cameraRotationSpeed;

        float pinchZoom;
        float pinchPrecision;
        float zoomFactor;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        public Tracking3DView(ApplicationOptions options) : base(options)
        {
            cameraPositionOffset = Vector3.Zero;
            meshScale = 0.1f;
            markerSphereScale = 10.0f;
            markerSphereScaleVector = new Vector3(markerSphereScale, markerSphereScale, markerSphereScale);
            cameraMovementSpeed = 0.2f;
            cameraRotationSpeed = 0.25f;
            pinchPrecision = 0.1f;
            zoomFactor = 5.0f;
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
            cameraNode.Position = new Vector3(50, 90, -300);            

            // Create light and attach to camera
            Node lightNode = cameraNode.CreateChild(name: "light");
            Light light = lightNode.CreateComponent<Light>();
            light.LightType = LightType.Directional;  
            lightNode.SetDirection(new Vector3(0.6f, -1.0f, 0.8f));  
         
            // Initialize marker sphere meshes   
            InitializeMarkerSpheres();               
        }

        /// <summary>
        /// Creates the objects that will represent a marker and adds them to a list
        /// </summary>
        private void InitializeMarkerSpheres()
        {            
            // Initialize list with capacity for the total number of markers 
            markerSpheres = new List<Sphere>(markerCount);

            // Create mesh node that will hold every marker
            meshNode = scene.CreateChild();

            // Create and add a sphere for each position in the list
            for (int i = 0; i < markerSpheres.Capacity; i++)
            {                
                Node node = meshNode.CreateChild("marker" + i);
                node.Position = Vector3.Zero;
                node.Scale = markerSphereScaleVector;
                
                Sphere sphere = node.CreateComponent<Sphere>();
                sphere.Color = Color.Cyan;                

                markerSpheres.Add(sphere);                
            }

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
            Vector3 tempPosition = Vector3.Zero;

            // Iterate through the list and update the marker positions
            for (int i = 0; i < streamData.Count; i++)
            {
                // Copy new position values to dummy vector
                tempPosition.X = streamData[i].Position.X;
                tempPosition.Y = streamData[i].Position.Y;
                tempPosition.Z = streamData[i].Position.Z;

                // Copy vector to node position
                markerSpheres[i].Node.Position = tempPosition;
            }

            // Update camera position according to touch events
            UpdateCameraPosition();
        }

        /// <summary>
        /// Updates camera position using values provided by the touch callback method
        /// </summary>
        void UpdateCameraPosition()
        {
            // Update camera offset and reset 
            camera.Node.Position += (camera.Node.Right * cameraPositionOffset.X) + 
                                    (camera.Node.Up * cameraPositionOffset.Y) +
                                    (camera.Node.Direction * cameraPositionOffset.Z);

            cameraPositionOffset = Vector3.Zero;
        }

        /// <summary>
        /// Fetches new stream data and updates the local structure
        /// </summary>
        private void UpdateStreamData()
        {
            streamData = CameraStream.Instance.GetStreamMarkerData();
            markerCount = streamData.Count;

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
            // Handle Rotation around mesh
            if (Input.NumTouches == 1)
            {
                // Handle Rotation locally                
                camera.Node.RotateAround(meshNode.Position, Quaternion.FromAxisAngle(camera.Node.WorldRight, eventArgs.DY * cameraRotationSpeed), TransformSpace.World);
                camera.Node.RotateAround(meshNode.Position, new Quaternion(0, eventArgs.DX * cameraRotationSpeed, 0), TransformSpace.World);
            }
            // Handle panning and pinching
            else if (Input.NumTouches >= 2)
            {
                // Panning<Begin>
                // Get Touchstates
                TouchState fingerOne = Input.GetTouch(0);
                TouchState fingerTwo = Input.GetTouch(1);

                // Average two fingers' delta positions IF they are both different than zero
                float averageDx = fingerOne.Delta.X == 0 || fingerTwo.Delta.X == 0 ? 0 : (fingerOne.Delta.X + fingerTwo.Delta.X) / 2.0f;
                float averageDy = fingerOne.Delta.Y == 0 || fingerTwo.Delta.Y == 0 ? 0 : (fingerOne.Delta.Y + fingerTwo.Delta.Y) / 2.0f;

                // Add values to camera offset
                cameraPositionOffset.X = -averageDx * cameraMovementSpeed;
                cameraPositionOffset.Y = averageDy * cameraMovementSpeed;                
                
                // Pinching
                // Get delta distance between both touches
                double oldDistance = GetDistance2D(fingerOne.LastPosition.X, fingerTwo.LastPosition.X, fingerOne.LastPosition.Y, fingerTwo.LastPosition.Y);
                double newDistance = GetDistance2D(fingerOne.Position.X, fingerTwo.Position.X, fingerOne.Position.Y, fingerTwo.Position.Y);
                double deltaDistance = oldDistance - newDistance;

                // Precision control
                if (Math.Abs(deltaDistance) > pinchPrecision)
                {
                    float scale = (float)(newDistance / oldDistance);
                    pinchZoom = (newDistance > oldDistance) ? scale : -scale;

                    // Update camera offset
                    cameraPositionOffset.Z += pinchZoom * zoomFactor; 
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
        private double GetDistance2D(float x1, float x2, float y1, float y2)
        {
            float deltaX = Math.Abs(x1 - x2);
            float deltaY = Math.Abs(y1 - y2);

            return Math.Sqrt(Math.Pow(deltaX, 2.0f) + Math.Pow(deltaY, 2.0f));
        }
    }
}
