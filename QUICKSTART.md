# PDFit Companion - Quick Start (5 Minutes)

## For the Impatient

**Goal:** Get automated MSI builds so you download an installer, users run it, done.

### Step 1: Push to GitHub

```bash
# On your Mac, in the PDFitCompanion folder:
git init
git add .
git commit -m "Initial: PDFit Companion"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/pdfit-companion.git
git push -u origin main
```

### Step 2: Verify GitHub Actions

1. Open your repo on GitHub
2. Click **Settings > Actions > General**
3. Select "Allow all actions and reusable workflows"
4. Click **Save**

### Step 3: Trigger a Build

1. Go to **Actions** tab
2. Click **"Build PDFit Companion MSI"** on the left
3. Click **"Run workflow"** button
4. Choose `main` branch
5. Click **"Run workflow"** (green button)

Wait 2–3 minutes.

### Step 4: Download the MSI

1. Go back to **Actions** tab
2. Click the latest successful run
3. Scroll down to **Artifacts**
4. Click **PDFitCompanion-Setup** to download

### Step 5: Test (On Windows)

1. Open the MSI on a Windows machine
2. Click through the installer
3. Done. Printer is installed, app runs in tray.

### Step 6: Host It

Upload the MSI to your download server:

```bash
aws s3 cp PDFitCompanion.msi s3://your-bucket/downloads/PDFitCompanion-Setup.msi
```

Share the link: `https://app.pdfit.co/downloads/PDFitCompanion-Setup.msi`

## That's It

From now on:
- **You make changes** → git push
- **GitHub builds automatically** → MSI ready in 2-3 minutes
- **You download and test**
- **Upload to your server** when ready

See `DEPLOYMENT.md` for advanced stuff (auto-uploads, releases, etc.).
