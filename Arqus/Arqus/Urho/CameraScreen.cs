using Arqus.Helpers;
using Arqus.Components;
using Arqus.DataModels;

using Urho;
using Urho.Urho2D;
using Xamarin.Forms;
using Arqus.Services;
using System.Diagnostics;
using Urho.Gui;
using System;

using System.Reflection;
using static Arqus.CameraApplication;

using QTMRealTimeSDK.Settings;

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
        private Node backdropNode;
        private Node cameraNode;

        private Node nodeGridTextLabel, nodeDetailTextLabel;
        private Text3D textLabel;

        private Node nodeTextMessage;
        private Text3D textMessage;

        private Urho.Camera urhoCamera;
        private Urho.Shapes.Plane imageScreen;
        private Urho.Shapes.Plane defaultScreen;

        private Node screenFrame;

        private bool isDisabledStreamModePlaceholderActive = false;
        private static byte[] disabledStreamModePlaceholder;

        public double targetDistanceFromCamera;
        private int orientation;

        // static fields
        static int screenCount;
        private float scale;

        public float Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                Width = scale;
                Height = Width / Camera.ImageResolution.PixelAspectRatio;
            }
        }

        public float Width { set; get; }
        public float Height { set; get; }

        public bool Focused { get; set; }

        private Material screenMaterial;
        
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
            get
            {
                return markerData;
            }
        }

        public bool IsImageMode()
        {
            return Camera.Settings.Mode != CameraMode.ModeMarker;
        }

        public Texture2D texture;

        private byte[] imageData;

        public byte[] ImageData
        {
            set
            {
                dirty = true;
                imageData = value;
            }
            get
            {
                return imageData;
            }
        }

        public LoadingSpinner loadingSpinner;

        // TODO: Add initialization for float frameHeight, float frameWidth, float min
        public CameraScreen(DataModels.Camera camera, Node cameraNode)
        {
            Camera = camera;
            this.cameraNode = cameraNode;
            urhoCamera = cameraNode.GetComponent<Urho.Camera>();

            orientation = camera.Orientation;

            ReceiveSceneUpdates = true;

            // Set position in relation to the number of cameras that are already initialized
            // so the screens can be positioned accordingly
            position = screenCount + 1;
            screenCount++;

            if(!QTMNetworkConnection.IsMaster)
            {
                // Load placeholder image to be displayed when image stream mode is unavailable
                Assembly assembly = typeof(CameraScreen).Assembly;

                // Run on separate task (don't slowdown exectution)
                System.Threading.Tasks.Task.Run(() => 
                {
                    using (System.IO.Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".disabled_stream_mode.jpg"))
                    {
                        disabledStreamModePlaceholder = SkiaSharp.SKBitmap.Decode(stream).Resize(new SkiaSharp.SKImageInfo(Constants.URHO_TEXTURE_SIZE, Constants.URHO_TEXTURE_SIZE), SkiaSharp.SKBitmapResizeMethod.Lanczos3).Bytes;
                    }
                });                
            }
        }
        
        public static void ResetScreenCounter()
        {
            screenCount = 0;
        }

        Node markerSphereNode;
        public Node screenNode;
        
        CustomGeometry geom;
        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);
            
            // Create screen Node, scale it accordingly and rotate it so it faces camera
            screenNode = node.CreateChild("screenNode");
            markerSphereNode = node.CreateChild("markerSphereNode"); 
            backdropNode = node.CreateChild("backdrop");

            loadingSpinner = new LoadingSpinner(node.CreateChild("spinner"), 40, 1);

            // Initialize marker sphere pool with arbitrary number of spheres
            Pool = new MarkerSpherePool(20, markerSphereNode);
            
            backdropNode.Scale = new Vector3(Height, 1, Width);

            // Rotate the camera in the clockwise direction with 90 degrees
            backdropNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);

            // Apply camera orientation and an offset to match the no rotation position with QTM
            backdropNode.Rotate(new Quaternion(0, 90 - Camera.Orientation, 0), TransformSpace.Local);
            markerSphereNode.Rotate(new Quaternion(0, 0, -Camera.Orientation), TransformSpace.Local);

            // Create marker screen node and its plane
            defaultScreen = backdropNode.CreateComponent<Urho.Shapes.Plane>();
            defaultScreen.SetMaterial(Material.FromColor(Urho.Color.Black, true));

            // Create intensity plane, its material and assign it
            imageScreen = backdropNode.CreateComponent<Urho.Shapes.Plane>();
            screenMaterial = new Material();

            SetImageTexture(Camera.ImageResolution.Width, Camera.ImageResolution.Height);

            // Set detail info label
            // TODO: Fix magic numbers
            nodeDetailTextLabel = screenNode.CreateChild();
            textLabel = nodeDetailTextLabel.CreateComponent<Text3D>();
            
            if(Camera.Orientation == 0)
                nodeDetailTextLabel.Position = new Vector3(-Width/2 + 0.2f, -Height/2 + 0.7f, -0.1f);
            else
                nodeDetailTextLabel.Position = new Vector3(-Height / 2 + 0.2f, -Width / 2 + 0.7f, -0.1f);

            textLabel.Text = Camera.ID.ToString();
            textLabel.SetFont(CoreAssets.Fonts.AnonymousPro, 55);
            textLabel.TextEffect = TextEffect.Stroke;

            // Set grid view info label
            nodeGridTextLabel = screenNode.CreateChild();
            textLabel = nodeGridTextLabel.CreateComponent<Text3D>();
                        
            if (Camera.Orientation == 0)
                nodeGridTextLabel.Position = new Vector3(-Width / 2 + 0.2f, -Height / 2 + 1f, -0.1f);
            else
                nodeGridTextLabel.Position = new Vector3(-Height / 2 + 0.2f, -Width / 2 + 1f, -0.1f);

            textLabel.Text = Camera.ID.ToString();
            textLabel.SetFont(CoreAssets.Fonts.AnonymousPro, 100);
            textLabel.TextEffect = TextEffect.Stroke;

            // Create a node and a text object to display messages centered in the camera screen
            nodeTextMessage = screenNode.CreateChild();
            textMessage = nodeTextMessage.CreateComponent<Text3D>();
            textMessage.SetFont(CoreAssets.Fonts.AnonymousPro, 35);
            textMessage.TextEffect = TextEffect.Stroke;
            textMessage.Text = "Intensity/Video mode is disabled in slave mode";
            
            textMessage.VerticalAlignment = VerticalAlignment.Center;
            textMessage.HorizontalAlignment = HorizontalAlignment.Center;
            textMessage.TextAlignment = HorizontalAlignment.Center;
            nodeTextMessage.Enabled = false;
            nodeTextMessage.Position = new Vector3(0.0f, 0.0f, -0.1f);

            // Disable both label nodes at start
            nodeGridTextLabel.Enabled = false;
            nodeDetailTextLabel.Enabled = false;

            // Initialize current camera mode
            SetImageMode(Camera.IsImageMode());

            // Subscribe to messages
            SubscribeToDataEvents();

            // Create frame
            CreateFrame(screenNode, 0.04f);

            // Start with loading spinner enabled
            loadingSpinner.Start();
        }
        
        public void SubscribeToDataEvents()
        {
            // Subscribe to demoMode stream data as well
            MessagingCenter.Subscribe<DemoStream, QTMRealTimeSDK.Data.Camera>(this, Messages.Subject.STREAM_DATA_SUCCESS.ToString() + Camera.ID, (sender, markerData) =>
            {
                if (!IsImageMode())
                    Urho.Application.InvokeOnMain(() => MarkerData = markerData);
            });

            MessagingCenter.Subscribe<Arqus.DataModels.Camera, CameraMode>(this, Messages.Subject.STREAM_MODE_CHANGED.ToString() + Camera.ID, (sender, mode) =>
            {
                SetImageMode(Camera.IsImageMode());
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
                SetMarkerMode();
            }
        }
    
        private void CleanMarkerScreen() { Urho.Application.InvokeOnMainAsync(() => Pool.Hide());  }

        private void SetImageMode()
        {
            OnUpdateHandler = OnImageUpdate;
        }

        private void SetMarkerMode()
        {
            OnUpdateHandler = OnMarkerUpdate;
        }

        private float tempTime = Time.TimeSinceEpoch;

        public void OnImageUpdate()
        {
            if(loadingSpinner.Running)
            {
                loadingSpinner.Stop();
            }

            if (!imageScreen.Enabled)
            {
                imageScreen.Enabled = true;
            }

            if(defaultScreen.Enabled)
            {
                defaultScreen.Enabled = false;
            }
            
            if (!QTMNetworkConnection.IsMaster && 
                !isDisabledStreamModePlaceholderActive &&
                IsImageMode())
            {
                nodeTextMessage.Enabled = true;
                defaultScreen.Enabled = true;
            }
            else if (ImageData != null)
            {
                nodeTextMessage.Enabled = false;
                LoadImage(ImageData);
            }
        }

        private void LoadImage(byte[] image)
        {
            if (screenMaterial == null || image == null || nodeTextMessage.Enabled)
                return;

            texture?.SetData(0, 0, 0, Constants.URHO_TEXTURE_SIZE, Constants.URHO_TEXTURE_SIZE, image);
        }
        
        private void SetImageTexture(int width, int height)
        {
            texture = new Texture2D();
            texture.SetNumLevels(1);
            texture.SetSize(Constants.URHO_TEXTURE_SIZE, Constants.URHO_TEXTURE_SIZE, Urho.Graphics.RGBAFormat, TextureUsage.Dynamic);
            screenMaterial.SetTexture(TextureUnit.Diffuse, texture);
            screenMaterial.SetTechnique(0, CoreAssets.Techniques.DiffUnlit, 0, 0);
            imageScreen.SetMaterial(screenMaterial);
        }
        
        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
           
            float deltaTime = Time.SystemTime - tempTime;

            if (deltaTime > 500)
            {
                loadingSpinner.Start();
            }

            loadingSpinner.UpdateSpinner(timeStep);
            
            if (dirty)
            {
                loadingSpinner.Stop();
                tempTime = Time.SystemTime;
                dirty = false;
                OnUpdateHandler?.Invoke();
            }
        }
                
        private int markerQuality = 40;

        public void OnMarkerUpdate()
        {
            nodeTextMessage.Enabled = false;

            if (loadingSpinner.Running)
            {
                loadingSpinner.Stop();
            }

            if (imageScreen.Enabled)
            {
                imageScreen.Enabled = false;
            }

            if (!defaultScreen.Enabled)
            {
                defaultScreen.Enabled = true;
            }
            
            // This index will be used as an array pointer to help identify and disable
            // markers which are not being currently used
            int lastUsedInArray = 0;

            float cameraScreenHalfWidth = Width / 2;
            float cameraScreenHalfHeight = Height / 2;

            // Iterate through the marker array, transform and draw spheres
            for (int i = 0; i < markerData.MarkerCount; i++)
            {
                // Transform from camera coordinates to frame coordinates
                // TODO: Add marker resolution to class
                float x = DataOperations.ConvertRange(0, Camera.Settings.MarkerResolution.Width, -cameraScreenHalfWidth, cameraScreenHalfWidth, markerData.MarkerData2D[i].X);
                float y = DataOperations.ConvertRange(0, Camera.Settings.MarkerResolution.Height, cameraScreenHalfHeight, -cameraScreenHalfHeight, markerData.MarkerData2D[i].Y);
                float width = DataOperations.ConvertRange(0, Camera.Settings.MarkerResolution.Width, 0, cameraScreenHalfWidth, markerData.MarkerData2D[i].DiameterX);
                float height = DataOperations.ConvertRange(0, Camera.Settings.MarkerResolution.Height, 0, cameraScreenHalfHeight, markerData.MarkerData2D[i].DiameterY);

                Pool.Get(i).Redraw(x, y, width, height, cameraScreenHalfWidth, cameraScreenHalfHeight);
    
                // Last element will set this variable
                lastUsedInArray = i;
            }

            // Hide the markers which were not used on this frame
            Pool.HideUnused(lastUsedInArray);
        }
        
        
        // Toggles between gid label (id) and detail label ( #id + model)
        public void ToggleUIInfo(ScreenLayoutType layoutType)
        {
            try
            {
                // Switch looks more neat
                switch (layoutType)
                {
                    case ScreenLayoutType.Carousel:
                        nodeGridTextLabel.Enabled = false;
                        nodeDetailTextLabel.Enabled = true;
                        break;

                    case ScreenLayoutType.Grid:
                        nodeGridTextLabel.Enabled = true;
                        nodeDetailTextLabel.Enabled = false;
                        break;
                }
            }
            catch (Exception e)
            {
                // An exception is sometimes thrown when switching layout mode.
                // -----------------------------------------------------------
                // Exception: Underlying native object was deleted for Handle=-1078990336. Node.SetEnabled
                // -----------------------------------------------------------
                // Attempting to access a node which is currently disabled? Either way,
                // catch the little troll and all is well.
                Debug.WriteLine("CameraScreen::ToggleUIInfo - " + e.Message);                
            }
        }

        // Turn screen frame on or off
        public void ToggleFrame(bool flag)
        {
            // Set both parent and children nodes
            screenFrame.SetEnabledRecursive(flag);
        }        


        // Creates nice frame around the screen
        private void CreateFrame(Node node, float frameWidth)
        {
            float originX = node.Position.X - Width / 2;
            float originY = Height / 2;

            // Create frame node to hold all pieces
            screenFrame = node.CreateChild("screenFrame");
            screenFrame.Rotate(new Quaternion(0, 0, -orientation), TransformSpace.Local);
            
            // Get correct colors for shadows and lit frame areas
            Urho.Color leftColor, topColor, rightColor, bottomColor;
            GetFrameColors(out leftColor, out topColor, out rightColor, out bottomColor);

            // Right frame
            Node framePieceNode = screenFrame.CreateChild("FrameRight");

            framePieceNode.Scale = new Vector3(frameWidth, 0, Height + frameWidth);
            framePieceNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
            framePieceNode.Position = new Vector3(originX + Width,
                                                 node.Position.Y,
                                                 node.Position.Z);

            Urho.Shapes.Plane framePiece = framePieceNode.CreateComponent<Urho.Shapes.Plane>();
            framePiece.SetMaterial(Material.FromColor(rightColor, true));

            // Top frame
            framePieceNode = screenFrame.CreateChild("FrameTop");

            framePieceNode.Scale = new Vector3(Width + frameWidth, 0, frameWidth);
            framePieceNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
            framePieceNode.Position = new Vector3(node.Position.X,
                                                 originY,
                                                 node.Position.Z);

            framePiece = framePieceNode.CreateComponent<Urho.Shapes.Plane>();
            framePiece.SetMaterial(Material.FromColor(topColor, true));

            // Bottom frame
            framePieceNode = screenFrame.CreateChild("FrameBottom");

            framePieceNode.Scale = new Vector3(Width + frameWidth, 0, frameWidth);
            framePieceNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
            framePieceNode.Position = new Vector3(node.Position.X,
                                                 -originY,
                                                 node.Position.Z);

            framePiece = framePieceNode.CreateComponent<Urho.Shapes.Plane>();
            framePiece.SetMaterial(Material.FromColor(bottomColor, true));

            // Left frame
            framePieceNode = screenFrame.CreateChild("FrameLeft");

            framePieceNode.Scale = new Vector3(frameWidth, 0, Height + frameWidth);
            framePieceNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
            framePieceNode.Position = new Vector3(originX,
                                                 node.Position.Y,
                                                 node.Position.Z);

            framePiece = framePieceNode.CreateComponent<Urho.Shapes.Plane>();
            framePiece.SetMaterial(Material.FromColor(leftColor, true));
        }

        // Imitates QTM's 3D frame colors on grid view
        private void GetFrameColors(out Urho.Color leftColor, out Urho.Color topColor, out Urho.Color rightColor, out Urho.Color bottomColor)
        {
            // Colors follow QTM conventions
            Urho.Color litFrame = Urho.Color.FromByteFormat(131, 131, 131, 255);
            Urho.Color shadowFrame = Urho.Color.FromByteFormat(29, 29, 29, 255);

            // Handle camera rotation in QTM
            switch (orientation)
            {
                default:
                    leftColor = shadowFrame;
                    topColor = shadowFrame;
                    rightColor = litFrame;
                    bottomColor = litFrame;
                    break;
                case 90:
                    leftColor = shadowFrame;
                    topColor = litFrame;
                    rightColor = litFrame;
                    bottomColor = shadowFrame;
                    break;
                case 180:
                    leftColor = litFrame;
                    topColor = litFrame;
                    rightColor = shadowFrame;
                    bottomColor = shadowFrame;
                    break;
                case 270:
                    leftColor = litFrame;
                    topColor = shadowFrame;
                    rightColor = shadowFrame;
                    bottomColor = litFrame;
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<MarkerStream, QTMRealTimeSDK.Data.Camera>(this, Messages.Subject.STREAM_DATA_SUCCESS.ToString() + Camera.ID);
            MessagingCenter.Unsubscribe<Arqus.DataModels.Camera, CameraMode>(this, Messages.Subject.STREAM_MODE_CHANGED.ToString() + Camera.ID);
            base.Dispose(disposing);
        }
    }
}
