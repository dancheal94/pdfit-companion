using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Web;
using Serilog;
using PDFitCompanion.Config;

namespace PDFitCompanion.Services
{
    public class AuthService
    {
        private string _accessToken;
        private string _refreshToken;
        private string _userId;
        private DateTime _tokenExpiry;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry;
        public string UserId => _userId;
        public string AccessToken => _accessToken;

        public event Action<bool> OnAuthStatusChanged;

        public AuthService()
        {
            LoadStoredToken();
        }

        public void OpenAuthBrowser()
        {
            try
            {
                Log.Information("Opening browser for authentication");
                Process.Start(new ProcessStartInfo
                {
                    FileName = AppConfig.AuthBrowserUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open auth browser");
            }
        }

        public void HandleAuthCallback(string uriString)
        {
            try
            {
                Log.Information("Processing auth callback");

                var uri = new Uri(uriString);
                var query = HttpUtility.ParseQueryString(uri.Query);
                var payload = query["payload"];

                if (string.IsNullOrEmpty(payload))
                {
                    Log.Warning("No payload in auth callback");
                    return;
                }

                var decodedPayload = HttpUtility.UrlDecode(payload);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var json = JsonSerializer.Deserialize<AuthPayload>(decodedPayload, options);

                if (json == null)
                    throw new InvalidOperationException("Invalid auth payload");

                _accessToken = json.access_token;
                _refreshToken = json.refresh_token;
                _userId = json.user_id;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(3600);

                SaveStoredToken();
                OnAuthStatusChanged?.Invoke(true);
                Log.Information("Authentication successful for user: {UserId}", _userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to handle auth callback");
                OnAuthStatusChanged?.Invoke(false);
            }
        }

        private void LoadStoredToken()
        {
            try
            {
                if (!File.Exists(AppConfig.CredentialsFile))
                    return;

                var json = File.ReadAllText(AppConfig.CredentialsFile);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var cred = JsonSerializer.Deserialize<StoredCredential>(json, options);

                if (cred != null)
                {
                    _refreshToken = cred.refresh_token;
                    _userId = cred.user_id;
                    _tokenExpiry = DateTime.UtcNow.AddHours(1);
                    Log.Information("Loaded stored credentials for user: {UserId}", _userId);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to load stored token");
            }
        }

        private void SaveStoredToken()
        {
            try
            {
                var cred = new StoredCredential
                {
                    refresh_token = _refreshToken,
                    user_id = _userId
                };

                var json = JsonSerializer.Serialize(cred);
                File.WriteAllText(AppConfig.CredentialsFile, json);
                Log.Information("Token saved to: {File}", AppConfig.CredentialsFile);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save token");
            }
        }

        private class AuthPayload
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public string user_id { get; set; }
        }

        private class StoredCredential
        {
            public string refresh_token { get; set; }
            public string user_id { get; set; }
        }
    }
}
