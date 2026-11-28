# Sinh sơ đồ kiến trúc 3 tầng của PopPop (Docs/kien-truc-he-thong.drawio + .png).
# Bố cục theo ảnh mẫu: mỗi tầng là một khung có thanh tiêu đề, bên trong là các thẻ thành phần; giữa các tầng là mũi tên hai chiều có nhãn giao thức.
param(
    [string]$OutDrawio = (Join-Path $PSScriptRoot '..\kien-truc-he-thong.drawio'),
    [string]$OutPng = (Join-Path $PSScriptRoot '..\kien-truc-he-thong.png')
)
$ErrorActionPreference = 'Stop'
$utf8 = New-Object Text.UTF8Encoding($false)
$font = 'Arial'
$fs = 14

function Esc([string]$s) { [System.Security.SecurityElement]::Escape($s) }
$cells = New-Object Text.StringBuilder
$script:id = 10
function NewId { $script:id++; "c$($script:id)" }
function Add([string]$x) { [void]$cells.AppendLine("        $x") }

function Box($x, $y, $w, $h, $style, $html) {
    Add ("<mxCell id=""{0}"" value=""{1}"" style=""{2}"" vertex=""1"" parent=""1""><mxGeometry x=""{3}"" y=""{4}"" width=""{5}"" height=""{6}"" as=""geometry""/></mxCell>" -f (NewId), (Esc $html), $style, $x, $y, $w, $h)
}

# Khung một tầng: viền màu + thanh tiêu đề
function Tier($y, $h, $color, $title) {
    Box 20 $y 1420 $h "rounded=0;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=$color;strokeWidth=2;" ''
    Box 20 $y 1420 40 "rounded=0;whiteSpace=wrap;html=1;fillColor=#f4f7fb;strokeColor=$color;strokeWidth=2;align=left;spacingLeft=10;fontFamily=$font;fontSize=17;fontStyle=1;fontColor=$color;" $title
}

# Thẻ thành phần: tiêu đề màu + nhãn vai trò bên phải + các gạch đầu dòng "<b>ý chính:</b> giải thích"
function Card($x, $y, $w, $h, $color, $title, $tag, [string[]]$bullets) {
    $body = ($bullets | ForEach-Object {
            $parts = $_ -split '\|', 2
            if ($parts.Count -eq 2) { "&bull; <b>$($parts[0])</b> $($parts[1])" } else { "&bull; $_" }
        }) -join '<br>'
    $html = "<div style='font-size:16px;font-weight:bold;color:$color;margin-bottom:6px'>$title</div><div style='border-top:1px dashed #c8cdd6;padding-top:6px;line-height:1.35'>$body</div>"
    Box $x $y $w $h "rounded=1;arcSize=5;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=$color;strokeWidth=2;align=left;verticalAlign=top;spacingLeft=12;spacingRight=10;spacingTop=8;fontFamily=$font;fontSize=$fs;fontColor=#1f2937;" $html
    $tw = [Math]::Max(90, $tag.Length * 8 + 20)
    Box ($x + $w - $tw - 12) ($y + 10) $tw 24 "rounded=1;arcSize=20;whiteSpace=wrap;html=1;fillColor=#eef2ff;strokeColor=$color;align=center;verticalAlign=middle;fontFamily=$font;fontSize=12;fontStyle=1;fontColor=$color;" $tag
}

# Mũi tên hai chiều giữa hai tầng + nhãn giao thức
function Link($y1, $y2, $color, $label) {
    $e = NewId
    Add ("<mxCell id=""$e"" style=""endArrow=block;startArrow=block;endFill=1;startFill=1;html=1;strokeWidth=2;strokeColor=$color;"" edge=""1"" parent=""1""><mxGeometry relative=""1"" as=""geometry""><mxPoint x=""730"" y=""$y1"" as=""sourcePoint""/><mxPoint x=""730"" y=""$y2"" as=""targetPoint""/></mxGeometry></mxCell>")
    Box 745 ($y1 + 4) 690 ($y2 - $y1 - 8) "text;html=1;whiteSpace=wrap;align=left;verticalAlign=middle;fontFamily=$font;fontSize=$fs;fontStyle=1;fontColor=$color;" $label
}

