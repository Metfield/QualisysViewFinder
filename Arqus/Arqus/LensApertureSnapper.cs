

using QTMRealTimeSDK.Settings;
using System;

namespace Arqus
{
    // Used for snapping slider to pre-determined values
    public class LensApertureSnapper : IDisposable
    {
        // Look-up table with pre-defined values
        // 31 Positions
        private readonly double[] snapLookupT = new double[] { 1.0, 1.1, 1.2, 1.4, 1.6, 1.8, 2, 2.2, 2.5, 2.8, 3.2, 3.5, 4, 4.5, 5.0, 5.6, 6.3, 7.1, 8, 9, 10, 11, 13, 14, 16, 18, 20, 22, 25, 29, 32 };

        public int LookupTSize { get; private set; }

        // Actual value to send and display in label
        public double snappedValue;

        // Needed to actually set values
        CameraPageViewModel cameraPageViewModel;

        public LensApertureSnapper(CameraPageViewModel cpvm)
        {
            LookupTSize = snapLookupT.Length;
            cameraPageViewModel = cpvm;
        }
        
        public double OnSliderValueChanged(double value)
        {           
            int lookupIndex = (int)value;
            snappedValue = snapLookupT[lookupIndex];

            return ClampToRange(lookupIndex);
        }

        // Adjust value to range 
        private double ClampToRange(int index)
        {
            float minValue = cameraPageViewModel.CurrentCamera.Settings.LensControl.Aperture.Min;
            float maxValue = cameraPageViewModel.CurrentCamera.Settings.LensControl.Aperture.Max;

            // Handle Min
            while ((snappedValue < minValue) && index < LookupTSize)
            {
                snappedValue = snapLookupT[index++];
            }

            // Handle Max
            while ((snappedValue > maxValue) && index > 0)
            {
                snappedValue = snapLookupT[index--];
            }

            return snappedValue;
        }

        public void Dispose()
        {
            Array.Clear(snapLookupT, 0, LookupTSize);
        }
    }
}