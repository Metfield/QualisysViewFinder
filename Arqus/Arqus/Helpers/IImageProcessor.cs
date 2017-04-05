using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus
{
    public interface IImageProcessor
    {
        byte[] DecodeJPG(byte[] data);
    }
}
