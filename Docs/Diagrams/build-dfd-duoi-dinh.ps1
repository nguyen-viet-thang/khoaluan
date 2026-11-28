param(
    [string]$OutDir = (Join-Path $PSScriptRoot '..\dfd-duoi-dinh'),
    [string]$OutMd = (Join-Path $PSScriptRoot '..\Mo-ta-tien-trinh-DFD-duoi-dinh.md'),
    [switch]$NoPng
)
# Sơ đồ luồng dữ liệu mức dưới đỉnh (DFD mức 1) – website PôpPôp, theo mẫu của người dùng:
#   - tiêu đề "SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH n.0: <TÊN TIẾN TRÌNH>";
#   - tác nhân = hình chữ nhật cao, tiến trình con = elip ghi "n.k" (đậm) và tên chức năng;
#   - kho dữ liệu = hai đường kẻ ngang ghi "Dn.k | Tên kho", mỗi kho chỉ vẽ MỘT lần, các tiến trình con dùng chung;
#   - mọi luồng đều có nhãn (luồng tới kho: ghi gì / đọc gì).
# Mỗi tiến trình mức đỉnh -> một file draw.io riêng (dfd-tien-trinh-NN.drawio) + ảnh PNG cùng tên.
# Tiến trình con = chức năng CNxx trong bảng "Gom nhóm các chức năng" (khoaluan.docx).
# Mũi tên tới kho = ghi, mũi tên về tiến trình = đọc, hai đầu = đọc và ghi.
# Chỗ hai đường bắt buộc phải cắt nhau được vẽ cầu vồng (jumpStyle=arc); vị trí kho và thứ tự làn được
# chọn bằng cách thử mọi hoán vị để số chỗ cắt ít nhất. Script kiểm tra: không đường nào đi xuyên hình,
# không hai đường nào chồng lên nhau, không nhãn nào đè nhãn/đường/hình khác.
$utf8 = New-Object System.Text.UTF8Encoding($false)
function Esc([string]$s) { [System.Security.SecurityElement]::Escape($s) }

$fs = 14
$charW = 0.56 * $fs
$lineH = 18
$fontName = 'Arial'
$font = "fontFamily=$fontName;"

$storeName = @{
    TK = 'Tài khoản'; SP = 'Sản phẩm, tồn kho'; DM = 'Danh mục'; TH = 'Thương hiệu'; GH = 'Giỏ hàng'
    DH = 'Đơn hàng'; TB = 'Thông báo'; YC = 'Yêu cầu bảo hành'; CS = 'Chính sách bảo hành'
}

