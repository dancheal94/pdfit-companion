using System.Windows;
using PDFitCompanion.Services;
using Serilog;

namespace PDFitCompanion
{
    public partial class App : Application
    {
        private AuthService? _authService;
        private SpoolMonitor? _spoolMonitor;
        private NotificationService? _notificationService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PDFit", "Logs", "pdfit-companion-.txt"),
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("PDFit Companion starting...");

            try
            {
                _notificationService = new NotificationService();
                _authService = new AuthService();
                _spoolMonitor = new SpoolMonitor(_authService, _notificationService);

                _spoolMonitor.Start();
                Log.Information("Spool monitor started");

                new MainWindow().Show();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to start PDFit Companion");
                MessageBox.Show($"Failed to start PDFit Companion: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("PDFit Companion shutting down");
            _spoolMonitor?.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
