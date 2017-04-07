using Arqus.Helpers;
using Arqus.Components;
using Urho;
using ImageSharp.Formats;
using Urho.Urho2D;
using QTMRealTimeSDK;

namespace Arqus.Visualization
{

    public enum CameraScreenMode
    {
        Marker,
        Intensity,
        Video,
        Video_MarkerOverlay
    }

    /// <summary>
    /// Displays and visualizes 2D streaming data on its children
    /// </summary>
    public class CameraScreen : Component
    {
        // Shared between instances
        static int screenCount;

        // General properties
        public int CameraID { private set; get; }
        public int position;
        
        private Node screenNode;
        private StaticModel frame;
        public ImageResolution Resolution { private set; get; }

        public float Width { private set; get; }
        public float Height { private set; get; }
        
        public Material Material { get; set; }
        private Material backgroundMaterial;

        private delegate void UpdateDelegate();
        private UpdateDelegate OnUpdateHandler;

        public CameraMode CurrentCameraMode { get; set; }

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
        private bool dirty;
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
        

        public CameraScreen(int cameraID, ImageResolution resolution, float frameHeight, float frameWidth, Urho.Color backgroundColor)
        {
            CameraID = cameraID;
            Resolution = resolution;

            Height = frameHeight;
            Width = frameWidth;

            ReceiveSceneUpdates = true;
            OnUpdateHandler += OnMarkerUpdate;

            backgroundMaterial = Material.FromColor(backgroundColor);

            // Set position in relation to the number of cameras that are already initialized
            // so the screens can be positioned accordingly
            position = screenCount;
            screenCount++;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);
            
            screenNode = node.CreateChild();
            screenNode.Scale = new Vector3(Height, 0, Width);

            Pool = new MarkerSpherePool(20, screenNode);
            
            // Rotate the plane to look at the camera
            // TODO: Try to multiply
            screenNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
            // Rotate the camera in the clockwise direction with 90 degrees
            screenNode.Rotate(new Quaternion(0, 90, 0), TransformSpace.Local);

            // Full namespace to avoid ambiquity
            frame = screenNode.CreateComponent<Urho.Shapes.Plane>();
            texture = new Texture2D();
            texture.SetNumLevels(1);

            texture.SetSize(Resolution.Width, Resolution.Height, Urho.Graphics.RGBAFormat, TextureUsage.Dynamic);

            Material = new Material();
            Material.SetTexture(TextureUnit.Diffuse, texture);
            Material.SetTechnique(0, CoreAssets.Techniques.DiffUnlit, 0, 0);
            frame.SetMaterial(Material);
        }
        
        private void CleanCamera()
        {
            switch(CurrentCameraMode)
            {
                case CameraMode.ModeMarker:
                    CleanMarker();
                    break;
                case CameraMode.ModeMarkerIntensity:
                    CleanIntensity();
                    break;
                default:
                    throw new System.Exception("Camera cleanup for this camera mode is not yet implemented");
            }
        }

        private void CleanMarker()
        {
            //Pool.Hide();
        }

        private void CleanIntensity()
        {

        }

        private void SetIntensityMode()
        {
            CurrentCameraMode = CameraMode.ModeMarkerIntensity;
            OnUpdateHandler = OnIntensityUpdate;
        }

        private void SetMarkerMode()
        {
            CurrentCameraMode = CameraMode.ModeMarker;
            OnUpdateHandler = OnMarkerUpdate;
        }

        public void SetMode(CameraMode mode)
        {
            // If camera is already running in the 
            if (CurrentCameraMode == mode)
                return;

            // Clean camera before setting new camera mode
            CleanCamera();
            
            // Clear the update handler to more predictibly determine which
            // methods will be called during update
            OnUpdateHandler = null;

            CurrentCameraMode = mode;

            switch (mode)
            {
                case CameraMode.ModeMarker:
                    SetMarkerMode();
                    break;
                case CameraMode.ModeMarkerIntensity:
                    SetIntensityMode();
                    break;
                default:
                    break;
            }
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

            OnUpdateHandler?.Invoke();

        }

        
        private void OnMarkerUpdate()
        {
            if(dirty)
            {
                // This index will be used as an array pointer to help identify and disable
                // markers which are not being currently used
                int lastUsedInArray = 0;

                // Iterate through the marker array, transform and draw spheres
                for (int i = 0; i < markerData.MarkerCount; i++)
                {
                    // Transform from camera coordinates to frame coordinates
                    float adjustedX = DataOperations.ConvertRange(0, Resolution.Width, -0.5f, 0.5f, markerData.MarkerData2D[i].X / 64);
                    float adjustedY = DataOperations.ConvertRange(0, Resolution.Height, -0.5f, 0.5f, markerData.MarkerData2D[i].Y / 64);
                    float adjustedScaleX = DataOperations.ConvertRange(0, Resolution.Width, 0, 1, markerData.MarkerData2D[i].DiameterX / 64);
                    float adjustedScaleY = DataOperations.ConvertRange(0, Resolution.Height, 0, 1, markerData.MarkerData2D[i].DiameterY / 64);

                    MarkerSphere sphere = Pool.Get(i);
                    // Set world position with new frame coordinates            
                    sphere.Position = new Vector3(adjustedY, 100, adjustedX);
                    sphere.Scale = new Vector3(adjustedScaleY, sphere.Scale.Y, adjustedScaleX);

                    // Last element will set this variable
                    //lastUsedInArray = i;
                }

                // Hide the markers which were not used on this frame
                //Pool.HideUnused(lastUsedInArray);
            }

        }

        private void OnIntensityUpdate()
        {
            if(dirty)
            {
                dirty = false;
                UpdateMaterialTexture(imageData);
            }
        }

    }
}
