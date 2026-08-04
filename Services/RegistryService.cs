using System;
using Microsoft.Win32;
using Serilog;

namespace PDFitCompanion.Services
{
    public static class RegistryService
    {
        public static void RegisterProtocolHandler()
        {
            try
            {
                var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\pdfit"))
                {
                    key.SetValue("", "URL:PDFit Protocol");
                    key.SetValue("URL Protocol", "");
                }

                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\pdfit\shell\open\command"))
                {
                    key.SetValue("", $"\"{appPath}\" \"%1\"");
                }

                Log.Information("Protocol handler registered: pdfit://");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to register protocol handler");
            }
        }

        public static void RegisterStartupTask()
        {
            try
            {
                var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    key.SetValue("PDFitCompanion", appPath);
                }

                Log.Information("Startup task registered");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to register startup task");
            }
        }
    }
}
