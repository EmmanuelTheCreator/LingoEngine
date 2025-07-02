# Requires PowerShell 5+
$ErrorActionPreference = "Stop"

$rootDir = Resolve-Path (Join-Path $PSScriptRoot "..")
$docfxJson = Join-Path $rootDir "docs/docfx/docfx.json"
$docfxOutput = Join-Path $rootDir "docs/docfx/_site"
$wikiTempDir = Join-Path $rootDir ".wiki-tmp"
$wikiRepoUrl = "https://github.com/EmmanuelTheCreator/LingoEngine.wiki.git"

# Ensure DocFX is available
if (-not (Get-Command "docfx" -ErrorAction SilentlyContinue)) {
    throw "DocFX not found. Install with 'dotnet tool install -g docfx'"
}

Write-Host "🧱 Running DocFX..."
docfx build $docfxJson --output $docfxOutput

Write-Host "🧹 Cleaning old wiki clone..."
Remove-Item -Recurse -Force -ErrorAction Ignore $wikiTempDir

Write-Host "🔄 Cloning wiki repository..."
git clone $wikiRepoUrl $wikiTempDir

# Copy generated Markdown to wiki
$articlesPath = Join-Path $docfxOutput "articles"
Write-Host "📄 Copying generated .md files from $articlesPath"
Get-ChildItem "$articlesPath\*.md" | ForEach-Object {
    Copy-Item $_.FullName -Destination $wikiTempDir -Force
}

# Commit and push
Set-Location $wikiTempDir
git add .
git commit -m "Update wiki with generated docs" -ErrorAction SilentlyContinue
# Push may fail on read-only or CI environments
try { git push } catch { Write-Warning $_ }

Write-Host "`n✅ Wiki updated!"
