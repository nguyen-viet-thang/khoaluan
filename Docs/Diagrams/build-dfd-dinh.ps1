param(
    [string]$OutDrawio = (Join-Path $PSScriptRoot '..\dfd-muc-dinh.drawio'),
    [string]$OutSvg = '',
    [string]$OutMd = (Join-Path $PSScriptRoot '..\Mo-ta-tien-trinh-DFD.md')
)
# Sơ đồ luồng dữ liệu mức đỉnh (DFD mức 0) – website PôpPôp.
# Ký hiệu như mẫu của người dùng: tác nhân = hình chữ nhật, tiến trình = hình elip, kho dữ liệu = hai đường kẻ ngang
# (kho được lặp lại cạnh từng tiến trình dùng nó). 15 tiến trình = 15 nhóm chức năng trong bảng "Gom nhóm các chức năng"
# của khoaluan.docx. Bố cục được tính tay bên dưới; cuối script kiểm tra: không đường nào cắt/chạm đường khác,
# không đường nào đi xuyên qua hình, không nhãn nào đè lên nhãn/đường/hình khác.
# Mũi tên tới kho = ghi, mũi tên về tiến trình = đọc, hai đầu = đọc và ghi.
$utf8 = New-Object System.Text.UTF8Encoding($false)
function Esc([string]$s) { [System.Security.SecurityElement]::Escape($s) }

$fs = 14
$charW = 0.53 * $fs
$lineH = 18
$font = 'fontFamily=Times New Roman;'

# ---------------------------------------------------------------- nodes
$nodes = [ordered]@{}
function Actor($id, $text, $x, $y, $w, $h) { $nodes[$id] = [pscustomobject]@{ Id = $id; Kind = 'actor'; Text = $text; X = $x; Y = $y; W = $w; H = $h } }
function Proc($id, $text, $cx, $cy, $w = 200, $h = 90) { $nodes[$id] = [pscustomobject]@{ Id = $id; Kind = 'proc'; Text = $text; X = $cx - $w / 2; Y = $cy - $h / 2; W = $w; H = $h } }
function Store($id, $text, $x, $cy) { $nodes[$id] = [pscustomobject]@{ Id = $id; Kind = 'store'; Text = $text; X = $x; Y = $cy - 15; W = 160; H = 30 } }
function CX($id) { $n = $nodes[$id]; $n.X + $n.W / 2 }
function CY($id) { $n = $nodes[$id]; $n.Y + $n.H / 2 }
# point on the ellipse arc: AX = x at height y (side L/R), AY = y at abscissa x (side T/B)
function AX($id, [double]$y, [string]$side) {
    $n = $nodes[$id]; $rx = $n.W / 2; $ry = $n.H / 2; $c = $n.Y + $ry; $cx = $n.X + $rx
    $t = ($y - $c) / $ry; $t = 1 - $t * $t; if ($t -lt 0) { $t = 0 }
    $d = $rx * [math]::Sqrt($t)
    if ($side -eq 'L') { [math]::Round($cx - $d, 1) } else { [math]::Round($cx + $d, 1) }
}
function AY($id, [double]$x, [string]$side) {
    $n = $nodes[$id]; $rx = $n.W / 2; $ry = $n.H / 2; $c = $n.X + $rx; $cy = $n.Y + $ry
    $t = ($x - $c) / $rx; $t = 1 - $t * $t; if ($t -lt 0) { $t = 0 }
    $d = $ry * [math]::Sqrt($t)
    if ($side -eq 'T') { [math]::Round($cy - $d, 1) } else { [math]::Round($cy + $d, 1) }
}

# tác nhân
Actor 'VL'  'Khách vãng lai' 240 30 190 80
Actor 'KH'  'Khách hàng'     610 520 150 890
Actor 'NV'  'Nhân viên'      960 520 150 890
Actor 'QTV' 'Quản trị viên' 1660 520 150 890

# tiến trình (tên = nhóm chức năng trong khoaluan.docx)
Proc 'P1'  '1. Quản lý tài khoản'            1385 110 220 110
Proc 'P2'  '2. Tra cứu sản phẩm'              335 330
Proc 'P3'  '3. Giỏ hàng'                      335 620
Proc 'P4'  '4. Quản lý đơn hàng'              335 860
Proc 'P6'  '6. Bảo hành và đổi hàng lỗi'      335 1100
Proc 'P5'  '5. Xử lý đơn hàng'               1385 760
Proc 'P7'  '7. Xử lý bảo hành'               1385 1180
Proc 'P14' '14. Thông báo'                   1385 1530 220 110
Proc 'P8'  '8. Cấu hình chính sách bảo hành' 2085 580
Proc 'P9'  '9. Quản lý sản phẩm'             2085 710
Proc 'P10' '10. Quản lý danh mục và thương hiệu' 2085 840
Proc 'P11' '11. Quản lý kho'                 2085 970
Proc 'P12' '12. Quản lý khách hàng'          2085 1100
Proc 'P13' '13. Quản lý nhân viên'           2085 1230
Proc 'P15' '15. Báo cáo thống kê'            2085 1360