# ---------------------------------------------------------------- dữ liệu
# Boxes: mã:phía(L/R):tên tác nhân hiển thị
# Rows : mã CN | tên chức năng | tác nhân (mã box) | dữ liệu vào | dữ liệu ra | kho:quyền:nhãn ; kho:quyền:nhãn ...
$groups = @(
    @{ No = 1; Name = 'Quản lý tài khoản'
        Boxes = 'VL:L:Khách vãng lai|KH:L:Khách hàng|NQ:R:Nhân viên / Quản trị viên|NV:R:Nhân viên'
        Rows = @(
            'CN01|Đăng ký tài khoản|VL|Họ tên, email, mật khẩu|Kết quả đăng ký, email xác nhận|TK:rw:Kiểm tra email, lưu tài khoản mới'
            'CN02|Xác nhận email|VL|Liên kết xác nhận|Kết quả xác nhận email|TK:rw:Ghi nhận email đã xác nhận'
            'CN03|Gửi lại email xác nhận|VL|Email đã đăng ký|Email xác nhận mới|TK:r:Đọc tài khoản chưa xác nhận'
            'CN04|Đăng nhập|KH|Email, mật khẩu|Kết quả đăng nhập|TK:r:Xác thực tài khoản'
            'CN05|Đăng nhập trang quản lý|NQ|Gmail hoặc số điện thoại, mật khẩu|Kết quả đăng nhập|TK:r:Xác thực tài khoản, vai trò'
            'CN06|Đăng xuất|KH,NQ|Yêu cầu đăng xuất|Kết quả đăng xuất|'
            'CN07|Cập nhật thông tin cá nhân|KH,NQ|Họ tên, số điện thoại|Thông tin cá nhân mới|TK:rw:Cập nhật thông tin cá nhân'
            'CN08|Đổi mật khẩu|KH,NQ|Mật khẩu cũ, mật khẩu mới|Kết quả đổi mật khẩu|TK:rw:Cập nhật mật khẩu'
            'CN09|Xem quyền hạn nhân viên|NV|Yêu cầu xem quyền hạn|Danh sách quyền hạn|TK:r:Đọc vai trò tài khoản'
        ) }
    @{ No = 2; Name = 'Tra cứu sản phẩm'
        Boxes = 'KV:L:Khách vãng lai / Khách hàng'
        Rows = @(
            'CN10|Xem trang chủ|KV|Yêu cầu xem trang chủ|Sản phẩm nổi bật, danh mục, thương hiệu|SP:r:Đọc sản phẩm bán chạy, mới;DM:r:Đọc danh mục;TH:r:Đọc thương hiệu'
            'CN11|Xem danh sách sản phẩm|KV|Danh mục, thương hiệu cần xem|Danh sách sản phẩm|SP:r:Đọc sản phẩm đang bán;DM:r:Đọc danh mục;TH:r:Đọc thương hiệu'
            'CN12|Lọc và sắp xếp sản phẩm|KV|Khoảng giá, tiêu chí sắp xếp|Danh sách đã lọc, sắp xếp|SP:r:Đọc sản phẩm theo bộ lọc'
            'CN13|Lọc sản phẩm theo thông số kỹ thuật|KV|Thông số kỹ thuật cần lọc|Sản phẩm phù hợp|SP:r:Đọc thông số kỹ thuật'
            'CN14|Tìm kiếm sản phẩm|KV|Từ khóa|Kết quả tìm kiếm|SP:r:Tìm theo tên, thông số;DM:r:Tìm theo tên danh mục;TH:r:Tìm theo tên thương hiệu'
            'CN15|Xem chi tiết sản phẩm|KV|Sản phẩm được chọn|Thông tin, phiên bản, tồn kho|SP:r:Đọc sản phẩm, phiên bản, ảnh'
        ) }
    @{ No = 3; Name = 'Giỏ hàng'
        Boxes = 'KH:L:Khách hàng'
        Rows = @(
            'CN16|Thêm sản phẩm vào giỏ hàng|KH|Phiên bản, số lượng|Kết quả thêm vào giỏ|GH:rw:Lưu sản phẩm vào giỏ;SP:r:Kiểm tra tồn kho'
            'CN17|Xem giỏ hàng|KH|Yêu cầu xem giỏ hàng|Giỏ hàng, tạm tính|GH:r:Đọc giỏ hàng;SP:r:Đọc giá, tồn kho'
            'CN18|Cập nhật số lượng trong giỏ|KH|Số lượng mới|Giỏ hàng đã cập nhật|GH:rw:Cập nhật số lượng;SP:r:Kiểm tra tồn kho'
            'CN19|Xóa sản phẩm khỏi giỏ|KH|Sản phẩm cần xóa|Giỏ hàng đã cập nhật|GH:rw:Xóa dòng giỏ hàng'
        ) }
    @{ No = 4; Name = 'Quản lý đơn hàng'
        Boxes = 'KH:L:Khách hàng'
        Rows = @(
            'CN20|Đặt hàng|KH|Thông tin giao hàng|Mã đơn hàng, kết quả đặt hàng|GH:rw:Đọc và làm trống giỏ;DH:w:Lưu đơn hàng mới;SP:rw:Kiểm tra, trừ tồn kho;TB:w:Báo đơn mới cho nhân viên'
            'CN21|Xem lịch sử đơn hàng|KH|Yêu cầu xem lịch sử|Danh sách đơn hàng|DH:r:Đọc đơn hàng của khách'
            'CN22|Xem chi tiết đơn hàng (khách)|KH|Đơn hàng được chọn|Chi tiết, trạng thái đơn hàng|DH:r:Đọc chi tiết đơn hàng'
            'CN23|Hủy đơn hàng (khách)|KH|Yêu cầu hủy đơn|Kết quả hủy đơn|DH:rw:Chuyển trạng thái đã hủy;SP:rw:Hoàn lại tồn kho;TB:w:Báo đơn bị hủy'
        ) }
    @{ No = 5; Name = 'Xử lý đơn hàng'
        Boxes = 'NQ:L:Nhân viên / Quản trị viên'
        Rows = @(
            'CN24|Xem danh sách đơn hàng|NQ|Yêu cầu xem đơn hàng|Danh sách đơn hàng|DH:r:Đọc danh sách đơn hàng'
            'CN25|Tìm kiếm, lọc đơn hàng|NQ|Mã đơn, trạng thái, thời gian|Đơn hàng phù hợp|DH:r:Đọc đơn hàng phù hợp'
            'CN26|Xem chi tiết đơn hàng (cửa hàng)|NQ|Đơn hàng được chọn|Chi tiết đơn hàng|DH:r:Đọc chi tiết đơn hàng'
            'CN27|Cập nhật trạng thái đơn hàng|NQ|Trạng thái mới|Kết quả cập nhật|DH:rw:Lưu trạng thái mới;TB:w:Báo trạng thái cho khách'
            'CN28|Hủy đơn hàng (cửa hàng)|NQ|Lý do hủy|Kết quả hủy đơn|DH:rw:Lưu trạng thái hủy, lý do;SP:rw:Hoàn lại tồn kho;TB:w:Báo đơn bị hủy cho khách'
        ) }
    @{ No = 6; Name = 'Bảo hành và đổi hàng lỗi'
        Boxes = 'KH:L:Khách hàng'
        Rows = @(
            'CN29|Tạo yêu cầu bảo hành / đổi hàng lỗi|KH|Sản phẩm, loại lỗi, mô tả, video|Mã yêu cầu bảo hành|DH:r:Đọc đơn hàng đã giao;CS:r:Đọc thời hạn, loại lỗi;YC:w:Lưu yêu cầu, video;TB:w:Báo yêu cầu mới'
            'CN30|Xem danh sách và chi tiết yêu cầu bảo hành|KH|Yêu cầu được chọn|Trạng thái, lịch sử xử lý|YC:r:Đọc yêu cầu của khách'
            'CN31|Bổ sung bằng chứng|KH|Tệp bằng chứng bổ sung|Kết quả bổ sung|YC:rw:Lưu bằng chứng bổ sung;TB:w:Báo có bằng chứng mới'
            'CN32|Khai báo thông tin gửi hàng|KH|Đơn vị vận chuyển, mã vận đơn|Kết quả khai báo|YC:rw:Lưu thông tin gửi hàng;TB:w:Báo khách đã gửi hàng'
            'CN33|Hủy yêu cầu bảo hành|KH|Yêu cầu hủy|Kết quả hủy yêu cầu|YC:rw:Chuyển trạng thái đã hủy;TB:w:Báo yêu cầu bị hủy'
            'CN34|Xác nhận đã nhận sản phẩm|KH|Xác nhận đã nhận hàng|Yêu cầu đã hoàn tất|YC:rw:Chuyển trạng thái hoàn tất;TB:w:Báo khách đã nhận hàng'
        ) }
    @{ No = 7; Name = 'Xử lý bảo hành'
        Boxes = 'NQ:L:Nhân viên / Quản trị viên'
        Rows = @(
            'CN35|Xem danh sách yêu cầu bảo hành|NQ|Trạng thái, từ khóa|Danh sách yêu cầu|YC:r:Đọc danh sách yêu cầu'
            'CN36|Xem chi tiết yêu cầu và bằng chứng|NQ|Yêu cầu được chọn|Chi tiết, video bằng chứng|YC:r:Đọc yêu cầu, bằng chứng'
            'CN37|Xử lý yêu cầu bảo hành (yêu cầu bổ sung, duyệt, từ chối, nhận hàng, gửi hãng, gửi trả, hoàn tất)|NQ|Quyết định xử lý, ghi chú|Trạng thái mới của yêu cầu|YC:rw:Lưu trạng thái, lịch sử;SP:rw:Trừ tồn kho hàng thay thế;TB:w:Báo kết quả cho khách'
            'CN38|Ghi chú nội bộ|NQ|Nội dung ghi chú|Ghi chú đã lưu|YC:rw:Lưu ghi chú nội bộ'
            'CN39|Gửi lời nhắn cho khách hàng|NQ|Nội dung lời nhắn|Lời nhắn đã gửi|YC:rw:Lưu lời nhắn;TB:w:Báo lời nhắn cho khách'
        ) }
    @{ No = 8; Name = 'Cấu hình chính sách bảo hành'
        Boxes = 'QTV:L:Quản trị viên'
        Rows = @(
            'CN40|Cập nhật thời hạn đổi hàng lỗi theo thương hiệu|QTV|Số ngày đổi hàng lỗi|Thời hạn theo thương hiệu|CS:rw:Lưu thời hạn đổi hàng lỗi'
            'CN41|Cập nhật chính sách bảo hành theo thương hiệu và danh mục|QTV|Thương hiệu, danh mục, thời hạn|Chính sách bảo hành|CS:rw:Lưu thời hạn bảo hành'
            'CN42|Cập nhật loại lỗi|QTV|Mã, tên, phạm vi loại lỗi|Danh sách loại lỗi|CS:rw:Lưu loại lỗi'
        ) }
    @{ No = 9; Name = 'Quản lý sản phẩm'
        Boxes = 'QTV:L:Quản trị viên'
        Rows = @(
            'CN43|Xem danh sách sản phẩm (quản trị)|QTV|Từ khóa, bộ lọc|Danh sách sản phẩm|SP:r:Đọc danh sách sản phẩm;DM:r:Đọc danh mục;TH:r:Đọc thương hiệu'
            'CN44|Thêm sản phẩm|QTV|Thông tin sản phẩm|Kết quả thêm sản phẩm|SP:w:Lưu sản phẩm mới;DM:r:Chọn danh mục;TH:r:Chọn thương hiệu'
            'CN45|Sửa sản phẩm, ẩn/hiện sản phẩm|QTV|Thông tin sửa, trạng thái hiển thị|Kết quả cập nhật|SP:rw:Cập nhật sản phẩm;DM:r:Chọn danh mục;TH:r:Chọn thương hiệu'
            'CN46|Xóa sản phẩm|QTV|Sản phẩm cần xóa|Kết quả xóa|SP:rw:Xóa sản phẩm'
            'CN47|Quản lý phiên bản sản phẩm (thêm, sửa, xóa)|QTV|Thông tin phiên bản, giá|Danh sách phiên bản|SP:rw:Lưu phiên bản'
            'CN48|Quản lý hình ảnh sản phẩm (tải lên, chọn ảnh chính, xóa)|QTV|Tệp ảnh, ảnh chính|Danh sách hình ảnh|SP:rw:Lưu hình ảnh'
            'CN49|Gán thông số kỹ thuật cho sản phẩm|QTV|Giá trị thông số|Thông số đã gán|SP:rw:Lưu giá trị thông số'
        ) }
    @{ No = 10; Name = 'Quản lý danh mục và thương hiệu'
        Boxes = 'QTV:L:Quản trị viên'
        Rows = @(
            'CN50|Quản lý danh mục (xem, thêm, sửa, xóa)|QTV|Thông tin danh mục|Danh sách danh mục|DM:rw:Lưu danh mục'
            'CN51|Quản lý thương hiệu (xem, thêm, sửa, xóa)|QTV|Thông tin thương hiệu|Danh sách thương hiệu|TH:rw:Lưu thương hiệu'
        ) }
    @{ No = 11; Name = 'Quản lý kho'
        Boxes = 'QTV:L:Quản trị viên'
        Rows = @(
            'CN52|Nhập kho|QTV|Phiên bản, số lượng nhập, ghi chú|Tồn kho mới|SP:rw:Cộng tồn kho, ghi lịch sử'
            'CN53|Xem lịch sử nhập xuất kho|QTV|Sản phẩm cần xem|Lịch sử nhập xuất kho|SP:r:Đọc lịch sử nhập xuất'
        ) }
    @{ No = 12; Name = 'Quản lý khách hàng'
        Boxes = 'QTV:L:Quản trị viên'
        Rows = @(
            'CN54|Xem danh sách khách hàng|QTV|Từ khóa, trạng thái|Danh sách khách hàng|TK:r:Đọc tài khoản khách hàng'
            'CN55|Xem chi tiết khách hàng|QTV|Khách hàng được chọn|Thông tin, lịch sử mua hàng|TK:r:Đọc thông tin khách hàng;DH:r:Đọc lịch sử mua hàng'
            'CN56|Khóa / mở khóa tài khoản khách hàng|QTV|Lý do khóa|Trạng thái tài khoản|TK:rw:Lưu trạng thái khóa'
            'CN57|Cấp mật khẩu tạm cho khách hàng|QTV|Khách hàng cần cấp|Mật khẩu tạm|TK:rw:Lưu mật khẩu tạm'
            'CN58|Xóa tài khoản khách hàng|QTV|Số điện thoại xác nhận|Kết quả xóa|TK:rw:Xóa tài khoản;DH:r:Kiểm tra đơn hàng'
        ) }
    @{ No = 13; Name = 'Quản lý nhân viên'
        Boxes = 'QTV:L:Quản trị viên'
        Rows = @(
            'CN59|Xem danh sách nhân viên|QTV|Từ khóa, trạng thái|Danh sách nhân viên|TK:r:Đọc tài khoản nhân viên'
            'CN60|Thêm nhân viên|QTV|Họ tên, Gmail, số điện thoại|Kết quả thêm nhân viên|TK:rw:Lưu tài khoản nhân viên'
            'CN61|Sửa thông tin nhân viên|QTV|Thông tin cần sửa|Kết quả cập nhật|TK:rw:Cập nhật thông tin'
            'CN62|Khóa / mở khóa tài khoản nhân viên|QTV|Lý do khóa|Trạng thái tài khoản|TK:rw:Lưu trạng thái khóa'
            'CN63|Đặt lại mật khẩu nhân viên|QTV|Nhân viên cần đặt lại|Kết quả đặt lại mật khẩu|TK:rw:Đặt mật khẩu về số điện thoại'
        ) }
    @{ No = 14; Name = 'Thông báo'
        Boxes = 'ALL:L:Khách hàng / Nhân viên / Quản trị viên'
        Rows = @(
            'CN64|Nhận thông báo|ALL||Thông báo mới|TB:r:Đọc thông báo mới'
            'CN65|Xem danh sách thông báo|ALL|Yêu cầu xem thông báo|Danh sách thông báo|TB:r:Đọc thông báo của người dùng'
            'CN66|Đánh dấu thông báo đã đọc|ALL|Thông báo được chọn|Trạng thái đã đọc|TB:rw:Lưu trạng thái đã đọc'
        ) }
    @{ No = 15; Name = 'Báo cáo thống kê'
        Boxes = 'QTV:L:Quản trị viên'
        Rows = @(
            'CN67|Xem tổng quan|QTV|Yêu cầu xem tổng quan|Số liệu tổng quan|DH:r:Đọc số liệu đơn hàng'
            'CN68|Xem báo cáo doanh thu|QTV|Khoảng thời gian|Báo cáo doanh thu|DH:r:Đọc đơn hàng hoàn tất'
            'CN69|Xuất báo cáo Excel|QTV|Khoảng thời gian|Tệp Excel báo cáo|DH:r:Đọc đơn hàng hoàn tất'
        ) }
)

