using System;
using System.Windows;
using System.Windows.Forms;
using Serilog;

namespace PDFitCompanion.Services
{
    public class TrayService : IDisposable
    {
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;

        public TrayService()
        {
            InitializeTray();
        }

        private void InitializeTray()
        {
            try
            {
                _contextMenu = new ContextMenuStrip();
                _contextMenu.Items.Add("Open PDFit", null, (s, e) => OpenPDFit());
                _contextMenu.Items.Add("-");
                _contextMenu.Items.Add("Exit", null, (s, e) => ExitApp());

                _notifyIcon = new NotifyIcon
                {
                    Icon = SystemIcons.WinLogo,
                    Visible = true,
                    Text = "PDFit Companion",
                    ContextMenuStrip = _contextMenu
                };

                _notifyIcon.DoubleClick += (s, e) => OpenPDFit();
                Log.Information("Tray service initialized");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize tray service");
            }
        }

        public void ShowMessage(string title, string message)
        {
            try
            {
                _notifyIcon?.ShowBalloonTip(5000, title, message, ToolTipIcon.Info);
                Log.Information("Toast shown: {Title} - {Message}", title, message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to show message");
            }
        }

        public void ShowError(string title, string message)
        {
            try
            {
                _notifyIcon?.ShowBalloonTip(5000, title, message, ToolTipIcon.Error);
                Log.Error("Error toast: {Title} - {Message}", title, message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to show error");
            }
        }

        private void OpenPDFit()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://app.pdfit.co",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open PDFit");
            }
        }

        private void ExitApp()
        {
            Application.Current?.Shutdown();
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
            _contextMenu?.Dispose();
        }
    }
}
