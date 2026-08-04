# PDFit Companion - Build Guide

## Overview

PDFit Companion is a Windows system-tray application that monitors a local folder for PDF files and automatically uploads them to PDFit for editing.

**Key flow:**
1. User prints from any application to the "PDFit" printer
2. PDF lands in `%LOCALAPPDATA%\PDFit\Spool\`
3. Companion detects the file, reads metadata, creates a project in Supabase
4. Uploads PDF to private storage bucket
5. Registers media row in Supabase
6. Shows "Opened in PDFit" toast notification
7. User clicks notification or visits PDFit web app to edit

## Prerequisites

- **Windows 10 or later** (for WPF and .NET 8)
- **.NET 8 SDK** — Download from https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- **WiX Toolset 4.0+** — Download from https://wixtoolset.org/
- **Visual Studio 2022** or **Visual Studio Code** with C# extension (optional; command-line builds work)

## Directory Structure

```
PDFitCompanion/
├── PDFitCompanion.csproj          # Project file
├── App.xaml / App.xaml.cs         # Application entry point
├── MainWindow.xaml / .cs          # System tray window
├── Config/
│   └── AppConfig.cs               # Configuration (Supabase URLs, paths)
├── Services/
│   ├── SupabaseService.cs         # Supabase API calls
│   ├── AuthService.cs             # OAuth and token management
│   ├── SpoolMonitor.cs            # File system watcher
│   ├── PrinterSetup.cs            # Printer registration
│   ├── NotificationService.cs     # User notifications
│   └── NotifyIconService.cs       # System tray icon
├── PDFitCompanion.wxs             # WiX installer definition
├── latest.json                    # Auto-update manifest
└── BUILD_GUIDE.md                 # This file
```

## Step 1: Clone / Set Up Project

Copy the entire `PDFitCompanion/` folder to your local machine:

```bash
cd C:\path\to\your\projects
# Place the PDFitCompanion folder here
```

## Step 2: Build the .NET Application

From the `PDFitCompanion/` directory:

```bash
dotnet restore
dotnet build -c Release -r win-x64
```

**Output:** `bin\Release\net8.0-windows\win-x64\PDFitCompanion.exe`

If the build fails:
- Ensure .NET 8 SDK is installed: `dotnet --version`
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Retry: `dotnet build -c Release`

## Step 3: Create the MSI Installer

### Option A: Using Visual Studio (Recommended)

1. Open Visual Studio 2022
2. **File > New Project** → Search for "WiX Project"
3. Replace the `Product.wxs` with `PDFitCompanion.wxs` from your project folder
4. Edit the path to the executable in the `.wxs` file to point to your `bin\Release\` output
5. Right-click the WiX project → **Build**
6. Output: `bin\Release\PDFitCompanion.msi`

### Option B: Command-line with WiX

After installing WiX:

```bash
wix build -o PDFitCompanion.msi PDFitCompanion.wxs -d PDFitCompanionSourceDir=bin\Release\net8.0-windows\win-x64
```

## Step 4: Code Signing (Optional but Recommended)

Signing prevents SmartScreen warnings and proves the publisher.

If you have an EV code-signing certificate:

```bash
signtool sign /f "path\to\cert.pfx" /p "password" /t http://timestamp.comodoca.com PDFitCompanion.exe
signtool sign /f "path\to\cert.pfx" /p "password" /t http://timestamp.comodoca.com PDFitCompanion.msi
```

Without a certificate, users will see a SmartScreen warning on first install (normal for unsigned apps).

## Step 5: Create the Auto-Update Manifest

Save this as `latest.json` and host it at `https://app.pdfit.co/downloads/latest.json`:

```json
{
  "version": "1.0.0",
  "downloadUrl": "https://app.pdfit.co/downloads/PDFit-Companion-Setup.msi",
  "releaseNotes": "Initial release of PDFit Companion",
  "releaseDate": "2024-01-15T00:00:00Z"
}
```

## Step 6: Host the MSI and Manifest

Upload to your CDN or download server:

1. `PDFitCompanion.msi` → `https://app.pdfit.co/downloads/PDFit-Companion-Setup.msi`
2. `latest.json` → `https://app.pdfit.co/downloads/latest.json`

Make both publicly accessible (no auth required).

