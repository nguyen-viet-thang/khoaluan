# Sơ đồ luồng dữ liệu mức dưới đỉnh – danh sách sơ đồ và mô tả tiến trình

Sinh tự động từ `Docs/Diagrams/build-dfd-duoi-dinh.ps1`. Mỗi tiến trình của sơ đồ mức đỉnh được phân rã thành một sơ đồ riêng trong `Docs/dfd-duoi-dinh/` (file `.drawio` để sửa, `.png` để chèn vào báo cáo). Tiến trình con là các chức năng trong bảng "Gom nhóm các chức năng" của khóa luận.

| STT | Sơ đồ | Tiến trình con | Kho dữ liệu | File |
|:---|:---|:---|:---|:---|
| 1 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 1.0: QUẢN LÝ TÀI KHOẢN | 1.1 Đăng ký tài khoản<br>1.2 Xác nhận email<br>1.3 Gửi lại email xác nhận<br>1.4 Đăng nhập<br>1.5 Đăng nhập trang quản lý<br>1.6 Đăng xuất<br>1.7 Cập nhật thông tin cá nhân<br>1.8 Đổi mật khẩu<br>1.9 Xem quyền hạn nhân viên | D1.1 Tài khoản | dfd-tien-trinh-01.drawio / .png |
| 2 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 2.0: TRA CỨU SẢN PHẨM | 2.1 Xem trang chủ<br>2.2 Xem danh sách sản phẩm<br>2.3 Lọc và sắp xếp sản phẩm<br>2.4 Lọc sản phẩm theo thông số kỹ thuật<br>2.5 Tìm kiếm sản phẩm<br>2.6 Xem chi tiết sản phẩm | D2.1 Danh mục<br>D2.2 Thương hiệu<br>D2.3 Sản phẩm, tồn kho | dfd-tien-trinh-02.drawio / .png |
| 3 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 3.0: GIỎ HÀNG | 3.1 Thêm sản phẩm vào giỏ hàng<br>3.2 Xem giỏ hàng<br>3.3 Cập nhật số lượng trong giỏ<br>3.4 Xóa sản phẩm khỏi giỏ | D3.1 Sản phẩm, tồn kho<br>D3.2 Giỏ hàng | dfd-tien-trinh-03.drawio / .png |
| 4 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 4.0: QUẢN LÝ ĐƠN HÀNG | 4.1 Đặt hàng<br>4.2 Xem lịch sử đơn hàng<br>4.3 Xem chi tiết đơn hàng (khách)<br>4.4 Hủy đơn hàng (khách) | D4.1 Sản phẩm, tồn kho<br>D4.2 Giỏ hàng<br>D4.3 Thông báo<br>D4.4 Đơn hàng | dfd-tien-trinh-04.drawio / .png |
| 5 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 5.0: XỬ LÝ ĐƠN HÀNG | 5.1 Xem danh sách đơn hàng<br>5.2 Tìm kiếm, lọc đơn hàng<br>5.3 Xem chi tiết đơn hàng (cửa hàng)<br>5.4 Cập nhật trạng thái đơn hàng<br>5.5 Hủy đơn hàng (cửa hàng) | D5.1 Đơn hàng<br>D5.2 Thông báo<br>D5.3 Sản phẩm, tồn kho | dfd-tien-trinh-05.drawio / .png |
| 6 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 6.0: BẢO HÀNH VÀ ĐỔI HÀNG LỖI | 6.1 Tạo yêu cầu bảo hành / đổi hàng lỗi<br>6.2 Xem danh sách và chi tiết yêu cầu bảo hành<br>6.3 Bổ sung bằng chứng<br>6.4 Khai báo thông tin gửi hàng<br>6.5 Hủy yêu cầu bảo hành<br>6.6 Xác nhận đã nhận sản phẩm | D6.1 Đơn hàng<br>D6.2 Chính sách bảo hành<br>D6.3 Yêu cầu bảo hành<br>D6.4 Thông báo | dfd-tien-trinh-06.drawio / .png |
| 7 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 7.0: XỬ LÝ BẢO HÀNH | 7.1 Xem danh sách yêu cầu bảo hành<br>7.2 Xem chi tiết yêu cầu và bằng chứng<br>7.3 Xử lý yêu cầu bảo hành (yêu cầu bổ sung, duyệt, từ chối, nhận hàng, gửi hãng, gửi trả, hoàn tất)<br>7.4 Ghi chú nội bộ<br>7.5 Gửi lời nhắn cho khách hàng | D7.1 Yêu cầu bảo hành<br>D7.2 Sản phẩm, tồn kho<br>D7.3 Thông báo | dfd-tien-trinh-07.drawio / .png |
| 8 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 8.0: CẤU HÌNH CHÍNH SÁCH BẢO HÀNH | 8.1 Cập nhật thời hạn đổi hàng lỗi theo thương hiệu<br>8.2 Cập nhật chính sách bảo hành theo thương hiệu và danh mục<br>8.3 Cập nhật loại lỗi | D8.1 Chính sách bảo hành | dfd-tien-trinh-08.drawio / .png |
| 9 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 9.0: QUẢN LÝ SẢN PHẨM | 9.1 Xem danh sách sản phẩm (quản trị)<br>9.2 Thêm sản phẩm<br>9.3 Sửa sản phẩm, ẩn/hiện sản phẩm<br>9.4 Xóa sản phẩm<br>9.5 Quản lý phiên bản sản phẩm (thêm, sửa, xóa)<br>9.6 Quản lý hình ảnh sản phẩm (tải lên, chọn ảnh chính, xóa)<br>9.7 Gán thông số kỹ thuật cho sản phẩm | D9.1 Danh mục<br>D9.2 Thương hiệu<br>D9.3 Sản phẩm, tồn kho | dfd-tien-trinh-09.drawio / .png |
| 10 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 10.0: QUẢN LÝ DANH MỤC VÀ THƯƠNG HIỆU | 10.1 Quản lý danh mục (xem, thêm, sửa, xóa)<br>10.2 Quản lý thương hiệu (xem, thêm, sửa, xóa) | D10.1 Danh mục<br>D10.2 Thương hiệu | dfd-tien-trinh-10.drawio / .png |
| 11 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 11.0: QUẢN LÝ KHO | 11.1 Nhập kho<br>11.2 Xem lịch sử nhập xuất kho | D11.1 Sản phẩm, tồn kho | dfd-tien-trinh-11.drawio / .png |
| 12 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 12.0: QUẢN LÝ KHÁCH HÀNG | 12.1 Xem danh sách khách hàng<br>12.2 Xem chi tiết khách hàng<br>12.3 Khóa / mở khóa tài khoản khách hàng<br>12.4 Cấp mật khẩu tạm cho khách hàng<br>12.5 Xóa tài khoản khách hàng | D12.1 Tài khoản<br>D12.2 Đơn hàng | dfd-tien-trinh-12.drawio / .png |
| 13 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 13.0: QUẢN LÝ NHÂN VIÊN | 13.1 Xem danh sách nhân viên<br>13.2 Thêm nhân viên<br>13.3 Sửa thông tin nhân viên<br>13.4 Khóa / mở khóa tài khoản nhân viên<br>13.5 Đặt lại mật khẩu nhân viên | D13.1 Tài khoản | dfd-tien-trinh-13.drawio / .png |
| 14 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 14.0: THÔNG BÁO | 14.1 Nhận thông báo<br>14.2 Xem danh sách thông báo<br>14.3 Đánh dấu thông báo đã đọc | D14.1 Thông báo | dfd-tien-trinh-14.drawio / .png |
| 15 | SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 15.0: BÁO CÁO THỐNG KÊ | 15.1 Xem tổng quan<br>15.2 Xem báo cáo doanh thu<br>15.3 Xuất báo cáo Excel | D15.1 Đơn hàng | dfd-tien-trinh-15.drawio / .png |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 1.0: QUẢN LÝ TÀI KHOẢN

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 1.1 Đăng ký tài khoản | CN01 | Khách vãng lai | Họ tên, email, mật khẩu | Kết quả đăng ký, email xác nhận | D1.1 Tài khoản – đọc, ghi: Kiểm tra email, lưu tài khoản mới |
| 1.2 Xác nhận email | CN02 | Khách vãng lai | Liên kết xác nhận | Kết quả xác nhận email | D1.1 Tài khoản – đọc, ghi: Ghi nhận email đã xác nhận |
| 1.3 Gửi lại email xác nhận | CN03 | Khách vãng lai | Email đã đăng ký | Email xác nhận mới | D1.1 Tài khoản – đọc: Đọc tài khoản chưa xác nhận |
| 1.4 Đăng nhập | CN04 | Khách hàng | Email, mật khẩu | Kết quả đăng nhập | D1.1 Tài khoản – đọc: Xác thực tài khoản |
| 1.5 Đăng nhập trang quản lý | CN05 | Nhân viên / Quản trị viên | Gmail hoặc số điện thoại, mật khẩu | Kết quả đăng nhập | D1.1 Tài khoản – đọc: Xác thực tài khoản, vai trò |
| 1.6 Đăng xuất | CN06 | Khách hàng; Nhân viên / Quản trị viên | Yêu cầu đăng xuất | Kết quả đăng xuất | – |
| 1.7 Cập nhật thông tin cá nhân | CN07 | Khách hàng; Nhân viên / Quản trị viên | Họ tên, số điện thoại | Thông tin cá nhân mới | D1.1 Tài khoản – đọc, ghi: Cập nhật thông tin cá nhân |
| 1.8 Đổi mật khẩu | CN08 | Khách hàng; Nhân viên / Quản trị viên | Mật khẩu cũ, mật khẩu mới | Kết quả đổi mật khẩu | D1.1 Tài khoản – đọc, ghi: Cập nhật mật khẩu |
| 1.9 Xem quyền hạn nhân viên | CN09 | Nhân viên | Yêu cầu xem quyền hạn | Danh sách quyền hạn | D1.1 Tài khoản – đọc: Đọc vai trò tài khoản |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 2.0: TRA CỨU SẢN PHẨM

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 2.1 Xem trang chủ | CN10 | Khách vãng lai / Khách hàng | Yêu cầu xem trang chủ | Sản phẩm nổi bật, danh mục, thương hiệu | D2.1 Danh mục – đọc: Đọc danh mục<br>D2.2 Thương hiệu – đọc: Đọc thương hiệu<br>D2.3 Sản phẩm, tồn kho – đọc: Đọc sản phẩm bán chạy, mới |
| 2.2 Xem danh sách sản phẩm | CN11 | Khách vãng lai / Khách hàng | Danh mục, thương hiệu cần xem | Danh sách sản phẩm | D2.1 Danh mục – đọc: Đọc danh mục<br>D2.2 Thương hiệu – đọc: Đọc thương hiệu<br>D2.3 Sản phẩm, tồn kho – đọc: Đọc sản phẩm đang bán |
| 2.3 Lọc và sắp xếp sản phẩm | CN12 | Khách vãng lai / Khách hàng | Khoảng giá, tiêu chí sắp xếp | Danh sách đã lọc, sắp xếp | D2.3 Sản phẩm, tồn kho – đọc: Đọc sản phẩm theo bộ lọc |
| 2.4 Lọc sản phẩm theo thông số kỹ thuật | CN13 | Khách vãng lai / Khách hàng | Thông số kỹ thuật cần lọc | Sản phẩm phù hợp | D2.3 Sản phẩm, tồn kho – đọc: Đọc thông số kỹ thuật |
| 2.5 Tìm kiếm sản phẩm | CN14 | Khách vãng lai / Khách hàng | Từ khóa | Kết quả tìm kiếm | D2.1 Danh mục – đọc: Tìm theo tên danh mục<br>D2.2 Thương hiệu – đọc: Tìm theo tên thương hiệu<br>D2.3 Sản phẩm, tồn kho – đọc: Tìm theo tên, thông số |
| 2.6 Xem chi tiết sản phẩm | CN15 | Khách vãng lai / Khách hàng | Sản phẩm được chọn | Thông tin, phiên bản, tồn kho | D2.3 Sản phẩm, tồn kho – đọc: Đọc sản phẩm, phiên bản, ảnh |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 3.0: GIỎ HÀNG

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 3.1 Thêm sản phẩm vào giỏ hàng | CN16 | Khách hàng | Phiên bản, số lượng | Kết quả thêm vào giỏ | D3.1 Sản phẩm, tồn kho – đọc: Kiểm tra tồn kho<br>D3.2 Giỏ hàng – đọc, ghi: Lưu sản phẩm vào giỏ |
| 3.2 Xem giỏ hàng | CN17 | Khách hàng | Yêu cầu xem giỏ hàng | Giỏ hàng, tạm tính | D3.1 Sản phẩm, tồn kho – đọc: Đọc giá, tồn kho<br>D3.2 Giỏ hàng – đọc: Đọc giỏ hàng |
| 3.3 Cập nhật số lượng trong giỏ | CN18 | Khách hàng | Số lượng mới | Giỏ hàng đã cập nhật | D3.1 Sản phẩm, tồn kho – đọc: Kiểm tra tồn kho<br>D3.2 Giỏ hàng – đọc, ghi: Cập nhật số lượng |
| 3.4 Xóa sản phẩm khỏi giỏ | CN19 | Khách hàng | Sản phẩm cần xóa | Giỏ hàng đã cập nhật | D3.2 Giỏ hàng – đọc, ghi: Xóa dòng giỏ hàng |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 4.0: QUẢN LÝ ĐƠN HÀNG

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 4.1 Đặt hàng | CN20 | Khách hàng | Thông tin giao hàng | Mã đơn hàng, kết quả đặt hàng | D4.1 Sản phẩm, tồn kho – đọc, ghi: Kiểm tra, trừ tồn kho<br>D4.2 Giỏ hàng – đọc, ghi: Đọc và làm trống giỏ<br>D4.3 Thông báo – ghi: Báo đơn mới cho nhân viên<br>D4.4 Đơn hàng – ghi: Lưu đơn hàng mới |
| 4.2 Xem lịch sử đơn hàng | CN21 | Khách hàng | Yêu cầu xem lịch sử | Danh sách đơn hàng | D4.4 Đơn hàng – đọc: Đọc đơn hàng của khách |
| 4.3 Xem chi tiết đơn hàng (khách) | CN22 | Khách hàng | Đơn hàng được chọn | Chi tiết, trạng thái đơn hàng | D4.4 Đơn hàng – đọc: Đọc chi tiết đơn hàng |
| 4.4 Hủy đơn hàng (khách) | CN23 | Khách hàng | Yêu cầu hủy đơn | Kết quả hủy đơn | D4.1 Sản phẩm, tồn kho – đọc, ghi: Hoàn lại tồn kho<br>D4.3 Thông báo – ghi: Báo đơn bị hủy<br>D4.4 Đơn hàng – đọc, ghi: Chuyển trạng thái đã hủy |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 5.0: XỬ LÝ ĐƠN HÀNG

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 5.1 Xem danh sách đơn hàng | CN24 | Nhân viên / Quản trị viên | Yêu cầu xem đơn hàng | Danh sách đơn hàng | D5.1 Đơn hàng – đọc: Đọc danh sách đơn hàng |
| 5.2 Tìm kiếm, lọc đơn hàng | CN25 | Nhân viên / Quản trị viên | Mã đơn, trạng thái, thời gian | Đơn hàng phù hợp | D5.1 Đơn hàng – đọc: Đọc đơn hàng phù hợp |
| 5.3 Xem chi tiết đơn hàng (cửa hàng) | CN26 | Nhân viên / Quản trị viên | Đơn hàng được chọn | Chi tiết đơn hàng | D5.1 Đơn hàng – đọc: Đọc chi tiết đơn hàng |
| 5.4 Cập nhật trạng thái đơn hàng | CN27 | Nhân viên / Quản trị viên | Trạng thái mới | Kết quả cập nhật | D5.1 Đơn hàng – đọc, ghi: Lưu trạng thái mới<br>D5.2 Thông báo – ghi: Báo trạng thái cho khách |
| 5.5 Hủy đơn hàng (cửa hàng) | CN28 | Nhân viên / Quản trị viên | Lý do hủy | Kết quả hủy đơn | D5.1 Đơn hàng – đọc, ghi: Lưu trạng thái hủy, lý do<br>D5.2 Thông báo – ghi: Báo đơn bị hủy cho khách<br>D5.3 Sản phẩm, tồn kho – đọc, ghi: Hoàn lại tồn kho |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 6.0: BẢO HÀNH VÀ ĐỔI HÀNG LỖI

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 6.1 Tạo yêu cầu bảo hành / đổi hàng lỗi | CN29 | Khách hàng | Sản phẩm, loại lỗi, mô tả, video | Mã yêu cầu bảo hành | D6.1 Đơn hàng – đọc: Đọc đơn hàng đã giao<br>D6.2 Chính sách bảo hành – đọc: Đọc thời hạn, loại lỗi<br>D6.3 Yêu cầu bảo hành – ghi: Lưu yêu cầu, video<br>D6.4 Thông báo – ghi: Báo yêu cầu mới |
| 6.2 Xem danh sách và chi tiết yêu cầu bảo hành | CN30 | Khách hàng | Yêu cầu được chọn | Trạng thái, lịch sử xử lý | D6.3 Yêu cầu bảo hành – đọc: Đọc yêu cầu của khách |
| 6.3 Bổ sung bằng chứng | CN31 | Khách hàng | Tệp bằng chứng bổ sung | Kết quả bổ sung | D6.3 Yêu cầu bảo hành – đọc, ghi: Lưu bằng chứng bổ sung<br>D6.4 Thông báo – ghi: Báo có bằng chứng mới |
| 6.4 Khai báo thông tin gửi hàng | CN32 | Khách hàng | Đơn vị vận chuyển, mã vận đơn | Kết quả khai báo | D6.3 Yêu cầu bảo hành – đọc, ghi: Lưu thông tin gửi hàng<br>D6.4 Thông báo – ghi: Báo khách đã gửi hàng |
| 6.5 Hủy yêu cầu bảo hành | CN33 | Khách hàng | Yêu cầu hủy | Kết quả hủy yêu cầu | D6.3 Yêu cầu bảo hành – đọc, ghi: Chuyển trạng thái đã hủy<br>D6.4 Thông báo – ghi: Báo yêu cầu bị hủy |
| 6.6 Xác nhận đã nhận sản phẩm | CN34 | Khách hàng | Xác nhận đã nhận hàng | Yêu cầu đã hoàn tất | D6.3 Yêu cầu bảo hành – đọc, ghi: Chuyển trạng thái hoàn tất<br>D6.4 Thông báo – ghi: Báo khách đã nhận hàng |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 7.0: XỬ LÝ BẢO HÀNH

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 7.1 Xem danh sách yêu cầu bảo hành | CN35 | Nhân viên / Quản trị viên | Trạng thái, từ khóa | Danh sách yêu cầu | D7.1 Yêu cầu bảo hành – đọc: Đọc danh sách yêu cầu |
| 7.2 Xem chi tiết yêu cầu và bằng chứng | CN36 | Nhân viên / Quản trị viên | Yêu cầu được chọn | Chi tiết, video bằng chứng | D7.1 Yêu cầu bảo hành – đọc: Đọc yêu cầu, bằng chứng |
| 7.3 Xử lý yêu cầu bảo hành (yêu cầu bổ sung, duyệt, từ chối, nhận hàng, gửi hãng, gửi trả, hoàn tất) | CN37 | Nhân viên / Quản trị viên | Quyết định xử lý, ghi chú | Trạng thái mới của yêu cầu | D7.1 Yêu cầu bảo hành – đọc, ghi: Lưu trạng thái, lịch sử<br>D7.2 Sản phẩm, tồn kho – đọc, ghi: Trừ tồn kho hàng thay thế<br>D7.3 Thông báo – ghi: Báo kết quả cho khách |
| 7.4 Ghi chú nội bộ | CN38 | Nhân viên / Quản trị viên | Nội dung ghi chú | Ghi chú đã lưu | D7.1 Yêu cầu bảo hành – đọc, ghi: Lưu ghi chú nội bộ |
| 7.5 Gửi lời nhắn cho khách hàng | CN39 | Nhân viên / Quản trị viên | Nội dung lời nhắn | Lời nhắn đã gửi | D7.1 Yêu cầu bảo hành – đọc, ghi: Lưu lời nhắn<br>D7.3 Thông báo – ghi: Báo lời nhắn cho khách |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 8.0: CẤU HÌNH CHÍNH SÁCH BẢO HÀNH

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 8.1 Cập nhật thời hạn đổi hàng lỗi theo thương hiệu | CN40 | Quản trị viên | Số ngày đổi hàng lỗi | Thời hạn theo thương hiệu | D8.1 Chính sách bảo hành – đọc, ghi: Lưu thời hạn đổi hàng lỗi |
| 8.2 Cập nhật chính sách bảo hành theo thương hiệu và danh mục | CN41 | Quản trị viên | Thương hiệu, danh mục, thời hạn | Chính sách bảo hành | D8.1 Chính sách bảo hành – đọc, ghi: Lưu thời hạn bảo hành |
| 8.3 Cập nhật loại lỗi | CN42 | Quản trị viên | Mã, tên, phạm vi loại lỗi | Danh sách loại lỗi | D8.1 Chính sách bảo hành – đọc, ghi: Lưu loại lỗi |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 9.0: QUẢN LÝ SẢN PHẨM

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 9.1 Xem danh sách sản phẩm (quản trị) | CN43 | Quản trị viên | Từ khóa, bộ lọc | Danh sách sản phẩm | D9.1 Danh mục – đọc: Đọc danh mục<br>D9.2 Thương hiệu – đọc: Đọc thương hiệu<br>D9.3 Sản phẩm, tồn kho – đọc: Đọc danh sách sản phẩm |
| 9.2 Thêm sản phẩm | CN44 | Quản trị viên | Thông tin sản phẩm | Kết quả thêm sản phẩm | D9.1 Danh mục – đọc: Chọn danh mục<br>D9.2 Thương hiệu – đọc: Chọn thương hiệu<br>D9.3 Sản phẩm, tồn kho – ghi: Lưu sản phẩm mới |
| 9.3 Sửa sản phẩm, ẩn/hiện sản phẩm | CN45 | Quản trị viên | Thông tin sửa, trạng thái hiển thị | Kết quả cập nhật | D9.1 Danh mục – đọc: Chọn danh mục<br>D9.2 Thương hiệu – đọc: Chọn thương hiệu<br>D9.3 Sản phẩm, tồn kho – đọc, ghi: Cập nhật sản phẩm |
| 9.4 Xóa sản phẩm | CN46 | Quản trị viên | Sản phẩm cần xóa | Kết quả xóa | D9.3 Sản phẩm, tồn kho – đọc, ghi: Xóa sản phẩm |
| 9.5 Quản lý phiên bản sản phẩm (thêm, sửa, xóa) | CN47 | Quản trị viên | Thông tin phiên bản, giá | Danh sách phiên bản | D9.3 Sản phẩm, tồn kho – đọc, ghi: Lưu phiên bản |
| 9.6 Quản lý hình ảnh sản phẩm (tải lên, chọn ảnh chính, xóa) | CN48 | Quản trị viên | Tệp ảnh, ảnh chính | Danh sách hình ảnh | D9.3 Sản phẩm, tồn kho – đọc, ghi: Lưu hình ảnh |
| 9.7 Gán thông số kỹ thuật cho sản phẩm | CN49 | Quản trị viên | Giá trị thông số | Thông số đã gán | D9.3 Sản phẩm, tồn kho – đọc, ghi: Lưu giá trị thông số |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 10.0: QUẢN LÝ DANH MỤC VÀ THƯƠNG HIỆU

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 10.1 Quản lý danh mục (xem, thêm, sửa, xóa) | CN50 | Quản trị viên | Thông tin danh mục | Danh sách danh mục | D10.1 Danh mục – đọc, ghi: Lưu danh mục |
| 10.2 Quản lý thương hiệu (xem, thêm, sửa, xóa) | CN51 | Quản trị viên | Thông tin thương hiệu | Danh sách thương hiệu | D10.2 Thương hiệu – đọc, ghi: Lưu thương hiệu |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 11.0: QUẢN LÝ KHO

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 11.1 Nhập kho | CN52 | Quản trị viên | Phiên bản, số lượng nhập, ghi chú | Tồn kho mới | D11.1 Sản phẩm, tồn kho – đọc, ghi: Cộng tồn kho, ghi lịch sử |
| 11.2 Xem lịch sử nhập xuất kho | CN53 | Quản trị viên | Sản phẩm cần xem | Lịch sử nhập xuất kho | D11.1 Sản phẩm, tồn kho – đọc: Đọc lịch sử nhập xuất |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 12.0: QUẢN LÝ KHÁCH HÀNG

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 12.1 Xem danh sách khách hàng | CN54 | Quản trị viên | Từ khóa, trạng thái | Danh sách khách hàng | D12.1 Tài khoản – đọc: Đọc tài khoản khách hàng |
| 12.2 Xem chi tiết khách hàng | CN55 | Quản trị viên | Khách hàng được chọn | Thông tin, lịch sử mua hàng | D12.1 Tài khoản – đọc: Đọc thông tin khách hàng<br>D12.2 Đơn hàng – đọc: Đọc lịch sử mua hàng |
| 12.3 Khóa / mở khóa tài khoản khách hàng | CN56 | Quản trị viên | Lý do khóa | Trạng thái tài khoản | D12.1 Tài khoản – đọc, ghi: Lưu trạng thái khóa |
| 12.4 Cấp mật khẩu tạm cho khách hàng | CN57 | Quản trị viên | Khách hàng cần cấp | Mật khẩu tạm | D12.1 Tài khoản – đọc, ghi: Lưu mật khẩu tạm |
| 12.5 Xóa tài khoản khách hàng | CN58 | Quản trị viên | Số điện thoại xác nhận | Kết quả xóa | D12.1 Tài khoản – đọc, ghi: Xóa tài khoản<br>D12.2 Đơn hàng – đọc: Kiểm tra đơn hàng |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 13.0: QUẢN LÝ NHÂN VIÊN

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 13.1 Xem danh sách nhân viên | CN59 | Quản trị viên | Từ khóa, trạng thái | Danh sách nhân viên | D13.1 Tài khoản – đọc: Đọc tài khoản nhân viên |
| 13.2 Thêm nhân viên | CN60 | Quản trị viên | Họ tên, Gmail, số điện thoại | Kết quả thêm nhân viên | D13.1 Tài khoản – đọc, ghi: Lưu tài khoản nhân viên |
| 13.3 Sửa thông tin nhân viên | CN61 | Quản trị viên | Thông tin cần sửa | Kết quả cập nhật | D13.1 Tài khoản – đọc, ghi: Cập nhật thông tin |
| 13.4 Khóa / mở khóa tài khoản nhân viên | CN62 | Quản trị viên | Lý do khóa | Trạng thái tài khoản | D13.1 Tài khoản – đọc, ghi: Lưu trạng thái khóa |
| 13.5 Đặt lại mật khẩu nhân viên | CN63 | Quản trị viên | Nhân viên cần đặt lại | Kết quả đặt lại mật khẩu | D13.1 Tài khoản – đọc, ghi: Đặt mật khẩu về số điện thoại |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 14.0: THÔNG BÁO

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 14.1 Nhận thông báo | CN64 | Khách hàng / Nhân viên / Quản trị viên | – | Thông báo mới | D14.1 Thông báo – đọc: Đọc thông báo mới |
| 14.2 Xem danh sách thông báo | CN65 | Khách hàng / Nhân viên / Quản trị viên | Yêu cầu xem thông báo | Danh sách thông báo | D14.1 Thông báo – đọc: Đọc thông báo của người dùng |
| 14.3 Đánh dấu thông báo đã đọc | CN66 | Khách hàng / Nhân viên / Quản trị viên | Thông báo được chọn | Trạng thái đã đọc | D14.1 Thông báo – đọc, ghi: Lưu trạng thái đã đọc |

