# PDFit Companion - Windows Print to PDF Editor

A lightweight Windows system-tray application that seamlessly integrates with PDFit, enabling users to print from any application directly to the PDFit web editor.

## What It Does

1. **Installs a virtual "PDFit" printer** on Windows
2. **Monitors print jobs** — when a user prints to PDFit, the PDF lands in a local spool folder
3. **Authenticates with Supabase** — securely stores user credentials locally
4. **Uploads to PDFit** — automatically creates a project, uploads the PDF, and registers it in the database
5. **Notifies the user** — shows a toast notification and the document is ready to edit in PDFit

## Key Features

- ✅ Works with any Windows application (Word, Excel, PDF readers, browsers, etc.)
- ✅ Zero user configuration — set once, print forever
- ✅ Secure authentication via OAuth flow captured by the browser
- ✅ Runs silently in system tray
- ✅ Automatic token refresh every hour
- ✅ Detailed logging for troubleshooting
- ✅ Auto-update capability via `latest.json` manifest

## Architecture

```
Supabase (PDFit backend)
       ↑
       │ (HTTPS)
       │
[PDFit Companion]
       ↑
       │ (File system monitoring)
       │
[Local Spool Folder]
       ↑
       │ (Print spooler)
       │
   [PDFit Printer]
       ↑
[Any Windows Application]
```

## File Manifest

### Core Application

- **PDFitCompanion.csproj** — .NET 8 project file with dependencies
- **App.xaml / App.xaml.cs** — Application entry point, logging setup
- **MainWindow.xaml / MainWindow.xaml.cs** — System tray UI (hidden by default)

### Services (Logic)

- **Config/AppConfig.cs** — Configuration constants (Supabase URLs, printer name, file paths)
- **Services/SupabaseService.cs** — Supabase API integration (create projects, upload files, register media)
- **Services/AuthService.cs** — OAuth flow and token management (Credential Manager integration)
- **Services/SpoolMonitor.cs** — File system watcher for spool folder
- **Services/PrinterSetup.cs** — Virtual printer registration via Windows Print Spooler
- **Services/NotificationService.cs** — In-app notification logic
- **Services/NotifyIconService.cs** — System tray icon and context menu

### Installer & Deployment

- **PDFitCompanion.wxs** — WiX installer definition
- **latest.json** — Auto-update manifest (version, download URL, release notes)

### Documentation

- **BUILD_GUIDE.md** — How to build the project locally (for developers)
- **INSTALLATION_SETUP.md** — How to install and use (for end users)
- **README.md** — This file

## Quick Start

### For Developers

1. Clone or download this folder
2. Ensure .NET 8 SDK and WiX 4.0+ are installed
3. Build: `dotnet build -c Release -r win-x64`
4. Create MSI: Follow `BUILD_GUIDE.md`
5. Host `PDFitCompanion-Setup.msi` and `latest.json` on a CDN

### For End Users

1. Download `PDFitCompanion-Setup.msi`
2. Run as Administrator (right-click > Run as Administrator)
3. Complete the installer
4. Authenticate in the browser window
5. Print to the "PDFit" printer from any application

## Technical Details

### Language & Stack

- **Language**: C# (.NET 8)
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Backend**: Supabase (PostgreSQL, Auth, Storage)
- **Installer**: WiX Toolset 4.0
- **Logging**: Serilog
- **PDF Library**: PdfSharpCore (for reading page count)

### Supabase Integration

The companion interacts with Supabase using the official .NET SDK:

- **Create Project**: `POST /rest/v1/projects` — Creates a new document project
- **Upload File**: `POST /storage/v1/object/project-files/{userId}/{projectId}/{filename}` — Stores the PDF
- **Register Media**: `POST /rest/v1/media` — Registers the file in the database

All requests use the **anon key** embedded at build time (safe for client apps).

### Authentication Flow