$blue = '#1d4ed8'; $purple = '#5b21b6'; $teal = '#0f766e'; $green = '#15803d'; $red = '#b91c1c'; $orange = '#c2410c'

# ------------------------------------------------------------------ Tầng 1
$t1 = 20; $c1h = 205
Tier $t1 ($c1h + 75) $blue 'TẦNG GIAO DIỆN NGƯỜI DÙNG (CLIENT LAYER) — Trình duyệt web: Razor View (HTML) + Bootstrap/jQuery + SignalR client'
$cw = 448; $cy = $t1 + 55
Card 40 $cy $cw $c1h $blue 'Cửa hàng trực tuyến' 'Khách vãng lai / Khách hàng' @(
    'Xem, tìm, lọc sản phẩm:|theo danh mục, thương hiệu, khoảng giá và thông số kỹ thuật',
    'Giỏ hàng &amp; đặt hàng COD:|theo dõi trạng thái đơn, tự hủy khi đơn còn chờ xác nhận',
    'Bảo hành / đổi hàng lỗi:|gửi yêu cầu kèm video minh chứng, theo dõi tiến độ',
    'Tài khoản:|đăng ký và xác nhận qua email, quên mật khẩu, xem thông báo')
Card (40 + $cw + 18) $cy $cw $c1h $blue 'Khu vực nhân viên (/Staff)' 'Nhân viên' @(
    'Xử lý đơn hàng:|xác nhận → đang xử lý → đang giao → đã giao, hoặc hủy (hoàn kho)',
    'Xử lý yêu cầu bảo hành:|duyệt, yêu cầu bổ sung, từ chối, ghi nhận gửi trả hàng, nhắn khách',
    'Chuông thông báo thời gian thực:|đơn mới, yêu cầu bảo hành mới, khách hủy đơn')
Card (40 + 2 * ($cw + 18)) $cy $cw $c1h $blue 'Khu vực quản trị (/Admin)' 'Quản trị viên' @(
    'Danh mục hàng hóa:|danh mục, thương hiệu, sản phẩm, phiên bản, ảnh, thông số, tồn kho',
    'Tài khoản:|khóa/mở khách hàng, cấp mật khẩu tạm; tạo và quản lý nhân viên',
    'Chính sách bảo hành:|số ngày theo thương hiệu × danh mục',
    'Thống kê:|bảng điều khiển, báo cáo doanh thu, xuất Excel')
$t1end = $t1 + $c1h + 75

# ------------------------------------------------------------------ Tầng 2
$t2 = $t1end + 75; $c2h = 205
Link $t1end $t2 $blue 'HTTPS: yêu cầu GET/POST (form HTML) + cookie đăng nhập + mã chống giả mạo • WebSocket SignalR /hubs/notifications (máy chủ chủ động đẩy thông báo)'
Tier $t2 ($c2h + 75) $purple 'TẦNG ỨNG DỤNG &amp; NGHIỆP VỤ (APPLICATION LAYER) — ASP.NET Core 10 MVC, kiến trúc N-tier: Controller → Service → Repository'
$cw2 = 331; $cy = $t2 + 55
Card 40 $cy $cw2 $c2h $purple 'Tiếp nhận &amp; Phân quyền' 'Controllers' @(
    'Pipeline (Program.cs):|HTTPS → định tuyến → tệp tĩnh → xác thực → bắt đổi mật khẩu → phân quyền',
    'Bộ điều khiển:|Controllers/, Areas/Admin, Areas/Staff, Areas/Identity (Razor Pages)',
    'Phân quyền theo vai trò:|[Authorize(Roles = "Admin")], "Admin,Staff"')
Card (40 + $cw2 + 18) $cy $cw2 $c2h $purple 'Xử lý nghiệp vụ' 'Services' @(
    'Dịch vụ:|OrderService, CartService, ProductService, WarrantyClaimService, ReportService…',
    'Quy tắc:|tồn kho, phí vận chuyển, vòng đời đơn hàng, hạn bảo hành',
    'Chuyển đổi:|Entity ↔ ViewModel (thư mục Models/)')
Card (40 + 2 * ($cw2 + 18)) $cy $cw2 $c2h $purple 'Truy cập dữ liệu' 'Repositories' @(
    'Repository dùng chung:|Repository&lt;T&gt; + repository riêng từng nghiệp vụ',
    'Lọc sản phẩm:|CatalogFiltering (thông số, từ khóa, giá)',
    'UnitOfWork:|gom thao tác vào một giao dịch Serializable, lỗi thì rollback')
