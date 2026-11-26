# Yêu cầu chức năng và chức năng hệ thống – Website quản lý và bán hàng PôpPôp

Tác nhân: Khách vãng lai, Khách hàng, Nhân viên, Quản trị viên.

Trình tự: **Yêu cầu chức năng (FR)** → phân rã thành **chức năng (CN)** → **gom nhóm chức năng**.

---

## 1. Yêu cầu chức năng

**FR01:** Hệ thống cho phép người dùng đăng ký tài khoản bằng email, xác nhận tài khoản qua liên kết được gửi về hộp thư và yêu cầu gửi lại liên kết xác nhận khi cần.

**FR02:** Hệ thống cho phép khách hàng đăng nhập bằng email và mật khẩu, duy trì phiên làm việc an toàn và thực hiện đăng xuất khỏi thiết bị. Khi quên mật khẩu, người dùng nhập email để nhận liên kết xác nhận (dùng một lần, hiệu lực 24 giờ) rồi đặt mật khẩu mới.

**FR03:** Hệ thống cung cấp trang đăng nhập riêng cho nội bộ cửa hàng, cho phép nhân viên và quản trị viên đăng nhập bằng email; sau khi đăng nhập, nhân viên được chuyển tới trang nhân viên (xử lý đơn hàng, yêu cầu bảo hành), quản trị viên được chuyển tới trang quản trị cửa hàng (sản phẩm, danh mục, thương hiệu, khách hàng, nhân viên, chính sách bảo hành, báo cáo).

**FR04:** Hệ thống cho phép người dùng cập nhật thông tin cá nhân (họ tên, số điện thoại) và đổi mật khẩu; người dùng đang dùng mật khẩu tạm bắt buộc phải đổi mật khẩu trước khi tiếp tục sử dụng.

**FR05:** Hệ thống phân quyền truy cập theo vai trò (khách hàng, nhân viên, quản trị viên) và không cho tài khoản đã bị khóa đăng nhập.

**FR06:** Hệ thống cho phép người dùng xem trang chủ gồm sản phẩm nổi bật, sản phẩm mới, sản phẩm bán chạy, danh mục và thương hiệu.

**FR07:** Hệ thống cho phép người dùng xem danh sách sản phẩm, lọc theo danh mục, thương hiệu, khoảng giá và sắp xếp theo sản phẩm mới, giá hoặc tên.

**FR08:** Hệ thống cho phép người dùng lọc sản phẩm theo thông số kỹ thuật như trọng lượng, độ cứng, điểm cân bằng, kích cỡ.

**FR09:** Hệ thống cho phép người dùng tìm kiếm nhanh theo từ khóa và hiển thị gợi ý kết quả ngay khi nhập.

**FR10:** Hệ thống cho phép người dùng xem chi tiết sản phẩm gồm hình ảnh, mô tả, các phiên bản, giá bán, tình trạng còn hàng và thông số kỹ thuật.

**FR11:** Hệ thống cho phép khách hàng thêm sản phẩm vào giỏ hàng, thay đổi số lượng và xóa sản phẩm khỏi giỏ.

**FR12:** Hệ thống cho phép khách hàng đặt hàng với hình thức thanh toán khi nhận hàng (COD), nhập thông tin giao hàng và tự động tính phí vận chuyển.

**FR13:** Hệ thống cho phép khách hàng xem lịch sử đơn hàng và theo dõi trạng thái từng đơn.

**FR14:** Hệ thống cho phép khách hàng hủy đơn hàng khi đơn chưa được cửa hàng xác nhận.

**FR15:** Hệ thống cho phép nhân viên và quản trị viên xem, tìm kiếm, lọc đơn hàng và cập nhật trạng thái đơn theo quy trình: xác nhận, xử lý, giao hàng, hoàn tất.

**FR16:** Hệ thống cho phép nhân viên và quản trị viên hủy đơn hàng chưa giao kèm lý do và hoàn lại sản phẩm vào kho.

**FR17:** Hệ thống cho phép khách hàng gửi yêu cầu đổi hàng lỗi hoặc bảo hành cho sản phẩm đã mua còn trong thời hạn, kèm mô tả lỗi và video bằng chứng.

