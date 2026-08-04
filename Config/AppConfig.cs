using System;
using System.IO;

namespace PDFitCompanion.Config
{
    public static class AppConfig
    {
        public const string SupabaseUrl = "https://wneevllgrryobsxocach.supabase.co";
        public const string SupabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InduZWV2bGxncnJ5b2JzeG9jYWNoIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjgzNTExNjIsImV4cCI6MjA4MzkyNzE2Mn0.5akyvq5gkPCzZPEnmO_-7Ksi38aG9FQhcKmcvvajSd4";
        public const string PrinterName = "PDFit";
        public const string StorageBucket = "project-files";
        public const string AuthBrowserUrl = "https://app.pdfit.co/auth?companion=1";

        public static string SpoolDirectory
        {
            get
            {
                var path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PDFit", "Spool");
                Directory.CreateDirectory(path);
                return path;
            }
        }

        public static string ConfigDirectory
        {
            get
            {
                var path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PDFit");
                Directory.CreateDirectory(path);
                return path;
            }
        }

        public static string LogDirectory
        {
            get
            {
                var path = Path.Combine(ConfigDirectory, "Logs");
                Directory.CreateDirectory(path);
                return path;
            }
        }

        public static string CredentialsFile
        {
            get { return Path.Combine(ConfigDirectory, "auth.json"); }
        }
    }
}
