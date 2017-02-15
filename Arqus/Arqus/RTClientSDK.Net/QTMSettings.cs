﻿// Realtime SDK for Qualisys Track Manager. Copyright 2015-2017 Qualisys AB
//
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using QTMRealTimeSDK.Data;

namespace QTMRealTimeSDK.Settings
{
    /// <summary>General Settings from QTM</summary>
    [XmlRoot("General")]
    public class SettingsGeneral
    {
        /// <summary>QTM Capture frequency </summary>
        [XmlElement("Frequency")]
        public int captureFrequency;

        /// <summary>length of QTM Capture. Time expressed in seconds</summary>
        [XmlElement("Capture_Time")]
        public float captureTime;

        /// <summary>Measurement start on any external trigger (qtm version 2.13 or less, or 2.14 running Oqus systems)</summary>
        [XmlElement("Start_On_External_Trigger")]
        public bool startOnExternalTrigger;

        /// <summary>Measurement start on MSU Trig NO/Oqus Trig in</summary>
        [XmlElement("Start_On_Trigger_NO")]
        public bool startOnTrigNO;

        /// <summary>Measurement start on MSU Trig NC</summary>
        [XmlElement("Start_On_Trigger_NC")]
        public bool startOnTrigNC;

        /// <summary>Measurement start on software trigger (app, keyboard etc)</summary>
        [XmlElement("Start_On_Trigger_Software")]
        public bool startOnTrigSoftware;

        [XmlElement("External_Time_Base")]
        public SettingsExternalTimeBase externalTimebase;

        [XmlElement("Processing_Actions")]
        public SettingProcessingActions processingActions;

        [XmlElement("RealTime_Processing_Actions")]
        public SettingProcessingActions realtimeProcessingActions;

        [XmlElement("Reprocessing_Actions")]
        public SettingProcessingActions ReprocessingActions;

        /// <summary>Camera Settings </summary>
        [XmlElement("Camera")]
        public List<SettingsGeneralCameraSystem> cameraSettings;

        public SettingsGeneral()
        {
        }
    }

    /// <summary>3D Bone Settings from QTM</summary>
    public class SettingsBone
    {
        /// <summary>name of marker bone starts from </summary>
        [XmlAttribute("From")]
        public string from;

        /// <summary>name of marker bone ends at</summary>
        [XmlAttribute("To")]
        public string to;

        /// <summary>Color of marker bone</summary>
        [XmlAttribute("Color")]
        public int color;

        SettingsBone()
        {
            color = 0xEEEEEE;
        }
    }

    /// <summary>3D Settings from QTM</summary>
    [XmlRoot("The_3D")]
    public class Settings3D
    {
        [XmlElement("AxisUpwards")]
        public Axis axisUpwards;
        [XmlElement("CalibrationTime")]
        public string calibrationTime;
        [XmlElement("Labels")]
        public int labelsCount;
        [XmlElement("Label")]
        public List<Settings3DLabel> labels3D;

        [XmlArray("Bones")]
        [XmlArrayItem("Bone", typeof(SettingsBone))]
        public SettingsBone[] bones;

        public Settings3D()
        {
        }
    }

    /// <summary>6D Settings from QTM</summary>
    [XmlRoot("The_6D")]
    public class Settings6D
    {
        public Settings6D()
        {
            eulerNames = new EulerNames();
        }

        [XmlElement("Bodies")]
        public int bodyCount;
        [XmlElement("Body")]
        public List<Settings6DOF> bodies;
        [XmlElement("Euler")]
        public EulerNames eulerNames;
    }

    [XmlRoot("Euler")]
    public class EulerNames
    {
        public EulerNames()
        {
            First = "Roll";
            Second = "Pitch";
            Third = "Yaw";
        }

        [XmlElement("First"), ]
        public string First;
        [XmlElement("Second")]
        public string Second;
        [XmlElement("Third")]
        public string Third;

    }

    /// <summary>Analog Settings from QTM</summary>
    [XmlRoot("Analog")]
    public class SettingsAnalog
    {
        [XmlElement("Device")]
        public List<AnalogDevice> devices;

        public SettingsAnalog()
        {

        }
    }

