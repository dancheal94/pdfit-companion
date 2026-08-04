using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using PDFitCompanion.Config;

namespace PDFitCompanion.Services
{
    public class SupabaseService
    {
        private readonly string _userId;
        private readonly string _accessToken;
        private readonly HttpClient _httpClient;

        public SupabaseService(string userId, string accessToken)
        {
            _userId = userId;
            _accessToken = accessToken;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            _httpClient.DefaultRequestHeaders.Add("apikey", AppConfig.SupabaseAnonKey);
        }

        public async Task<string?> CreateProjectAsync(string projectName, string? organizationId = null)
        {
            try
            {
                var project = new Dictionary<string, object>
                {
                    { "name", projectName },
                    { "user_id", _userId },
                    { "status", "active" }
                };

                if (!string.IsNullOrEmpty(organizationId))
                    project["organization_id"] = organizationId;

                var url = $"{AppConfig.SupabaseUrl}/rest/v1/projects?select=id";
                var response = await _httpClient.PostAsJsonAsync(url, new[] { project });
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var results = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(content, options);

                if (results == null || results.Count == 0)
                    throw new InvalidOperationException("No project ID returned");

                var projectId = results[0]["id"].GetString();
                Log.Information("Project created: {ProjectName} ({ProjectId})", projectName, projectId);
                return projectId;
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
                var fileName = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Path.GetFileName(filePath)}";
                var storagePath = $"{_userId}/{projectId}/{fileName}";

                var fileBytes = await File.ReadAllBytesAsync(filePath);
                var content = new ByteArrayContent(fileBytes);
                content.Headers.Add("Content-Type", "application/pdf");

                var url = $"{AppConfig.SupabaseUrl}/storage/v1/object/{AppConfig.StorageBucket}/{storagePath}";
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

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
                var media = new Dictionary<string, object>
                {
                    { "project_id", projectId },
                    { "file_path", storagePath },
                    { "file_name", fileName },
                    { "page_count", pageCount },
                    { "created_at", DateTime.UtcNow.ToUniversalTime() }
                };

                var url = $"{AppConfig.SupabaseUrl}/rest/v1/media";
                var response = await _httpClient.PostAsJsonAsync(url, new[] { media });
                response.EnsureSuccessStatusCode();

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