**FR18:** Hệ thống cho phép khách hàng theo dõi tiến trình yêu cầu, bổ sung bằng chứng, khai báo thông tin gửi hàng, hủy yêu cầu và xác nhận đã nhận sản phẩm.

**FR19:** Hệ thống cho phép nhân viên và quản trị viên tiếp nhận và xử lý yêu cầu bảo hành: xem bằng chứng, yêu cầu bổ sung, duyệt hoặc từ chối, nhận hàng, gửi hãng, gửi sản phẩm thay thế và hoàn tất yêu cầu.

**FR20:** Hệ thống cho phép nhân viên và quản trị viên ghi chú nội bộ và gửi lời nhắn cho khách hàng trên từng yêu cầu bảo hành.

**FR21:** Hệ thống cho phép quản trị viên cấu hình chính sách bảo hành: thời hạn đổi hàng lỗi theo thương hiệu, thời hạn bảo hành theo thương hiệu và danh mục, danh sách loại lỗi được chấp nhận.

**FR22:** Hệ thống cho phép quản trị viên quản lý sản phẩm, phiên bản, hình ảnh và thông số kỹ thuật, đồng thời ẩn hoặc hiện sản phẩm trên cửa hàng.

**FR23:** Hệ thống cho phép quản trị viên quản lý danh mục và thương hiệu sản phẩm.

**FR24:** Hệ thống cho phép quản trị viên nhập kho và xem lịch sử nhập xuất kho (xuất kho khi bán/đổi bảo hành và hoàn kho khi hủy đơn được ghi tự động).

**FR25:** Hệ thống cho phép quản trị viên quản lý khách hàng: xem thông tin và lịch sử mua hàng, khóa hoặc mở khóa, cấp mật khẩu tạm và xóa tài khoản chưa phát sinh giao dịch.

**FR26:** Hệ thống cho phép quản trị viên quản lý tài khoản nhân viên: tạo mới, cập nhật thông tin, khóa hoặc mở khóa và đặt lại mật khẩu.

**FR27:** Hệ thống cho phép người dùng nhận thông báo tức thời về đơn hàng và yêu cầu bảo hành, xem danh sách thông báo và đánh dấu đã đọc.

**FR28:** Hệ thống cho phép quản trị viên xem tổng quan, thống kê doanh thu theo khoảng thời gian và xuất báo cáo ra tệp Excel.

---

## 2. Danh sách chức năng suy ra từ yêu cầu

Mỗi yêu cầu được phân rã thành các chức năng, mỗi chức năng là **một hành động cụ thể do một tác nhân thực hiện** và có thể kiểm tra được trên giao diện.

Viết tắt tác nhân: VL – Khách vãng lai, KH – Khách hàng, NV – Nhân viên, QTV – Quản trị viên.

