using Arqus.Visualization;
using System;
using Urho;

namespace Arqus
{
    /// <summary>
    /// Loading spinner to be used when no content can currently be displayed
    /// in an Urho component
    /// </summary>
    public class LoadingSpinner
    {
        private Node[] circles;
        private Node root;
        private float offset = 0;

        public uint NumberOfCircles { get; set; }
        public int Radius { get; set; }
        public float Speed { get; set; }
        public bool Running { get; set; }

        public LoadingSpinner(Node root, uint numberOfCircles, int radius, float speed = 20.0f, float circleSize = 0.001f)
        {
            // We need a root node, it is recommended that this does not contain any other components to prevent unexpected behavior
            this.root = root;
            NumberOfCircles = numberOfCircles;
            Radius = radius;
            Speed = speed;
            circles = new Node[NumberOfCircles];
            
            double angle = 2 * Math.PI / NumberOfCircles;
            for (int i = 0; i < NumberOfCircles; i++)
            {
                Node circleNode = root.CreateChild();
                Circle circle = circleNode.CreateComponent<Circle>();
                circle.Enabled = true;
                circle.Redraw(0, 0, circleSize, circleSize, 999, 999);
                circles[i] = circleNode;

                float x = Radius * (float)Math.Sin(i * angle);
                float y = Radius * (float)Math.Cos(i * angle);
                circles[i].SetPosition2D(x, y);
            }
            
        }

        /// <summary>
        /// Updates the spinner with a snakeball animation
        /// </summary>
        /// <param name="timeStep">The timestamp used to determine the offset of the animation</param>
        public void UpdateSpinner(float timeStep)
        {
            // Get the current offset in relation to the timestep since that will be used to update the animation
            offset = (offset + timeStep * Speed) % NumberOfCircles;

            for (uint circle = 0; circle < NumberOfCircles; circle++)
            {
                if(circles[circle].Enabled)
                {
                    // Calculate the difference between the offset and the current circle to determine the scale value
                    float diff = circle - offset;

                    if (diff < 0)
                        diff = NumberOfCircles + diff;

                    // Cap the scale to be no lower than 0 to prevent it from scaling in "reverse"
                    float scale = 1 - diff > 0 ? 0 : 1 - diff;
                    
                    circles[circle].SetScale2D(new Vector2(scale, scale));
                }
               
            }
        }

        public void Start()
        {
            if(!Running)
            {
                Running = true;
                root.SetEnabledRecursive(true);
            }
        }
        
        public void Stop()
        {
            if(Running)
            {
                Running = false;
                root.SetEnabledRecursive(false);
            }
        }
        
        
    }
}
