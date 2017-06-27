using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Services
{

    public enum DeviceOrientations
    {
        Undefined,
        Landscape,
        Portrait
    }

    interface IOrientation
    {
        DeviceOrientations GetOrientation();
    }
}