| Mã | Chức năng | Tác nhân | Suy ra từ |
|:---|:---|:---|:---|
| CN01 | Đăng ký tài khoản | VL | FR01 |
| CN02 | Xác nhận email | VL | FR01 |
| CN03 | Gửi lại email xác nhận | VL | FR01 |
| CN04 | Đăng nhập | KH | FR02 |
| CN05 | Đăng nhập trang quản lý | NV, QTV | FR03 |
| CN06 | Đăng xuất | KH, NV, QTV | FR02 |
| CN07 | Cập nhật thông tin cá nhân | KH, NV, QTV | FR04 |
| CN08 | Đổi mật khẩu | KH, NV, QTV | FR04 |
| CN09 | Xem quyền hạn nhân viên | NV | FR05 |
| CN10 | Xem trang chủ | VL, KH | FR06 |
| CN11 | Xem danh sách sản phẩm | VL, KH | FR07 |
| CN12 | Lọc và sắp xếp sản phẩm | VL, KH | FR07 |
| CN13 | Lọc sản phẩm theo thông số kỹ thuật | VL, KH | FR08 |
| CN14 | Tìm kiếm sản phẩm | VL, KH | FR09 |
| CN15 | Xem chi tiết sản phẩm | VL, KH | FR10 |
| CN16 | Thêm sản phẩm vào giỏ hàng | KH | FR11 |
| CN17 | Xem giỏ hàng | KH | FR11 |
| CN18 | Cập nhật số lượng trong giỏ | KH | FR11 |
| CN19 | Xóa sản phẩm khỏi giỏ | KH | FR11 |
| CN20 | Đặt hàng | KH | FR12 |
| CN21 | Xem lịch sử đơn hàng | KH | FR13 |
| CN22 | Xem chi tiết đơn hàng | KH | FR13 |
| CN23 | Hủy đơn hàng | KH | FR14 |
| CN24 | Xem danh sách đơn hàng | NV, QTV | FR15 |
| CN25 | Tìm kiếm, lọc đơn hàng | NV, QTV | FR15 |
| CN26 | Xem chi tiết đơn hàng | NV, QTV | FR15 |
| CN27 | Cập nhật trạng thái đơn hàng | NV, QTV | FR15 |
| CN28 | Hủy đơn hàng | NV, QTV | FR16 |
| CN29 | Tạo yêu cầu bảo hành / đổi hàng lỗi | KH | FR17 |
| CN30 | Xem danh sách và chi tiết yêu cầu bảo hành | KH | FR18 |
| CN31 | Bổ sung bằng chứng | KH | FR18 |
| CN32 | Khai báo thông tin gửi hàng | KH | FR18 |
| CN33 | Hủy yêu cầu bảo hành | KH | FR18 |
| CN34 | Xác nhận đã nhận sản phẩm | KH | FR18 |
| CN35 | Xem danh sách yêu cầu bảo hành | NV, QTV | FR19 |
| CN36 | Xem chi tiết yêu cầu và bằng chứng | NV, QTV | FR19 |
| CN37 | Xử lý yêu cầu bảo hành | NV, QTV | FR19 |
| CN38 | Ghi chú nội bộ | NV, QTV | FR20 |
| CN39 | Gửi lời nhắn cho khách hàng | NV, QTV | FR20 |
| CN40 | Cập nhật thời hạn đổi hàng lỗi theo thương hiệu | QTV | FR21 |
| CN41 | Cập nhật chính sách bảo hành theo thương hiệu và danh mục | QTV | FR21 |
| CN42 | Cập nhật loại lỗi | QTV | FR21 |
| CN43 | Xem danh sách sản phẩm (quản trị) | QTV | FR22 |
| CN44 | Thêm sản phẩm | QTV | FR22 |
| CN45 | Sửa sản phẩm, ẩn/hiện sản phẩm | QTV | FR22 |
| CN46 | Xóa sản phẩm | QTV | FR22 |
| CN47 | Quản lý phiên bản sản phẩm | QTV | FR22 |
| CN48 | Quản lý hình ảnh sản phẩm | QTV | FR22 |
| CN49 | Gán thông số kỹ thuật cho sản phẩm | QTV | FR22 |
| CN50 | Quản lý danh mục | QTV | FR23 |
| CN51 | Quản lý thương hiệu | QTV | FR23 |
| CN52 | Nhập kho | QTV | FR24 |
| CN53 | Xem lịch sử nhập xuất kho | QTV | FR24 |
| CN54 | Xem danh sách khách hàng | QTV | FR25 |
| CN55 | Xem chi tiết khách hàng | QTV | FR25 |
| CN56 | Khóa / mở khóa tài khoản khách hàng | QTV | FR25 |
| CN57 | Cấp mật khẩu tạm cho khách hàng | QTV | FR25 |
| CN58 | Xóa tài khoản khách hàng | QTV | FR25 |
| CN59 | Xem danh sách nhân viên | QTV | FR26 |
| CN60 | Thêm nhân viên | QTV | FR26 |
| CN61 | Sửa thông tin nhân viên | QTV | FR26 |
| CN62 | Khóa / mở khóa tài khoản nhân viên | QTV | FR26 |
| CN63 | Đặt lại mật khẩu nhân viên | QTV | FR26 |
| CN64 | Nhận thông báo | KH, NV, QTV | FR27 |
| CN65 | Xem danh sách thông báo | KH, NV, QTV | FR27 |
| CN66 | Đánh dấu thông báo đã đọc | KH, NV, QTV | FR27 |
| CN67 | Xem tổng quan | QTV | FR28 |
| CN68 | Xem báo cáo doanh thu | QTV | FR28 |
| CN69 | Xuất báo cáo Excel | QTV | FR28 |

