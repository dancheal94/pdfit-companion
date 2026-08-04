using System.Diagnostics;
using PDFitCompanion.Config;
using Serilog;

namespace PDFitCompanion.Services
{
    public class PrinterSetup
    {
        public static async Task SetupPrinterAsync()
        {
            try
            {
                await RemoveExistingPrinterAsync();
                await CreatePrinterAsync();
                Log.Information("Printer setup completed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Printer setup failed");
                throw;
            }
        }

        private static async Task CreatePrinterAsync()
        {
            // Create spool directory
            Directory.CreateDirectory(AppConfig.SpoolDirectory);

            // Use Windows Print Management to add printer port
            var portName = "PDFit_Port:";
            var printerName = AppConfig.PrinterName;

            try
            {
                // Add port
                await RunPowerShellAsync($@"
Add-PrinterPort -Name '{portName}' -PrinterHostAddress '{AppConfig.SpoolDirectory}' -ErrorAction SilentlyContinue
");

                // Add printer using Print to PDF driver
                await RunPowerShellAsync($@"
Add-Printer -Name '{printerName}' -DriverName 'Microsoft Print to PDF' -PortName '{portName}' -ErrorAction SilentlyContinue
");

                Log.Information("Printer created: {PrinterName}", printerName);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to create printer via PowerShell, trying alternative method");
                await CreatePrinterViaPrintUIAsync();
            }
        }

        private static async Task CreatePrinterViaPrintUIAsync()
        {
            var spoolPath = AppConfig.SpoolDirectory;
            var printerName = AppConfig.PrinterName;

            // Run PrintUI.dll to add printer
            var psi = new ProcessStartInfo
            {
                FileName = "rundll32.exe",
                Arguments = $"printui.dll,PrintUIEntry /ii /n \"{printerName}\" /m \"Microsoft Print to PDF\" /h \"local\" /r \"{spoolPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            await process!.WaitForExitAsync();

            if (process.ExitCode != 0)
                Log.Warning("PrintUI printer creation returned exit code: {ExitCode}", process.ExitCode);
            else
                Log.Information("Printer created via PrintUI");
        }

        private static async Task RemoveExistingPrinterAsync()
        {
            try
            {
                await RunPowerShellAsync($@"
Remove-Printer -Name '{AppConfig.PrinterName}' -ErrorAction SilentlyContinue
");
                Log.Information("Removed existing printer");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to remove existing printer");
            }
        }

        private static async Task RunPowerShellAsync(string script)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            var output = await process!.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(error) && process.ExitCode != 0)
                throw new InvalidOperationException($"PowerShell error: {error}");
        }
    }
}
