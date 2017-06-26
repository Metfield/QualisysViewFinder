using System;
using System.Collections.Generic;
using QTMRealTimeSDK.Data;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Arqus
{
    public class DemoMode : IDisposable
    {
        public List<List<Camera>> frames;
        private int frameCount;
        private string filename;

        public DemoMode(string demoFilename)
        {
            filename = demoFilename;
        }

        public void LoadDemoFile()
        {
            // Get assembly object
            Assembly assembly = typeof(DemoMode).Assembly;    

            // Get camera frames information
            using (Stream stream = assembly.GetManifestResourceStream("Arqus." + filename))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                frames = (List<List<Camera>>)binaryFormatter.Deserialize(stream);
            }

            frameCount = frames.Count;

            assembly = null;            
        }

        public int GetFrameCount()
        {
            return frameCount;
        }

        public void Dispose()
        {
            // Dispose of demo structure
            frames.Clear();
            frames = null;
        }
    }
}
