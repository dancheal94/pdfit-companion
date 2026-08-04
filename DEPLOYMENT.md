# PDFit Companion - GitHub Actions Deployment Guide

## Overview

This project uses **GitHub Actions** to automatically build the Windows MSI installer on every push to `main` (or when manually triggered). No Windows machine or local setup required—just push code and the MSI is built in the cloud.

## One-Time Setup

### 1. Push the Project to GitHub

```bash
# From your local machine
git init
git add .
git commit -m "Initial commit: PDFit Companion"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/pdfit-companion.git
git push -u origin main
```

### 2. Verify GitHub Actions is Enabled

1. Go to **Settings > Actions > General**
2. Ensure "Allow all actions and reusable workflows" is selected
3. Click **Save**

### 3. (Optional) Set Up Auto-Upload to S3 or CDN

If you want the MSI automatically uploaded to your download server, add a step to the workflow:

Edit `.github/workflows/build-msi.yml` and add before the final step:

```yaml
      - name: Upload to S3
        if: github.ref == 'refs/heads/main'
        run: |
          aws s3 cp PDFitCompanion.msi s3://YOUR_BUCKET/downloads/PDFitCompanion-Setup.msi --acl public-read
        env:
          AWS_ACCESS_KEY_ID: ${{ secrets.AWS_ACCESS_KEY_ID }}
          AWS_SECRET_ACCESS_KEY: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          AWS_REGION: us-east-1
```

Then add your AWS credentials to **Settings > Secrets and variables > Actions**.

## Build Workflow

### Automatic Builds (On Push)

Every time you push to `main`, GitHub Actions:

1. Checks out your code
2. Installs .NET 8 SDK
3. Installs WiX Toolset
4. Restores NuGet packages
5. Builds the .NET application
6. Packages it as an MSI
7. Uploads the MSI as a downloadable artifact

### Manual Triggers

To manually trigger a build:

1. Go to **Actions** tab on GitHub
2. Select **"Build PDFit Companion MSI"** workflow
3. Click **"Run workflow"**
4. Choose branch and click **"Run workflow"**

The build starts immediately.

## Downloading the MSI

### From GitHub Artifacts (Temporary, 30 days)

1. Go to **Actions** tab
2. Click the latest successful run
3. Scroll to **Artifacts** section
4. Download **PDFitCompanion-Setup**

### From GitHub Releases (Permanent)

For releases, create a GitHub tag:

```bash
git tag v1.0.0
git push origin v1.0.0
```

The workflow automatically creates a release and attaches the MSI. Users can download from **Releases** page.

### From Your CDN (Recommended)

1. Download the MSI from GitHub Artifacts
2. Upload to your hosting server:
   ```bash
   aws s3 cp PDFitCompanion.msi s3://your-bucket/downloads/
   ```
3. Link users to: `https://app.pdfit.co/downloads/PDFitCompanion-Setup.msi`

## Development Workflow

### Making Changes

1. **Edit code** on your Mac or any machine
2. **Commit and push**:
   ```bash
   git add .
   git commit -m "Fix: improve printer detection"
   git push
   ```
3. **GitHub Actions builds automatically**
4. **Download the new MSI** from Actions > Artifacts
5. **Test on a Windows machine** (or VM)
6. **If good, upload to your CDN** for users to download

### Updating Configuration

To change Supabase credentials, printer name, or spool location:

1. Edit `Config/AppConfig.cs`
2. Commit and push
3. GitHub Actions rebuilds with new config
4. Download new MSI and deploy

### Versioning

Update version in two places:

```csharp
// PDFitCompanion.csproj
<Version>1.0.1</Version>
```

```json
// latest.json
{
  "version": "1.0.1",
  "downloadUrl": "https://app.pdfit.co/downloads/PDFitCompanion-Setup.msi",
  "releaseNotes": "Bug fixes and improvements",
  "releaseDate": "2024-01-20T00:00:00Z"
}
```

Then commit, push, tag, and GitHub Actions does the rest.

## Troubleshooting

### Build Failed

1. Go to **Actions** tab
2. Click the failed run
3. Click **"Build application"** or **"Build MSI"** step
4. Review the error message
5. Common issues:
   - **NuGet package not found**: Check internet, try `dotnet restore` locally
   - **WiX build error**: Verify .wxs syntax (XML well-formed?)
   - **Publish path mismatch**: Confirm output directory in .wxs matches publish folder

### Need to Skip a Build

Add `[skip ci]` to your commit message:

```bash
git commit -m "Update docs [skip ci]"
git push
```

### Need to Rebuild Specific Version

1. Go to **Actions** > **"Build PDFit Companion MSI"**
2. Click **"Run workflow"**
3. Choose branch
4. Click **"Run workflow"**

## Continuous Deployment (Advanced)

To auto-upload MSI to your server on every successful build:

1. Add your hosting credentials as GitHub Secrets:
   - **Settings > Secrets and variables > Actions**
   - Add: `DOWNLOAD_SERVER_URL`, `DOWNLOAD_SERVER_CREDENTIALS`, etc.

2. Modify `.github/workflows/build-msi.yml`:
   ```yaml
   - name: Upload to download server
     if: success()
     run: |
       curl -X POST https://app.pdfit.co/upload \
         -F "file=@PDFitCompanion.msi" \
         -H "Authorization: Bearer ${{ secrets.UPLOAD_TOKEN }}"
   ```

3. Push the updated workflow
4. Future builds auto-upload to your server

## Monitoring

### View Build Status

1. Go to **Actions** tab
2. See all past builds with status (✅ passed / ❌ failed)
3. Click any build to see logs

### Email Notifications

GitHub sends you email on:
- **Workflow failure** (automatic)
- **Workflow success** (optional, enable in Settings)

## Cleanup

### Remove Old Artifacts

GitHub automatically deletes artifacts after 30 days. To manually delete:

1. Go to **Settings > Actions > Artifact and log retention**
2. Set retention to desired days
3. Manually delete: **Actions** > select run > **Delete all artifacts**

### Archive Old Releases

For long-term storage, download old MSI files and store locally, then delete from GitHub Releases.

---

## Quick Reference

| Task | Command |
|------|---------|
| Push code and trigger build | `git push` |
| Manually trigger build | Go to Actions > Run workflow |
| Download MSI | Actions > Latest run > Artifacts |
| Create release | `git tag v1.0.0 && git push origin v1.0.0` |
| View build logs | Actions > Click run > Click step |
| Skip build | Commit with `[skip ci]` message |

---

**That's it!** Your workflow is now fully automated. Make changes, push, download the built MSI. No Windows setup needed.
