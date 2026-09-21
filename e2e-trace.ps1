$ErrorActionPreference = 'Stop'
$baseUrl = 'http://localhost:5079/api/v1'

# ── Step 1: Login ──────────────────────────────────────────────────────────
Write-Host '=== Step 1: Login ==='
$loginResp = Invoke-RestMethod -Method Post -Uri "$baseUrl/auth/login" `
    -ContentType 'application/json' `
    -Body '{"email":"admin@sainin.com","password":"P@ssw0rd123!"}'
$token = $loginResp.data.accessToken
Write-Host "  accessToken (first 40 chars): $($token.Substring(0,40))..."

# ── Step 2: Request upload URL ────────────────────────────────────────────
Write-Host ''
Write-Host '=== Step 2: Request video upload URL ==='
$headers = @{ Authorization = "Bearer $token" }
$uploadResp = Invoke-RestMethod -Method Post `
    -Uri "$baseUrl/admin/media/video-upload-url" `
    -Headers $headers -ContentType 'application/json' -Body '{}'
$uploadUrl = $uploadResp.data.uploadUrl
$assetId   = $uploadResp.data.cloudflareStreamId
Write-Host "  uploadUrl : $uploadUrl"
Write-Host "  assetId   : $assetId"

# ── Step 3: PUT (POST) a real test file to the local upload endpoint ───────
Write-Host ''
Write-Host '=== Step 3: Upload test file ==='
[System.IO.File]::WriteAllBytes('test-video.mp4', [byte[]](0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07))
$uploadAck = Invoke-RestMethod -Method Post -Uri $uploadUrl -InFile 'test-video.mp4'
Write-Host "  Server ack: $($uploadAck | ConvertTo-Json -Compress)"

# ── Step 4: Confirm file is on disk inside container ──────────────────────
Write-Host ''
Write-Host '=== Step 4: Confirm file on disk (docker exec) ==='
docker exec mediaportfolio_api ls -lh /media-storage/videos/

# ── Step 5: Fetch an existing category then create video ──────────────────
Write-Host ''
Write-Host '=== Step 5: Create video with real category ID ==='
$catListResp = Invoke-RestMethod -Method Get -Uri "$baseUrl/categories"
$catId = [string]$catListResp.data[0].id
Write-Host "  Using category: $catId ($($catListResp.data[0].nameEn))"

$createBody = @"
{
  "cloudflareStreamId": "$assetId",
  "titleAr": "Test Title",
  "descriptionAr": "Test Description",
  "categoryIds": ["$catId"],
  "status": "Draft"
}
"@
$createResp = Invoke-RestMethod -Method Post -Uri "$baseUrl/admin/videos" `
    -Headers $headers -ContentType 'application/json' -Body $createBody
$newVideoId = [string]$createResp.data
Write-Host "  Created video ID: $newVideoId"

# ── Step 6: List admin videos ─────────────────────────────────────────────
Write-Host ''
Write-Host '=== Step 6: List admin videos (page=1, pageSize=5) ==='
$listResp = Invoke-RestMethod -Method Get `
    -Uri "$baseUrl/admin/videos?page=1&pageSize=5" -Headers $headers
Write-Host ($listResp | ConvertTo-Json -Depth 6)

# ── Step 6.5: Publish the newly created video ─────────────────────────────
Write-Host ''
Write-Host '=== Step 6.5: Publish video ==='
$statusBody = @"
{ "id": "$newVideoId", "status": "Published" }
"@
$publishResp = Invoke-RestMethod -Method Patch `
    -Uri "$baseUrl/admin/videos/$newVideoId/status" `
    -Headers $headers -ContentType 'application/json' -Body $statusBody
Write-Host "  publishedAt: $($publishResp.data.publishedAt)"

# ── Step 7: Retrieve the file via the local serving endpoint ──────────────
Write-Host ''
Write-Host '=== Step 7: Retrieve file via local serving endpoint ==='
$serveUrl = "http://localhost:5079/local-media/video/$assetId"
Write-Host "  GET $serveUrl"
Invoke-RestMethod -Method Get -Uri $serveUrl -OutFile 'downloaded.mp4'
$downloaded = Get-Item 'downloaded.mp4'
Write-Host "  Downloaded file size: $($downloaded.Length) bytes"

Write-Host ''
Write-Host '=== ALL STEPS PASSED ==='
