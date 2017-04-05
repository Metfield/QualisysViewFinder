using Android.Content;
using Android.Util;
using Com.Bumptech.Glide;
using System;

namespace Arqus.Droid
{
    class ImageProcessor : IImageProcessor
    {
        private Context _context;

        public ImageProcessor(Context context)
        {
            _context = context;
        }

        public byte[] DecodeJPG(byte[] data)
        {
            Log.Info("arqus", "Attempting to decode JPG");
            Glide.With(_context)
                .FromBytes()
                .Load(data);

            return new byte[0];
        }
    }
}