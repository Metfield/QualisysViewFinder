﻿using Arqus.Helpers;
using Arqus.Components;

using Urho;
using Urho.Urho2D;

namespace Arqus.Visualization
{

    /// <summary>
    /// Constructs and updates camera screens acording to their given resolution and curren stream mode
    /// It is not meant to be used inside an urho application in its own but rather employ one of the
    /// visualization techniques such as Grid or Carousel.
    /// </summary>
    internal class CameraScreen : Component
    {
        // General properties
        public int position;
        public bool IsImageMode { get; set; }
        public int CameraID { private set; get; }

        // private fields
        private bool dirty;
        private Node screenNode;
        private Urho.Shapes.Plane imageScreen;
        private Urho.Shapes.Plane markerScreen;

        // static fields
        static int screenCount;

        public ImageResolution Resolution { private set; get; }

        private float scale;

        public float Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                Height = scale;
                Width = Resolution.PixelAspectRatio * Height;
            }
        }

        public float Width { private set; get; }
        public float Height { private set; get; }
        
        public Material Material { get; set; }
        
        private delegate void UpdateDelegate();
        private UpdateDelegate OnUpdateHandler;

        // Marker mode properties
        public MarkerSpherePool Pool { set; get; }

        private QTMRealTimeSDK.Data.Camera markerData;
        public QTMRealTimeSDK.Data.Camera MarkerData
        {
            set
            {
                dirty = true;
                markerData = value;
            } 
        }

        // Intensity mode properties
        private Texture2D texture;

        private ImageSharp.Color[] imageData;

        public ImageSharp.Color[] ImageData
        {
            set
            {
                dirty = true;
                imageData = value;
            }
        }

        // TODO: Add initialization for float frameHeight, float frameWidth, float min
        public CameraScreen(int cameraID, int cameraWidth, int cameraHeight, bool isImageMode)
        {
            CameraID = cameraID;
            Resolution = new ImageResolution(cameraWidth, cameraHeight);
            IsImageMode = isImageMode;

            ReceiveSceneUpdates = true;
            OnUpdateHandler += OnMarkerUpdate;

            // Set position in relation to the number of cameras that are already initialized
            // so the screens can be positioned accordingly
            position = screenCount;
            screenCount++;
        }
        
        public static void ResetScreenCounter()
        {
            screenCount = 0;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);
            
            // Create screen Node, scale it accordingly and rotate it so it faces camera
            screenNode = node.CreateChild("screenNode");

            // Initialize marker sphere pool with arbitrary number of spheres
            Pool = new MarkerSpherePool(20, screenNode);

            screenNode.Scale = new Vector3(Height, 1, Width);

            // Rotate the camera in the clockwise direction with 90 degrees
            screenNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
            screenNode.Rotate(new Quaternion(0, 90, 0), TransformSpace.Local);



            // Create marker screen node and its plane
            markerScreen = screenNode.CreateComponent<Urho.Shapes.Plane>();
            markerScreen.SetMaterial(Material.FromColor(new Urho.Color(0.215f, 0.301f, 0.337f), true));

            // Create intensity plane, its material and assign it
            imageScreen = screenNode.CreateComponent<Urho.Shapes.Plane>();
            Material = new Material();

            texture = new Texture2D();
            texture.SetNumLevels(1);
            texture.SetSize(Resolution.Width, Resolution.Height, Urho.Graphics.RGBAFormat, TextureUsage.Dynamic);
            Material.SetTexture(TextureUnit.Diffuse, texture);
            Material.SetTechnique(0, CoreAssets.Techniques.DiffUnlit, 0, 0);
            imageScreen.SetMaterial(Material);
        
            // Initialize current camera mode
            SetImageMode(IsImageMode);
        }

        public void SetImageMode(bool enable)
        {
            IsImageMode = enable;

            // Clear the update handler to more predictibly determine which
            // methods will be called during update
            OnUpdateHandler = null;

            if (enable)
            {
                CleanMarkerScreen();
                SetImageMode();
            }
            else
            {
                CleanImageScreen();
                SetMarkerMode();
            }
        }
    
        private void CleanImageScreen(){ }
        private void CleanMarkerScreen() { Pool.Hide(); }

        private void SetImageMode()
        {   
            markerScreen.Enabled = false;
            imageScreen.Enabled = true;
            OnUpdateHandler = OnImageUpdate;
        }

        private void SetMarkerMode()
        {
            imageScreen.Enabled = false;
            markerScreen.Enabled = true;
            OnUpdateHandler = OnMarkerUpdate;
        }
        
        public unsafe bool UpdateMaterialTexture(ImageSharp.Color[] imageData)
        {
            fixed (ImageSharp.Color* bptr = imageData)
            {
                return texture.SetData(0, 0, 0, Resolution.Width, Resolution.Height, bptr);
            }
        }
        
        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            /*
            if (screenNode.WorldPosition.Z >= min - 20 && screenNode.WorldPosition.Z <= min + 20)
            {
                screenNode.Enabled = true;
            }
            else if (screenNode.Enabled)
            {
                screenNode.Enabled = false;
                Pool.Hide();
            }
            */

            if (dirty && screenNode.Enabled)
            {
                OnUpdateHandler?.Invoke();
                dirty = false;
            }
        }
                
        private void OnMarkerUpdate()
        {
            // This index will be used as an array pointer to help identify and disable
            // markers which are not being currently used
            int lastUsedInArray = 0;

            // Get necessary frame information to position markers
            // Horizontal bounds
            float leftBound = screenNode.WorldPosition.X - (Width * 0.5f);
            float rightBound = leftBound + Width;

            // Vertical bounds
            float upperBound = screenNode.WorldPosition.Y + (Height * 0.5f);
            float lowerBound = upperBound - Height;

            // Iterate through the marker array, transform and draw spheres
            for (int i = 0; i < markerData.MarkerCount; i++)
            {
                // Transform from camera coordinates to frame coordinates
                float adjustedX = Helpers.DataOperations.ConvertRange(0, Resolution.Width, leftBound, rightBound, markerData.MarkerData2D[i].X / 64);
                float adjustedY = Helpers.DataOperations.ConvertRange(0, Resolution.Height, upperBound, lowerBound, markerData.MarkerData2D[i].Y / 64);
                float adjustedScaleX = DataOperations.ConvertRange(0, Resolution.Width, 0, 1, markerData.MarkerData2D[i].DiameterX / 64);
                float adjustedScaleY = DataOperations.ConvertRange(0, Resolution.Height, 0, 1, markerData.MarkerData2D[i].DiameterY / 64);

                // Set world position with new frame coordinates            
                Pool.Get(i).SetWorldPosition(new Vector3(adjustedX, adjustedY, screenNode.WorldPosition.Z - 1));

                // Maybe unnecessary to do this every time?
                Pool.Get(i).Scale = new Vector3(adjustedScaleX, 0, adjustedScaleY);

                // Last element will set this variable
                lastUsedInArray = i;
            }

            // Hide the markers which were not used on this frame
            Pool.HideUnused(lastUsedInArray);
        }

        private void OnImageUpdate()
        {
            UpdateMaterialTexture(imageData);
        }        
        

        /// <summary>
        /// Creates nice frame around the screen
        /// </summary>
        /// <param name="node"></param>
        /// <param name="color"></param>
        /// <param name="frameWidth"></param>
        private void CreateFrame(Node node, Urho.Color color, float frameWidth)
        {
            float originX = node.Position.X - Width / 2;
            float originY = Height / 2;

            // Top frame
            Node framePieceNode = node.CreateChild("FrameTop");

            framePieceNode.Scale = new Vector3(Width + frameWidth, 0, frameWidth);
            framePieceNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
            framePieceNode.Position = new Vector3(node.Position.X,
                                                 originY,
                                                 node.Position.Z);

            Urho.Shapes.Plane framePiece = framePieceNode.CreateComponent<Urho.Shapes.Plane>();
            framePiece.SetMaterial(Material.FromColor(new Urho.Color(0.305f, 0.388f, 0.415f), true));

            // Right frame
            framePieceNode = node.CreateChild("FrameRight");

            framePieceNode.Scale = new Vector3(frameWidth, 0, Height + frameWidth);
            framePieceNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
            framePieceNode.Position = new Vector3(originX + Width,
                                                 node.Position.Y,
                                                 node.Position.Z);

            framePiece = framePieceNode.CreateComponent<Urho.Shapes.Plane>();
            framePiece.SetMaterial(Material.FromColor(new Urho.Color(0.305f, 0.388f, 0.415f), true));

            // Bottom frame
            framePieceNode = node.CreateChild("FrameBottom");

            framePieceNode.Scale = new Vector3(Width + frameWidth, 0, frameWidth);
            framePieceNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
            framePieceNode.Position = new Vector3(node.Position.X,
                                                 -originY,
                                                 node.Position.Z);

            framePiece = framePieceNode.CreateComponent<Urho.Shapes.Plane>();
            framePiece.SetMaterial(Material.FromColor(new Urho.Color(0.305f, 0.388f, 0.415f), true));

            // Left frame
            framePieceNode = node.CreateChild("FrameLeft");

            framePieceNode.Scale = new Vector3(frameWidth, 0, Height + frameWidth);
            framePieceNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
            framePieceNode.Position = new Vector3(originX,
                                                 node.Position.Y,
                                                 node.Position.Z);

            framePiece = framePieceNode.CreateComponent<Urho.Shapes.Plane>();
            framePiece.SetMaterial(Material.FromColor(new Urho.Color(0.305f, 0.388f, 0.415f), true));
        }
    }
}
