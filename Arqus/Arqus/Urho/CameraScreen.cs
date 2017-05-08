using Arqus.Helpers;
using Arqus.Components;
using Arqus.DataModels;

using Urho;
using Urho.Urho2D;
using Xamarin.Forms;
using QTMRealTimeSDK;
using Arqus.Services;
using Arqus.Service;
using System.Diagnostics;
using Urho.Gui;

namespace Arqus.Visualization
{

    /// <summary>
    /// Constructs and updates camera screens acording to their given resolution and curren stream mode
    /// It is not meant to be used inside an urho application in its own but rather employ one of the
    /// visualization techniques such as Grid or Carousel.
    /// </summary>
    public class CameraScreen : Component
    {
        // General properties
        public int position;

        // private fields
        private bool dirty;
        private Node screenNode;
        private Node cameraNode;
        private Node labelNode;
        private Node markerScreenNode;
        private Text3D label;
        private Urho.Camera urhoCamera;
        private Urho.Shapes.Plane imageScreen;
        private Urho.Shapes.Plane markerScreen;


        // static fields
        static int screenCount;
        private float scale;

        public float Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                Height = scale;
                Width = Camera.ImageResolution.PixelAspectRatio * Height;
            }
        }

        public float Width { private set; get; }
        public float Height { private set; get; }
        
        public Material Material { get; set; }
        
        private delegate void UpdateDelegate();
        private UpdateDelegate OnUpdateHandler;

        public DataModels.Camera Camera { get; private set; }

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


        public bool IsImageMode()
        {
            return Camera.Mode != CameraMode.ModeMarker;
        }

        // Intensity mode properties
        private Texture2D texture;

        private ImageSharp.PixelFormats.Rgba32[] imageData;

        public ImageSharp.PixelFormats.Rgba32[] ImageData
        {
            set
            {
                dirty = true;
                imageData = value;
            }
        }

        // TODO: Add initialization for float frameHeight, float frameWidth, float min
        public CameraScreen(DataModels.Camera camera, Node cameraNode)
        {
            Camera = camera;
            this.cameraNode = cameraNode;
            urhoCamera = cameraNode.GetComponent<Urho.Camera>();

            ReceiveSceneUpdates = true;
            OnUpdateHandler += OnMarkerUpdate;

            // Set position in relation to the number of cameras that are already initialized
            // so the screens can be positioned accordingly
            position = screenCount + 1;
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
            // Apply camera orientation and an offset to match the no rotation position with QTM
            screenNode.Rotate(new Quaternion(0, Camera.Orientation + 90, 0), TransformSpace.Local);

            // Create marker screen node and its plane
            markerScreen = screenNode.CreateComponent<Urho.Shapes.Plane>();
            markerScreen.SetMaterial(Material.FromColor(new Urho.Color(0.215f, 0.301f, 0.337f), true));

            // Create intensity plane, its material and assign it
            imageScreen = screenNode.CreateComponent<Urho.Shapes.Plane>();
            Material = new Material();

            texture = new Texture2D();
            texture.SetNumLevels(1);
            texture.SetSize(Camera.ImageResolution.Width, Camera.ImageResolution.Height, Urho.Graphics.RGBAFormat, TextureUsage.Dynamic);
            Material.SetTexture(TextureUnit.Diffuse, texture);
            Material.SetTechnique(0, CoreAssets.Techniques.DiffUnlit, 0, 0);
            imageScreen.SetMaterial(Material);

            labelNode = screenNode.CreateChild();
            label = labelNode.CreateComponent<Text3D>();
            labelNode.Rotate(new Quaternion(90, 90, 180), TransformSpace.World);
            labelNode.Position = new Vector3(0.3f, 0.01f, 0.4f);
            label.Text = Camera.ID.ToString();
            label.SetFont(CoreAssets.Fonts.AnonymousPro, 14);
            label.TextEffect = TextEffect.Stroke;

            // Initialize current camera mode
            SetImageMode(IsImageMode());

            SubscribeToDataEvents();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            MessagingCenter.Unsubscribe<MarkerStream, QTMRealTimeSDK.Data.Camera>(this, MessageSubject.STREAM_DATA_SUCCESS.ToString() + Camera.ID);
            MessagingCenter.Unsubscribe<ImageStream, ImageSharp.PixelFormats.Rgba32[]>(this, MessageSubject.STREAM_DATA_SUCCESS.ToString() + Camera.ID);
            MessagingCenter.Unsubscribe<Arqus.DataModels.Camera, CameraMode>(this, MessageSubject.STREAM_MODE_CHANGED.ToString() + Camera.ID);
        }
        

        public void SubscribeToDataEvents()
        {
            // Every time we recieve new data we invoke it on the main thread to update the graphics accordingly
            MessagingService.Subscribe<MarkerStream, QTMRealTimeSDK.Data.Camera>(this, MessageSubject.STREAM_DATA_SUCCESS.ToString() + Camera.ID, (sender, markerData) =>
            {
                if(!IsImageMode())
                    Urho.Application.InvokeOnMain(() => MarkerData = markerData);
            });

            // Every time we recieve new data we invoke it on the main thread to update the graphics accordingly
            MessagingService.Subscribe<ImageStream, ImageSharp.PixelFormats.Rgba32[]>(this, MessageSubject.STREAM_DATA_SUCCESS.ToString() + Camera.ID, (sender, imageData) =>
             {
                if(IsImageMode())
                    Urho.Application.InvokeOnMain(() => ImageData = imageData);
            });

            MessagingService.Subscribe<Arqus.DataModels.Camera, CameraMode>(this, MessageSubject.STREAM_MODE_CHANGED.ToString() + Camera.ID, (sender, mode) =>
            {
                SetImageMode(mode != CameraMode.ModeMarker);
            });
        }

        public void SetImageMode(bool enable)
        {
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
        private void CleanMarkerScreen() { Urho.Application.InvokeOnMainAsync(() => Pool.Hide());  }

        private void SetImageMode()
        {
            OnUpdateHandler = OnImageUpdate;
            Urho.Application.InvokeOnMainAsync(() =>
            {
                markerScreen.Enabled = false;
                imageScreen.Enabled = true;
            });
        }

        private void SetMarkerMode()
        {
            Urho.Application.InvokeOnMainAsync(() =>
            {
                imageScreen.Enabled = false;
                markerScreen.Enabled = true;
            });
            OnUpdateHandler = OnMarkerUpdate;
        }
        
        public unsafe bool UpdateMaterialTexture(ImageSharp.PixelFormats.Rgba32[] imageData)
        {
            fixed (ImageSharp.PixelFormats.Rgba32* bptr = imageData)
            {
                return texture.SetData(0, 0, 0, Camera.ImageResolution.Width, Camera.ImageResolution.Height, bptr);
            }
        }
        
        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            if (screenNode.Enabled && Node.Distance(cameraNode) > urhoCamera.FarClip)
            {
                Camera.Disable();
                screenNode.Enabled = false;
            }
            else if (!screenNode.Enabled && Node.Distance(cameraNode) < urhoCamera.FarClip)
            {
                Camera.Enable();
                screenNode.Enabled = true;
            }
                
            

            if (dirty && screenNode.Enabled)
            {
                dirty = false;
                OnUpdateHandler?.Invoke();
            }
        }
                
        private void OnMarkerUpdate()
        {
            // This index will be used as an array pointer to help identify and disable
            // markers which are not being currently used
            int lastUsedInArray = 0;
            
            // Iterate through the marker array, transform and draw spheres
            for (int i = 0; i < markerData.MarkerCount; i++)
            {
                // Transform from camera coordinates to frame coordinates
                // TODO: Add marker resolution to class
                float adjustedX = DataOperations.ConvertRange(0, Camera.MarkerResolution.Width, -0.5f, 0.5f, markerData.MarkerData2D[i].X);
                float adjustedY = DataOperations.ConvertRange(0, Camera.MarkerResolution.Height, -0.5f, 0.5f, markerData.MarkerData2D[i].Y);
                float adjustedScaleX = DataOperations.ConvertRange(0, Camera.MarkerResolution.Width, 0, 1, markerData.MarkerData2D[i].DiameterX);
                float adjustedScaleY = DataOperations.ConvertRange(0, Camera.MarkerResolution.Height, 0, 1, markerData.MarkerData2D[i].DiameterY);

                MarkerSphere sphere = Pool.Get(i);
                // Set world position with new frame coordinates            
                sphere.Position = new Vector3(adjustedY, 0.5f, adjustedX);
                sphere.Scale = new Vector3(adjustedScaleY, sphere.Scale.Y, adjustedScaleX);

                // Last element will set this variable
                lastUsedInArray = i;
            }

            // Hide the markers which were not used on this frame
            Pool.HideUnused(lastUsedInArray);
        }

        private void OnImageUpdate()
        {
            if(imageData != null)
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