# ---------------------------------------------------------------- flows
$flows = New-Object System.Collections.ArrayList
function Flow($src, $dst, $pts, $label, $labelAt = 'H', [string]$arrows = 'end', $group = $null) {
    $clean = @()
    foreach ($p in $pts) {
        $p = @([math]::Round([double]$p[0], 1), [math]::Round([double]$p[1], 1))
        if ($clean.Count -and $clean[-1][0] -eq $p[0] -and $clean[-1][1] -eq $p[1]) { continue }
        $clean += , $p
    }
    if (-not $group) { $group = "F$($flows.Count)" }
    [void]$flows.Add([pscustomobject]@{ Src = $src; Dst = $dst; Pts = $clean; Label = $label; LabelAt = $labelAt; Arrows = $arrows; Group = $group })
}
# actor <-> process side by side: two horizontal flows at cy-22 (actor -> process) and cy+22 (process -> actor)
function SideFlows($actor, $proc, $inLabel, $outLabel) {
    $a = $nodes[$actor]; $p = $nodes[$proc]; $cy = CY $proc
    if ((CX $actor) -lt (CX $proc)) { $ax = $a.X + $a.W; $side = 'L' } else { $ax = $a.X; $side = 'R' }
    Flow $actor $proc @(@($ax, ($cy - 22)), @((AX $proc ($cy - 22) $side), ($cy - 22))) $inLabel
    Flow $proc $actor @(@((AX $proc ($cy + 22) $side), ($cy + 22)), @($ax, ($cy + 22))) $outLabel
}
# process -> its own copies of the data stores, fanned out through a vertical bus
function StoreLinks($proc, [string]$side, [double]$busX, $stores) {
    $cy = CY $proc
    foreach ($s in $stores) {
        $st = $nodes[$s[0]]; $sy = CY $s[0]
        if ($side -eq 'L') { $start = @((AX $proc $cy 'L'), $cy); $edge = $st.X + $st.W }
        elseif ($side -eq 'R') { $start = @((AX $proc $cy 'R'), $cy); $edge = $st.X }
        $pts = @($start, @($busX, $cy), @($busX, $sy), @($edge, $sy))
        $arr = @{ r = 'start'; w = 'end'; rw = 'both' }[$s[1]]
        Flow $proc $s[0] $pts '' 'none' $arr "S$proc"
    }
}
# stores above (T) / below (B) a process, bus leaves the arc at x = busX and enters the stores' right edge
function StoreLinksV($proc, [string]$side, [double]$busX, $stores) {
    foreach ($s in $stores) {
        $st = $nodes[$s[0]]; $sy = CY $s[0]
        $pts = @(@($busX, (AY $proc $busX $side)), @($busX, $sy), @(($st.X + $st.W), $sy))
        $arr = @{ r = 'start'; w = 'end'; rw = 'both' }[$s[1]]
        Flow $proc $s[0] $pts '' 'none' $arr "S$proc"
    }
}

# ---- 1. Quản lý tài khoản (trên cùng, nối cả bốn tác nhân)
Flow 'VL' 'P1' @(@(430, 60), @((AX 'P1' 60 'L'), 60)) 'Thông tin đăng ký, xác nhận email'
Flow 'P1' 'VL' @(@((AX 'P1' 88 'L'), 88), @(430, 88)) 'Kết quả đăng ký, email xác nhận'
Flow 'KH' 'P1' @(@(745, 520), @(745, 150), @((AX 'P1' 150 'L'), 150)) 'Đăng nhập, hồ sơ, mật khẩu'
Flow 'P1' 'KH' @(@((AX 'P1' 125 'L'), 125), @(720, 125), @(720, 520)) 'Kết quả, email đặt lại mật khẩu'
Flow 'NV' 'P1' @(@(1050, 520), @(1050, 200), @(1330, 200), @(1330, (AY 'P1' 1330 'B'))) 'Đăng nhập, hồ sơ, mật khẩu'
Flow 'P1' 'NV' @(@(1360, (AY 'P1' 1360 'B')), @(1360, 225), @(1080, 225), @(1080, 520)) 'Kết quả, email đặt lại mật khẩu'
Flow 'QTV' 'P1' @(@(1725, 520), @(1725, 115), @((AX 'P1' 115 'R'), 115)) 'Đăng nhập trang quản lý'
Flow 'P1' 'QTV' @(@((AX 'P1' 140 'R'), 140), @(1700, 140), @(1700, 520)) 'Kết quả đăng nhập'
Store 'S1a' 'Tài khoản' 1520 60
Flow 'P1' 'S1a' @(@((AX 'P1' 60 'R'), 60), @(1520, 60)) '' 'none' 'both' 'SP1'

