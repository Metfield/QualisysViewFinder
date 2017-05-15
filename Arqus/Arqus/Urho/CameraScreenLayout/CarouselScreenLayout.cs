using System;
using System.Collections.Generic;
using System.Text;
using Urho;

namespace Arqus.Visualization
{

    public class CarouselScreenLayout : CameraScreenLayout
    {        
        public double Radius
        {
            get { return length / (2 * Math.PI); }
        }

        private double length;

        public double Length
        {
            get { return length; }
            set { length = value; }
        }

        public override int ItemCount { get; set; }
        public override float Offset { get; set; }
        public double PivotX { get; set; }
        public double PivotY { get; set; }

        public float Min { get; set; }
        

        public CarouselScreenLayout(int itemCount, Camera camera)
        {
            // focus on the first position
            Selection = 0;
            Length = itemCount * 30;
            ItemCount = itemCount;

            // Set center pivot of the carousel
            PivotX = 0;
            PivotY = camera.Node.Position.Z + Radius + 20;
            
            // Offset the minimum somewhat to emphasize on having at least a small distance from the screen
            Min -= 10;
        }
        

        private double GetAngle(int position)
        {
            // if the position we are retrieving an angle for is less
            // than the focused item increment the positional value with the
            // item count to account for how the relate to the focused item
            if (position < Selection)
                position += ItemCount;

            position -= Selection;

            return (2 * Math.PI / (float) ItemCount) * (float) position + Offset - Math.PI/2;
        }

        public override void Select(int id)
        {
            Selection = id;
            Offset = 0;
        }

        public override void SetCameraScreenPosition(CameraScreen screen)
        {
            double angle = GetAngle(screen.position);
            double x = Radius * Math.Cos(angle) + PivotX;
            double y = Radius * Math.Sin(angle) + PivotY;
            screen.Node.SetWorldPosition(new Vector3((float)x, 0, (float)y));
        }
    }
}
