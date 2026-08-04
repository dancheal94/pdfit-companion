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
        private TrayService _trayService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(System.IO.Path.Combine(AppConfig.LogDirectory, "pdfit-companion.log"))
                .CreateLogger();

            Log.Information("PDFit Companion starting");

            try
            {
                // Register protocol handler and startup task
                RegistryService.RegisterProtocolHandler();
                RegistryService.RegisterStartupTask();

                // Initialize tray service
                _trayService = new TrayService();

                // Setup printer
                _ = PrinterSetup.SetupPrinterAsync();

                // Initialize auth service
                _authService = new AuthService();
                _authService.OnAuthStatusChanged += OnAuthStatusChanged;

                // Check for auth callback in command line
                if (e.Args.Length > 0 && e.Args[0].StartsWith("pdfit://"))
                {
                    _authService.HandleAuthCallback(e.Args[0]);
                }

                // Initialize spool monitor
                _spoolMonitor = new SpoolMonitor(_authService);
                _spoolMonitor.Start();

                // Hide main window (we use tray only)
                MainWindow.Hide();
                MainWindow.ShowInTaskbar = false;

                if (_authService.IsAuthenticated)
                {
                    _trayService.ShowMessage("PDFit Companion", "Ready to print to PDF");
                }
                else
                {
                    _trayService.ShowMessage("PDFit Companion", "Please sign in to get started");
                    _authService.OpenAuthBrowser();
                }

                Log.Information("Application started successfully");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application startup failed");
                _trayService?.ShowError("Error", $"Failed to start: {ex.Message}");
                Shutdown(1);
            }
        }

        private void OnAuthStatusChanged(bool isAuthenticated)
        {
            if (isAuthenticated)
            {
                _trayService?.ShowMessage("PDFit Companion", "Authenticated! Ready to print");
                Log.Information("User authenticated successfully");
            }
            else
            {
                _trayService?.ShowError("PDFit Companion", "Authentication failed");
                Log.Warning("Authentication failed");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("PDFit Companion shutting down");
            _spoolMonitor?.Dispose();
            _trayService?.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
