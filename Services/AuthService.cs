using System.Diagnostics;
using System.Text.Json;
using System.Web;
using Serilog;

namespace PDFitCompanion.Services
{
    public class AuthService
    {
        private string? _accessToken;
        private string? _refreshToken;
        private string? _userId;
        private DateTime _tokenExpiry;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry;
        public string? UserId => _userId;
        public string? AccessToken => _accessToken;

        public AuthService()
        {
            LoadStoredToken();
            SetupProtocolHandler();
        }

        public void OpenAuthBrowser()
        {
            try
            {
                var authUrl = "https://app.pdfit.co/auth?companion=1";
                Process.Start(new ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                });
                Log.Information("Browser opened for authentication");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open auth browser");
                throw;
            }
        }

        public void HandleAuthCallback(string payload)
        {
            try
            {
                var json = JsonSerializer.Deserialize<AuthPayload>(payload);
                if (json == null) throw new InvalidOperationException("Invalid auth payload");

                _accessToken = json.access_token;
                _refreshToken = json.refresh_token;
                _userId = json.user_id;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(3600);

                SaveStoredToken();
                Log.Information("Authentication successful for user: {UserId}", _userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to handle auth callback");
                throw;
            }
        }

        private void LoadStoredToken()
        {
            try
            {
                var credFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PDFit", "auth.json");

                if (!File.Exists(credFile)) return;

                var json = File.ReadAllText(credFile);
                var cred = JsonSerializer.Deserialize<StoredCredential>(json);
                if (cred != null)
                {
                    _refreshToken = cred.refresh_token;
                    _userId = cred.user_id;
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
                var credDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PDFit");
                Directory.CreateDirectory(credDir);

                var cred = new StoredCredential
                {
                    refresh_token = _refreshToken,
                    user_id = _userId
                };

                var json = JsonSerializer.Serialize(cred);
                File.WriteAllText(Path.Combine(credDir, "auth.json"), json);
                Log.Information("Token saved securely");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save token");
            }
        }

        private void SetupProtocolHandler()
        {
            // In production, this would register pdfit:// protocol in Windows registry
            // For now, we'll handle it through command-line arguments
        }

        private class AuthPayload
        {
            public string? access_token { get; set; }
            public string? refresh_token { get; set; }
            public string? user_id { get; set; }
        }

        private class StoredCredential
        {
            public string? refresh_token { get; set; }
            public string? user_id { get; set; }
        }
    }
}
