# PDFit Companion - Installation Instructions

For **end users** who have downloaded `PDFitCompanion-Setup.msi`.

## System Requirements

- **Windows 10 or later** (64-bit)
- **Administrator privileges** (required to install printer)
- **.NET 8 Runtime** (will be installed if missing)
- **Internet connection** (for initial Supabase authentication)

## Installation Steps

### 1. Download the MSI

Download `PDFitCompanion-Setup.msi` from https://app.pdfit.co/downloads/

### 2. Run the Installer

1. Right-click `PDFitCompanion-Setup.msi` → **Run as Administrator**
2. Click **Next** on the welcome screen
3. Accept the license terms
4. Choose installation folder (default: `C:\Program Files\PDFit\`)
5. Click **Install**
6. Wait for installation to complete
7. Click **Finish**

### 3. First Run - Authentication

After installation:

1. A browser window opens automatically
2. Sign in with your PDFit account
3. After successful login, the browser redirects to a confirmation page
4. The companion app captures your authentication in the background
5. The browser closes, and the companion is ready to use

**Your authentication token is securely stored locally** and will auto-refresh every hour.

### 4. Print to PDFit

From any application (Word, Excel, Outlook, PDF reader, etc.):

1. **File > Print** (or Ctrl+P)
2. Select **PDFit** from the printer list
3. Click **Print**
4. The document is automatically uploaded to PDFit
5. A **"Opened in PDFit"** notification appears in your system tray
6. Visit https://app.pdfit.co to see and edit your document

## Using the Companion

### System Tray Icon

The companion runs in the background as a system-tray application:

- **Double-click the tray icon** to open the companion window
- **Right-click the tray icon** to see options:
  - Open PDFit (opens https://app.pdfit.co)
  - Exit (stops monitoring for print jobs)

### Notifications

You'll see toast notifications for:

- **"Opened in PDFit"** when a document is successfully processed
- **Error messages** if something goes wrong (e.g., not authenticated, network error)

### Files and Logs

The companion stores files in `%LOCALAPPDATA%\PDFit\`:

- **`Spool/`** — Temporary folder where printed PDFs land before upload
- **`Logs/`** — Diagnostic logs (useful for troubleshooting)

## Troubleshooting

### "PDFit" printer doesn't appear in Print dialog

The printer should have been installed automatically. If it's missing:

1. Open Windows **Settings > Devices > Printers & Scanners**
2. Look for "PDFit" in the list
3. If not there, reinstall the companion:
   - Uninstall from Control Panel > Programs
   - Download the latest `PDFitCompanion-Setup.msi`
   - Run as Administrator

### Authentication window didn't open

1. Open a browser manually and visit https://app.pdfit.co
2. Sign in
3. Open the companion to try again

### Print job doesn't upload

1. Check the notification tray for error messages
2. Look at the logs: Open File Explorer → type `%LOCALAPPDATA%\PDFit\Logs\` in the address bar
3. Share the latest log file with PDFit support

### Companion crashes or stops working

1. Open Task Manager (Ctrl+Shift+Esc)
2. Look for **PDFitCompanion.exe** in the process list
3. If found, click it and select **End Task**
4. Restart your computer
5. The companion will restart automatically (it's set to run at startup)

## Uninstallation

To remove the companion:

1. Open **Control Panel > Programs > Programs and Features**
2. Find **"PDFit Companion"** in the list
3. Click **Uninstall**
4. Confirm the uninstall
5. The "PDFit" printer will also be removed

## Privacy & Security

- Your **authentication token** is stored locally in Windows Credential Manager (encrypted)
- **Printed documents** are sent directly to PDFit's Supabase server over HTTPS
- The companion **never collects usage data** or telemetry
- Logs are stored locally and are **not uploaded** unless you explicitly share them

## Getting Help

If you encounter issues:

1. Check the troubleshooting section above
2. Visit https://app.pdfit.co/support
3. Email support@pdfit.co with:
   - Windows version (Settings > System > About)
   - When the issue started
   - Log files from `%LOCALAPPDATA%\PDFit\Logs\`

---

**Have a question?** Check the [FAQ](https://app.pdfit.co/faq) or contact support.