# ---------------------------------------------------------------- hình học chung
$AL = 40; $AW = 200            # tác nhân trái
$EX = 540; $EW = 250           # elip tiến trình con
$ECX = $EX + $EW / 2
$T0 = $EX + $EW + 250          # làn dọc đầu tiên dẫn tới kho
$LANE = 30
$SW = 270                      # bề rộng kho
$SLOT = 44                     # khoảng cách giữa các luồng trên cùng một cạnh elip
$TOP = 100                     # dưới tiêu đề

function WrapText([string]$t, [double]$maxW) {
    $out = @(); $cur = ''
    foreach ($w in $t.Split(' ')) { $try = ($cur + ' ' + $w).Trim(); if ($cur -and ($try.Length * $charW) -gt $maxW) { $out += $cur; $cur = $w } else { $cur = $try } }
    $out + $cur
}
function TextW([string]$t) { [math]::Ceiling($t.Length * $charW) + 8 }
function SplitTwo([string]$t) {
    $w = $t.Split(' '); $best = $null; $bestMax = 1e9
    for ($i = 1; $i -lt $w.Count; $i++) {
        $a = ($w[0..($i - 1)] -join ' '); $b = ($w[$i..($w.Count - 1)] -join ' ')
        $m = [math]::Max($a.Length, $b.Length); if ($m -lt $bestMax) { $bestMax = $m; $best = @($a, $b) }
    }
    $best
}
function Overlap($ax1, $ay1, $ax2, $ay2, $bx1, $by1, $bx2, $by2) { $ax1 -le $bx2 -and $bx1 -le $ax2 -and $ay1 -le $by2 -and $by1 -le $ay2 }
function Perms($arr) {
    if ($arr.Count -le 1) { return , @($arr) }
    $res = @()
    for ($i = 0; $i -lt $arr.Count; $i++) {
        $rest = @(for ($j = 0; $j -lt $arr.Count; $j++) { if ($j -ne $i) { $arr[$j] } })
        foreach ($p in (Perms $rest)) { $res += , (@($arr[$i]) + @($p)) }
    }
    $res
}
function ArcX($cx, $cy, $rx, $ry, [double]$y, [string]$side) {
    $t = ($y - $cy) / $ry; $t = 1 - $t * $t; if ($t -lt 0) { $t = 0 }
    $d = $rx * [math]::Sqrt($t)
    if ($side -eq 'L') { [math]::Round($cx - $d, 1) } else { [math]::Round($cx + $d, 1) }
}

