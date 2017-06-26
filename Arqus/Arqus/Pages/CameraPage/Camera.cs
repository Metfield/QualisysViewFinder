
﻿using Arqus.Helpers;

using System.Diagnostics;
using System.Threading.Tasks;
        // public properties
        private CameraMode mode;

        public bool LensControlEnabled { get; private set; }

        public string Model { get; private set; }
        public CameraProfile Profile { get; set; }
        public CameraMode Mode { get { return mode;  }  set { SetProperty(ref mode, value); } }
        public int Orientation { get; private set; }

            Model = GetModelName(settings.Model);
            Orientation = settings.Orientation;

            if(settings.LensControl.Focus.Max != 0 ||
               settings.LensControl.Focus.Min != 0 ||
               settings.LensControl.Focus.Value != 0)
            {
                LensControlEnabled = true;
            }

        }        


        /// Select the camera

        }        


        }

        /// <summary>
        /// Check to see if the camera is in image mode, such as video or intensity
        /// </summary>
        /// <returns>true if the camera is running in image mode</returns>
        private bool IsImageMode()
        {
            return Mode != CameraMode.ModeMarker;

        // Gets model string
        // TODO: Keep an eye out for changes hier
        private string GetModelName(CameraModel cameraModel)
        {
            switch (cameraModel)
            {
                case CameraModel.ModelQqus100:
                    return "Oqus 100 ";

                case CameraModel.ModelQqus200C:
                    return "Oqus 200 C";

                case CameraModel.ModelQqus300:
                    return "Oqus 300";

                case CameraModel.ModelQqus300Plus:
                    return "Oqus 300 Plus";

                case CameraModel.ModelQqus400:
                    return "Oqus 400";

                case CameraModel.ModelQqus500:
                    return "Oqus 500";

                case CameraModel.ModelQqus500Plus:
                    return "Oqus 500 Plus";

                case CameraModel.ModelQqus700:
                    return "Oqus 700";

                case CameraModel.ModelQqus700Plus:
                    return "Oqus 700 Plus";

                case CameraModel.ModelMiqusM1:
                    return "Miqus M1";

                case CameraModel.ModelMiqusM3:
                    return "Miqus M3";