# Creates sample warranty claims in every status by driving the REAL web flow (customer submits with a video,
# staff reviews/approves/receives/forwards/ships, customer reports shipment/cancels/confirms). Codes, history,
# notifications and stock movements are therefore exactly what the app itself would produce.
# Needs: the app running (BaseUrl), sample delivered orders from Docs\Seed-DeliveredOrders.ps1, PS 5.1+.
# Usage: powershell -ExecutionPolicy Bypass -File Docs\Seed-WarrantyClaims.ps1 -BaseUrl http://localhost:5263
param(
    [string]$BaseUrl = 'http://localhost:5263',
    [string]$VideoPath = (Join-Path $PSScriptRoot 'SampleData\claim-evidence-sample.mp4'),
    [string]$Server = '(localdb)\MSSQLLocalDB',
    [string]$Database = 'PopPopShop'
)
$ErrorActionPreference = 'Stop'
$tag = '[Dữ liệu mẫu bảo hành]'
$customerPassword = 'PopPop@123'
$staffLogin = '0911222301'   # Trần Văn Long (seeded staff)

# ---------------------------------------------------------------- HTTP helpers (one cookie jar per person)
function New-Session { @{ Jar = New-Object System.Net.CookieContainer } }

function Invoke-Http($session, [string]$method, [string]$path, $form = $null, [string]$filePath = $null) {
    $request = [System.Net.HttpWebRequest]::Create($BaseUrl + $path)
    $request.Method = $method; $request.CookieContainer = $session.Jar; $request.AllowAutoRedirect = $false
    $request.Timeout = 120000
    if ($form) {
        $stream = New-Object System.IO.MemoryStream
        if ($filePath) {
            $boundary = '----PopPopSeed' + [Guid]::NewGuid().ToString('N')
            $request.ContentType = "multipart/form-data; boundary=$boundary"
            $write = { param($text) $bytes = [Text.Encoding]::UTF8.GetBytes($text); $stream.Write($bytes, 0, $bytes.Length) }
            foreach ($key in $form.Keys) {
                & $write "--$boundary`r`nContent-Disposition: form-data; name=`"$key`"`r`n`r`n$($form[$key])`r`n"
            }
            & $write "--$boundary`r`nContent-Disposition: form-data; name=`"files`"; filename=`"video-loi.mp4`"`r`nContent-Type: video/mp4`r`n`r`n"
            $fileBytes = [IO.File]::ReadAllBytes($filePath); $stream.Write($fileBytes, 0, $fileBytes.Length)
            & $write "`r`n--$boundary--`r`n"
        }
        else {
            $request.ContentType = 'application/x-www-form-urlencoded'
            $body = ($form.Keys | ForEach-Object { [Uri]::EscapeDataString($_) + '=' + [Uri]::EscapeDataString([string]$form[$_]) }) -join '&'
            $bytes = [Text.Encoding]::UTF8.GetBytes($body); $stream.Write($bytes, 0, $bytes.Length)
        }
        $request.ContentLength = $stream.Length
        $requestStream = $request.GetRequestStream(); $stream.WriteTo($requestStream); $requestStream.Close()
    }
    try { $response = $request.GetResponse() } catch [System.Net.WebException] { $response = $_.Exception.Response; if (-not $response) { throw } }
    $html = [Net.WebUtility]::HtmlDecode((New-Object IO.StreamReader($response.GetResponseStream())).ReadToEnd())
    $result = [pscustomobject]@{ Status = [int]$response.StatusCode; Location = $response.Headers['Location']; Html = $html }
    $response.Close(); $result
}

function Get-Token($html) {
    $match = [regex]::Match($html, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
    if (-not $match.Success) { throw 'Antiforgery token not found (not signed in?).' }
    $match.Groups[1].Value
}

function Connect-Customer([string]$login) {
    $session = New-Session
    $page = Invoke-Http $session GET '/login'
    $result = Invoke-Http $session POST '/login' @{ 'Input.Login' = $login; 'Input.Password' = $customerPassword; '__RequestVerificationToken' = (Get-Token $page.Html) }
    if ($result.Status -ne 302) { throw "Customer $login could not sign in." }
    $session
}

function Connect-Staff([string]$login) {
    $session = New-Session
    $page = Invoke-Http $session GET '/admin/login'
    $result = Invoke-Http $session POST '/admin/login' @{ 'Input.PhoneNumber' = $login; 'Input.Password' = $customerPassword; '__RequestVerificationToken' = (Get-Token $page.Html) }
    if ($result.Status -ne 302 -or $result.Location -notmatch '/(Staff|Admin)') { throw "Staff $login could not sign in." }
    $session
}

# ---------------------------------------------------------------- business steps
function New-Claim($customer, [int]$orderItemId, [int]$defectId, [string]$description, [string]$serial = '') {
    $orderId = Invoke-Sql "SELECT OrderId FROM OrderItems WHERE Id = $orderItemId"
    $page = Invoke-Http $customer GET "/Claims/Create?orderId=$orderId&orderItemId=$orderItemId"
    $result = Invoke-Http $customer POST '/Claims/Create' ([ordered]@{
        'OrderId' = $orderId; 'ProductLocked' = 'true'; 'OrderItemId' = $orderItemId; 'DefectTypeId' = $defectId
        'Quantity' = 1; 'SerialNumber' = $serial; 'Description' = $description
        '__RequestVerificationToken' = (Get-Token $page.Html)
    }) $VideoPath
    if ($result.Location -notmatch '/Claims/Details/(\d+)') {
        $errors = [regex]::Matches($result.Html, '<li>([^<]+)</li>') | ForEach-Object { $_.Groups[1].Value }
        throw "Could not create claim for order item ${orderItemId}: $($errors -join ' | ')"
    }
    [int]$Matches[1]
}

function Set-ClaimStatus($staff, [int]$claimId, [int]$nextStatus, [string]$note = '', $extra = @{}) {
    $page = Invoke-Http $staff GET "/Staff/Claims/Details/$claimId"
    $form = @{ 'NextStatus' = $nextStatus; 'Note' = $note; '__RequestVerificationToken' = (Get-Token $page.Html) }
    foreach ($key in $extra.Keys) { $form[$key] = $extra[$key] }
    Invoke-Http $staff POST "/Staff/Claims/Decide/$claimId" $form | Out-Null
    $check = Invoke-Http $staff GET "/Staff/Claims/Details/$claimId"
    if ($check.Html -match 'alert-danger[^>]*>\s*([^<]+)<') { throw "Claim ${claimId}: $($Matches[1].Trim())" }
}

function Invoke-CustomerAction($customer, [int]$claimId, [string]$action, $form = @{}) {
    $page = Invoke-Http $customer GET "/Claims/Details/$claimId"
    $form['__RequestVerificationToken'] = Get-Token $page.Html
    Invoke-Http $customer POST "/Claims/$action/$claimId" $form | Out-Null
}

function Invoke-Sql([string]$query) {
    $value = sqlcmd -S $Server -d $Database -I -h -1 -W -Q "SET NOCOUNT ON; $query"
    ($value | Where-Object { $_ -and $_.Trim() }) | Select-Object -First 1
}

function Get-SampleItem([string]$userName, [string]$productLike, [int]$daysAgoMin, [int]$daysAgoMax) {
    $id = Invoke-Sql @"
SELECT TOP 1 oi.Id FROM OrderItems oi JOIN Orders o ON o.Id = oi.OrderId JOIN AspNetUsers u ON u.Id = o.UserId
WHERE o.Note = N'$tag' AND u.UserName = N'$userName' AND oi.ProductName LIKE N'$productLike'
  AND DATEDIFF(day, o.PaidAt, GETUTCDATE()) BETWEEN $daysAgoMin AND $daysAgoMax
  AND NOT EXISTS (SELECT 1 FROM WarrantyClaims c WHERE c.OrderItemId = oi.Id AND c.Status NOT IN (6, 7, 8))
ORDER BY oi.Id
"@
    if (-not $id) { throw "No free sample order item for $userName / $productLike (run Docs\Seed-DeliveredOrders.ps1 first)." }
    [int]$id
}

# ---------------------------------------------------------------- scenarios
if (-not (Test-Path $VideoPath)) { throw "Sample video not found: $VideoPath" }
$an = Connect-Customer '0901234501'      # Nguyễn Văn An
$ngoc = Connect-Customer '0912345602'    # Trần Thị Bích Ngọc
$staff = Connect-Staff $staffLogin
$shopAddress = 'Gửi về: Cửa hàng PôpPôp (địa chỉ demo), người nhận Trần Văn Long, SĐT 0911222301. Ghi mã yêu cầu ngoài gói hàng.'
$created = @()

# 1. Submitted – receipt defect, waiting for review
$id = New-Claim $an (Get-SampleItem '0901234501' 'Vợt Yonex%' 0 3) 6 'Mở hộp thấy khung vợt bị trầy một vệt dài khoảng 3cm ở đầu vợt, màng bọc còn nguyên. Video quay lúc mở hộp.'
$created += "Chờ duyệt (lỗi khi nhận)      #$id"

# 2. NeedMoreInfo – warranty, staff asks for a clearer video
$id = New-Claim $an (Get-SampleItem '0901234501' 'Vợt Victor%' 3 10) 1 'Đánh được khoảng 3 buổi thì phát hiện vết nứt nhỏ ở khung vợt vị trí 10 giờ, không va chạm. Căng dây 11kg.' 'VT-RYUGA2-58321'
Set-ClaimStatus $staff $id 2 'Video chưa thấy rõ vết nứt. Bạn vui lòng quay cận cảnh vị trí nứt dưới ánh sáng tốt và quay cả tem/serial trên cán vợt.'
$created += "Cần bổ sung minh chứng        #$id"

# 3. Approved + customer reported the return shipment – shop must mark "received"
$id = New-Claim $an (Get-SampleItem '0901234501' 'Giày%Victor%' 3 10) 15 'Mở hộp giày thấy mũi giày bên phải bị lem keo vàng và có vết bẩn, chưa đi lần nào.'
Set-ClaimStatus $staff $id 3 $shopAddress
Invoke-CustomerAction $an $id 'ReportShipment' @{ 'Carrier' = 'Giao Hàng Nhanh (GHN)'; 'TrackingCode' = 'GHN5K8D2Q7' }
$created += "Đã duyệt – khách đã gửi hàng  #$id"

# 4. SentToBrand – warranty, shop received it and forwarded to the distributor
$id = New-Claim $an (Get-SampleItem '0901234501' 'Vợt Li-Ning%' 15 30) 3 'Cán vợt bị lỏng và xoay nhẹ khi cầm, có tiếng kêu lạch cạch trong cán. Dùng bình thường, căng dây 10.5kg.' 'LN-AXF80-77410'
Set-ClaimStatus $staff $id 3 $shopAddress
Invoke-CustomerAction $an $id 'ReportShipment' @{ 'Carrier' = 'Trực tiếp tại cửa hàng'; 'TrackingCode' = '' }
Set-ClaimStatus $staff $id 4 'Đã nhận vợt, kiểm tra thấy cán lỏng đúng như mô tả.'
Set-ClaimStatus $staff $id 5 'Đã gửi vợt cho nhà phân phối Li-Ning để thẩm định bảo hành, dự kiến 7–10 ngày làm việc.'
$created += "Đã gửi hãng thẩm định         #$id"

# 5. Rejected – warranty claim refused with a reason
$id = New-Claim $an (Get-SampleItem '0901234501' 'Túi%Li-Ning%' 15 30) 31 'Khóa kéo ngăn chính bị tách răng, kéo không khép lại được dù dùng bình thường.'
Set-ClaimStatus $staff $id 7 'Theo video, khóa kéo bị kẹt vải lót và có vết lực kéo mạnh; đây là hư hỏng do sử dụng, không thuộc diện bảo hành của hãng.'
$created += "Từ chối                       #$id"

# 6. Completed – warranty replaced, customer confirmed receipt
$id = New-Claim $an (Get-SampleItem '0901234501' 'Giày%Li-Ning%' 15 30) 12 'Đế giày bên trái bong keo phần mũi sau khoảng 2 tuần sử dụng trên sân thảm.'
Set-ClaimStatus $staff $id 3 $shopAddress
Invoke-CustomerAction $an $id 'ReportShipment' @{ 'Carrier' = 'Viettel Post'; 'TrackingCode' = 'VTP88213450' }
Set-ClaimStatus $staff $id 4 'Đã nhận giày, xác nhận bong keo đế.'
Set-ClaimStatus $staff $id 9 'Hãng đồng ý thay đôi mới.' @{ 'Resolution' = 2; 'OutboundCarrier' = 'Viettel Post'; 'OutboundTrackingCode' = 'VTP88299017' }
Invoke-CustomerAction $an $id 'ConfirmReceived'
$created += "Hoàn tất (hãng thay mới)      #$id"

# 7. ReturningToCustomer – receipt defect exchanged from shop stock, on its way
$id = New-Claim $an (Get-SampleItem '0901234501' 'Giày%Yonex%' 0 3) 14 'Đặt size 42 nhưng hộp giao tới là size 41, tem trên hộp ghi 41. Giày chưa sử dụng.'
Set-ClaimStatus $staff $id 3 $shopAddress
Invoke-CustomerAction $an $id 'ReportShipment' @{ 'Carrier' = 'Giao Hàng Tiết Kiệm (GHTK)'; 'TrackingCode' = 'S2213.A91.77' }
Set-ClaimStatus $staff $id 4 'Đã nhận giày size 41, còn nguyên tem.'
Set-ClaimStatus $staff $id 9 'Đã gửi đổi đúng size 42.' @{ 'Resolution' = 1; 'RestockReturnedItem' = 'true'; 'OutboundCarrier' = 'Giao Hàng Tiết Kiệm (GHTK)'; 'OutboundTrackingCode' = 'S2213.B04.31' }
$created += "Đang gửi hàng thay thế        #$id"

# 8. Cancelled by the customer, then 9. a new Submitted claim for the same racket
$item = Get-SampleItem '0912345602' 'Vợt Victor%' 0 5
$id = New-Claim $ngoc $item 5 'Đặt bản 4U nhưng nhận được vợt ghi 3U trên cán, chưa đan dây.'
Invoke-CustomerAction $ngoc $id 'Cancel'
$created += "Khách đã hủy                  #$id"
$id = New-Claim $ngoc $item 4 'Đan dây 11kg tại cửa hàng gần nhà thì xuất hiện vết nứt ở lỗ xỏ dây số 2 phía trên đầu vợt.' 'VT-RYUGA2-60417'
$created += "Chờ duyệt (bảo hành)          #$id"

Write-Host 'Đã tạo các yêu cầu bảo hành mẫu:'
$created | ForEach-Object { Write-Host "  $_" }