    /// <summary>Force Settings from QTM</summary>
    [XmlRoot("Force")]
    public class SettingsForce
    {
        [XmlElement("Unit_Length")]
        public string unitLength;
        [XmlElement("Unit_Force")]
        public string unitForce;
        [XmlElement("Plate")]
        public List<ForcePlateSettings> plates;

        public SettingsForce() { }
    }

    /// <summary>Image Settings from QTM</summary>
    [XmlRoot("Image")]
    public class SettingsImage
    {
        [XmlElement("Camera")]
        public List<ImageCamera> cameraList;
    }

    /// <summary>Gaze vector Settings from QTM</summary>
    [XmlRoot("Gaze_Vector")]
    public class SettingsGazeVector
    {
        [XmlElement("Name")]
        public string Name;
    }

    /// <summary>General settings for Camera System</summary>
    public struct SettingsGeneralCameraSystem
    {
        /// <summary>ID of camera</summary>
        [XmlElement("ID")]
        public int CameraId;
        /// <summary>Model of camera</summary>
        [XmlElement("Model")]
        public CameraModel Model;
        /// <summary>If the camera is an underwater camera</summary>
        [XmlElement("UnderWater")]
        public bool UnderWater;
        /// <summary>If the camera supports hardware sync (like Oqus and Miqus Sync Units)</summary>
        [XmlElement("Supports_HW_Sync")]
        public bool SupportsHardwareSync;
        /// <summary>Serial number of the selected camera</summary>
        [XmlElement("Serial")]
        public int Serial;
        /// <summary>Camera mode the camera is set to</summary>
        [XmlElement("Mode")]
        public CameraMode Mode;
       /// <summary>Values for camera video mode, current, min and max</summary>
        [XmlElement("Video_Frequency")]
        public int VideoFrequency;
        /// <summary>Values for camera video exposure, current, min and max</summary>
        [XmlElement("Video_Exposure")]
        public CameraSetting VideoExposure;
        /// <summary>Values for camera video flash time, current, min and max</summary>
        [XmlElement("Video_Flash_Time")]
        public CameraSetting VideoFlashTime;
        /// <summary>Values for camera marker exposure, current, min and max</summary>
        [XmlElement("Marker_Exposure")]
        public CameraSetting MarkerExposure;
        /// <summary>Values for camera marker threshold, current, min and max</summary>
        [XmlElement("Marker_Threshold")]
        public CameraSetting MarkerThreshold;
        /// <summary>Position of camera</summary>
        [XmlElement("Position")]
        public CameraPosition Position;
        /// <summary>Orientation of camera</summary>
        [XmlElement("Orientation")]
        public int Orientation;
        /// <summary>Marker resolution of camera, width and height</summary>
        [XmlElement("Marker_Res")]
        public Resolution MarkerResolution;
        /// <summary>Video resolution of camera, width and height</summary>
        [XmlElement("Video_Res")]
        public Resolution VideoResolution;
        /// <summary>Marker Field Of View, left,top,right and bottom coordinates</summary>
        [XmlElement("Marker_FOV")]
        public FieldOfView MarkerFOV;
        /// <summary>Video Field Of View, left,top,right and bottom coordinates</summary>
        [XmlElement("Video_FOV")]
        public FieldOfView VideoFOV;
        /// <summary>Sync out settings for Oqus sync out or Miqus Sync Unit Out1</summary>
        [XmlElement("Sync_Out")]
        public SettingsSyncOut SyncOut;
        /// <summary>Sync out settings for Miqus Sync Unit Out2</summary>
        [XmlElement("Sync_Out2")]
        public SettingsSyncOut SyncOut2;
        /// <summary>Sync out settings for Miqus Sync Unit Measurement Time (MT)</summary>
        [XmlElement("Sync_Out_MT")]
        public SettingsSyncOut SyncOutMT;
    }

    /// <summary>settings regarding sync for Camera</summary>
    public struct SettingsSyncOut
    {
        /// <summary>Sync mode for camera</summary>
        [XmlElement("Mode")]
        public SyncOutFrequencyMode SyncMode;
        /// <summary>Sync value, depending on mode</summary>
        [XmlElement("Value")]
        public int SyncValue;
        /// <summary>Output duty cycle in percent</summary>
        [XmlElement("Duty_Cycle")]
        public float DutyCycle;
        /// <summary>TTL signal polarity. no used in SRAM or 100Hz mode</summary>
        [XmlElement("Signal_Polarity")]
        public SignalPolarity SignalPolarity;
    }