# ---- 2. Tra cứu sản phẩm (khách vãng lai ở trên, khách hàng ở dưới bên phải)
Flow 'VL' 'P2' @(@(310, 110), @(310, (AY 'P2' 310 'T'))) 'Từ khóa, bộ lọc' 'VL'
Flow 'P2' 'VL' @(@(360, (AY 'P2' 360 'T')), @(360, 110)) 'Danh sách, chi tiết sản phẩm' 'VR'
Flow 'KH' 'P2' @(@(640, 520), @(640, 352), @((AX 'P2' 352 'R'), 352)) 'Từ khóa, bộ lọc'
Flow 'P2' 'KH' @(@((AX 'P2' 308 'R'), 308), @(680, 308), @(680, 520)) 'Danh sách, chi tiết sản phẩm'
Store 'S2a' 'Sản phẩm, tồn kho' 20 292
Store 'S2b' 'Danh mục' 20 330
Store 'S2c' 'Thương hiệu' 20 368
StoreLinks 'P2' 'L' 207 @(, @('S2a', 'r')) ; StoreLinks 'P2' 'L' 207 @(, @('S2b', 'r')) ; StoreLinks 'P2' 'L' 207 @(, @('S2c', 'r'))

# ---- 3, 4, 6: tiến trình của khách hàng, bên trái
SideFlows 'KH' 'P3' 'Sản phẩm, số lượng' 'Giỏ hàng, tạm tính'
Store 'S3a' 'Giỏ hàng' 20 601
Store 'S3b' 'Sản phẩm, tồn kho' 20 639
StoreLinks 'P3' 'L' 207 @(@('S3a', 'rw'), @('S3b', 'r'))

SideFlows 'KH' 'P4' 'Thông tin giao hàng, yêu cầu hủy đơn' 'Chi tiết, trạng thái đơn hàng'
Store 'S4a' 'Giỏ hàng' 20 803
Store 'S4b' 'Đơn hàng' 20 841
Store 'S4c' 'Sản phẩm, tồn kho' 20 879
Store 'S4d' 'Thông báo' 20 917
StoreLinks 'P4' 'L' 207 @(@('S4a', 'rw'), @('S4b', 'rw'), @('S4c', 'rw'), @('S4d', 'w'))

SideFlows 'KH' 'P6' 'Yêu cầu, video, thông tin gửi hàng' 'Tiến trình yêu cầu bảo hành'
Store 'S6a' 'Yêu cầu bảo hành' 20 1043
Store 'S6b' 'Đơn hàng' 20 1081
Store 'S6c' 'Chính sách bảo hành' 20 1119
Store 'S6d' 'Thông báo' 20 1157
StoreLinks 'P6' 'L' 207 @(@('S6a', 'rw'), @('S6b', 'r'), @('S6c', 'r'), @('S6d', 'w'))

# ---- 5, 7: giữa nhân viên và quản trị viên
SideFlows 'NV'  'P5' 'Trạng thái mới, lý do hủy' 'Danh sách, chi tiết đơn hàng'
SideFlows 'QTV' 'P5' 'Trạng thái mới, lý do hủy' 'Danh sách, chi tiết đơn hàng'
Store 'S5a' 'Đơn hàng' 1270 610
Store 'S5b' 'Sản phẩm, tồn kho' 1270 648
Store 'S5c' 'Thông báo' 1270 686
StoreLinksV 'P5' 'T' 1450 @(@('S5a', 'rw'), @('S5b', 'rw'), @('S5c', 'w'))

SideFlows 'NV'  'P7' 'Quyết định xử lý, lời nhắn' 'Yêu cầu bảo hành, minh chứng'
SideFlows 'QTV' 'P7' 'Quyết định xử lý, lời nhắn' 'Yêu cầu bảo hành, minh chứng'
Store 'S7a' 'Yêu cầu bảo hành' 1270 1260
Store 'S7b' 'Sản phẩm, tồn kho' 1270 1298
Store 'S7c' 'Thông báo' 1270 1336
StoreLinksV 'P7' 'B' 1450 @(@('S7a', 'rw'), @('S7b', 'rw'), @('S7c', 'w'))

# ---- 14. Thông báo (dưới cùng, nối khách hàng, nhân viên, quản trị viên)
Flow 'KH' 'P14' @(@(745, 1410), @(745, 1515), @((AX 'P14' 1515 'L'), 1515)) 'Đánh dấu đã đọc'
Flow 'P14' 'KH' @(@((AX 'P14' 1545 'L'), 1545), @(720, 1545), @(720, 1410)) 'Thông báo đơn hàng, bảo hành'
Flow 'NV' 'P14' @(@(1050, 1410), @(1050, 1455), @(1330, 1455), @(1330, (AY 'P14' 1330 'T'))) 'Đánh dấu đã đọc'
Flow 'P14' 'NV' @(@(1360, (AY 'P14' 1360 'T')), @(1360, 1430), @(1080, 1430), @(1080, 1410)) 'Thông báo đơn hàng, bảo hành'
Flow 'QTV' 'P14' @(@(1725, 1410), @(1725, 1545), @((AX 'P14' 1545 'R'), 1545)) 'Đánh dấu đã đọc'
Flow 'P14' 'QTV' @(@((AX 'P14' 1515 'R'), 1515), @(1700, 1515), @(1700, 1410)) 'Thông báo đơn hàng, bảo hành'
Store 'S14a' 'Thông báo' 1305 1640
Flow 'P14' 'S14a' @(@(1385, 1585), @(1385, 1625)) '' 'none' 'both' 'SP14'

