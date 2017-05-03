using Arqus.Services.MobileCenterService;
using System;
using Xamarin.Forms;
using Nest;
using Arqus.Services;

namespace Arqus.Service
{
    static class MessageSubject
    {
        public static readonly string STREAM_MODE_CHANGED = "STREAM_MODE_CHANGED";
        public static readonly string STREAM_DATA_SUCCESS = "STREAM_DATA_SUCCESS ";
        public static readonly string ENTER_PASSWORD = "ENTER_PASSWORD";
        public static readonly string SET_CAMERA_SELECTION = "SET_CAMERA_SELECTION";
        public static readonly string CONNECTED = "CONNECTED";
        public static readonly string DISCONNECTED = "DISCONNECTED";
        public static readonly string CAMERA_SETTINGS_CHANGED = "CAMERA_SETTINGS_CHANGED";
    }


    public static class MessagingCenterService
	{

        public static void Subscribe<TSender>(this object subscriber, string message, Action<TSender> action, bool track = true) where TSender : class
        {
            if (track)
                TrackEvent(subscriber.GetType().Name, message);

            MessagingCenter.Subscribe(subscriber, message, action);
        }

        public static void Subscribe<TSender, TArgs>(this object subscriber, string message, Action<TSender, TArgs> action, bool track = true) where TSender : class
        {
            if (track)
                TrackEvent(subscriber.GetType().Name, message);
              
            MessagingCenter.Subscribe(subscriber, message, action);
        }

        public static void Send<TSender>(this TSender sender, string message, bool track = true) where TSender : class
        {
            if (track)
                TrackEvent(sender.GetType().Name, message);

            MessagingCenter.Send<TSender>(sender, message);
        }

        public static void Send<TSender, TArgs>(this TSender sender, string message, TArgs args, bool track = true) where TSender : class
        {
            if (track)
                TrackEvent(sender.GetType().Name, message);

            MessagingCenter.Send<TSender, TArgs>(sender, message, args);
        }

        private static void TrackEvent(string name, string message)
        {

            var elasticEvent = new ElasticEvent
            {
                Name = name,
                Message = message
            };

            ElasticsearchService.TrackEvent(elasticEvent);
            MobileCenterService.TrackEvent(name, message);
        }

    }
}