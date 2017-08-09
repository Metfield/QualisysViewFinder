namespace SharedProjects
{
    internal class Notification
    {
        static Notification_iOS notification = new Notification_iOS();

        internal static void Show(string title, string message)
        {
            notification.Show(title, message);
        }
    }
}