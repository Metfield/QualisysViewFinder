using System;

namespace Arqus.Helpers
{
    public enum MessageSubject
    {
        STREAM_MODE_CHANGED,
        STREAM_DATA_SUCCESS,
        FETCH_IMAGE_DATA,
        FETCH_MARKER_DATA,
        ENTER_PASSWORD,
        SET_CAMERA_SELECTION,
        CONNECTED,
        DISCONNECTED,
        CAMERA_SETTINGS_CHANGED
    }

    public class MessagingCenterAlert
	{
		/// <summary>
		/// Init this instance.
		/// </summary>
		public static void Init()
		{
			var time = DateTime.UtcNow;
		}

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		public string Title { get; set; }

		/// <summary>
		/// Gets or sets the message.
		/// </summary>
		/// <value>The message.</value>
		public string Message { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance cancel/OK text.
		/// </summary>
		/// <value><c>true</c> if this instance cancel; otherwise, <c>false</c>.</value>
		public string Cancel { get; set; }

		/// <summary>
		/// Gets or sets the OnCompleted Action.
		/// </summary>
		/// <value>The OnCompleted Action.</value>
		public Action OnCompleted { get; set; }

        /// <summary>
        /// Gets or sets the subject of the message
        /// </summary>
        public MessageSubject Subject { get; set; }
	}
}