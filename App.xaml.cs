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
                .WriteTo.File(System.IO.Path.Combine(AppConfig.LogDirectory, "pdfit-companion.log"))
                .CreateLogger();

            Log.Information("PDFit Companion starting");

            try
            {
                RegistryService.RegisterProtocolHandler();
                RegistryService.RegisterStartupTask();

                _authService = new AuthService();
                _authService.OnAuthStatusChanged += OnAuthStatusChanged;

                if (e.Args.Length > 0 && e.Args[0].StartsWith("pdfit://"))
                {
                    _authService.HandleAuthCallback(e.Args[0]);
                }

                _spoolMonitor = new SpoolMonitor(_authService);
                _spoolMonitor.Start();

                MainWindow.Hide();
                MainWindow.ShowInTaskbar = false;

                if (!_authService.IsAuthenticated)
                {
                    Log.Information("Opening browser for authentication");
                    _authService.OpenAuthBrowser();
                }

                Log.Information("Application started successfully");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application startup failed");
                MessageBox.Show($"Failed to start: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void OnAuthStatusChanged(bool isAuthenticated)
        {
            Log.Information("Auth status changed: {IsAuthenticated}", isAuthenticated);
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