1. **First run**: Companion opens browser to `https://app.pdfit.co/auth?companion=1`
2. **User signs in**: Browser authenticates with Supabase
3. **Redirect capture**: PDFit web app redirects to `pdfit://auth?payload=<JSON>`
4. **Token storage**: Companion captures the URL, extracts `access_token`, `refresh_token`, `user_id`
5. **Secure storage**: Tokens saved to `%LOCALAPPDATA%\PDFit\auth.json`
6. **Auto-refresh**: Every hour, refresh token is used to get a new access token

### File Processing Flow

1. **Detect**: FileSystemWatcher sees new PDF in spool folder
2. **Wait**: Waits 500ms for file write to complete (handle release)
3. **Read**: Opens PDF with PdfSharpCore to get page count
4. **Create**: Creates project in Supabase
5. **Upload**: Uploads PDF to `project-files` storage bucket
6. **Register**: Inserts media row in database
7. **Notify**: Shows "Opened in PDFit" toast notification
8. **Cleanup**: Deletes spool file

## Configuration

All configuration is centralized in `Config/AppConfig.cs`:

```csharp
public const string SupabaseUrl = "https://wneevllgrryobsxocach.supabase.co";
public const string SupabaseAnonKey = "eyJ...";
public const string PrinterName = "PDFit";
```

To change:
1. Edit `AppConfig.cs`
2. Rebuild: `dotnet build -c Release`
3. Rebuild MSI with new executable

## Deployment

### Hosting

1. Build the MSI
2. Optionally code-sign it (avoids SmartScreen warnings)
3. Upload to a CDN or download server:
   - `PDFitCompanion-Setup.msi` at `https://app.pdfit.co/downloads/`
   - `latest.json` at `https://app.pdfit.co/downloads/latest.json`

### Distribution

- **Direct download link** on https://app.pdfit.co
- **In-app download prompt** (e.g., "Install the companion for seamless printing")
- **Auto-update** — Companion checks `latest.json` periodically

## Troubleshooting

**Printer doesn't appear:**
- MSI must run as Administrator to register the printer
- If missing, uninstall and reinstall

**Can't authenticate:**
- Ensure internet connection
- Check firewall / proxy settings
- Confirm https://app.pdfit.co is reachable

**Files don't upload:**
- Check logs at `%LOCALAPPDATA%\PDFit\Logs\`
- Verify Supabase credentials are correct
- Ensure storage bucket and tables exist

**Companion won't start:**
- Check Task Manager for crashes
- Look at Event Viewer > Application > Windows Logs
- Review logs in `%LOCALAPPDATA%\PDFit\Logs\`

See `BUILD_GUIDE.md` for more detailed troubleshooting.

## Future Enhancements

- [ ] **UI improvements**: Better tray menu, settings dialog for printer name / spool location
- [ ] **Auto-update**: Companion checks `latest.json` and self-updates
- [ ] **Multiple printers**: Support "Append to existing project" mode
- [ ] **Offline support**: Queue uploads when offline, sync when online
- [ ] **Supabase auth SDK**: Use official Supabase C# SDK for auth (simpler token refresh)
- [ ] **Signed MSI**: Add code-signing certificate to avoid SmartScreen warnings

## Security Considerations

- ✅ Tokens stored locally, never transmitted in logs
- ✅ HTTPS only for all Supabase communication
- ✅ Anon key is public (by design), only has insert/select permissions
- ✅ RLS policies enforce user isolation (can only access own projects/files)
- ✅ PDF files stored in private Supabase bucket
- ✅ No telemetry or analytics

## Support & Issues

- **User Support**: https://app.pdfit.co/support
- **Developer Documentation**: See `BUILD_GUIDE.md`
- **Bug Reports**: Create an issue in the project repository
- **Questions**: Contact support@pdfit.co

---

**Version**: 1.0.0  
**Last Updated**: January 2024  
**License**: Proprietary (PDFit, Inc.)