# ---- 8 – 15: tiến trình của quản trị viên, bên phải
SideFlows 'QTV' 'P8'  'Thời hạn bảo hành, loại lỗi' 'Chính sách bảo hành'
SideFlows 'QTV' 'P9'  'Sản phẩm, phiên bản, ảnh, thông số' 'Danh sách sản phẩm'
SideFlows 'QTV' 'P10' 'Thông tin danh mục, thương hiệu' 'Danh mục, thương hiệu'
SideFlows 'QTV' 'P11' 'Số lượng nhập kho, ghi chú' 'Tồn kho, lịch sử nhập xuất'
SideFlows 'QTV' 'P12' 'Khóa, xóa, cấp mật khẩu tạm' 'Thông tin, lịch sử mua hàng'
SideFlows 'QTV' 'P13' 'Thông tin tài khoản nhân viên' 'Danh sách nhân viên'
SideFlows 'QTV' 'P15' 'Khoảng thời gian thống kê' 'Báo cáo doanh thu, tệp Excel'
$sx = 2245; $bus = 2215
Store 'S8a'  'Chính sách bảo hành' $sx 580
StoreLinks 'P8' 'R' $bus @(, @('S8a', 'rw'))
Store 'S9a'  'Sản phẩm, tồn kho' $sx 672
Store 'S9b'  'Danh mục' $sx 710
Store 'S9c'  'Thương hiệu' $sx 748
StoreLinks 'P9' 'R' $bus @(@('S9a', 'rw'), @('S9b', 'r'), @('S9c', 'r'))
Store 'S10a' 'Danh mục' $sx 821
Store 'S10b' 'Thương hiệu' $sx 859
StoreLinks 'P10' 'R' $bus @(@('S10a', 'rw'), @('S10b', 'rw'))
Store 'S11a' 'Sản phẩm, tồn kho' $sx 970
StoreLinks 'P11' 'R' $bus @(, @('S11a', 'rw'))
Store 'S12a' 'Tài khoản' $sx 1081
Store 'S12b' 'Đơn hàng' $sx 1119
StoreLinks 'P12' 'R' $bus @(@('S12a', 'rw'), @('S12b', 'r'))
Store 'S13a' 'Tài khoản' $sx 1230
StoreLinks 'P13' 'R' $bus @(, @('S13a', 'rw'))
Store 'S15a' 'Đơn hàng' $sx 1360
StoreLinks 'P15' 'R' $bus @(, @('S15a', 'r'))

