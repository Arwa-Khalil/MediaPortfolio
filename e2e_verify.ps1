$ErrorActionPreference = "Stop"

Write-Host "=== PHASE 2 LOCAL-MODE E2E TRACE ===" -ForegroundColor Cyan
Write-Host ""

# ─────────────────────────────────────────────────────────────────────────────
Write-Host "--- STEP 1: Login as admin ---" -ForegroundColor Yellow
$loginBody = @{
    email = "admin@sainin.com"
    password = "P@ssw0rd123!"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod `
    -Uri "http://localhost:5079/api/v1/auth/login" `
    -Method Post `
    -Body $loginBody `
    -ContentType "application/json"

$token = $loginResponse.data.accessToken
Write-Host "Login succeeded. Token (first 40 chars): $($token.Substring(0, 40))..."

$authHeaders = @{ "Authorization" = "Bearer $token" }

# ─────────────────────────────────────────────────────────────────────────────
Write-Host "`n--- STEP 2: Request a video upload URL ---" -ForegroundColor Yellow
$uploadUrlResponse = Invoke-RestMethod `
    -Uri "http://localhost:5079/api/v1/admin/media/video-upload-url" `
    -Method Post `
    -Headers $authHeaders

Write-Host "Full upload-url response:"
$uploadUrlResponse | ConvertTo-Json -Depth 4

$uploadUrl  = $uploadUrlResponse.data.uploadUrl
$assetId    = $uploadUrlResponse.data.cloudflareStreamId
Write-Host "Upload URL : $uploadUrl"
Write-Host "Asset ID   : $assetId"

# ─────────────────────────────────────────────────────────────────────────────
Write-Host "`n--- STEP 3: PUT a real test file to the upload URL ---" -ForegroundColor Yellow
$testFilePath = ".\test_video.mp4"
"dummy video content for e2e test $(Get-Date -Format 'o')" | Out-File -FilePath $testFilePath -Encoding ascii

Invoke-RestMethod `
    -Uri $uploadUrl `
    -Method Post `
    -InFile $testFilePath `
    -ContentType "application/octet-stream"

Write-Host "Upload POST complete."

# ─────────────────────────────────────────────────────────────────────────────
Write-Host "`n--- STEP 4: Confirm file is on disk inside container ---" -ForegroundColor Yellow
$diskLs = docker exec mediaportfolio_api ls -la /media-storage/videos/
Write-Host $diskLs

# ─────────────────────────────────────────────────────────────────────────────
Write-Host "`n--- STEP 5: Get a seeded category ID ---" -ForegroundColor Yellow
$categoriesResponse = Invoke-RestMethod `
    -Uri "http://localhost:5079/api/v1/categories" `
    -Method Get

Write-Host "Categories response:"
$categoriesResponse | ConvertTo-Json -Depth 4

$categoryId = $categoriesResponse.data[0].id
Write-Host "Using category ID: $categoryId"

# ─────────────────────────────────────────────────────────────────────────────
Write-Host "`n--- STEP 6: Create a video with real category ID ---" -ForegroundColor Yellow
$createVideoBodyJson = @{
    titleAr                  = "فيديو تجريبي E2E"
    descriptionAr            = "وصف تجريبي"
    categoryIds              = @($categoryId)
    cloudflareStreamId       = $assetId
    cloudflareThumbnailImageId = $null
    status                   = 1    # Published = 1
} | ConvertTo-Json

# Explicitly encode body as UTF-8 bytes so Arabic is transmitted correctly
$utf8Bytes = [System.Text.Encoding]::UTF8.GetBytes($createVideoBodyJson)

$createVideoResponse = Invoke-RestMethod `
    -Uri "http://localhost:5079/api/v1/admin/videos" `
    -Method Post `
    -Headers $authHeaders `
    -Body $utf8Bytes `
    -ContentType "application/json; charset=utf-8"

Write-Host "Create video response:"
$createVideoResponse | ConvertTo-Json -Depth 4
$videoId = $createVideoResponse.data

# ─────────────────────────────────────────────────────────────────────────────
Write-Host "`n--- STEP 7: List admin videos (page 1, size 20) ---" -ForegroundColor Yellow
$listResponse = Invoke-RestMethod `
    -Uri "http://localhost:5079/api/v1/admin/videos?page=1&pageSize=20" `
    -Method Get `
    -Headers $authHeaders

Write-Host "Admin videos list (full JSON):"
$listResponse | ConvertTo-Json -Depth 6

# ─────────────────────────────────────────────────────────────────────────────
Write-Host "`n--- STEP 8: Retrieve file via local serving endpoint ---" -ForegroundColor Yellow
$downloadPath = ".\downloaded_video.mp4"
Invoke-RestMethod `
    -Uri "http://localhost:5079/local-media/video/$assetId" `
    -Method Get `
    -OutFile $downloadPath

$downloadSize = (Get-Item $downloadPath).Length
Write-Host "Downloaded file size: $downloadSize bytes"

# ─────────────────────────────────────────────────────────────────────────────
Write-Host "`n--- CLEANUP ---" -ForegroundColor Yellow
Remove-Item $testFilePath -ErrorAction SilentlyContinue
Remove-Item $downloadPath -ErrorAction SilentlyContinue
Write-Host "Temp files removed."

Write-Host "`n=== E2E TRACE COMPLETE ===" -ForegroundColor Cyan
