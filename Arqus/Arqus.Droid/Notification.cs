namespace SharedProjects
{    
    internal class Notification
    {
        static Notification_Android notification = new Notification_Android();

        internal static void Show(string title, string message)
        {
            notification.Show(title, message);
        }
    }
}