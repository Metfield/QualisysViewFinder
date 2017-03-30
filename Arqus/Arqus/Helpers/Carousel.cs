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

        public Carousel(double length, int itemCount, int pivotX, int pivotY)
        {
            Length = length;
            ItemCount = itemCount;

            // Set center pivot of the carousel
            PivotX = pivotX;
            PivotY = pivotY;
        }

        public Position GetCoordinates(int position)
        {
            double angle = GetAngle(position);
            double x = Radius * Math.Cos(angle) + PivotX;
            double y = Radius * Math.Sin(angle) + PivotY;

            return new Position(x, y);
        }

        private double GetAngle(int position)
        {
            return (2 * Math.PI / (float) ItemCount) * (float) position + Offset;
        }
        
    }
}
