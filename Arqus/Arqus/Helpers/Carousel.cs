using System;
using System.Collections.Generic;
using System.Text;
using Urho;

namespace Arqus.Visualization
{

    public class Carousel : CameraScreenLayout
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
        public int PivotX { get; set; }
        public int PivotY { get; set; }

        public float Min { get; set; }

        private int selected;

        public Carousel(double length, int itemCount, int pivotX, int pivotY)
        {
            // focus on the first position
            selected = 0;
            Length = length;
            ItemCount = itemCount;

            // Set center pivot of the carousel
            PivotX = pivotX;
            PivotY = pivotY;
            
            // Offset the minimum somewhat to emphasize on having at least a small distance from the screen
            Min -= 10;
        }
        

        private double GetAngle(int position)
        {
            // if the position we are retrieving an angle for is less
            // than the focused item increment the positional value with the
            // item count to account for how the relate to the focused item
            if (position < selected)
                position += ItemCount;

            position -= selected;

            return (2 * Math.PI / (float) ItemCount) * (float) position + Offset - Math.PI/2;
        }

        public override void Select(int id)
        {
            selected = id;
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
