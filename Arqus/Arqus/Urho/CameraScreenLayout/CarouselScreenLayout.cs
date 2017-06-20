using Arqus.Helpers;
using Arqus.Services;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Urho;
using Urho.Actions;
using Xamarin.Forms.Internals;

namespace Arqus.Visualization
{

    public class CarouselScreenLayout : CameraScreenLayout
    {

        // Create carousel
        private float carouselInitialDistance = -80;

        // TODO: Fix number of camerascreens
        private float cameraMovementSpeed = 0.5f;
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
        public override Camera Camera { get; }

        private Orientation orientation;

        public CarouselScreenLayout(int itemCount, Camera camera)
        {
            // focus on the first position
            Selection = 0;
            Length = itemCount * 30;
            ItemCount = itemCount;
            Camera = camera;

            // Set center pivot of the carousel
            PivotX = 0;
            PivotY = Camera.Node.Position.Z + Radius + 20;
            
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

        float screenDistance = 25;

        public override void Select(int id)
        {

            if(Offset > 0)
                Offset = Offset - screenDistance;
            else if(Offset < 0)
                Offset = Offset + screenDistance;

            Selection = id;
        }
        
        
        public override void SetCameraScreenPosition(CameraScreen screen, DeviceOrientations orientation)
        {
            double x;
            double positionFromCameraFocus = 0;


            positionFromCameraFocus = screen.position - Selection;

            if (swipingDirection == SwipingDirection.RIGHT)
            {
                if (positionFromCameraFocus > 0)
                    positionFromCameraFocus = -(ItemCount - positionFromCameraFocus);
            }
            else if (swipingDirection == SwipingDirection.LEFT)
            {
                if (positionFromCameraFocus < 0)
                    positionFromCameraFocus = ItemCount + positionFromCameraFocus;
            }

            x = positionFromCameraFocus * screenDistance + Offset;
            

            double distance;
            
            if (orientation == DeviceOrientations.Portrait)
                distance = DataOperations.GetDistanceForFrustrumWidth(screen.Width, Camera.AspectRatio, Camera.Fov);
            else
                distance = DataOperations.GetDistanceForFrustrumHeight(screen.Height, Camera.Fov);


            if (screen.targetDistanceFromCamera != distance)
            {
                if (screen.position == Selection)
                    screen.Node.RunActionsAsync(new EaseBackOut(new MoveTo(0.25f, new Vector3(0, 0, (float)distance))));
                else if (screen.position != Selection)
                    screen.Node.RunActionsAsync(new EaseBackOut(new MoveTo(0.25f, new Vector3(0, 0, (float)distance * 1.5f))));
            }

            screen.Node.SetWorldPosition(new Vector3((float)x, 0, screen.Node.Position.Z));
        }

        double touchThrottleTime;

        enum SwipingDirection
        {
            LEFT,
            RIGHT,
            NONE
        }

        SwipingDirection swipingDirection;

        public override void OnTouch(Input input, TouchMoveEventArgs eventArgs)
        {
            
            if (input.NumTouches == 1)
            {
                
                // We want to scroll 
                Offset += eventArgs.DX * cameraMovementSpeed;

                double deltaTime = Time.SystemTime - touchThrottleTime;


                if (eventArgs.DX > 0)
                    swipingDirection = SwipingDirection.RIGHT;
                else if (eventArgs.DX < 0)
                    swipingDirection = SwipingDirection.LEFT;

                // Check if we are panning or just scrolling the carousel 
                // based on current zoom
                if (deltaTime > 200)
                {
                    if (Offset > 30 / 2)
                    {
                        Select(Selection - 1 < 1 ? ItemCount : Selection - 1);
                    }
                    else if (Offset < -30 / 2)
                    {
                        Select((Selection + 1 > ItemCount) ? 1 : Selection + 1);
                    }

                    touchThrottleTime = Time.SystemTime;
                }
                else
                {
                    // We want to Pan
                    //Camera.Pan(eventArgs.DX, eventArgs.DY, cameraMovementSpeed * 5, carouselInitialDistance);

                }

            }
            else if (input.NumTouches >= 2)
            {
                // Get Touchstates
                TouchState fingerOne = input.GetTouch(0);
                TouchState fingerTwo = input.GetTouch(1);

                // HACK: Current max is not calculated, this should be fixed to more closesly corelate to
                // what a full screen actually is...
                Camera.PinchAndZoom(fingerOne, fingerTwo, carouselInitialDistance, carouselInitialDistance + 20);
            }

        }

        int tapTouchID;
        float startTouchTime;
        //float tapTimeMargin = 0.1f;

        public override void OnTouchBegan(TouchBeginEventArgs eventArgs)
        {
            touchThrottleTime = Time.SystemTime;
        }


        public override void OnTouchReleased(Input input, TouchEndEventArgs eventArgs)
        {
            Offset = 0;
        }
        
    }
}