# ---------------------------------------------------------------- bảng mô tả tiến trình (sinh từ chính dữ liệu sơ đồ)
$info = [ordered]@{
    P1  = @('CN01 – CN09', 'Ghi nhận đăng ký tài khoản khách hàng bằng email và gửi liên kết xác nhận; xác thực đăng nhập (khách hàng ở trang bán hàng, nhân viên và quản trị viên ở trang quản lý); cập nhật thông tin cá nhân, đổi mật khẩu, gửi email đặt lại mật khẩu.')
    P2  = @('CN10 – CN15', 'Đọc các sản phẩm đang bán (sản phẩm, danh mục và thương hiệu đều đang hoạt động), tìm kiếm theo từ khóa, lọc theo danh mục, thương hiệu, giá, thông số kỹ thuật và sắp xếp; trả về danh sách và chi tiết sản phẩm.')
    P3  = @('CN16 – CN19', 'Thêm sản phẩm vào giỏ, cập nhật số lượng, xóa khỏi giỏ; kiểm tra số lượng không vượt tồn kho và tính tạm tính.')
    P4  = @('CN20 – CN23', 'Tạo đơn hàng thanh toán khi nhận hàng (COD) từ giỏ hàng: kiểm tra tồn kho, tính phí vận chuyển, trừ tồn kho, làm trống giỏ và báo cho nhân viên; cho khách xem lịch sử, chi tiết đơn và hủy đơn khi đơn còn chờ xác nhận (hoàn lại tồn kho).')
    P5  = @('CN24 – CN28', 'Cho nhân viên, quản trị viên xem, tìm kiếm, lọc đơn hàng; cập nhật trạng thái đơn; hủy đơn kèm lý do trước khi giao (hoàn lại tồn kho); ghi thông báo cho khách hàng.')
    P6  = @('CN29 – CN34', 'Khách chọn sản phẩm trong đơn đã giao, loại lỗi và gửi video minh chứng; hệ thống kiểm tra thời hạn theo chính sách bảo hành rồi tạo yêu cầu; khách bổ sung bằng chứng, khai báo thông tin gửi hàng, hủy yêu cầu, xác nhận đã nhận sản phẩm; mỗi thay đổi đều báo cho nhân viên.')
    P7  = @('CN35 – CN39', 'Nhân viên, quản trị viên xem yêu cầu và minh chứng; yêu cầu bổ sung, duyệt, từ chối, xác nhận đã nhận hàng, gửi hãng, gửi sản phẩm thay thế (trừ tồn kho), hoàn tất; ghi chú nội bộ, gửi lời nhắn; ghi thông báo cho khách hàng.')
    P8  = @('CN40 – CN42', 'Quản trị viên cập nhật thời hạn đổi hàng lỗi theo thương hiệu, thời hạn bảo hành theo thương hiệu và danh mục, danh sách loại lỗi.')
    P9  = @('CN43 – CN49', 'Quản trị viên xem, thêm, sửa, ẩn/hiện, xóa sản phẩm; quản lý phiên bản, hình ảnh và gán thông số kỹ thuật.')
    P10 = @('CN50 – CN51', 'Quản trị viên xem, thêm, sửa, xóa danh mục và thương hiệu.')
    P11 = @('CN52 – CN53', 'Quản trị viên nhập kho cho phiên bản sản phẩm (cộng tồn kho, ghi lịch sử) và xem lịch sử nhập xuất kho.')
    P12 = @('CN54 – CN58', 'Quản trị viên xem danh sách, chi tiết và lịch sử mua hàng của khách; khóa/mở khóa tài khoản; cấp mật khẩu tạm; xóa tài khoản chưa có đơn hàng và yêu cầu bảo hành.')
    P13 = @('CN59 – CN63', 'Quản trị viên xem, thêm nhân viên (Gmail và số điện thoại, mật khẩu ban đầu là số điện thoại), sửa thông tin, khóa/mở khóa, đặt lại mật khẩu.')
    P14 = @('CN64 – CN66', 'Đọc thông báo do các tiến trình 4, 5, 6, 7 ghi ra, đẩy thông báo mới tới người dùng, hiển thị danh sách và đánh dấu đã đọc.')
    P15 = @('CN67 – CN69', 'Quản trị viên xem tổng quan và báo cáo doanh thu từ các đơn hàng đã hoàn tất theo khoảng thời gian; xuất báo cáo ra tệp Excel.')
}
if ($OutMd) {
    $order = 'P1', 'P2', 'P3', 'P4', 'P5', 'P6', 'P7', 'P8', 'P9', 'P10', 'P11', 'P12', 'P13', 'P14', 'P15'
    $actorOrder = 'VL', 'KH', 'NV', 'QTV'
    function JoinFlows($list, $actorField) {
        # gộp các tác nhân có cùng nội dung: "Nhân viên, Quản trị viên: ..."
        $byLabel = [ordered]@{}
        foreach ($f in ($list | Sort-Object { [array]::IndexOf($actorOrder, $_.$actorField) })) {
            if (-not $byLabel.Contains($f.Label)) { $byLabel[$f.Label] = @() }
            $byLabel[$f.Label] += $nodes[$f.$actorField].Text
        }
        ($byLabel.Keys | ForEach-Object { "$($byLabel[$_] -join ', '): $_" }) -join '<br>'
    }
    $md = New-Object System.Text.StringBuilder
    [void]$md.AppendLine('# Mô tả các tiến trình – Sơ đồ luồng dữ liệu mức đỉnh')
    [void]$md.AppendLine()
    [void]$md.AppendLine('Bảng được sinh tự động từ `Docs/Diagrams/build-dfd-dinh.ps1` (cùng dữ liệu với `Docs/dfd-muc-dinh.drawio`), nên tên tiến trình, luồng dữ liệu và kho dữ liệu luôn khớp với sơ đồ. Tên tiến trình là tên nhóm chức năng trong bảng "Gom nhóm các chức năng" của khóa luận.')
    [void]$md.AppendLine()
    [void]$md.AppendLine('| STT | Tiến trình | Chức năng đáp ứng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu | Mô tả |')
    [void]$md.AppendLine('|:---|:---|:---|:---|:---|:---|:---|:---|')
    $i = 0
    foreach ($p in $order) {
        $i++
        $ins = @($flows | Where-Object { $_.Dst -eq $p -and $nodes[$_.Src].Kind -eq 'actor' })
        $outs = @($flows | Where-Object { $_.Src -eq $p -and $nodes[$_.Dst].Kind -eq 'actor' })
        $actors = ($actorOrder | Where-Object { $a = $_; ($ins + $outs) | Where-Object { $_.Src -eq $a -or $_.Dst -eq $a } } | ForEach-Object { $nodes[$_].Text }) -join ', '
        $stores = [ordered]@{}
        foreach ($f in ($flows | Where-Object { $_.Src -eq $p -and $nodes[$_.Dst].Kind -eq 'store' })) {
            $stores[$nodes[$f.Dst].Text] = @{ start = 'đọc'; end = 'ghi'; both = 'đọc, ghi' }[$f.Arrows]
        }
        $storeText = ($stores.Keys | ForEach-Object { "$_ ($($stores[$_]))" }) -join '<br>'
        $name = $nodes[$p].Text -replace '^\d+\.\s*', ''
        [void]$md.AppendLine("| $i | $name | $($info[$p][0]) | $actors | $(JoinFlows $ins 'Src') | $(JoinFlows $outs 'Dst') | $storeText | $($info[$p][1]) |")
    }
    [void]$md.AppendLine()
    [void]$md.AppendLine('Ghi chú: "đọc" – tiến trình lấy dữ liệu từ kho; "ghi" – tiến trình thêm/cập nhật dữ liệu vào kho. Kho "Sản phẩm, tồn kho" gồm sản phẩm, phiên bản, số lượng tồn và lịch sử nhập xuất kho; kho "Chính sách bảo hành" gồm thời hạn bảo hành, thời hạn đổi hàng lỗi và danh sách loại lỗi.')
    [IO.File]::WriteAllText($OutMd, $md.ToString(), (New-Object System.Text.UTF8Encoding($true)))
    Write-Output "table: $OutMd"
}

