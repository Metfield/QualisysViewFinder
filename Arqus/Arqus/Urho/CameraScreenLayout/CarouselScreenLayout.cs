using System;

using Urho;
using Urho.Actions;
using Xamarin.Forms;

using Arqus.Helpers;
using Arqus.Services;

namespace Arqus.Visualization
{
    public class CarouselScreenLayout : CameraScreenLayout
    {
        private double length;
        private float scrollSpeed = 0.045f;

        public double Radius
        {
            get { return length / (2 * Math.PI); }
        }

        public double Length
        {
            get { return length; }
            set { length = value; }
        }

        public override int ItemCount { get; set; }
        public override float Offset { get; set; }
        public double PivotX { get; set; }
        public double PivotY { get; set; }
        public override Camera Camera { get; }
        
        public CarouselScreenLayout(int itemCount, Camera camera)
        {
            // focus on the first position
            Selection = 0;
            Length = itemCount * screenDistance;
            ItemCount = itemCount;
            Camera = camera;
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

        float screenDistance = 15;
        float halfScreenDistance
        {
            get
            {
                return screenDistance * 0.5f;
            }
        }

        private bool selected = false;

        public override void Select(int id)
        {
            if(Offset > 0)
                Offset = Offset - screenDistance;
            else if(Offset < 0)
                Offset = Offset + screenDistance;

            Selection = id;
            selected = true;

            // Only communicate camera selection when user is not scrolling
            if (Offset == 0)
                MessagingCenter.Send(this, Messages.Subject.SET_CAMERA_SELECTION, Selection);
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
            
            double distance = 0;
            
            if (orientation == DeviceOrientations.Portrait)
            {
                switch (screen.Camera.Orientation)
                {
                    case 0:
                    case 180:
                        distance = DataOperations.GetDistanceForFrustrumWidth(screen.Width, Camera.AspectRatio, Camera.Fov);
                        break;
                    case 90:
                    case 360:
                        distance = DataOperations.GetDistanceForFrustrumWidth(screen.Height, Camera.AspectRatio, Camera.Fov);
                        break;
                }
            }
            else
            {
                // Increades the screen distance in the x axis when in landscape mode
                // TODO: Remove magic number and determine how much space is needed to get off screen
                x = x * 2;
                switch(screen.Camera.Orientation)
                {
                    case 0:
                    case 180:
                        distance = DataOperations.GetDistanceForFrustrumHeight(screen.Height, Camera.Fov);
                        break;
                    case 90:
                    case 360:
                        distance = DataOperations.GetDistanceForFrustrumHeight(screen.Width, Camera.Fov);
                        break;
                }
            }

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

        private float swipeThreshold = 50.0f;
        private float swipeSelectionThrottle = 500;

        public override void OnTouch(Input input, TouchMoveEventArgs eventArgs)
        {
            if (input.NumTouches == 1)
            {
                if (Camera.Zoom == 1)
                {
                    if (Math.Abs(eventArgs.DX) > swipeThreshold && swipeSelectionThrottle < Time.SystemTime)
                    {
                        swipeSelectionThrottle = Time.SystemTime + 500;
                        SelectNeighbour(eventArgs.DX > swipeThreshold);
                        return;
                    }

                    // We want to scroll 
                    Offset += eventArgs.DX * scrollSpeed;

                    double deltaTime = Time.SystemTime - touchThrottleTime;

                    if (eventArgs.DX > 0)
                        swipingDirection = SwipingDirection.RIGHT;
                    else if (eventArgs.DX < 0)
                        swipingDirection = SwipingDirection.LEFT;

                    // Check if we are panning or just scrolling the carousel 
                    // based on current zoom
                    if (deltaTime > 200)
                    {
                        // If the offset is greater than half the screen distance then the neighbouring camera is closer 
                        // to the center and thus is selected.
                        if (Math.Abs(Offset) > Math.Abs(halfScreenDistance))
                        {
                            SelectNeighbour(Offset > halfScreenDistance);
                        }

                        touchThrottleTime = Time.SystemTime;
                    }
                }
                else
                {
                    // We want to Pan
                    Camera.Pan(eventArgs.DX,
                        eventArgs.DY,
                        0.005f,
                        true,
                        CameraManager.CurrentCamera.Screen.Height / 2,
                        -CameraManager.CurrentCamera.Screen.Height / 2,
                        CameraManager.CurrentCamera.Screen.Width / 2,
                        -CameraManager.CurrentCamera.Screen.Width / 2);
                }

            }
            else if (input.NumTouches >= 2)
            {
                // Get Touchstates
                TouchState fingerOne = input.GetTouch(0);
                TouchState fingerTwo = input.GetTouch(1);

                Camera.Zoom += Gestures.GetZoomAmountFromPinch(fingerOne, fingerTwo) * 0.2f;

                if (Camera.Zoom < 1)
                {
                    Camera.Zoom = 1;

                    // Reset panning instantly instead of waiting for next finger touch
                    Camera.Pan(eventArgs.DX, eventArgs.DY);
                }
            }
        }

        private void SelectNeighbour(bool leftNeighbour)
        {
            if (leftNeighbour)
            {
                int neighbour = (Selection - 1 < 1) ? ItemCount : Selection - 1;
                Select(neighbour);
            }
            else
            {
                int neighbour = (Selection + 1 > ItemCount) ? 1 : Selection + 1;
                Select(neighbour);
            }
        }

        public override void OnTouchBegan(TouchBeginEventArgs eventArgs)
        {
            CameraManager.EnableCameraScreens(true, Selection);
            touchThrottleTime = Time.SystemTime;
        }

        public override void OnTouchReleased(Input input, TouchEndEventArgs eventArgs)
        {
            // Only communicate camera selection when user is not scrolling
            if (selected)
            {
                MessagingCenter.Send(this, Messages.Subject.SET_CAMERA_SELECTION, Selection);
                selected = false;
            }

            CameraManager.EnableCameraScreens(false, Selection);
            Offset = 0;
        }
    }
}
