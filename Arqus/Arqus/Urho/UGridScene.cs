using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using System.Text;

using Urho;
using Urho.Shapes;

namespace Arqus
{
    public class UGridScene : Application
    {
        Camera camera;
        Scene scene;
        Octree sceneOctree;

        Vector3 cameraPositionOffset;
        float cameraMovementSpeed;

        float pinchZoom;
        float pinchPrecision;
        float zoomFactor;

        // List of camera stream information
        List<QTMRealTimeSDK.Data.Camera> streamDataCameraList;
        int cameraCount;

        Urho.Shapes.Plane backPlane;

        // Grid related variables
        Vector3 gridPosition;
        float gridMovementSpeed;
        Vector3 gridViewOrigin;

        int gridFrameHeight;
        int gridNumColumns;
        float gridFramePadding;
        float gridViewBottom;

        // Holds gridView (and all its children)
        Node gridViewNode;

        public UGridScene(ApplicationOptions options) : base(options)
        {
            cameraCount = 0;
            cameraMovementSpeed = 0.05f;

            pinchPrecision = 0.2f;
            zoomFactor = 6.0f;

            cameraPositionOffset = Vector3.Zero;

            gridMovementSpeed = 0.05f;
            gridPosition = gridViewOrigin = Vector3.Zero;

            // More arbitrary numbers (temporary!)
            gridFrameHeight = 30;
            gridNumColumns = 2;
            gridFramePadding = 3;
        }

        protected override void Start()
        {
            base.Start();

            CreateScene();
            InitializeGrid();
            SetupViewport();
        }        

        // Sets up scene elements 
        private void CreateScene()
        {
            // Subscribe to touch event
            Input.SubscribeToTouchMove(OnTouched);

            // Createnew scene
            scene = new Scene();

            // Creates default scene octree (-1000:1000)
            sceneOctree = scene.CreateComponent<Octree>();

            // Create camera node and then attach camera object to it
            Node cameraNode = scene.CreateChild("camera");
            camera = cameraNode.CreateComponent<Camera>();

            // HACK: Arbitraty position.. for now!
            cameraNode.Position = new Vector3(20, -40, -140);            

            // Create background plane
            Node backPlaneNode = scene.CreateChild("backPlane");
            backPlane = backPlaneNode.CreateComponent<Urho.Shapes.Plane>();
            backPlane.SetMaterial(Material.FromColor(new Color(0.1f, 0.1f, 0.1f), true));

            // Rotate plane
            backPlaneNode.SetScale(500);
            backPlaneNode.Position = new Vector3(0, 0, 180);
            backPlaneNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
        }

        // Setup viewport..
        private void SetupViewport()
        {
            Renderer renderer = Renderer;
            renderer.SetViewport(0, new Viewport(Context, scene, camera, null));            
        }

        /// <summary>
        /// Creates grid view and initializes it
        /// </summary>
        private void InitializeGrid()
        {
            // HACK: Arbitrary values (for now)
            gridViewOrigin = new Vector3(-20.7f, 85, 0);
            camera.Node.Position = new Vector3(0, 0, -173);

            // Create gridView.. HACKed height, column number 
            GridViewComponent gridView = new GridViewComponent(gridViewOrigin, gridFrameHeight, gridNumColumns, new Color(0.215f, 0.301f, 0.337f));
            gridView.Padding = gridFramePadding;

            // Add node to scene, along with gridview component
            gridViewNode = scene.CreateChild("gridView");
            gridViewNode.AddComponent(gridView);

            // Update stream data before using the camera count variable
            UpdateStreamData();                
            gridViewBottom = gridViewOrigin.Y + ((gridFramePadding + gridFrameHeight) * cameraCount);
        }

        // Called on every tick
        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
            
            // Update camera stream
            UpdateStreamData();

            // Update Camera position
            UpdateCameraPosition();
        }
                
        /// <summary>
        /// Fills streamDataCameraList with new frame information
        /// </summary>
        private void UpdateStreamData()
        {
            // Update stream data and camera count            
            //CameraStream.Instance.UpdateStreamData();
            //cameraCount = CameraStream.Instance.GetCameraCount();
        }

        // Updates camera position using values provided by the touch callback method        
        void UpdateCameraPosition()
        {
            // Update camera offset and reset 
            camera.Node.Position += (camera.Node.Right * cameraPositionOffset.X) +
                                    (camera.Node.Up * cameraPositionOffset.Y) +
                                    (camera.Node.Direction * cameraPositionOffset.Z);

            cameraPositionOffset = Vector3.Zero;
        }

        private void DragGrid(Vector3 offset)
        {
            // Clamp upper bounds
            if((gridViewNode.Position.Y + offset.Y) < gridViewOrigin.Y)
            {
                offset = Vector3.Zero;
                return;
            }
            
            // Lower one... TODO!!!!
            //else if((gridViewNode.Position.Y + offset.Y + gridViewBottom - 140) < gridViewOrigin.Y + gridViewBottom - 140)
            {
                //offset = Vector3.Zero;
               // return;
            }
            
            gridViewNode.Position += offset;
        }

        void OnTouched(TouchMoveEventArgs eventArgs)
        {
            // Handle GridView scrolling
            if (Input.NumTouches == 1)
            {
                DragGrid(new Vector3(0, -eventArgs.DY * cameraMovementSpeed, 0));
                //gridViewNode.Position += new Vector3(0 /*eventArgs.DX * cameraMovementSpeed*/, -eventArgs.DY * cameraMovementSpeed, 0);
                //System.Console.WriteLine("x: " + gridViewNode.Position.X + " y: " + gridViewNode.Position.Y);
            }
            // Handle panning and pinching
            else if (Input.NumTouches >= 2)
            {                
                // Get Touchstates
                TouchState fingerOne = Input.GetTouch(0);
                TouchState fingerTwo = Input.GetTouch(1);
                
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

        bool isPinching(ref int x1, ref int x2, ref int y1, ref int y2)
        {
            return (x1 * x2 < 0) || (y1 * y2 < 0) ? true : false;
        }

        private double GetDistance2D(float x1, float x2, float y1, float y2)
        {
            float deltaX = Math.Abs(x1 - x2);
            float deltaY = Math.Abs(y1 - y2);

            return Math.Sqrt(Math.Pow(deltaX, 2.0f) + Math.Pow(deltaY, 2.0f));
        }
    }

    public class GridElement : Node
    {
        private int id;
        Urho.Shapes.Plane backgroundPlane;        

        public GridElement(int _id, Vector3 position, int width, int height, Color color)
        {
            Position = position;
            Width = width;
            Height = height;
            id = _id;

            // Create plane node and attach a plane shape to it
            Node planeNode = CreateChild("plane");
            backgroundPlane = planeNode.CreateComponent<Urho.Shapes.Plane>();

            // Set unlit color
            backgroundPlane.SetMaterial(Material.FromColor(color, true));

            // Set scale
            planeNode.SetScale(20);

            // Rotate plane
            planeNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
        }

        public int Width { get; set; }
        public int Height { get; set; }
    }
}
