using System.Windows;
using Serilog;

namespace PDFitCompanion
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("pdfit-companion.log")
                .CreateLogger();
            Log.Information("PDFit Companion started");
        }
    }
}