# ---------------------------------------------------------------- labels
function TextW([string]$t) { [math]::Ceiling($t.Length * $charW) + 6 }
function SplitTwo([string]$t) {
    $w = $t.Split(' '); $best = $null; $bestMax = 1e9
    for ($i = 1; $i -lt $w.Count; $i++) {
        $a = ($w[0..($i - 1)] -join ' '); $b = ($w[$i..($w.Count - 1)] -join ' ')
        $m = [math]::Max($a.Length, $b.Length); if ($m -lt $bestMax) { $bestMax = $m; $best = @($a, $b) }
    }
    $best
}
$labels = @(); $problems = @()
foreach ($f in $flows) {
    if (-not $f.Label) { continue }
    $p = $f.Pts; $best = $null; $bestLen = -1
    $wantH = $f.LabelAt -eq 'H'
    for ($i = 0; $i -lt $p.Count - 1; $i++) {
        $horiz = $p[$i][1] -eq $p[$i + 1][1]
        if ($horiz -ne $wantH) { continue }
        $len = [math]::Abs($p[$i][0] - $p[$i + 1][0]) + [math]::Abs($p[$i][1] - $p[$i + 1][1])
        if ($len -gt $bestLen) { $bestLen = $len; $best = $i }
    }
    $a = $p[$best]; $b = $p[$best + 1]
    $lines = @($f.Label); $w = TextW $f.Label
    if ($wantH -and $w -gt $bestLen - 24) { $lines = SplitTwo $f.Label; $w = [math]::Max((TextW $lines[0]), (TextW $lines[1])) }
    if ($wantH -and $w -gt $bestLen - 12) { $problems += "label too long for its segment: '$($f.Label)'" }
    $h = $lines.Count * $lineH
    $mx = ($a[0] + $b[0]) / 2; $my = ($a[1] + $b[1]) / 2
    switch ($f.LabelAt) {
        'H'  { $x = $mx - $w / 2 }
        'VL' { $x = $a[0] - 6 - $w }
        'VR' { $x = $a[0] + 6 }
    }
    $labels += [pscustomobject]@{ Flow = $f; Lines = $lines; X = [math]::Round($x, 1); Y = [math]::Round($my - $h / 2, 1); W = $w; H = $h }
}