## SƠ ĐỒ LUỒNG DỮ LIỆU TIẾN TRÌNH 15.0: BÁO CÁO THỐNG KÊ

| Tiến trình con | Chức năng | Tác nhân | Dữ liệu vào | Dữ liệu ra | Kho dữ liệu (luồng) |
|:---|:---|:---|:---|:---|:---|
| 15.1 Xem tổng quan | CN67 | Quản trị viên | Yêu cầu xem tổng quan | Số liệu tổng quan | D15.1 Đơn hàng – đọc: Đọc số liệu đơn hàng |
| 15.2 Xem báo cáo doanh thu | CN68 | Quản trị viên | Khoảng thời gian | Báo cáo doanh thu | D15.1 Đơn hàng – đọc: Đọc đơn hàng hoàn tất |
| 15.3 Xuất báo cáo Excel | CN69 | Quản trị viên | Khoảng thời gian | Tệp Excel báo cáo | D15.1 Đơn hàng – đọc: Đọc đơn hàng hoàn tất |

Ghi chú: mã kho đánh số riêng trong từng sơ đồ (Dn.k – kho thứ k của sơ đồ tiến trình n). "đọc" – tiến trình lấy dữ liệu từ kho; "ghi" – tiến trình thêm/cập nhật dữ liệu vào kho. Kho "Sản phẩm, tồn kho" gồm sản phẩm, phiên bản, hình ảnh, thông số, số lượng tồn và lịch sử nhập xuất kho; kho "Chính sách bảo hành" gồm thời hạn bảo hành, thời hạn đổi hàng lỗi và danh sách loại lỗi; kho "Yêu cầu bảo hành" gồm yêu cầu, bằng chứng, lịch sử xử lý và lời nhắn.
