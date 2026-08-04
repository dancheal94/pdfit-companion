using System;
using Serilog;

namespace PDFitCompanion.Services
{
    public class NotificationService
    {
        public void ShowSuccess(string title, string message)
        {
            Log.Information("Success: {Title} - {Message}", title, message);
        }

        public void ShowError(string title, string message)
        {
            Log.Error("Error: {Title} - {Message}", title, message);
        }

        public void ShowInfo(string title, string message)
        {
            Log.Information("Info: {Title} - {Message}", title, message);
        }
    }
}
