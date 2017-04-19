using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Helpers
{
    public class Position
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Position(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class Carousel
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

        public int ItemCount { get; set; }
        public float Offset { get; set; }
        public int PivotX { get; set; }
        public int PivotY { get; set; }

        public float Min { get; set; }

        private int focus;

        public Carousel(double length, int itemCount, int pivotX, int pivotY)
        {
            // focus on the first position
            focus = 0;
            Length = length;
            ItemCount = itemCount;

            // Set center pivot of the carousel
            PivotX = pivotX;
            PivotY = pivotY;

            // The minimum position without going beyond the center of the focused
            // item in the carousel
            Min = (float) GetCoordinatesForPosition(0).Y;
            // Offset the minimum somewhat to emphasize on having at least a small distance from the screen
            Min -= 10;
        }

        public Position GetCoordinatesForPosition(int position)
        {
            double angle = GetAngle(position);
            double x = Radius * Math.Cos(angle) + PivotX;
            double y = Radius * Math.Sin(angle) + PivotY;

            return new Position(x, y);
        }
        

        private double GetAngle(int position)
        {
            // if the position we are retrieving an angle for is less
            // than the focused item increment the positional value with the
            // item count to account for how the relate to the focused item
            if (position < focus)
                position += ItemCount;

            position -= focus;

            return (2 * Math.PI / (float) ItemCount) * (float) position + Offset - Math.PI/2;
        }

        public void SetFocus(int focus)
        {
            this.focus = focus;
            Offset = 0;
        }                
    }
}