function Build-Page($g) {
    $no = $g.No
    # --- phân tích dữ liệu
    $boxes = [ordered]@{}
    foreach ($b in $g.Boxes.Split('|')) { $id, $sd, $tx = $b.Split(':'); $boxes[$id] = [pscustomobject]@{ Id = $id; Side = $sd; Text = $tx; Rows = @() } }
    $rows = @()
    $storeKeys = @()
    $i = 0
    foreach ($line in $g.Rows) {
        $i++
        $f = $line.Split('|')
        $links = @()
        foreach ($s in ($f[5].Split(';') | Where-Object { $_ })) {
            $k, $m, $lb = $s.Split(':')
            $links += , [pscustomobject]@{ Store = $k; Mode = $m; Label = $lb; Y = 0 }
            if ($storeKeys -notcontains $k) { $storeKeys += $k }
        }
        $bx = @($f[2].Split(','))
        foreach ($b in $bx) { $boxes[$b].Rows += $i }
        $rows += [pscustomobject]@{
            Idx = $i; Code = $f[0]; Name = $f[1]; Boxes = $bx; In = $f[3]; Out = $f[4]; Links = $links
            Left = @($bx | Where-Object { $boxes[$_].Side -eq 'L' }); Right = @($bx | Where-Object { $boxes[$_].Side -eq 'R' })
            Cy = 0; H = 0
        }
    }
    $hasRight = @($boxes.Values | Where-Object Side -eq 'R').Count -gt 0
    $problems = @()

    # --- kích thước từng dòng
    $y = $TOP
    foreach ($r in $rows) {
        if ($r.Left.Count -gt 1 -or $r.Right.Count -gt 1) { $problems += "row $($r.Idx): more than one actor on a side" }
        $nL = if ($r.Left.Count) { if ($r.In) { 2 } else { 1 } } else { 0 }
        $nR = $r.Links.Count + $(if ($r.Right.Count) { if ($r.In) { 2 } else { 1 } } else { 0 })
        $n = [math]::Max($nL, $nR)
        $txtLines = (WrapText $r.Name ($EW - 80)).Count + 1
        $h = [math]::Max([math]::Max(110, $SLOT * $n + 40), $txtLines * $lineH + 56)
        $r.H = $h; $r.Cy = $y + $h / 2
        $y = $r.Cy + $h / 2 + 50
    }
    $rowsBottom = $y - 50

    # --- tác nhân: hình chữ nhật cao phủ các dòng nó tham gia (phải liền mạch, không chồng tác nhân cùng phía)
    foreach ($b in $boxes.Values) {
        $first = ($b.Rows | Measure-Object -Minimum).Minimum; $last = ($b.Rows | Measure-Object -Maximum).Maximum
        $b | Add-Member -NotePropertyName Top -NotePropertyValue ($rows[$first - 1].Cy - $rows[$first - 1].H / 2) -Force
        $b | Add-Member -NotePropertyName Bottom -NotePropertyValue ($rows[$last - 1].Cy + $rows[$last - 1].H / 2) -Force
    }
    foreach ($a in $boxes.Values) { foreach ($b in $boxes.Values) {
        if ($a.Id -lt $b.Id -and $a.Side -eq $b.Side -and $a.Top -lt $b.Bottom + 20 -and $b.Top -lt $a.Bottom + 20) { $problems += "actors $($a.Id) and $($b.Id) overlap" }
    } }

    # --- y của các luồng tới tác nhân (dải cấm đặt kho với tác nhân bên phải)
    $forbid = @()
    foreach ($r in $rows) { if ($r.Right.Count) {
        $k = $r.Links.Count + $(if ($r.In) { 2 } else { 1 })
        for ($s = 0; $s -lt $(if ($r.In) { 2 } else { 1 }); $s++) { $forbid += $r.Cy + ($s - ($k - 1) / 2) * $SLOT }
    } }

    # các độ cao cổng bên phải của mọi tiến trình con: kho không đặt trùng độ cao này (tránh hai đường chồng nhau)
    $slotYs = @()
    foreach ($r in $rows) { $k = $r.Links.Count + $(if ($r.Right.Count) { if ($r.In) { 2 } else { 1 } } else { 0 }); for ($s = 0; $s -lt $k; $s++) { $slotYs += $r.Cy + ($s - ($k - 1) / 2) * $SLOT } }
    # --- chọn thứ tự kho (dọc) và thứ tự làn sao cho ít chỗ cắt nhất
    $nS = $storeKeys.Count
    $desired = @{}
    foreach ($k in $storeKeys) { $desired[$k] = (($rows | Where-Object { $_.Links.Store -contains $k } | ForEach-Object { $_.Cy }) | Measure-Object -Average).Average }
    $best = $null
    $orders = if ($nS) { Perms $storeKeys } else { @(, @()) }
    foreach ($ord in $orders) {
        # vị trí y của kho theo thứ tự này
        $sy = @{}; $prev = -1e9
        foreach ($k in $ord) {
            $v = [math]::Max($desired[$k], $prev + 80)
            $moved = $true
            while ($moved) { $moved = $false; foreach ($fy in $forbid) { if ([math]::Abs($v - $fy) -lt 34) { $v = $fy + 34; $moved = $true } }; foreach ($fy in $slotYs) { if ([math]::Abs($v - $fy) -lt 21.9) { $v = $fy + 22; $moved = $true } } }
            $sy[$k] = $v; $prev = $v
        }
        # cổng bên phải của mỗi dòng: tác nhân phải ở trên cùng, các kho xếp theo y của kho
        $ports = @()
        foreach ($r in $rows) {
            $nA = if ($r.Right.Count) { if ($r.In) { 2 } else { 1 } } else { 0 }
            $k = $r.Links.Count + $nA
            $sorted = @($r.Links | Sort-Object { $sy[$_.Store] })
            for ($s = 0; $s -lt $sorted.Count; $s++) { $ports += [pscustomobject]@{ Row = $r; Link = $sorted[$s]; Y = $r.Cy + ($nA + $s - ($k - 1) / 2) * $SLOT } }
        }
        foreach ($lanes in (Perms (0..([math]::Max(0, $nS - 1)) | Select-Object -First $nS))) {
            if (-not $nS) { $lanes = @() }
            $laneOf = @{}; for ($q = 0; $q -lt $nS; $q++) { $laneOf[$storeKeys[$q]] = $lanes[$q] }
            # khoảng dọc của mỗi làn
            $span = @{}
            foreach ($k in $storeKeys) { $ys = @($ports | Where-Object { $_.Link.Store -eq $k } | ForEach-Object { $_.Y }) + $sy[$k]; $span[$k] = @(($ys | Measure-Object -Minimum).Minimum, ($ys | Measure-Object -Maximum).Maximum) }
            $cross = 0
            # một đường ngang ở độ cao yy đi qua làn của kho k: cắt (1) hoặc chạm sát đầu làn (phạt nặng)
            function HitLane($yy, $k) {
                if ($yy -lt $span[$k][0] - 10 -or $yy -gt $span[$k][1] + 10) { return 0 }
                if ([math]::Abs($yy - $span[$k][0]) -lt 10 -or [math]::Abs($yy - $span[$k][1]) -lt 10) { return 1000 }
                1
            }
            foreach ($p in $ports) { foreach ($k in $storeKeys) {
                if ($laneOf[$k] -lt $laneOf[$p.Link.Store]) { $cross += HitLane $p.Y $k } } }
            foreach ($k in $storeKeys) { foreach ($k2 in $storeKeys) {
                if ($laneOf[$k2] -gt $laneOf[$k]) { $cross += HitLane $sy[$k] $k2 } } }
            foreach ($fy in $forbid) { foreach ($k in $storeKeys) { $cross += HitLane $fy $k } }
            $dev = 0; foreach ($k in $storeKeys) { $dev += [math]::Abs($sy[$k] - $desired[$k]) }
            $score = $cross * 10000 + $dev
            if ($null -eq $best -or $score -lt $best.Score) { $best = [pscustomobject]@{ Score = $score; Cross = $cross; Sy = $sy; Ports = $ports; Lane = $laneOf } }
        }
    }

    # --- nút
    $nodes = [ordered]@{}
    function Node($id, $kind, $text, $x, $y, $w, $h) { $nodes[$id] = [pscustomobject]@{ Id = $id; Kind = $kind; Text = $text; X = $x; Y = $y; W = $w; H = $h } }
    $SX = $T0 + [math]::Max(0, $nS - 1) * $LANE + 50
    $RX = $SX + $SW + 320
    if (-not $nS) { $RX = $EX + $EW + 320 }
    foreach ($b in $boxes.Values) { Node $b.Id 'actor' $b.Text $(if ($b.Side -eq 'L') { $AL } else { $RX }) $b.Top $AW ($b.Bottom - $b.Top) }
    foreach ($r in $rows) { Node "P$($r.Idx)" 'proc' "$no.$($r.Idx)|$($r.Name)" $EX ($r.Cy - $r.H / 2) $EW $r.H }
    $storeId = @{}; $q = 0
    foreach ($k in ($storeKeys | Sort-Object { $best.Sy[$_] })) { $q++; $storeId[$k] = "D$no.$q"; Node "S$k" 'store' "D$no.$q | $($storeName[$k])" $SX ($best.Sy[$k] - 20) $SW 40 }

    # --- luồng
    $flows = New-Object System.Collections.ArrayList
    function Flow($src, $dst, $pts, $label, $arrows, $group, $lx1, $lx2) {
        $clean = @()
        foreach ($p in $pts) {
            $p = @([math]::Round([double]$p[0], 1), [math]::Round([double]$p[1], 1))
            if ($clean.Count -and $clean[-1][0] -eq $p[0] -and $clean[-1][1] -eq $p[1]) { continue }
            $clean += , $p
        }
        if (-not $group) { $group = "F$($flows.Count)" }
        [void]$flows.Add([pscustomobject]@{ Src = $src; Dst = $dst; Pts = $clean; Label = $label; Arrows = $arrows; Group = $group; LX1 = $lx1; LX2 = $lx2 })
    }
    foreach ($r in $rows) {
        $pid_ = "P$($r.Idx)"; $ery = $r.H / 2; $erx = $EW / 2
        foreach ($b in $r.Left) {
            $ax = $AL + $AW
            if ($r.In) {
                $yi = $r.Cy - $SLOT / 2; $yo = $r.Cy + $SLOT / 2
                Flow $b $pid_ @(@($ax, $yi), @((ArcX $ECX $r.Cy $erx $ery $yi 'L'), $yi)) $r.In 'end' $null $ax (ArcX $ECX $r.Cy $erx $ery $yi 'L')
            } else { $yo = $r.Cy }
            Flow $pid_ $b @(@((ArcX $ECX $r.Cy $erx $ery $yo 'L'), $yo), @($ax, $yo)) $r.Out 'end' $null $ax (ArcX $ECX $r.Cy $erx $ery $yo 'L')
        }
        $nA = if ($r.Right.Count) { if ($r.In) { 2 } else { 1 } } else { 0 }
        $k = $r.Links.Count + $nA
        foreach ($b in $r.Right) {
            $lx1 = if ($nS) { $SX + $SW } else { $EX + $EW }
            if ($r.In) {
                $yi = $r.Cy + (0 - ($k - 1) / 2) * $SLOT; $yo = $r.Cy + (1 - ($k - 1) / 2) * $SLOT
                Flow $b $pid_ @(@($RX, $yi), @((ArcX $ECX $r.Cy $erx $ery $yi 'R'), $yi)) $r.In 'end' $null $lx1 $RX
            } else { $yo = $r.Cy + (0 - ($k - 1) / 2) * $SLOT }
            Flow $pid_ $b @(@((ArcX $ECX $r.Cy $erx $ery $yo 'R'), $yo), @($RX, $yo)) $r.Out 'end' $null $lx1 $RX
        }
    }
    foreach ($p in $best.Ports) {
        $r = $p.Row; $k = $p.Link.Store
        $sx0 = ArcX $ECX $r.Cy ($EW / 2) ($r.H / 2) $p.Y 'R'
        $tx = $T0 + $best.Lane[$k] * $LANE
        $arr = @{ r = 'start'; w = 'end'; rw = 'both' }[$p.Link.Mode]
        Flow "P$($r.Idx)" "S$k" @(@($sx0, $p.Y), @($tx, $p.Y), @($tx, $best.Sy[$k]), @($SX, $best.Sy[$k])) $p.Link.Label $arr "T$k" $sx0 ($T0 - 12)
        $p.Link | Add-Member -NotePropertyName StoreId -NotePropertyValue $storeId[$k] -Force
    }

    # --- nhãn (đặt giữa khoảng LX1..LX2 trên đoạn ngang đầu tiên)
    $labels = @()
    foreach ($f in $flows) {
        if (-not $f.Label) { continue }
        $len = $f.LX2 - $f.LX1
        $lines = @($f.Label); $w = TextW $f.Label
        if ($w -gt $len - 30) { $lines = SplitTwo $f.Label; $w = [math]::Max((TextW $lines[0]), (TextW $lines[1])) }
        if ($w -gt $len - 16) { $problems += "label too long: '$($f.Label)'" }
        $lh = $lines.Count * $lineH
        $labels += [pscustomobject]@{ Flow = $f; Lines = $lines; X = [math]::Round(($f.LX1 + $f.LX2) / 2 - $w / 2, 1); Y = [math]::Round($f.Pts[0][1] - $lh / 2, 1); W = $w; H = $lh }
    }

    # --- kiểm tra
    $segs = @(foreach ($f in $flows) { for ($k = 0; $k -lt $f.Pts.Count - 1; $k++) {
        $a = $f.Pts[$k]; $b = $f.Pts[$k + 1]
        [pscustomobject]@{ F = $f; H = ($a[1] -eq $b[1]); X1 = [math]::Min($a[0], $b[0]); X2 = [math]::Max($a[0], $b[0]); Y1 = [math]::Min($a[1], $b[1]); Y2 = [math]::Max($a[1], $b[1]) }
    } })
    function FName($f) { "$($f.Src)->$($f.Dst) '$($f.Label)'" }
    $crossings = 0
    for ($k = 0; $k -lt $segs.Count; $k++) { for ($l = $k + 1; $l -lt $segs.Count; $l++) {
        $a = $segs[$k]; $b = $segs[$l]; if ($a.F.Group -eq $b.F.Group) { continue }
        if (-not (Overlap ($a.X1 - 3) ($a.Y1 - 3) ($a.X2 + 3) ($a.Y2 + 3) $b.X1 $b.Y1 $b.X2 $b.Y2)) { continue }
        if ($a.H -ne $b.H) {
            # cắt vuông góc: chấp nhận nếu điểm cắt nằm hẳn bên trong cả hai đoạn (vẽ cầu vồng)
            $hz = if ($a.H) { $a } else { $b }; $vt = if ($a.H) { $b } else { $a }
            if ($vt.X1 -gt $hz.X1 + 8 -and $vt.X1 -lt $hz.X2 - 8 -and $hz.Y1 -gt $vt.Y1 + 8 -and $hz.Y1 -lt $vt.Y2 - 8) { $crossings++; continue }
        }
        $problems += "$(FName $a.F) touches $(FName $b.F)"
    } }
    foreach ($s in $segs) { foreach ($nd in $nodes.Values) {
        if ($nd.Id -eq $s.F.Src -or $nd.Id -eq $s.F.Dst) { continue }
        if (Overlap $s.X1 $s.Y1 $s.X2 $s.Y2 ($nd.X - 4) ($nd.Y - 4) ($nd.X + $nd.W + 4) ($nd.Y + $nd.H + 4)) { $problems += "$(FName $s.F) runs through $($nd.Id)" }
    } }
    for ($k = 0; $k -lt $labels.Count; $k++) {
        $l = $labels[$k]
        for ($j = $k + 1; $j -lt $labels.Count; $j++) {
            $m = $labels[$j]
            if (Overlap $l.X $l.Y ($l.X + $l.W) ($l.Y + $l.H) ($m.X - 4) ($m.Y - 2) ($m.X + $m.W + 4) ($m.Y + $m.H + 2)) { $problems += "labels overlap: '$($l.Flow.Label)' / '$($m.Flow.Label)'" }
        }
        foreach ($s in $segs) { if ($s.F -eq $l.Flow) { continue }
            if (Overlap $l.X $l.Y ($l.X + $l.W) ($l.Y + $l.H) $s.X1 $s.Y1 $s.X2 $s.Y2) { $problems += "label '$($l.Flow.Label)' covers $(FName $s.F)" } }
        foreach ($nd in $nodes.Values) {
            if (Overlap $l.X $l.Y ($l.X + $l.W) ($l.Y + $l.H) $nd.X $nd.Y ($nd.X + $nd.W) ($nd.Y + $nd.H)) { $problems += "label '$($l.Flow.Label)' covers $($nd.Id)" } }
    }

    $W = [math]::Ceiling((($nodes.Values | ForEach-Object { $_.X + $_.W }) | Measure-Object -Maximum).Maximum + 40)
    $H = [math]::Ceiling([math]::Max($rowsBottom, (($nodes.Values | ForEach-Object { $_.Y + $_.H }) | Measure-Object -Maximum).Maximum) + 40)
    [pscustomobject]@{ Group = $g; Rows = $rows; Boxes = $boxes; StoreId = $storeId; Nodes = $nodes; Flows = $flows; Labels = $labels
        Problems = @($problems | Select-Object -Unique); Crossings = $crossings; W = $W; H = $H
        Title = "SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH $no.0: $($g.Name.ToUpper())" }
}

