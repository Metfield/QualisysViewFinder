using Arqus.Helpers;
using Arqus.Components;
using Urho;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using ImageSharp.Formats;
using Urho.Urho2D;
using System.Threading.Tasks;
using ImageSharp;
using Xamarin.Forms;
using System.Diagnostics;

namespace Arqus.Visualization
{
    /// <summary>
    /// Displays and visualizes 2D streaming data on its children
    /// </summary>
    public class CameraScreen : Component
    {
        public MarkerSpherePool Pool { set; get; }
        static int screenCount;
        public int position;

        public double CenterX { get; set; }
        public double CenterY { get; set; }

        private Node screenNode;
        public int CameraID { private set; get; }
        public ImageResolution Resolution { private set; get; }

        public float Height { private set; get; }
        public float Width { private set; get; }

        public ImageSharp.Color[] ImageData { get; set; }

        private Material backgroundMaterial;
        private Texture2D texture;
        public Material Material { get; set; }
        private Urho.Resources.Image Image;
        private IImageProcessor imageProcessor;
        JpegDecoder decoder;

        public int currentFrameNumber, updatedFrameNumber;

        public CameraScreen(int cameraID, ImageResolution resolution)
        {
            // Set position according to screenCount and increment the counter
            position = screenCount;
            screenCount++;

            CameraID = cameraID;
            Resolution = resolution;
            ReceiveSceneUpdates = true;
            ImageData = new ImageSharp.Color[1823*1087];
            decoder = new JpegDecoder();
            backgroundMaterial = null;
            
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);
            
            screenNode = node.CreateChild();
            Pool = new MarkerSpherePool(20, node.CreateChild());

            Height = 50;
            Width = Resolution.PixelAspectRatio * Height;

            screenNode.Scale = new Vector3(Height, 0, Width);
            screenNode.Rotate(new Quaternion(-90, 0, 0), TransformSpace.Local);
            screenNode.Rotate(new Quaternion(0, 90, 0), TransformSpace.Local);

            // Full namespace to avoid ambiquity
            var frame = screenNode.CreateComponent<StaticModel>();
            frame.Model = CoreAssets.Models.Plane;

            // Initialize the background material if it was not defined during instantiation
            //if(backgroundMaterial == null)
            //{
            //    backgroundMaterial = Material.FromColor(new Color(0f, 0.1f, 0.1f), true);
            //}

            // By default the material of the camera screen will be the background color
            // Override the Material property when updating the frame of the screen
            texture = new Texture2D();
            texture.SetNumLevels(1);

            texture.SetSize(1824, 1088, Urho.Graphics.RGBAFormat, TextureUsage.Dynamic);

            Material = new Material();
            Material.SetTexture(TextureUnit.Diffuse, texture);

            //Material.SetUVTransform(new Vector2(Height, Width), 0, 0);

            Material.SetTechnique(0, CoreAssets.Techniques.DiffUnlit, 0, 0);
            frame.SetMaterial(Material);
        }


       

        public unsafe bool UpdateMaterialTexture(ImageSharp.Color[] imageData)
        {
            fixed (ImageSharp.Color* bptr = imageData)
            {
                return texture.SetData(0, 0, 0, 1824, 1088, bptr);
            }
        }
        

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
        }
        
    }
}
