using System.Windows;
using System.Windows.Forms;

namespace PDFitCompanion.Services
{
    public class NotifyIconService : IDisposable
    {
        private NotifyIcon? _notifyIcon;

        public NotifyIconService()
        {
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open PDFit", null, (s, e) => OpenPDFit());
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, (s, e) => System.Windows.Application.Current.Shutdown());

            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.WinLogo, // TODO: Use PDFit icon
                Visible = true,
                Text = "PDFit Companion",
                ContextMenuStrip = contextMenu
            };

            _notifyIcon.DoubleClick += (s, e) => OpenPDFit();
        }

        public void ShowMessage(string title, string message)
        {
            _notifyIcon?.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
        }

        private void OpenPDFit()
        {
            var pdfit = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault();
            if (pdfit != null)
            {
                pdfit.Show();
                pdfit.WindowState = WindowState.Normal;
                pdfit.Activate();
            }
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
        }
    }
}
