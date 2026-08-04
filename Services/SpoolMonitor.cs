using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using PDFitCompanion.Config;

namespace PDFitCompanion.Services
{
    public class SpoolMonitor : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly AuthService _authService;
        private readonly TrayService _trayService;
        private readonly Dictionary<string, DateTime> _processingFiles;
        private SupabaseService _supabaseService;

        public SpoolMonitor(AuthService authService, TrayService trayService = null)
        {
            _authService = authService;
            _trayService = trayService;
            _processingFiles = new Dictionary<string, DateTime>();

            _watcher = new FileSystemWatcher(AppConfig.SpoolDirectory)
            {
                Filter = "*.pdf",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size
            };

            _watcher.Created += OnFileCreated;
            _watcher.Changed += OnFileChanged;
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
            Log.Information("Spool monitor started, watching: {Directory}", AppConfig.SpoolDirectory);

            if (_authService.IsAuthenticated && !string.IsNullOrEmpty(_authService.UserId))
            {
                InitializeSupabaseService();
            }
        }

        private void InitializeSupabaseService()
        {
            if (_authService.UserId == null || _authService.AccessToken == null)
                return;

            _supabaseService = new SupabaseService(_authService.UserId, _authService.AccessToken);
            Log.Information("Supabase service initialized");
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            _processingFiles[e.FullPath] = DateTime.UtcNow;
            Log.Information("File detected: {FileName}", e.Name);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (!_processingFiles.ContainsKey(e.FullPath))
                return;

            var lastChange = _processingFiles[e.FullPath];
            if ((DateTime.UtcNow - lastChange).TotalMilliseconds < 500)
                return;

            _processingFiles[e.FullPath] = DateTime.UtcNow;

            if (IsFileLocked(e.FullPath))
                return;

            _ = ProcessPdfAsync(e.FullPath);
        }

        private async Task ProcessPdfAsync(string filePath)
        {
            try
            {
                if (!_authService.IsAuthenticated)
                {
                    Log.Warning("Not authenticated, opening browser");
                    _trayService?.ShowError("Not Authenticated", "Please sign in to PDFit");
                    _authService.OpenAuthBrowser();
                    return;
                }

                if (_supabaseService == null)
                {
                    if (_authService.UserId == null || _authService.AccessToken == null)
                        throw new InvalidOperationException("User ID or access token not available");

                    InitializeSupabaseService();
                }

                Log.Information("Processing PDF: {FilePath}", filePath);
                _trayService?.ShowMessage("Processing", "Uploading to PDFit...");

                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var projectId = await _supabaseService.CreateProjectAsync(fileName);

                if (string.IsNullOrEmpty(projectId))
                    throw new InvalidOperationException("Failed to create project");

                var storagePath = await _supabaseService.UploadFileAsync(projectId, filePath);
                if (string.IsNullOrEmpty(storagePath))
                    throw new InvalidOperationException("Failed to upload file");

                await _supabaseService.RegisterMediaAsync(projectId, storagePath, 1, fileName);

                _trayService?.ShowMessage("Success", $"'{fileName}' is ready to edit in PDFit");
                Log.Information("PDF processed successfully: {FileName}", fileName);
                _processingFiles.Remove(filePath);

                try { File.Delete(filePath); }
                catch { }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to process PDF: {FilePath}", filePath);
                _trayService?.ShowError("Upload Failed", ex.Message);
            }
        }

        private bool IsFileLocked(string filePath)
        {
            try
            {
                using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return false;
            }
            catch
            {
                return true;
            }
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}
