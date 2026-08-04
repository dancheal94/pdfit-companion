using System;
using System.Windows;
using Serilog;
using PDFitCompanion.Services;
using PDFitCompanion.Config;

namespace PDFitCompanion
{
    public partial class App : Application
    {
        private SpoolMonitor _spoolMonitor;
        private AuthService _authService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(System.IO.Path.Combine(AppConfig.LogDirectory, "pdfit-companion-.txt"),
                    rollingInterval: Serilog.Events.RollingInterval.Day)
                .CreateLogger();

            Log.Information("PDFit Companion starting");

            try
            {
                _ = PrinterSetup.SetupPrinterAsync();
                
                _authService = new AuthService();
                _spoolMonitor = new SpoolMonitor(_authService);
                _spoolMonitor.Start();

                new MainWindow().Show();
                Log.Information("Application started successfully");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application startup failed");
                MessageBox.Show($"Failed to start: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