Card (40 + 3 * ($cw2 + 18)) $cy $cw2 $c2h $purple 'Thông báo thời gian thực' 'SignalR' @(
    'NotificationService:|lưu bảng Notifications rồi đẩy qua NotificationHub',
    'Đúng người nhận:|Clients.User(mã người dùng)',
    'An toàn giao dịch:|chỉ gửi sau khi đã commit; lỗi gửi thông báo không làm hỏng nghiệp vụ')
$t2end = $t2 + $c2h + 75

# ------------------------------------------------------------------ Tầng 3
$t3 = $t2end + 75; $c3h = 180
Link $t2end $t3 $teal 'EF Core 10 (LINQ → T-SQL qua PopPopDbContext) • Đọc/ghi tệp trên ổ đĩa máy chủ • Gửi email SMTP'
Tier $t3 ($c3h + 75) $teal 'TẦNG LƯU TRỮ &amp; DỊCH VỤ NGOẠI VI (DATA &amp; EXTERNAL SERVICES)'
$cy = $t3 + 55
Card 40 $cy $cw $c3h $green 'Hệ quản trị CSDL SQL Server' 'RDBMS' @(
    'CSDL PopPopShop:|22 thực thể (gồm bảng tài khoản, vai trò của ASP.NET Core Identity)',
    'Lược đồ:|EF Core Migrations (Data/Migrations), dữ liệu mẫu Data/Seed',
    'Toàn vẹn:|khóa ngoại, chỉ mục duy nhất (SKU, mã đơn PP-…, mã yêu cầu BH-…)')
Card (40 + $cw + 18) $cy $cw $c3h $red 'Lưu trữ tệp trên máy chủ' 'File system' @(
    'Ảnh sản phẩm:|wwwroot/uploads/products (công khai)',
    'Minh chứng bảo hành:|App_Data/claims/{mã yêu cầu} — ngoài wwwroot, chỉ tải qua action có kiểm tra quyền')
Card (40 + 2 * ($cw + 18)) $cy $cw $c3h $orange 'Dịch vụ bên ngoài' 'External' @(
    'Máy chủ SMTP:|email xác nhận tài khoản, email đặt lại mật khẩu (SmtpEmailSender)',
    'CDN jsDelivr / cdnjs:|thư viện SignalR client, Popper.js')
$t3end = $t3 + $c3h + 75

$pageW = 1460; $pageH = $t3end + 20
$xml = @"
<mxfile host="build-kien-truc.ps1">
  <diagram id="kientruc" name="Kiến trúc hệ thống">
    <mxGraphModel dx="1400" dy="1000" grid="0" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="$pageW" pageHeight="$pageH" math="0" shadow="0">
      <root>
        <mxCell id="0"/>
        <mxCell id="1" parent="0"/>
$($cells.ToString().TrimEnd())
      </root>
    </mxGraphModel>
  </diagram>
</mxfile>
"@
[IO.File]::WriteAllText($OutDrawio, $xml, $utf8)
[xml]$null = $xml
Write-Output "drawio: $OutDrawio (${pageW}x$pageH)"

if ($OutPng) {
    $edge = @("${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe", "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1
    $json = @{ xml = $xml; toolbar = ''; highlight = 'none'; nav = $false; 'auto-fit' = $false } | ConvertTo-Json -Compress
    $html = "<!doctype html><html><head><meta charset='utf-8'><style>body{margin:0;background:#fff}</style></head><body><div class='mxgraph' data-mxgraph=""$([Net.WebUtility]::HtmlEncode($json))""></div><script src='https://viewer.diagrams.net/js/viewer-static.min.js'></script></body></html>"
    $tmp = Join-Path $env:TEMP 'kientruc-view.html'
    [IO.File]::WriteAllText($tmp, $html, $utf8)
    $ErrorActionPreference = 'Continue' # Edge ghi log vô hại ra stderr
    & $edge --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=2 --virtual-time-budget=20000 "--screenshot=$OutPng" "--window-size=$pageW,$pageH" ([Uri]$tmp).AbsoluteUri 2>$null | Out-Null
    Write-Output "png: $OutPng"
}
