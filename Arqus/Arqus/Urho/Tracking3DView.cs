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

        BoundingBox meshBoundingBox;
        Vector3 meshCenter;

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
            markerSphereScale = 200.0f * meshScale;
            markerSphereScaleVector = new Vector3(markerSphereScale, markerSphereScale, markerSphereScale);
            cameraMovementSpeed = 0.2f;
            cameraRotationSpeed = 0.25f;
            pinchPrecision = 0.2f;
            zoomFactor = 6.0f;
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

            // Move camera to arbitrary position
            cameraNode.Position = new Vector3(50, 90, -300);            
            
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

            // Vector to hold initial marker positions
            Vector3 tempPosition = new Vector3();

            // Create and add a sphere for each position in the list
            for (int i = 0; i < markerSpheres.Capacity; i++)
            {                
                Node node = meshNode.CreateChild("marker" + i);

                // Copy new position values to dummy vector
                tempPosition.X = streamData[i].Position.X;
                tempPosition.Y = streamData[i].Position.Y;
                tempPosition.Z = streamData[i].Position.Z;

                // Copy vector to node position
                node.Position = tempPosition;

                // Set sphere scale to predefined value
                node.Scale = markerSphereScaleVector;
                
                // Create sphere in node and set it to unlit
                Sphere sphere = node.CreateComponent<Sphere>();
                sphere.SetMaterial(Material.FromColor(Color.White, true));                
                
                // Add sphere to list
                // TODO: maybe add nodes instead?
                markerSpheres.Add(sphere);                
            }

            // Scale down mesh
            meshNode.Scale = new Vector3(meshScale, meshScale, meshScale);

            // Rotate it to stand on Y axis instead of Z
            meshNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
       
            // Get mesh's bounding box
            GenerateMeshBoundingBox();
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
            //CameraStream.Instance.GetStreamMarkerData();
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
        /// Gets mesh bounding box and stores it in the object member
        /// =========================================================
        /// TODO: Need to generate this for each frame in real time.. Use marker update algorithm
        /// in the OnUpdate method
        /// </summary>
        void GenerateMeshBoundingBox()
        {
            Vector3 tempMax = Vector3.Zero;
            Vector3 tempMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            // Go through the markers searching for the highest and lowest bounding values
            foreach(Node markerNode in meshNode.Children)
            {
                // Get Max X
                if (markerNode.Position.X > tempMax.X)
                    tempMax.X = markerNode.Position.X;

                // Get Max Y
                if (markerNode.Position.Y > tempMax.Y)
                    tempMax.Y = markerNode.Position.Y;

                // Get Max Z
                if (markerNode.Position.Z > tempMax.Z)
                    tempMax.Z = markerNode.Position.Z;

                // Get Min X
                if (markerNode.Position.X < tempMin.X)
                    tempMin.X = markerNode.Position.X;

                // Get Min Y
                if (markerNode.Position.Y < tempMin.Y)
                    tempMin.Y = markerNode.Position.Y;

                // Get Min Z
                if (markerNode.Position.Z < tempMin.Z)
                    tempMin.Z = markerNode.Position.Z;
            }

            // Create bounding box
            meshBoundingBox = new BoundingBox(tempMin, tempMax);
            meshCenter = meshBoundingBox.Center;

            // Scale down (just like we did with the meshNode)
            meshCenter = Vector3.Transform(meshCenter, Matrix4.Scale(meshScale)).Xyz;

            // Rotate (like we did on meshNode, convert to Urho's coordinate system)
            meshCenter = Vector3.Transform(meshCenter, Matrix4.CreateRotationX(-90)).Xyz;
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
                // Handle Rotation in this method for convenience                
                camera.Node.RotateAround(meshCenter, Quaternion.FromAxisAngle(camera.Node.WorldRight, eventArgs.DY * cameraRotationSpeed), TransformSpace.World);
                camera.Node.RotateAround(meshCenter, new Quaternion(0, eventArgs.DX * cameraRotationSpeed, 0), TransformSpace.World);
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
                if (isPinching(ref fingerOne.Delta.X, ref fingerTwo.Delta.X, ref fingerOne.Delta.Y, ref fingerTwo.Delta.Y))
                { 
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
        }

        /// <summary>
        /// Checks whether touch gesture is a pinch or not
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        bool isPinching(ref int x1, ref int x2, ref int y1, ref int y2)
        {
            return (x1 * x2 < 0) || (y1 * y2 < 0) ? true : false;
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