# ---------------------------------------------------------------- dựng & xuất
New-Item -ItemType Directory -Force $OutDir | Out-Null
$style = @{
    actor = "rounded=0;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#000000;strokeWidth=1.5;fontSize=$fs;fontStyle=1;$font"
    proc  = "ellipse;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#000000;strokeWidth=1.5;fontSize=$fs;spacing=8;$font"
    store = "shape=partialRectangle;whiteSpace=wrap;html=1;top=1;bottom=1;left=0;right=0;fillColor=#ffffff;strokeColor=#000000;strokeWidth=1.5;fontSize=$fs;fontStyle=1;$font"
}
$lab = "text;html=1;align=center;verticalAlign=middle;whiteSpace=wrap;fillColor=#ffffff;strokeColor=none;spacing=0;fontSize=$fs;$font"
$titleStyle = "text;html=1;align=center;verticalAlign=middle;strokeColor=none;fillColor=none;fontSize=18;fontStyle=1;$font"
$edge = @("${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe", "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1

function NodeValue($n) {
    if ($n.Kind -eq 'proc') { $num, $name = $n.Text.Split('|'); return "<b>$(Esc $num)</b><br>$(Esc $name)" }
    Esc $n.Text
}
function SvgText($cx, $cy, $lines, $boldFirst = $false, $boldAll = $false) {
    $y0 = $cy - ($lines.Count - 1) * $lineH / 2 + 5; $i = 0
    ($lines | ForEach-Object {
        $fw = if ($boldAll -or ($boldFirst -and $i -eq 0)) { ' font-weight="bold"' } else { '' }
        "<text x=""$cx"" y=""$($y0 + $lineH * $i++)"" text-anchor=""middle""$fw>$(Esc $_)</text>" }) -join ''
}

$pages = @()
foreach ($g in $groups) {
    $p = Build-Page $g
    $pages += $p
    $base = Join-Path $OutDir ("dfd-tien-trinh-{0:00}" -f $g.No)
    $W = $p.W; $H = $p.H

    # draw.io
    $cells = New-Object System.Text.StringBuilder
    [void]$cells.AppendLine("        <mxCell id=""title"" value=""$(Esc $p.Title)"" style=""$titleStyle"" vertex=""1"" parent=""1""><mxGeometry x=""0"" y=""25"" width=""$W"" height=""40"" as=""geometry"" /></mxCell>")
    foreach ($n in $p.Nodes.Values) {
        [void]$cells.AppendLine("        <mxCell id=""$($n.Id)"" value=""$(Esc (NodeValue $n))"" style=""$($style[$n.Kind])"" vertex=""1"" parent=""1""><mxGeometry x=""$($n.X)"" y=""$($n.Y)"" width=""$($n.W)"" height=""$($n.H)"" as=""geometry"" /></mxCell>")
    }
    $k = 0
    foreach ($f in $p.Flows) {
        $k++; $pt = $f.Pts
        $arr = switch ($f.Arrows) { 'end' { 'startArrow=none;endArrow=classic;endFill=1;' } 'start' { 'startArrow=classic;startFill=1;endArrow=none;' } 'both' { 'startArrow=classic;startFill=1;endArrow=classic;endFill=1;' } }
        $es = "edgeStyle=none;rounded=0;html=1;jumpStyle=arc;jumpSize=12;${arr}endSize=8;startSize=8;strokeColor=#000000;strokeWidth=1.2;"
        $mid = ''
        if ($pt.Count -gt 2) { $mid = '<Array as="points">' + (($pt[1..($pt.Count - 2)] | ForEach-Object { "<mxPoint x=""$($_[0])"" y=""$($_[1])"" />" }) -join '') + '</Array>' }
        [void]$cells.AppendLine("        <mxCell id=""f$k"" style=""$es"" edge=""1"" parent=""1""><mxGeometry relative=""1"" as=""geometry""><mxPoint x=""$($pt[0][0])"" y=""$($pt[0][1])"" as=""sourcePoint"" /><mxPoint x=""$($pt[-1][0])"" y=""$($pt[-1][1])"" as=""targetPoint"" />$mid</mxGeometry></mxCell>")
    }
    $k = 0
    foreach ($l in $p.Labels) {
        $k++
        [void]$cells.AppendLine("        <mxCell id=""l$k"" value=""$(Esc (($l.Lines | ForEach-Object { Esc $_ }) -join '<br>'))"" style=""$lab"" vertex=""1"" parent=""1""><mxGeometry x=""$($l.X)"" y=""$($l.Y)"" width=""$($l.W)"" height=""$($l.H)"" as=""geometry"" /></mxCell>")
    }
    $xml = @"
<mxfile host="app.diagrams.net" type="device">
  <diagram id="dfd1-$($g.No)" name="$(Esc ("Tiến trình $($g.No).0 - $($g.Name)"))">
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
    [IO.File]::WriteAllText("$base.drawio", $xml, $utf8)
    [xml]$null = [IO.File]::ReadAllText("$base.drawio", $utf8)

    # xem trước SVG -> PNG (cầu vồng tại chỗ cắt giống draw.io)
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.Append("<svg xmlns=""http://www.w3.org/2000/svg"" width=""$W"" height=""$H"" viewBox=""0 0 $W $H"" font-family=""$fontName"" font-size=""$fs""><rect width=""100%"" height=""100%"" fill=""#fff""/>")
    [void]$sb.Append('<defs><marker id="a" viewBox="0 0 10 10" refX="10" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse"><path d="M0,0 L10,5 L0,10 z"/></marker></defs>')
    [void]$sb.Append("<text x=""$($W/2)"" y=""52"" text-anchor=""middle"" font-size=""18"" font-weight=""bold"">$(Esc $p.Title)</text>")
    $vsegs = @(foreach ($f in $p.Flows) { for ($k = 0; $k -lt $f.Pts.Count - 1; $k++) { $a = $f.Pts[$k]; $b = $f.Pts[$k + 1]; if ($a[0] -eq $b[0]) { [pscustomobject]@{ G = $f.Group; X = $a[0]; Y1 = [math]::Min($a[1], $b[1]); Y2 = [math]::Max($a[1], $b[1]) } } } })
    foreach ($f in $p.Flows) {
        $d = "M$($f.Pts[0][0]),$($f.Pts[0][1])"
        for ($k = 0; $k -lt $f.Pts.Count - 1; $k++) {
            $a = $f.Pts[$k]; $b = $f.Pts[$k + 1]
            if ($a[1] -eq $b[1]) {
                $dir = [math]::Sign($b[0] - $a[0])
                $xs = @($vsegs | Where-Object { $_.G -ne $f.Group -and $_.X -gt [math]::Min($a[0], $b[0]) + 8 -and $_.X -lt [math]::Max($a[0], $b[0]) - 8 -and $a[1] -gt $_.Y1 + 8 -and $a[1] -lt $_.Y2 - 8 } | ForEach-Object { $_.X } | Sort-Object { $_ * $dir })
                foreach ($x in $xs) { $d += " L$($x - 6 * $dir),$($a[1]) A6,6 0 0 $(if ($dir -gt 0) { 1 } else { 0 }) $($x + 6 * $dir),$($a[1])" }
            }
            $d += " L$($b[0]),$($b[1])"
        }
        $m = switch ($f.Arrows) { 'end' { ' marker-end="url(#a)"' } 'start' { ' marker-start="url(#a)"' } 'both' { ' marker-start="url(#a)" marker-end="url(#a)"' } }
        [void]$sb.Append("<path d=""$d"" fill=""none"" stroke=""#000"" stroke-width=""1.2""$m/>")
    }
    foreach ($n in $p.Nodes.Values) {
        $cx = $n.X + $n.W / 2; $cy = $n.Y + $n.H / 2
        switch ($n.Kind) {
            'actor' { [void]$sb.Append("<rect x=""$($n.X)"" y=""$($n.Y)"" width=""$($n.W)"" height=""$($n.H)"" fill=""#fff"" stroke=""#000"" stroke-width=""1.5""/>" + (SvgText $cx $cy (WrapText $n.Text ($n.W - 20)) $false $true)) }
            'proc'  { $num, $name = $n.Text.Split('|'); [void]$sb.Append("<ellipse cx=""$cx"" cy=""$cy"" rx=""$($n.W/2)"" ry=""$($n.H/2)"" fill=""#fff"" stroke=""#000"" stroke-width=""1.5""/>" + (SvgText $cx $cy (@($num) + @(WrapText $name ($n.W - 80))) $true)) }
            'store' { [void]$sb.Append("<rect x=""$($n.X)"" y=""$($n.Y)"" width=""$($n.W)"" height=""$($n.H)"" fill=""#fff""/><line x1=""$($n.X)"" y1=""$($n.Y)"" x2=""$($n.X+$n.W)"" y2=""$($n.Y)"" stroke=""#000"" stroke-width=""1.5""/><line x1=""$($n.X)"" y1=""$($n.Y+$n.H)"" x2=""$($n.X+$n.W)"" y2=""$($n.Y+$n.H)"" stroke=""#000"" stroke-width=""1.5""/>" + (SvgText $cx $cy @($n.Text) $false $true)) }
        }
    }
    foreach ($l in $p.Labels) { [void]$sb.Append("<rect x=""$($l.X)"" y=""$($l.Y)"" width=""$($l.W)"" height=""$($l.H)"" fill=""#fff""/>" + (SvgText ($l.X + $l.W / 2) ($l.Y + $l.H / 2) $l.Lines)) }
    [void]$sb.Append('</svg>')
    $svg = Join-Path $env:TEMP ("dfd-tien-trinh-{0:00}.svg" -f $g.No)
    [IO.File]::WriteAllText($svg, $sb.ToString(), $utf8)
    if ($edge -and -not $NoPng) {
        $uri = ([Uri]$svg).AbsoluteUri
        & $edge --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=2 "--screenshot=$base.png" "--window-size=$W,$H" $uri 2>$null | Out-Null
    }

    $status = if ($p.Problems.Count) { 'LỖI' } else { 'ok' }
    Write-Output ("{0:00}. {1}: {2} tiến trình con, {3} kho, {4} luồng, {5} chỗ cắt, {6}x{7} [{8}]" -f $g.No, $g.Name, $p.Rows.Count, $p.StoreId.Count, $p.Flows.Count, $p.Crossings, $W, $H, $status)
    $p.Problems | ForEach-Object { Write-Output "   PROBLEM: $_" }
}

# ---------------------------------------------------------------- bảng mô tả (sinh từ cùng dữ liệu)
if ($OutMd) {
    $md = New-Object System.Text.StringBuilder
    [void]$md.AppendLine('# Sơ đồ luồng dữ liệu mức dưới đỉnh – danh sách sơ đồ và mô tả tiến trình')
    [void]$md.AppendLine()
    [void]$md.AppendLine('Sinh tự động từ `Docs/Diagrams/build-dfd-duoi-dinh.ps1`. Mỗi tiến trình của sơ đồ mức đỉnh được phân rã thành một sơ đồ riêng trong `Docs/dfd-duoi-dinh/` (file `.drawio` để sửa, `.png` để chèn vào báo cáo). Tiến trình con là các chức năng trong bảng "Gom nhóm các chức năng" của khóa luận.')
    [void]$md.AppendLine()
    [void]$md.AppendLine('| STT | Sơ đồ | Tiến trình con | Kho dữ liệu | File |')
    [void]$md.AppendLine('|:---|:---|:---|:---|:---|')
    foreach ($p in $pages) {
        $g = $p.Group
        $subs = ($p.Rows | ForEach-Object { "$($g.No).$($_.Idx) $($_.Name)" }) -join '<br>'
        $sts = ($p.StoreId.Keys | Sort-Object { $p.StoreId[$_] } | ForEach-Object { "$($p.StoreId[$_]) $($storeName[$_])" }) -join '<br>'
        if (-not $sts) { $sts = '–' }
        [void]$md.AppendLine("| $($g.No) | $($p.Title) | $subs | $sts | dfd-tien-trinh-$('{0:00}' -f $g.No).drawio / .png |")
    }
    foreach ($p in $pages) {
        $g = $p.Group
        [void]$md.AppendLine()
        [void]$md.AppendLine("## $($p.Title)")
        [void]$md.AppendLine()
        [void]$md.AppendLine('| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |')
        [void]$md.AppendLine('|:---|:---|:---|:---|:---|:---|')
        foreach ($r in $p.Rows) {
            $actors = ($r.Boxes | ForEach-Object { $p.Boxes[$_].Text }) -join '; '
            $st = ($r.Links | Sort-Object StoreId | ForEach-Object { "$($_.StoreId) $($storeName[$_.Store]) – $(@{ r = 'đọc'; w = 'ghi'; rw = 'đọc, ghi' }[$_.Mode]): $($_.Label)" }) -join '<br>'
            if (-not $st) { $st = '–' }
            $in = if ($r.In) { $r.In } else { '–' }
            [void]$md.AppendLine("| $($g.No).$($r.Idx) $($r.Name) | $($r.Code) | $actors | $in | $($r.Out) | $st |")
        }
    }
    [void]$md.AppendLine()
    [void]$md.AppendLine('Ghi chú: mã kho đánh số riêng trong từng sơ đồ (Dn.k – kho thứ k của sơ đồ tiến trình n). "đọc" – tiến trình lấy dữ liệu từ kho; "ghi" – tiến trình thêm/cập nhật dữ liệu vào kho. Kho "Sản phẩm, tồn kho" gồm sản phẩm, phiên bản, hình ảnh, thông số, số lượng tồn và lịch sử nhập xuất kho; kho "Chính sách bảo hành" gồm thời hạn bảo hành, thời hạn đổi hàng lỗi và danh sách loại lỗi; kho "Yêu cầu bảo hành" gồm yêu cầu, bằng chứng, lịch sử xử lý và lời nhắn.')
    [IO.File]::WriteAllText($OutMd, $md.ToString(), (New-Object System.Text.UTF8Encoding($true)))
    Write-Output "table: $OutMd"
}
