namespace PDFitCompanion.Config
{
    public static class AppConfig
    {
        public const string SupabaseUrl = "https://wneevllgrryobsxocach.supabase.co";
        public const string SupabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InduZWV2bGxncnJ5b2JzeG9jYWNoIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjgzNTExNjIsImV4cCI6MjA4MzkyNzE2Mn0.5akyvq5gkPCzZPEnmO_-7Ksi38aG9FQhcKmcvvajSd4";
        public const string PrinterName = "PDFit";
        public const string StorageBucket = "project-files";

        public static string SpoolDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PDFit", "Spool");

        public static string ConfigDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PDFit");

        public static string LogDirectory => Path.Combine(ConfigDirectory, "Logs");

        public static void EnsureDirectories()
        {
            Directory.CreateDirectory(SpoolDirectory);
            Directory.CreateDirectory(ConfigDirectory);
            Directory.CreateDirectory(LogDirectory);
        }
    }
}