# ---------------------------------------------------------------- checks
function Overlap($ax1, $ay1, $ax2, $ay2, $bx1, $by1, $bx2, $by2) { $ax1 -le $bx2 -and $bx1 -le $ax2 -and $ay1 -le $by2 -and $by1 -le $ay2 }
$segs = foreach ($f in $flows) { for ($i = 0; $i -lt $f.Pts.Count - 1; $i++) {
    $a = $f.Pts[$i]; $b = $f.Pts[$i + 1]
    [pscustomobject]@{ F = $f; X1 = [math]::Min($a[0], $b[0]); X2 = [math]::Max($a[0], $b[0]); Y1 = [math]::Min($a[1], $b[1]); Y2 = [math]::Max($a[1], $b[1]) }
} }
$segs = @($segs)
function FName($f) { "$($f.Src)->$($f.Dst) '$($f.Label)'" }
for ($i = 0; $i -lt $segs.Count; $i++) { for ($j = $i + 1; $j -lt $segs.Count; $j++) {
    $a = $segs[$i]; $b = $segs[$j]; if ($a.F.Group -eq $b.F.Group) { continue }
    if (Overlap ($a.X1 - 3) ($a.Y1 - 3) ($a.X2 + 3) ($a.Y2 + 3) $b.X1 $b.Y1 $b.X2 $b.Y2) { $problems += "$(FName $a.F) touches $(FName $b.F)" }
} }
foreach ($s in $segs) { foreach ($n in $nodes.Values) {
    if ($n.Id -eq $s.F.Src -or $n.Id -eq $s.F.Dst) { continue }
    if (Overlap $s.X1 $s.Y1 $s.X2 $s.Y2 ($n.X - 4) ($n.Y - 4) ($n.X + $n.W + 4) ($n.Y + $n.H + 4)) { $problems += "$(FName $s.F) runs through $($n.Id)" }
} }
for ($i = 0; $i -lt $labels.Count; $i++) {
    $l = $labels[$i]
    for ($j = $i + 1; $j -lt $labels.Count; $j++) {
        $m = $labels[$j]
        if (Overlap $l.X $l.Y ($l.X + $l.W) ($l.Y + $l.H) ($m.X - 4) ($m.Y - 2) ($m.X + $m.W + 4) ($m.Y + $m.H + 2)) { $problems += "labels overlap: '$($l.Flow.Label)' / '$($m.Flow.Label)'" }
    }
    foreach ($s in $segs) { if ($s.F -eq $l.Flow) { continue }
        if (Overlap $l.X $l.Y ($l.X + $l.W) ($l.Y + $l.H) $s.X1 $s.Y1 $s.X2 $s.Y2) { $problems += "label '$($l.Flow.Label)' covers $(FName $s.F)" } }
    foreach ($n in $nodes.Values) {
        if (Overlap $l.X $l.Y ($l.X + $l.W) ($l.Y + $l.H) $n.X $n.Y ($n.X + $n.W) ($n.Y + $n.H)) { $problems += "label '$($l.Flow.Label)' covers $($n.Id)" } }
}
if ($problems) { $problems | Select-Object -Unique | ForEach-Object { Write-Output "PROBLEM: $_" } } else { Write-Output 'check passed: no crossings, nothing runs through a shape, no overlapping labels' }