    /// <summary>Position for a camera</summary>
    public struct CameraPosition
    {
        /// <summary>X position</summary>
        [XmlElement("X")]
        public float X;
        /// <summary>Y position</summary>
        [XmlElement("Y")]
        public float Y;
        /// <summary>Z position</summary>
        [XmlElement("Z")]
        public float Z;
        /// <summary>Rotation matrix - [1,1] value</summary>
        [XmlElement("Rot_1_1")]
        public float Rot11;
        /// <summary>Rotation matrix - [2,1] value</summary>
        [XmlElement("Rot_2_1")]
        public float Rot21;
        /// <summary>Rotation matrix - [3,1] value</summary>
        [XmlElement("Rot_3_1")]
        public float Rot31;
        /// <summary>Rotation matrix - [1,2] value</summary>
        [XmlElement("Rot_1_2")]
        public float Rot12;
        /// <summary>Rotation matrix - [2,2] value</summary>
        [XmlElement("Rot_2_2")]
        public float Rot22;
        /// <summary>Rotation matrix - [3,2] value</summary>
        [XmlElement("Rot_3_2")]
        public float Rot32;
        /// <summary>Rotation matrix - [1,3] value</summary>
        [XmlElement("Rot_1_3")]
        public float Rot13;
        /// <summary>Rotation matrix - [2,3] value</summary>
        [XmlElement("Rot_2_3")]
        public float Rot23;
        /// <summary>Rotation matrix - [3,3] value</summary>
        [XmlElement("Rot_3_3")]
        public float Rot33;
    }

    /// <summary>Resolution (width/height)</summary>
    public struct Resolution
    {
        /// <summary>Width</summary>
        [XmlElement("Width")]
        public int Width;
        /// <summary>Height</summary>
        [XmlElement("Height")]
        public int Height;
    }

    /// <summary>Field of View</summary>
    public struct FieldOfView
    {
        /// <summary>Left</summary>
        [XmlElement("Left")]
        public int Left;
        /// <summary>Top</summary>
        [XmlElement("Top")]
        public int Top;
        /// <summary>Right</summary>
        [XmlElement("Right")]
        public int Right;
        /// <summary>Bottom</summary>
        [XmlElement("Bottom")]
        public int Bottom;
    }

    /// <summary>settings for Camera values (min,max and current)</summary>
    public struct CameraSetting
    {
        /// <summary>Current value</summary>
        [XmlElement("Current")]
        public int Current;
        /// <summary>Minimum value</summary>
        [XmlElement("Min")]
        public int Min;
        /// <summary>Maximum value</summary>
        [XmlElement("Max")]
        public int Max;
    }

    /// <summary>Settings regarding post processing actions</summary>
    public struct SettingProcessingActions
    {
        /// <summary>Preprocessing 2d data</summary>
        [XmlElement("PreProcessing2D")]
        public bool PreProcessing2D;
        /// <summary>Tracking processing</summary>
        [XmlElement("Tracking")]
        public SettingsTrackingProcessingActions TrackingActions;
        /// <summary>Twin system merge processing</summary>
        [XmlElement("TwinSystemMerge")]
        public bool TwinSystemMerge;
        /// <summary>Gapfill processing</summary>
        [XmlElement("SplineFill")]
        public bool SplineFill;
        /// <summary>AIM processing</summary>
        [XmlElement("AIM")]
        public bool Aim;
        /// <summary>6DOF tracking processing</summary>
        [XmlElement("Track6DOF")]
        public bool Track6DOF;
        /// <summary>Force data</summary>
        [XmlElement("ForceData")]
        public bool ForceData;
        /// <summary>GazeVector</summary>
        [XmlElement("GazeVector")]
        public bool GazeVector;
        /// <summary>Export to TSV</summary>
        [XmlElement("ExportTSV")]
        public bool ExportTSV;
        /// <summary>Export to C3D</summary>
        [XmlElement("ExportC3D")]
        public bool ExportC3D;
        /// <summary>Export to Matlab file</summary>
        [XmlElement("ExportMatlabFile")]
        public bool ExportMatlab;
        [XmlElement("ExportAviFile")]
        public bool ExportAviFile;
    }

