using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus
{
    public class MarkerFrequency
    {
        public int Max { get; set; }
        public int Min { get; set; }
        public int Value { get; set; }
    }

    public class AllCameras
    {
        public MarkerFrequency MarkerFrequency { get; set; }
    }

    public class ExportC3d
    {
        public bool ExcludeEmpty { get; set; }
        public bool ExcludeNonFullFrames { get; set; }
        public bool ExcludeUnidentified { get; set; }
        public string FileName { get; set; }
        public bool FullLabels { get; set; }
        public bool UseCroppedStartTimeForEvents { get; set; }
        public bool UseZeroBaseline { get; set; }
        public int ZeroBaselineStart { get; set; }
        public int ZeroBaselineStop { get; set; }
    }

    public class ExportMat
    {
        [JsonProperty(PropertyName = "3D")]
        public bool Is3D { get; set; }
        [JsonProperty(PropertyName = "6D")]
        public bool Is6D { get; set; }
        public bool Analog { get; set; }
        public bool Events { get; set; }
        public bool ExcludeUnidentified { get; set; }
        public bool EyeTracker { get; set; }
        public string FileName { get; set; }
        public bool Force { get; set; }
        public bool SMPTE { get; set; }
    }

    public class ExportTsv
    {
        public bool Analog { get; set; }
        public bool ColumnHeaders { get; set; }
        public bool Events { get; set; }
        public bool ExcludeEmpty { get; set; }
        public bool ExcludeNonFullFrames { get; set; }
        public bool ExcludeUnidentified { get; set; }
        public int ExportType { get; set; }
        public bool EyeTracker { get; set; }
        public string FileName { get; set; }
        public bool Force { get; set; }
        public bool FrameTimes { get; set; }
        public bool Header { get; set; }
        public string NullString { get; set; }
    }

    public class Twin
    {
        public string FileName { get; set; }
        public bool MergeTrajectories { get; set; }
    }

    public class Reprocess
    {
        public bool ApplyAim { get; set; }
        public bool Calculate6Dof { get; set; }
        public bool CalculateForce { get; set; }
        public bool CalculateGaze { get; set; }
        public bool GapFill { get; set; }
        public bool MergeTwin { get; set; }
        public bool Preprocess2d { get; set; }
        public bool Reidentify { get; set; }
        public bool Track { get; set; }
        public bool Track2d { get; set; }
        public Twin Twin { get; set; }
    }

    public class ExternalTimebase
    {
        public int Delay { get; set; }
        public int Divisor { get; set; }
        public bool IsEnabled { get; set; }
        public int Multiplier { get; set; }
        public int NominalFrequency { get; set; }
        public string SignalEdge { get; set; }
        public string SignalMode { get; set; }
        public string SignalSource { get; set; }
        public int Timeout { get; set; }
        public int Tolerance { get; set; }
        public bool UseNominalFrequency { get; set; }
    }

    public class Pretrigger
    {
        public bool IsEnabled { get; set; }
        public int Time { get; set; }
    }

    public class SMPTE
    {
        public int Frequency { get; set; }
        public bool UseTimestamp { get; set; }
    }

    public class Event
    {
        public double HoldOffTime { get; set; }
        public string SignalEdge { get; set; }
    }

    public class NormallyClosed
    {
        public string SignalEdge { get; set; }
        public bool StartOnTrigger { get; set; }
        public bool StopOnTrigger { get; set; }
    }

    public class NormallyOpen
    {
        public string SignalEdge { get; set; }
        public bool StartOnTrigger { get; set; }
        public bool StopOnTrigger { get; set; }
    }

    public class Trigger
    {
        public int Delay { get; set; }
        public Event Event { get; set; }
        public bool GenerateEvents { get; set; }
        public double HoldOffTime { get; set; }
        public int MinimumTimeBetweenStartStop { get; set; }
        public NormallyClosed NormallyClosed { get; set; }
        public NormallyOpen NormallyOpen { get; set; }
        public bool WaitForSoftwareTrigger { get; set; }
    }

    public class Timing
    {
        public ExternalTimebase ExternalTimebase { get; set; }
        public Pretrigger Pretrigger { get; set; }
        public SMPTE SMPTE { get; set; }
        public Trigger Trigger { get; set; }
    }

    public class Settings
    {
        public AllCameras AllCameras { get; set; }
        public List<object> Cameras { get; set; }
        public ExportC3d ExportC3d { get; set; }
        public ExportMat ExportMat { get; set; }
        public ExportTsv ExportTsv { get; set; }
        public Reprocess Reprocess { get; set; }
        public Timing Timing { get; set; }
    }

}