Ghi chú:

- Phần "phân quyền theo vai trò" và "chặn tài khoản bị khóa" của FR05 là **ràng buộc** áp dụng cho mọi chức năng, không phải một hành động riêng; việc khóa tài khoản được thực hiện qua CN56 và CN62.
- "Bắt buộc đổi mật khẩu tạm" (FR04) là ràng buộc của CN08; "tính phí vận chuyển" (FR12) là một bước bên trong CN20.

---

## 3. Gom nhóm chức năng

| STT | Nhóm chức năng | Mã | Tên chức năng | Tác nhân |
|:---|:---|:---|:---|:---|
| 1 | Quản lý tài khoản | CN01 | Đăng ký tài khoản | VL |
| | | CN02 | Xác nhận email | VL |
| | | CN03 | Gửi lại email xác nhận | VL |
| | | CN04 | Đăng nhập | KH |
| | | CN05 | Đăng nhập trang quản lý | NV, QTV |
| | | CN06 | Đăng xuất | KH, NV, QTV |
| | | CN07 | Cập nhật thông tin cá nhân | KH, NV, QTV |
| | | CN08 | Đổi mật khẩu | KH, NV, QTV |
| | | CN09 | Xem quyền hạn nhân viên | NV |
| 2 | Tra cứu sản phẩm | CN10 | Xem trang chủ | VL, KH |
| | | CN11 | Xem danh sách sản phẩm | VL, KH |
| | | CN12 | Lọc và sắp xếp sản phẩm | VL, KH |
| | | CN13 | Lọc sản phẩm theo thông số kỹ thuật | VL, KH |
| | | CN14 | Tìm kiếm sản phẩm | VL, KH |
| | | CN15 | Xem chi tiết sản phẩm | VL, KH |
| 3 | Quản lý giỏ hàng | CN16 | Thêm sản phẩm vào giỏ hàng | KH |
| | | CN17 | Xem giỏ hàng | KH |
| | | CN18 | Cập nhật số lượng trong giỏ | KH |
| | | CN19 | Xóa sản phẩm khỏi giỏ | KH |
| 4 | Đặt hàng và theo dõi đơn hàng | CN20 | Đặt hàng | KH |
| | | CN21 | Xem lịch sử đơn hàng | KH |
| | | CN22 | Xem chi tiết đơn hàng | KH |
| | | CN23 | Hủy đơn hàng | KH |
| 5 | Xử lý đơn hàng | CN24 | Xem danh sách đơn hàng | NV, QTV |
| | | CN25 | Tìm kiếm, lọc đơn hàng | NV, QTV |
| | | CN26 | Xem chi tiết đơn hàng | NV, QTV |
| | | CN27 | Cập nhật trạng thái đơn hàng | NV, QTV |
| | | CN28 | Hủy đơn hàng | NV, QTV |
| 6 | Gửi và theo dõi yêu cầu bảo hành | CN29 | Tạo yêu cầu bảo hành / đổi hàng lỗi | KH |
| | | CN30 | Xem danh sách và chi tiết yêu cầu bảo hành | KH |
| | | CN31 | Bổ sung bằng chứng | KH |
| | | CN32 | Khai báo thông tin gửi hàng | KH |
| | | CN33 | Hủy yêu cầu bảo hành | KH |
| | | CN34 | Xác nhận đã nhận sản phẩm | KH |
| 7 | Xử lý bảo hành | CN35 | Xem danh sách yêu cầu bảo hành | NV, QTV |
| | | CN36 | Xem chi tiết yêu cầu và bằng chứng | NV, QTV |
| | | CN37 | Xử lý yêu cầu bảo hành | NV, QTV |
| | | CN38 | Ghi chú nội bộ | NV, QTV |
| | | CN39 | Gửi lời nhắn cho khách hàng | NV, QTV |
| 8 | Cấu hình chính sách bảo hành | CN40 | Cập nhật thời hạn đổi hàng lỗi theo thương hiệu | QTV |
| | | CN41 | Cập nhật chính sách bảo hành theo thương hiệu và danh mục | QTV |
| | | CN42 | Cập nhật loại lỗi | QTV |
| 9 | Quản lý sản phẩm | CN43 | Xem danh sách sản phẩm (quản trị) | QTV |
| | | CN44 | Thêm sản phẩm | QTV |
| | | CN45 | Sửa sản phẩm, ẩn/hiện sản phẩm | QTV |
| | | CN46 | Xóa sản phẩm | QTV |
| | | CN47 | Quản lý phiên bản sản phẩm | QTV |
| | | CN48 | Quản lý hình ảnh sản phẩm | QTV |
| | | CN49 | Gán thông số kỹ thuật cho sản phẩm | QTV |
| 10 | Quản lý danh mục và thương hiệu | CN50 | Quản lý danh mục | QTV |
| | | CN51 | Quản lý thương hiệu | QTV |
| 11 | Quản lý kho | CN52 | Nhập kho | QTV |
| | | CN53 | Xem lịch sử nhập xuất kho | QTV |
| 12 | Quản lý khách hàng | CN54 | Xem danh sách khách hàng | QTV |
| | | CN55 | Xem chi tiết khách hàng | QTV |
| | | CN56 | Khóa / mở khóa tài khoản khách hàng | QTV |
| | | CN57 | Cấp mật khẩu tạm cho khách hàng | QTV |
| | | CN58 | Xóa tài khoản khách hàng | QTV |
| 13 | Quản lý nhân viên | CN59 | Xem danh sách nhân viên | QTV |
| | | CN60 | Thêm nhân viên | QTV |
| | | CN61 | Sửa thông tin nhân viên | QTV |
| | | CN62 | Khóa / mở khóa tài khoản nhân viên | QTV |
| | | CN63 | Đặt lại mật khẩu nhân viên | QTV |
| 14 | Quản lý thông báo | CN64 | Nhận thông báo | KH, NV, QTV |
| | | CN65 | Xem danh sách thông báo | KH, NV, QTV |
| | | CN66 | Đánh dấu thông báo đã đọc | KH, NV, QTV |
| 15 | Thống kê báo cáo | CN67 | Xem tổng quan | QTV |
| | | CN68 | Xem báo cáo doanh thu | QTV |
| | | CN69 | Xuất báo cáo Excel | QTV |