## Step 7: Test Installation

On a clean Windows VM:

```bash
# Download and run the MSI
msiexec /i PDFitCompanion.msi /qb
```

Expected behavior:
- PDFit printer appears in Settings > Devices > Printers & Scanners
- System tray icon shows "PDFit Companion"
- Clicking the tray icon shows recent jobs
- First run prompts for authentication (browser opens to `https://app.pdfit.co/auth?companion=1`)
- After login, companion is ready to accept print jobs

## Step 8: Test the Full Workflow

1. Install the MSI
2. Authenticate in the browser (companion catches the `pdfit://auth?payload=...` redirect)
3. Print a document from Word/Excel/PDF reader to the "PDFit" printer
4. File appears in `%LOCALAPPDATA%\PDFit\Spool\` briefly
5. Companion uploads to Supabase
6. Toast notification: "Opened in PDFit"
7. Visit `https://app.pdfit.co` and see the new project with the PDF

## Troubleshooting

### Printer doesn't appear in Print dialog

The printer setup runs with elevated permissions during MSI install. If it fails:

1. Open PowerShell as Administrator
2. Run: `Get-Printer | Where-Object {$_.Name -eq "PDFit"}`
3. If not found, manually add:
   ```powershell
   Add-Printer -Name "PDFit" -DriverName "Microsoft Print to PDF" -PortName "FILE:"
   ```

### Companion doesn't authenticate

1. Check log file: `%LOCALAPPDATA%\PDFit\Logs\`
2. Ensure `https://app.pdfit.co` is accessible
3. Confirm `pdfit://` protocol handler is registered in Windows:
   ```
   HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.pdfit\UserChoice
   ```

### Files don't upload to Supabase

1. Verify Supabase credentials in `Config/AppConfig.cs`
2. Check that storage bucket exists: `project-files`
3. Confirm RLS policies allow anon inserts on `projects`, `media` tables
4. View companion logs for detailed error messages

### Spool folder fills up

If files aren't being processed:
- Manually check `%LOCALAPPDATA%\PDFit\Spool\` for stuck files
- Verify companion process is running: Task Manager > pdfit.exe
- Check Windows Event Viewer > Application for errors

## Configuration

All configuration is in `Config/AppConfig.cs`:

```csharp
public const string SupabaseUrl = "https://wneevllgrryobsxocach.supabase.co";
public const string SupabaseAnonKey = "eyJ...";
public const string PrinterName = "PDFit";
public const string StorageBucket = "project-files";
```

To change printer name, spool location, or Supabase credentials:
1. Edit `AppConfig.cs`
2. Rebuild: `dotnet build -c Release`
3. Rebuild MSI with updated executable
4. Reinstall on target machines

## Auto-Update Implementation (Planned)

The companion should periodically check `latest.json`:

```csharp
// In MainWindow or a background task
var manifest = await http.GetAsync("https://app.pdfit.co/downloads/latest.json");
var latestVersion = JsonSerializer.Deserialize<UpdateManifest>(await manifest.Content.ReadAsStringAsync());

if (latestVersion.Version > CurrentVersion)
{
    // Download and run PDFit-Companion-Setup.msi
    // Companion gracefully closes and lets installer run
}
```

This keeps users always on the latest version with printer setup fixes and Supabase schema updates.

## Deployment Checklist

Before releasing to users:

- [ ] Dotnet build succeeds with zero errors
- [ ] WiX builds MSI without warnings
- [ ] MSI installs on clean Windows 10 VM
- [ ] Printer "PDFit" appears in Settings > Printers
- [ ] System tray icon shows and is clickable
- [ ] First authentication flow works end-to-end
- [ ] Test print job from Word is uploaded to Supabase
- [ ] Toast notification appears on success
- [ ] Project and media row appear in Supabase dashboard
- [ ] Logs in `%LOCALAPPDATA%\PDFit\Logs\` show no errors
- [ ] Uninstall works cleanly and removes all files

## Support

For issues:
1. Check logs: `%LOCALAPPDATA%\PDFit\Logs\pdfit-companion-*.txt`
2. Verify Supabase credentials and RLS policies
3. Ensure Windows .NET 8 runtime is installed
4. Test printer manually: `Get-Printer` in PowerShell
