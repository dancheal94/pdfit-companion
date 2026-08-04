using System.Windows;
using PDFitCompanion.Services;

namespace PDFitCompanion
{
    public partial class MainWindow : Window
    {
        private NotifyIconService? _trayIcon;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _trayIcon = new NotifyIconService();
            _trayIcon.ShowMessage("PDFit Companion started", "Ready to print to PDF");
            Hide();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();
            base.OnStateChanged(e);
        }
    }
}