Tổng cộng: 15 nhóm chức năng, 69 chức năng.

---

## 4. Ma trận rà soát yêu cầu (FR × CN)

Cột là yêu cầu chức năng, hàng là chức năng. Dấu **X** cho biết chức năng đó đáp ứng (một phần) yêu cầu ở cột tương ứng. Hàng cuối đếm số chức năng đáp ứng mỗi yêu cầu: mọi cột đều phải ≥ 1, nghĩa là không có yêu cầu nào bị bỏ sót.

Ngoài cột "Suy ra từ" ở mục 2, ma trận đánh dấu thêm các chức năng thực hiện ràng buộc của FR05 (phân quyền, chặn tài khoản bị khóa): CN04, CN05 (kiểm tra vai trò và trạng thái khóa khi đăng nhập), CN56, CN62 (khóa tài khoản).

| CN \ FR | FR01 | FR02 | FR03 | FR04 | FR05 | FR06 | FR07 | FR08 | FR09 | FR10 | FR11 | FR12 | FR13 | FR14 | FR15 | FR16 | FR17 | FR18 | FR19 | FR20 | FR21 | FR22 | FR23 | FR24 | FR25 | FR26 | FR27 | FR28 |
|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|:---|
| CN01 Đăng ký tài khoản | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN02 Xác nhận email | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN03 Gửi lại email xác nhận | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN04 Đăng nhập |   | X |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN05 Đăng nhập trang quản lý |   |   | X |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN06 Đăng xuất |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN07 Cập nhật thông tin cá nhân |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN08 Đổi mật khẩu |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN09 Xem quyền hạn nhân viên |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN10 Xem trang chủ |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN11 Xem danh sách sản phẩm |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN12 Lọc và sắp xếp sản phẩm |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN13 Lọc sản phẩm theo thông số kỹ thuật |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN14 Tìm kiếm sản phẩm |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN15 Xem chi tiết sản phẩm |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN16 Thêm sản phẩm vào giỏ hàng |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN17 Xem giỏ hàng |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN18 Cập nhật số lượng trong giỏ |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN19 Xóa sản phẩm khỏi giỏ |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN20 Đặt hàng |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN21 Xem lịch sử đơn hàng |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN22 Xem chi tiết đơn hàng |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN23 Hủy đơn hàng |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN24 Xem danh sách đơn hàng |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN25 Tìm kiếm, lọc đơn hàng |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN26 Xem chi tiết đơn hàng |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN27 Cập nhật trạng thái đơn hàng |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |
| CN28 Hủy đơn hàng |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |
| CN29 Tạo yêu cầu bảo hành / đổi hàng lỗi |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |
| CN30 Xem danh sách và chi tiết yêu cầu bảo hành |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |
| CN31 Bổ sung bằng chứng |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |
| CN32 Khai báo thông tin gửi hàng |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |
| CN33 Hủy yêu cầu bảo hành |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |
| CN34 Xác nhận đã nhận sản phẩm |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |
| CN35 Xem danh sách yêu cầu bảo hành |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |
| CN36 Xem chi tiết yêu cầu và bằng chứng |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |
| CN37 Xử lý yêu cầu bảo hành |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |   |
| CN38 Ghi chú nội bộ |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |
| CN39 Gửi lời nhắn cho khách hàng |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |   |
| CN40 Cập nhật thời hạn đổi hàng lỗi theo thương hiệu |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |
| CN41 Cập nhật chính sách bảo hành theo thương hiệu và danh mục |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |
| CN42 Cập nhật loại lỗi |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |   |
| CN43 Xem danh sách sản phẩm (quản trị) |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |
| CN44 Thêm sản phẩm |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |
| CN45 Sửa sản phẩm, ẩn/hiện sản phẩm |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |
| CN46 Xóa sản phẩm |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |
| CN47 Quản lý phiên bản sản phẩm |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |
| CN48 Quản lý hình ảnh sản phẩm |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |
| CN49 Gán thông số kỹ thuật cho sản phẩm |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |   |
| CN50 Quản lý danh mục |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |
| CN51 Quản lý thương hiệu |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |   |
| CN52 Nhập kho |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |
| CN53 Xem lịch sử nhập xuất kho |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |   |
| CN54 Xem danh sách khách hàng |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |
| CN55 Xem chi tiết khách hàng |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |
| CN56 Khóa / mở khóa tài khoản khách hàng |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |
| CN57 Cấp mật khẩu tạm cho khách hàng |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |
| CN58 Xóa tài khoản khách hàng |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |   |
| CN59 Xem danh sách nhân viên |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |
| CN60 Thêm nhân viên |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |
| CN61 Sửa thông tin nhân viên |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |
| CN62 Khóa / mở khóa tài khoản nhân viên |   |   |   |   | X |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |
| CN63 Đặt lại mật khẩu nhân viên |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |   |
| CN64 Nhận thông báo |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |
| CN65 Xem danh sách thông báo |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |
| CN66 Đánh dấu thông báo đã đọc |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |   |
| CN67 Xem tổng quan |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |
| CN68 Xem báo cáo doanh thu |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |
| CN69 Xuất báo cáo Excel |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   | X |
| **Tổng** | **3** | **2** | **1** | **2** | **5** | **1** | **2** | **1** | **1** | **1** | **4** | **1** | **2** | **1** | **4** | **1** | **1** | **5** | **3** | **2** | **3** | **7** | **2** | **2** | **5** | **5** | **3** | **3** |