    /// <summary>Settings regarding external Time Base</summary>
    public struct SettingsExternalTimeBase
    {
        [XmlElement("Enabled")]
        public bool Enabled;
        [XmlElement("Signal_Source")]
        public SignalSource SignalSource;
        [XmlElement("Signal_Mode")]
        public SignalMode SignalMode;
        [XmlElement("Frequency_Multiplier")]
        public int FreqMultiplier;
        [XmlElement("Frequency_Divisor")]
        public int FreqDivisor;
        [XmlElement("Frequency_Tolerance")]
        public int FreqTolerance;
        [XmlElement("Nominal_Frequency")]
        public float NominalFrequency;
        [XmlElement("Signal_Edge")]
        public SignalEdge SignalEdge;
        [XmlElement("Signal_Shutter_Delay")]
        public int SignalShutterDelay;
        [XmlElement("Non_Periodic_Timeout")]
        public float NonPeriodicTimeout;
    }

    /// <summary>settings for 6DOF bodies</summary>
    public struct Settings6DOF
    {
        /// <summary>Name of 6DOF body</summary>
        [XmlElement("Name")]
        public string Name;
        /// <summary>Color of 6DOF body</summary>
        [XmlElement("RGBColor")]
        public int ColorRGB;
        /// <summary>List of points in 6DOF body</summary>
        [XmlElement("Point")]
        public List<Point> Points;
    }

    /// <summary>General settings for Analog devices</summary>
    public struct AnalogDevice
    {
        /// <summary>Analog device ID</summary>
        [XmlElement("Device_ID")]
        public int DeviceID;
        /// <summary>Analog device name</summary>
        [XmlElement("Device_Name")]
        public string DeviceName;
        /// <summary>Number of channels in device</summary>
        [XmlElement("Channels")]
        public int ChannelCount;
        /// <summary>Frequency of channels</summary>
        [XmlElement("Frequency")]
        public int Frequency;
        /// <summary>Range of channels</summary>
        [XmlElement("Range")]
        public AnalogRange ChannelRange;
        /// <summary>Information of channels</summary>
        [XmlElement("Channel")]
        public List<AnalogChannelInfo> Labels;
    }

    /// <summary>Analog range and channels</summary>
    public struct AnalogRange
    {
        /// <summary>Minimum value</summary>
        [XmlElement("Min")]
        public float Min;
        /// <summary>Maximum value</summary>
        [XmlElement("Max")]
        public float Max;
    }

    /// <summary>settings for Analog channel</summary>
    public struct AnalogChannelInfo
    {
        /// <summary>Channel label</summary>
        [XmlElement("Label")]
        public string Label;
        /// <summary>Unit used by channel</summary>
        [XmlElement("Unit")]
        public string Unit;
    }

    /// <summary>Settings for Force plate</summary>
    public struct ForcePlateSettings
    {
        /// <summary>Force plate index number</summary>
        [XmlElement("Force_Plate_Index")]
        public int ForcePlateIndex;
        /// <summary>ID of force plate</summary>
        [XmlElement("Plate_ID")]
        public int PlateID;
        /// <summary>ID of analog device connected to force plate. 0 = no analog device associated with force plate</summary>
        [XmlElement("Analog_Device_ID")]
        public int AnalogDeviceID;
        /// <summary>Measurement frequency of analog device connected to force plate</summary>
        [XmlElement("Frequency")]
        public float Frequency;
        /// <summary>Force plate type</summary>
        [XmlElement("Type")]
        public string Type;
        /// <summary>Name of force plate</summary>
        [XmlElement("Name")]
        public string Name;
        /// <summary>Force plate length</summary>
        [XmlElement("Length")]
        public float Length;
        /// <summary>Force plate width</summary>
        [XmlElement("Width")]
        public float Width;
        /// <summary>four blocks with the corners of the force plate</summary>
        [XmlElement("Location")]
        public Location Location;
        /// <summary>Force plate origin</summary>
        [XmlElement("Origin")]
        public Point Origin;
        /// <summary>Analog channels connected to force plate</summary>
        [XmlArray("Channels")]
        public List<ForceChannel> Channels;
        /// <summary>Calibration of the force plate</summary>
        [XmlElement("Calibration_Matrix")]
        public CalibrationMatrix CalibrationMatrix;

    }

