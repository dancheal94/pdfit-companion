using Supabase;
using Supabase.Gotrue;
using PDFitCompanion.Config;
using Serilog;

namespace PDFitCompanion.Services
{
    public class SupabaseService
    {
        private Client? _client;
        private readonly string _userId;

        public SupabaseService(string userId)
        {
            _userId = userId;
        }

        public async Task InitializeAsync()
        {
            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false
            };

            _client = new Client(AppConfig.SupabaseUrl, AppConfig.SupabaseAnonKey, options);
            await _client.InitializeAsync();
            Log.Information("Supabase client initialized");
        }

        public async Task<string?> CreateProjectAsync(string projectName, string? organizationId = null)
        {
            try
            {
                if (_client?.Realtime == null)
                    throw new InvalidOperationException("Supabase client not initialized");

                var project = new Dictionary<string, object>
                {
                    { "name", projectName },
                    { "user_id", _userId },
                    { "status", "active" }
                };

                if (!string.IsNullOrEmpty(organizationId))
                    project["organization_id"] = organizationId;

                var response = await _client.From("projects")
                    .Insert(project)
                    .Execute();

                Log.Information("Project created: {ProjectName}", projectName);
                return response.Models?.FirstOrDefault()?.Get("id")?.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create project: {ProjectName}", projectName);
                throw;
            }
        }

        public async Task<string?> UploadFileAsync(string projectId, string filePath)
        {
            try
            {
                if (_client?.Storage == null)
                    throw new InvalidOperationException("Supabase client not initialized");

                var fileName = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Path.GetFileName(filePath)}";
                var storagePath = $"{_userId}/{projectId}/{fileName}";

                var fileBytes = await File.ReadAllBytesAsync(filePath);
                var response = await _client.Storage
                    .From(AppConfig.StorageBucket)
                    .Upload(fileBytes, storagePath);

                Log.Information("File uploaded: {StoragePath}", storagePath);
                return storagePath;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to upload file: {FilePath}", filePath);
                throw;
            }
        }

        public async Task<bool> RegisterMediaAsync(string projectId, string storagePath, int pageCount, string fileName)
        {
            try
            {
                if (_client?.Realtime == null)
                    throw new InvalidOperationException("Supabase client not initialized");

                var media = new Dictionary<string, object>
                {
                    { "project_id", projectId },
                    { "file_path", storagePath },
                    { "file_name", fileName },
                    { "page_count", pageCount },
                    { "created_at", DateTime.UtcNow.ToUniversalTime() }
                };

                await _client.From("media")
                    .Insert(media)
                    .Execute();

                Log.Information("Media registered: {FileName} ({PageCount} pages)", fileName, pageCount);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to register media: {FileName}", fileName);
                throw;
            }
        }
    }
}