# ---------------------------------------------------------------- draw.io
$W = [math]::Ceiling((($nodes.Values | ForEach-Object { $_.X + $_.W }) | Measure-Object -Maximum).Maximum + 20)
$H = [math]::Ceiling((($nodes.Values | ForEach-Object { $_.Y + $_.H }) | Measure-Object -Maximum).Maximum + 20)
$style = @{
    actor = "rounded=0;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#000000;strokeWidth=2;fontSize=$fs;fontStyle=1;$font"
    proc  = "ellipse;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#000000;strokeWidth=2;fontSize=$fs;spacing=6;$font"
    store = "shape=partialRectangle;whiteSpace=wrap;html=1;top=1;bottom=1;left=0;right=0;fillColor=#ffffff;strokeColor=#000000;strokeWidth=2;fontSize=$fs;$font"
}
$lab = "text;html=1;align=center;verticalAlign=middle;whiteSpace=wrap;fillColor=#ffffff;strokeColor=none;spacing=0;fontSize=$fs;$font"
$cells = New-Object System.Text.StringBuilder
function Add-Cell([string]$xml) { [void]$cells.AppendLine("        $xml") }
foreach ($n in $nodes.Values) {
    Add-Cell "<mxCell id=""$($n.Id)"" value=""$(Esc $n.Text)"" style=""$($style[$n.Kind])"" vertex=""1"" parent=""1""><mxGeometry x=""$($n.X)"" y=""$($n.Y)"" width=""$($n.W)"" height=""$($n.H)"" as=""geometry"" /></mxCell>"
}
$k = 0
foreach ($f in $flows) {
    $k++; $p = $f.Pts
    $arr = switch ($f.Arrows) { 'end' { 'startArrow=none;endArrow=classic;endFill=1;' } 'start' { 'startArrow=classic;startFill=1;endArrow=none;' } 'both' { 'startArrow=classic;startFill=1;endArrow=classic;endFill=1;' } }
    $es = "edgeStyle=none;rounded=0;html=1;${arr}endSize=8;startSize=8;strokeColor=#000000;strokeWidth=1.5;"
    $mid = ''
    if ($p.Count -gt 2) { $mid = '<Array as="points">' + (($p[1..($p.Count - 2)] | ForEach-Object { "<mxPoint x=""$($_[0])"" y=""$($_[1])"" />" }) -join '') + '</Array>' }
    Add-Cell "<mxCell id=""f$k"" style=""$es"" edge=""1"" parent=""1""><mxGeometry relative=""1"" as=""geometry""><mxPoint x=""$($p[0][0])"" y=""$($p[0][1])"" as=""sourcePoint"" /><mxPoint x=""$($p[-1][0])"" y=""$($p[-1][1])"" as=""targetPoint"" />$mid</mxGeometry></mxCell>"
}
$k = 0
foreach ($l in $labels) {
    $k++
    Add-Cell "<mxCell id=""l$k"" value=""$(Esc ($l.Lines -join "`n"))"" style=""$lab"" vertex=""1"" parent=""1""><mxGeometry x=""$($l.X)"" y=""$($l.Y)"" width=""$($l.W)"" height=""$($l.H)"" as=""geometry"" /></mxCell>"
}
$xml = @"
<mxfile host="app.diagrams.net" type="device">
  <diagram id="dfd1" name="DFD mức đỉnh">
    <mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="$W" pageHeight="$H" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />
$($cells.ToString().TrimEnd())
      </root>
    </mxGraphModel>
  </diagram>
</mxfile>
"@
[IO.File]::WriteAllText($OutDrawio, $xml, $utf8)
[xml]$null = [IO.File]::ReadAllText($OutDrawio, $utf8)
Write-Output ("{0}: {1} tiến trình, {2} kho, {3} luồng, {4}x{5}" -f (Split-Path $OutDrawio -Leaf), @($nodes.Values | Where-Object Kind -eq 'proc').Count, @($nodes.Values | Where-Object Kind -eq 'store').Count, $flows.Count, $W, $H)

# ---------------------------------------------------------------- optional SVG preview (same geometry)
if ($OutSvg) {
    function WrapText([string]$t, [double]$maxW) {
        $out = @(); $cur = ''
        foreach ($w in $t.Split(' ')) { $try = ($cur + ' ' + $w).Trim(); if ($cur -and ($try.Length * $charW) -gt $maxW) { $out += $cur; $cur = $w } else { $cur = $try } }
        $out + $cur
    }
    function SvgText($cx, $cy, $lines, $bold = $false) {
        $y0 = $cy - ($lines.Count - 1) * $lineH / 2 + 5; $i = 0
        $fw = if ($bold) { ' font-weight="bold"' } else { '' }
        ($lines | ForEach-Object { "<text x=""$cx"" y=""$($y0 + $lineH * $i++)"" text-anchor=""middle""$fw>$(Esc $_)</text>" }) -join ''
    }
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.Append("<svg xmlns=""http://www.w3.org/2000/svg"" width=""$W"" height=""$H"" viewBox=""0 0 $W $H"" font-family=""Times New Roman"" font-size=""$fs""><rect width=""100%"" height=""100%"" fill=""#fff""/>")
    [void]$sb.Append('<defs><marker id="a" viewBox="0 0 10 10" refX="10" refY="5" markerWidth="8" markerHeight="8" orient="auto-start-reverse"><path d="M0,0 L10,5 L0,10 z"/></marker></defs>')
    foreach ($f in $flows) {
        $pts = ($f.Pts | ForEach-Object { "$($_[0]),$($_[1])" }) -join ' '
        $m = switch ($f.Arrows) { 'end' { ' marker-end="url(#a)"' } 'start' { ' marker-start="url(#a)"' } 'both' { ' marker-start="url(#a)" marker-end="url(#a)"' } }
        [void]$sb.Append("<polyline points=""$pts"" fill=""none"" stroke=""#000"" stroke-width=""1.5""$m/>")
    }
    foreach ($n in $nodes.Values) {
        $cx = $n.X + $n.W / 2; $cy = $n.Y + $n.H / 2
        switch ($n.Kind) {
            'actor' { [void]$sb.Append("<rect x=""$($n.X)"" y=""$($n.Y)"" width=""$($n.W)"" height=""$($n.H)"" fill=""#fff"" stroke=""#000"" stroke-width=""2""/>" + (SvgText $cx $cy (WrapText $n.Text ($n.W - 16)) $true)) }
            'proc'  { [void]$sb.Append("<ellipse cx=""$cx"" cy=""$cy"" rx=""$($n.W/2)"" ry=""$($n.H/2)"" fill=""#fff"" stroke=""#000"" stroke-width=""2""/>" + (SvgText $cx $cy (WrapText $n.Text ($n.W - 40)))) }
            'store' { [void]$sb.Append("<rect x=""$($n.X)"" y=""$($n.Y)"" width=""$($n.W)"" height=""$($n.H)"" fill=""#fff""/><line x1=""$($n.X)"" y1=""$($n.Y)"" x2=""$($n.X+$n.W)"" y2=""$($n.Y)"" stroke=""#000"" stroke-width=""2""/><line x1=""$($n.X)"" y1=""$($n.Y+$n.H)"" x2=""$($n.X+$n.W)"" y2=""$($n.Y+$n.H)"" stroke=""#000"" stroke-width=""2""/>" + (SvgText $cx $cy @($n.Text))) }
        }
    }
    foreach ($l in $labels) { [void]$sb.Append("<rect x=""$($l.X)"" y=""$($l.Y)"" width=""$($l.W)"" height=""$($l.H)"" fill=""#fff""/>" + (SvgText ($l.X + $l.W / 2) ($l.Y + $l.H / 2) $l.Lines)) }
    [void]$sb.Append('</svg>')
    [IO.File]::WriteAllText($OutSvg, $sb.ToString(), $utf8)
    Write-Output "preview: $OutSvg"
}