    /// <summary>Struct with calibration matrix for force plate</summary>
    public struct CalibrationMatrix
    {
        [XmlElement("Row1")]
        public CalibrationRow Row1;
        [XmlElement("Row2")]
        public CalibrationRow Row2;
        [XmlElement("Row3")]
        public CalibrationRow Row3;
        [XmlElement("Row4")]
        public CalibrationRow Row4;
        [XmlElement("Row5")]
        public CalibrationRow Row5;
        [XmlElement("Row6")]
        public CalibrationRow Row6;
        [XmlElement("Row7")]
        public CalibrationRow Row7;
        [XmlElement("Row8")]
        public CalibrationRow Row8;
        [XmlElement("Row9")]
        public CalibrationRow Row9;
        [XmlElement("Row10")]
        public CalibrationRow Row10;
        [XmlElement("Row11")]
        public CalibrationRow Row11;
        [XmlElement("Row12")]
        public CalibrationRow Row12;

    }

    /// <summary>row for calibration matrix of force plates</summary>
    public struct CalibrationRow
    {
        [XmlElement("Col1")]
        public float Col1;
        [XmlElement("Col2")]
        public float Col2;
        [XmlElement("Col3")]
        public float Col3;
        [XmlElement("Col4")]
        public float Col4;
        [XmlElement("Col5")]
        public float Col5;
        [XmlElement("Col6")]
        public float Col6;
        [XmlElement("Col7")]
        public float Col7;
        [XmlElement("Col8")]
        public float Col8;
        [XmlElement("Col9")]
        public float Col9;
        [XmlElement("Col10")]
        public float Col10;
        [XmlElement("Col11")]
        public float Col11;
        [XmlElement("Col12")]
        public float Col12;
    }

    /// <summary>Settings for channel</summary>
    [XmlType("Channel")]
    public struct ForceChannel
    {
        /// <summary>Channel number</summary>
        [XmlElement("Channel_No")]
        public int ChannelNumber;
        /// <summary>Conversion factor of channel</summary>
        [XmlElement("ConversionFactor")]
        public float ConversionFactor;
    }

    /// <summary>Location for force plate</summary>
    public struct Location
    {
        /// <summary>First corner</summary>
        [XmlElement("Corner1")]
        public Point Corner1;
        /// <summary>Second corner</summary>
        [XmlElement("Corner2")]
        public Point Corner2;
        /// <summary>Third corner</summary>
        [XmlElement("Corner3")]
        public Point Corner3;
        /// <summary>Fourth corner</summary>
        [XmlElement("Corner4")]
        public Point Corner4;
    }

    /// <summary>Settings for image from camera</summary>
    public struct ImageCamera
    {
        /// <summary>ID of camera</summary>
        [XmlElement("ID")]
        public int CameraID;
        /// <summary>Image streaming on or off</summary>
        [XmlElement("Enabled")]
        public bool Enabled;
        /// <summary>Format of image</summary>
        [XmlElement("Format")]
        public ImageFormat ImageFormat;
        /// <summary>Image width</summary>
        [XmlElement("Width")]
        public int Width;
        /// <summary>Image height</summary>
        [XmlElement("Height")]
        public int Height;
        /// <summary>Left edge relative to original image</summary>
        [XmlElement("Left_Crop")]
        public float CropLeft;
        /// <summary>Top edge relative to original image</summary>
        [XmlElement("Top_Crop")]
        public float CropTop;
        /// <summary>Right edge relative to original image</summary>
        [XmlElement("Right_Crop")]
        public float CropRight;
        /// <summary>Bottom edge relative to original image</summary>
        [XmlElement("Bottom_Crop")]
        public float CropBottom;
    }

    /// <summary>settings for labeled marker</summary>
    public struct Settings3DLabel
    {
        /// <summary>Name of marker</summary>
        [XmlElement("Name")]
        public string Name;
        /// <summary>Color of marker</summary>
        [XmlElement("RGBColor")]
        public int ColorRGB;
    }

    /// <summary>Tracking processing actions</summary>
    public enum SettingsTrackingProcessingActions
    {
        [XmlEnum("false")]
        ProcessingNone = 0,
        [XmlEnum("2D")]
        ProcessingTracking2D,
        [XmlEnum("3D")]
        ProcessingTracking3D
    }
}