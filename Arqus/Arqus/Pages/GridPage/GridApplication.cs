using Arqus.Helpers;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using System.Text;

using Urho;
using Urho.Shapes;
using Xamarin.Forms;

namespace Arqus
{
    public class GridApplication : Urho.Application
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
        private GridViewComponent gridView;

        int gridFrameHeight;
        int gridNumColumns;
        float gridFramePadding;

        Ray lowerViewportRay;

        // Holds gridView (and all its children)
        Node gridViewNode;

        public GridApplication(ApplicationOptions options) : base(options)
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

            // Every time we recieve new data we invoke it on the main thread to update the graphics accordingly
            MessagingCenter.Subscribe<CameraStreamService, List<QTMRealTimeSDK.Data.Camera>>(this, MessageSubject.STREAM_DATA_SUCCESS.ToString(), (sender, cameras) =>
            {
                SetMarkerData(cameras);
            });

            // Every time we recieve new data we invoke it on the main thread to update the graphics accordingly
            MessagingCenter.Subscribe<CameraStreamService, List<ImageSharp.Color[]>>(this, MessageSubject.STREAM_DATA_SUCCESS.ToString(), (sender, imageData) =>
            {
                SetImageData(imageData);
            });
        }

        // Sets up scene elements 
        private void CreateScene()
        {
            // Subscribe to touch events
            Input.SubscribeToTouchMove(OnTouchesMoved);
            Input.SubscribeToTouchBegin(OnTouchesBegan);
            Input.SubscribeToTouchEnd(OnTouchesEnded);

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
            backPlane.SetMaterial(Material.FromColor(new Urho.Color(0.1f, 0.1f, 0.1f), true));

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
            gridView = new GridViewComponent(gridViewOrigin, gridFrameHeight, gridNumColumns, new Urho.Color(0.215f, 0.301f, 0.337f));
            gridView.Padding = gridFramePadding;

            // Add node to scene, along with gridview component
            gridViewNode = scene.CreateChild("gridView");
            gridViewNode.AddComponent(gridView);
        }

        // Called on every tick
        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
           

            // Update Camera position
            UpdateCameraPosition();
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

        /// <summary>
        /// Handles grid "scrolling" 
        /// </summary>
        /// <param name="offset">Amount to drag</param>
        private void DragGrid(Vector3 offset)
        {
            float d = gridViewOrigin.Y - 140;
            float rayDistance = Vector3.Distance(camera.Node.Position, gridViewNode.Position);

            // Clamp upper bounds
            if ((gridViewNode.Position.Y + offset.Y) < gridViewOrigin.Y)
                return;

            // Clamp lower bounds through ray casting and querying for the lower bounds collision plane
            // Test only if user wants to draw grid view up
            if (offset.Y > 0)
            {
                // Get ray at bottom-middle screen and cast it
                lowerViewportRay = camera.GetScreenRay(0.5f, 1.0f);
                RayQueryResult? rayResult = sceneOctree.RaycastSingle(lowerViewportRay, RayQueryLevel.Triangle, rayDistance, DrawableFlags.Geometry, uint.MaxValue);

                if (rayResult.HasValue)
                {
                    if (rayResult.Value.Node.Name == "lowerBounds")
                    {
                        return;
                    }
                }
            }

            gridViewNode.Position += offset;
        }

        /// <summary>
        /// Casts camera ray from the viewport's coordinates
        /// </summary>
        /// <param name="x">Touch X Coordinate - raw (not normalized)</param>
        /// <param name="y">Touch Y Coordinate - raw (not normalized)</param>
        void CastTouchRay(int x, int y)
        {
            // Shoot from camera to grid view
            float rayDistance = Vector3.Distance(camera.Node.Position, gridViewNode.Position);

            // Create ray with normalized screen coordinates
            Ray camRay = camera.GetScreenRay((float)x / Graphics.Width, (float)y / Graphics.Height);

            // Cast the ray looking for 3D geometry (our grid screen planes) and store result in variable
            RayQueryResult? rayResult = sceneOctree.RaycastSingle(camRay, RayQueryLevel.Triangle, rayDistance, DrawableFlags.Geometry, uint.MaxValue);

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
                    int cameraID = screenNode.Parent.GetComponent<Visualization.CameraScreen>().CameraID;

                    // Start Camera view.
                    MessagingCenter.Send(Xamarin.Forms.Application.Current, MessageSubject.SET_CAMERA_SELECTION.ToString(), cameraID);
                    //this.Stop();
                }
            }
        }

        int tapTouchID;
        float tapTimeStamp;

        // TODO: Get paper or something to justify this time
        float tapTimeMargin = 0.1f;

        public void OnTouchesBegan(TouchBeginEventArgs eventArgs)
        {
            // Fill out variables for tap gesture
            tapTouchID = eventArgs.TouchID;
            tapTimeStamp = Time.ElapsedTime;
        }

        public void OnTouchesEnded(TouchEndEventArgs eventArgs)
        {
            // Return if this is not our original tap finger
            if (eventArgs.TouchID != tapTouchID)
                return;

            // Get delta time
            float dt = Time.ElapsedTime - tapTimeStamp;

            // If it is lesser than our tap margin, it is a tap
            if (dt < tapTimeMargin)
            {
                CastTouchRay(eventArgs.X, eventArgs.Y);
            }
        }

        void OnTouchesMoved(TouchMoveEventArgs eventArgs)
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


        private void SetImageData(List<ImageSharp.Color[]> data)
        {
            int count = 0;

            foreach (ImageSharp.Color[] image in data)
            {
                if (image == null)
                    break;

                if (gridView.screens.Count > count && gridView.screens[count].CurrentCameraMode != CameraMode.ModeMarker)
                    gridView.screens[count].ImageData = image;

                count++;
            }
        }

        private bool updatingMarkerData;

        private void SetMarkerData(List<QTMRealTimeSDK.Data.Camera> data)
        {
            int count = 0;

            foreach (QTMRealTimeSDK.Data.Camera camera in data)
            {
                if (gridView.screens.Count > count && gridView.screens[count].CurrentCameraMode == CameraMode.ModeMarker)
                    gridView.screens[count].MarkerData = camera;
                count++;
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

        protected override void Stop()
        {
            base.Stop();
            //scene.SetEnabledRecursive(false);
        }
    }   
}
