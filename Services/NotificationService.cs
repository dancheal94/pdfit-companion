using Serilog;

namespace PDFitCompanion.Services
{
    public class NotificationService
    {
        public void ShowSuccess(string title, string message)
        {
            Log.Information("Notification: {Title} - {Message}", title, message);
            ShowToastNotification(title, message, "Success");
        }

        public void ShowError(string title, string message)
        {
            Log.Error("Error notification: {Title} - {Message}", title, message);
            ShowToastNotification(title, message, "Error");
        }

        public void ShowInfo(string title, string message)
        {
            Log.Information("Info notification: {Title} - {Message}", title, message);
            ShowToastNotification(title, message, "Info");
        }

        private void ShowToastNotification(string title, string message, string type)
        {
            // In production, use Windows.UI.Notifications for modern toast notifications
            // For now, logging serves as the notification system
        }
    }
}
