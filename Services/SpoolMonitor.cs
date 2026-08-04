using PdfSharpCore.Pdf;
using PDFitCompanion.Config;
using Serilog;
using System;
using System.IO;

namespace PDFitCompanion.Services
{
    public class SpoolMonitor : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly AuthService _authService;
        private readonly NotificationService _notificationService;
        private readonly Dictionary<string, DateTime> _processingFiles = new();
        private SupabaseService? _supabaseService;

        public SpoolMonitor(AuthService authService, NotificationService notificationService)
        {
            _authService = authService;
            _notificationService = notificationService;

            AppConfig.EnsureDirectories();

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
            else
            {
                _authService.OpenAuthBrowser();
            }
        }

        private void InitializeSupabaseService()
        {
            if (_authService.UserId == null || _authService.AccessToken == null) return;
            _supabaseService = new SupabaseService(_authService.UserId, _authService.AccessToken);
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            _processingFiles[e.FullPath] = DateTime.UtcNow;
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
                    _notificationService?.ShowError("Not authenticated", "Please sign in to PDFit to continue");
                    _authService.OpenAuthBrowser();
                    return;
                }

                if (_supabaseService == null)
                {
                    if (_authService.UserId == null || _authService.AccessToken == null)
                        throw new InvalidOperationException("User ID or access token not available");
                    _supabaseService = new SupabaseService(_authService.UserId, _authService.AccessToken);
                }

                Log.Information("Processing PDF: {FilePath}", filePath);

                var pageCount = GetPageCount(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var projectId = await _supabaseService.CreateProjectAsync(fileName);

                if (string.IsNullOrEmpty(projectId))
                    throw new InvalidOperationException("Failed to create project");

                var storagePath = await _supabaseService.UploadFileAsync(projectId, filePath);
                if (string.IsNullOrEmpty(storagePath))
                    throw new InvalidOperationException("Failed to upload file");

                await _supabaseService.RegisterMediaAsync(projectId, storagePath, pageCount, fileName);

                _notificationService?.ShowSuccess("Opened in PDFit", $"{fileName} is ready to edit");
                Log.Information("PDF processed successfully: {FileName}", fileName);

                _processingFiles.Remove(filePath);
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to process PDF: {FilePath}", filePath);
                _notificationService?.ShowError("Error", $"Failed to process PDF: {ex.Message}");
            }
        }

        private int GetPageCount(string filePath)
        {
            try
            {
                using var document = PdfDocument.Open(filePath);
                return document.PageCount;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to read page count from {FilePath}", filePath);
                return 0;
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
