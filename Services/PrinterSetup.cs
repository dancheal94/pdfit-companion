using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;
using PDFitCompanion.Config;

namespace PDFitCompanion.Services
{
    public static class PrinterSetup
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
            try
            {
                // Use PowerShell to add printer
                var script = $@"
Add-PrinterPort -Name '{AppConfig.PrinterName}:' -PrinterHostAddress '{AppConfig.SpoolDirectory}' -ErrorAction SilentlyContinue
Add-Printer -Name '{AppConfig.PrinterName}' -DriverName 'Microsoft Print to PDF' -PortName '{AppConfig.PrinterName}:' -ErrorAction SilentlyContinue
";
                await RunPowerShellAsync(script);
                Log.Information("Printer created: {PrinterName}", AppConfig.PrinterName);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to create printer");
            }
        }

        private static async Task RemoveExistingPrinterAsync()
        {
            try
            {
                var script = $"Remove-Printer -Name '{AppConfig.PrinterName}' -ErrorAction SilentlyContinue";
                await RunPowerShellAsync(script);
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
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(error) && process.ExitCode != 0)
            {
                Log.Warning("PowerShell warning: {Error}", error);
            }
        }
    }
}
