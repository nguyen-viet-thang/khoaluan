# Kế hoạch kiểm thử hệ thống (System Testing) – Website PôpPôp

## 1. Mục tiêu và phạm vi

**Mục tiêu:** xác nhận website quản lý và bán hàng cầu lông PôpPôp đáp ứng yêu cầu chức năng của 3 nhóm người dùng khi chạy như một hệ thống hoàn chỉnh (giao diện → controller → service → CSDL → thông báo SignalR → email).

| Đối tượng | Vai trò trong hệ thống | Khu vực |
|---|---|---|
| Chủ cửa hàng | `Admin` | `/admin/login`, `/Admin/*` (và toàn bộ `/Staff/*`) |
| Nhân viên | `Staff` | `/admin/login`, `/Staff/*` |
| Khách hàng | `Customer` (và khách vãng lai chưa đăng nhập) | Storefront, `/Cart`, `/Orders`, `/Claims`, `/Notifications`, `/Identity/Account/*` |

**Trong phạm vi:** chức năng, phân quyền, luồng nghiệp vụ liên vai trò (đặt hàng → xử lý → giao → bảo hành), thông báo, báo cáo/xuất Excel, giao diện trên desktop và mobile.

**Ngoài phạm vi:** kiểm thử đơn vị, kiểm thử tải quy mô lớn, kiểm thử bảo mật chuyên sâu (pentest) – để dành cho giai đoạn bảo mật cuối.

## 2. Môi trường và dữ liệu kiểm thử

| Hạng mục | Giá trị |
|---|---|
| Ứng dụng | ASP.NET Core 10 MVC, build `Release`, chạy cổng riêng (ví dụ `http://localhost:5299`) |
| CSDL | SQL Server LocalDB `PopPopShop`, đã chạy đủ migration và seed |
| Trình duyệt | Chrome (bản mới nhất), Edge; mobile giả lập 375×812 |
| Email | `Smtp:Host` để trống → link xác nhận hiển thị trên trang (Development) |
| Dữ liệu mẫu | Catalog 9 danh mục / 4 thương hiệu / 46 sản phẩm; `Docs\Seed-DeliveredOrders.ps1`, `Docs\Seed-WarrantyClaims.ps1` |

**Tài khoản kiểm thử** (mật khẩu lấy theo file seed, không ghi trong tài liệu này):

| Vai trò | Tài khoản | Ghi chú |
|---|---|---|
| Admin | `0900000000` | Tài khoản chủ cửa hàng seed |
| Staff | `0911222301`, `0911222302` | Đăng nhập bằng số điện thoại |
| Customer | `0901234501` … `0901234509` | Tài khoản đang hoạt động |
| Customer bị khóa | customer-010 | Dùng để kiểm tra đăng nhập bị chặn |
| Customer mới | Email tự tạo khi kiểm thử | Dùng cho luồng đăng ký + xác nhận email |

**Chuẩn bị trước mỗi vòng:** khôi phục CSDL về trạng thái seed (drop + `dotnet ef database update`), ghi lại tồn kho của 3–5 SKU sẽ dùng để kiểm tra trừ/hoàn kho.

## 3. Tiêu chí bắt đầu / kết thúc

- **Bắt đầu:** build Release thành công, migration chạy không lỗi, đăng nhập được bằng cả 3 vai trò.
- **Kết thúc:** 100% test case ưu tiên **Cao** đã chạy, không còn lỗi mức **Nghiêm trọng/Cao** đang mở; ≥ 95% tổng số test case đạt.

**Mức độ lỗi:** Nghiêm trọng (sai dữ liệu đơn/tồn kho/tiền, lộ quyền) · Cao (chức năng chính không dùng được) · Trung bình (sai thông báo, sai hiển thị có cách khắc phục) · Thấp (giao diện, chính tả).

---

## 4. Test case – Khách hàng (Customer)

### 4.1. Tài khoản

| ID | Tình huống | Các bước chính | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| KH-01 | Đăng ký bằng email hợp lệ | Nhập họ tên, email, mật khẩu, xác nhận mật khẩu → Đăng ký | Chuyển sang trang RegisterConfirmation; tài khoản có role Customer, chưa đăng nhập được khi chưa xác nhận | Cao |
| KH-02 | Đăng ký email đã tồn tại | Đăng ký lại email ở KH-01 | Báo lỗi email đã được dùng, không tạo tài khoản mới | Cao |
| KH-03 | Đăng ký dữ liệu sai | Bỏ trống họ tên / email sai định dạng / mật khẩu yếu / xác nhận không khớp | Hiện lỗi kiểm tra ở từng trường, không gửi form | TB |
| KH-04 | Xác nhận email | Mở link xác nhận | Báo xác nhận thành công, không tự đăng nhập; sau đó đăng nhập được | Cao |
| KH-05 | Đăng nhập khi chưa xác nhận | Đăng nhập đúng mật khẩu trước bước KH-04 | Không vào được; hệ thống gửi lại link xác nhận | Cao |
| KH-06 | Gửi lại email xác nhận | Trang ResendEmailConfirmation | Gửi link mới, thông báo chung chung (không lộ email có tồn tại hay không) | TB |
| KH-07 | Đăng nhập bằng email / số điện thoại | Dùng tài khoản email mới và `0901234501` | Đăng nhập thành công, header hiện tên người dùng | Cao |
| KH-08 | Sai mật khẩu | Nhập sai mật khẩu | Báo đăng nhập không thành công, không nói rõ trường nào sai | Cao |
| KH-09 | Tài khoản bị khóa | Đăng nhập customer-010 | Chuyển trang Lockout / báo tài khoản bị khóa | Cao |
| KH-10 | Cập nhật hồ sơ | Manage → đổi họ tên, số điện thoại | Lưu thành công; số điện thoại sai định dạng bị từ chối | TB |
| KH-11 | Đổi mật khẩu | Manage → ChangePassword | Đổi thành công; đăng nhập lại bằng mật khẩu mới | Cao |
| KH-12 | Bắt buộc đổi mật khẩu tạm | Admin cấp mật khẩu tạm (CH-25) → khách đăng nhập | Mọi trang đều chuyển về ChangePassword cho đến khi đổi xong | Cao |
| KH-13 | Đăng xuất | Bấm Đăng xuất | Về trang chủ, truy cập `/Orders` bị chuyển về trang đăng nhập | TB |

### 4.2. Xem và tìm kiếm sản phẩm (kể cả khách vãng lai)

| ID | Tình huống | Các bước chính | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| KH-20 | Trang chủ | Mở `/` | Hiện banner, danh mục, thương hiệu, sản phẩm mới; không có dữ liệu giả | TB |
| KH-21 | Danh sách sản phẩm + lọc | `/Products` lọc theo danh mục, thương hiệu, khoảng giá | Chỉ hiện sản phẩm khớp; số lượng facet đúng | Cao |
| KH-22 | Lọc thông số | Chọn 2 giá trị cùng thuộc tính (OR) và 2 thuộc tính khác nhau (AND) | Kết quả đúng quy tắc OR trong thuộc tính, AND giữa các thuộc tính; lọc theo biến thể phải khớp cùng một SKU còn bán | Cao |
| KH-23 | Tìm kiếm từ khóa | Ô tìm kiếm header: tên sản phẩm, tên thương hiệu, giá trị thông số | Kết quả gợi ý trực tiếp hiển thị đúng; từ khóa không có kết quả báo rỗng | Cao |
| KH-24 | Chi tiết sản phẩm | Mở một sản phẩm | Hiện ảnh, giá, các phiên bản, tồn kho, thông số, chính sách bảo hành | Cao |
| KH-25 | Sản phẩm bị ẩn | Admin ẩn sản phẩm / danh mục / thương hiệu (CH-06) | Sản phẩm biến mất khỏi danh sách, tìm kiếm; mở trực tiếp URL không mua được | Cao |
| KH-26 | Khách vãng lai thêm giỏ | Chưa đăng nhập → Thêm vào giỏ | Chuyển sang trang đăng nhập, sau đăng nhập quay lại đúng trang | TB |

### 4.3. Giỏ hàng và đặt hàng

| ID | Tình huống | Các bước chính | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| KH-30 | Thêm vào giỏ | Chọn phiên bản, số lượng 1 → Thêm | Giỏ có sản phẩm, badge giỏ tăng | Cao |
| KH-31 | Vượt tồn kho | Thêm số lượng lớn hơn tồn kho | Báo "Số lượng yêu cầu vượt quá tồn kho", giỏ không đổi | Cao |
| KH-32 | Cập nhật / xóa khỏi giỏ | Đổi số lượng, xóa dòng | Tổng tiền tính lại đúng | Cao |
| KH-33 | Phí vận chuyển | Thanh toán đơn < 500.000đ và ≥ 500.000đ | Phí 30.000đ ở đơn nhỏ, miễn phí ở đơn lớn | Cao |
| KH-34 | Đặt hàng COD thành công | Checkout → nhập người nhận, SĐT, địa chỉ → Đặt hàng | Tạo đơn mã `PP-yyMMdd-xxxxxx`, trạng thái Chờ xác nhận, chưa thanh toán; giỏ trống; tồn kho giảm đúng | Cao |
| KH-35 | Thiếu thông tin giao hàng | Bỏ trống/ sai SĐT, địa chỉ | Báo lỗi, không tạo đơn | Cao |
| KH-36 | Hết hàng lúc đặt | Mở 2 tab, tab 2 mua hết hàng, tab 1 bấm Đặt hàng | Báo "không đủ số lượng tồn kho", không tạo đơn, tồn kho không âm | Cao |
| KH-37 | Sản phẩm bị ẩn trong giỏ | Có trong giỏ rồi admin ẩn → Checkout | Bị chặn thanh toán kèm thông báo | TB |
| KH-38 | Lịch sử đơn hàng | `/Orders`, mở chi tiết | Chỉ thấy đơn của mình, trạng thái và tổng tiền đúng | Cao |
| KH-39 | Hủy đơn chờ xác nhận | Đơn Pending → Hủy, nhập lý do | Đơn Đã hủy, tồn kho được hoàn lại; nhân viên nhận thông báo | Cao |
| KH-40 | Không hủy được đơn đã xác nhận | Đơn Confirmed trở đi | Không có nút hủy; gửi POST trực tiếp bị từ chối | Cao |
| KH-41 | Xem đơn người khác | Đổi `id` trên URL `/Orders/Details/{id}` | 404 / không có quyền, không lộ dữ liệu | Cao |

### 4.4. Bảo hành / đổi hàng lỗi

| ID | Tình huống | Các bước chính | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| KH-50 | Bắt đầu yêu cầu | `/Claims/Start` | Liệt kê đơn đã giao của khách, mỗi dòng có hạn đổi lỗi / hạn bảo hành; tìm theo mã đơn, tên sản phẩm được | Cao |
| KH-51 | Tạo yêu cầu hợp lệ | Chọn sản phẩm → loại lỗi → mô tả → tải ≥ 1 video | Tạo mã `BH-…`, trạng thái Đã gửi; nhân viên nhận thông báo | Cao |
| KH-52 | Thiếu video | Chỉ tải ảnh hoặc không tải file | Báo bắt buộc có ít nhất 1 video | Cao |
| KH-53 | File giả mạo | Đổi đuôi file `.exe` thành `.mp4` | Bị từ chối (kiểm tra magic byte) | Cao |
| KH-54 | Quá hạn | Sản phẩm quá hạn đổi lỗi/bảo hành, hoặc thương hiệu có hạn 0 ngày | Không cho chọn loại yêu cầu đã hết hạn | Cao |
| KH-55 | Bổ sung bằng chứng | Khi trạng thái Cần bổ sung → thêm ghi chú/file | Lưu thành công, nhân viên được báo | TB |
| KH-56 | Khai báo gửi hàng | Trạng thái Đã duyệt → nhập hãng vận chuyển + mã vận đơn hoặc "Trực tiếp tại cửa hàng" | Lưu thông tin gửi hàng | TB |
| KH-57 | Hủy yêu cầu | Hủy trước khi gửi hàng; thử hủy sau khi gửi | Trước: hủy được; sau: không có nút / bị từ chối | TB |
| KH-58 | Xác nhận đã nhận hàng | Trạng thái Đang gửi trả → "Đã nhận hàng" | Yêu cầu Hoàn tất | TB |
| KH-59 | Xem video yêu cầu người khác | Đổi id trong URL `/Claims/Media/{id}` | Bị từ chối | Cao |

### 4.5. Thông báo

| ID | Tình huống | Các bước chính | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| KH-60 | Nhận thông báo thời gian thực | Đang mở web, nhân viên đổi trạng thái đơn | Chuông cập nhật số chưa đọc ngay, không cần tải lại trang | Cao |
| KH-61 | Mở / đánh dấu đã đọc | Bấm thông báo; bấm "Đánh dấu tất cả đã đọc" | Chuyển đúng trang đơn/yêu cầu; số chưa đọc về 0 | TB |
| KH-62 | Khách vãng lai bấm chuông | Chưa đăng nhập | Chuyển sang trang đăng nhập | Thấp |

---

## 5. Test case – Nhân viên (Staff)

| ID | Tình huống | Các bước chính | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| NV-01 | Đăng nhập bằng SĐT | `/admin/login` với `0911222301` | Vào `/Staff/Dashboard` | Cao |
| NV-02 | Đăng nhập bằng Gmail | Nhân viên được admin tạo bằng Gmail (CH-21) | Đăng nhập được; lần đầu bắt buộc đổi mật khẩu | Cao |
| NV-03 | Nhân viên bị khóa | Admin khóa (CH-24) → đăng nhập; khóa khi đang đăng nhập | Không đăng nhập được; phiên đang mở bị đăng xuất trong ~1 phút | Cao |
| NV-04 | Khách hàng vào trang nhân viên | Đăng nhập customer → `/Staff/Orders` | AccessDenied | Cao |
| NV-05 | Nhân viên vào trang Admin | `/Admin/Products`, `/Admin/Reports`, `/Admin/StaffAccounts` | AccessDenied cho mọi trang Admin | Cao |
| NV-06 | Trang quyền hạn | `/Staff/Dashboard/Permissions` | Danh sách quyền khớp thực tế (NV-04, NV-05) | Thấp |
| NV-10 | Danh sách đơn | `/Staff/Orders` lọc theo trạng thái, tìm theo mã đơn / SĐT | Kết quả đúng; menu hiện số đơn chờ xử lý | Cao |
| NV-11 | Chuyển trạng thái đơn đúng thứ tự | Chờ xác nhận → Đã xác nhận → Đang xử lý → Đang giao → Hoàn tất | Mỗi bước lưu đúng; khách nhận thông báo; khi Hoàn tất thanh toán = Đã thanh toán | Cao |
| NV-12 | Chuyển trạng thái sai thứ tự | Gửi POST nhảy cóc (Pending → Completed) hoặc lùi trạng thái | Bị từ chối, trạng thái không đổi | Cao |
| NV-13 | Hủy đơn | Hủy đơn ở trạng thái ≤ Đang giao, có lý do | Đơn Đã hủy, hoàn tồn kho, khách nhận thông báo | Cao |
| NV-14 | Hủy đơn thiếu lý do / đơn đã hoàn tất | Bỏ trống lý do; hủy đơn Completed | Bị từ chối | Cao |
| NV-15 | Nhận thông báo đơn mới | Khách đặt đơn (KH-34) khi nhân viên đang mở web | Chuông nhân viên tăng ngay, mở được đơn | Cao |
| NV-20 | Danh sách yêu cầu bảo hành | `/Staff/Claims` lọc trạng thái, loại, tìm mã | Kết quả đúng; menu hiện số yêu cầu chờ | Cao |
| NV-21 | Xem bằng chứng | Mở chi tiết, phát video, tua video | Video phát và tua được | Cao |
| NV-22 | Yêu cầu bổ sung | Chọn Cần bổ sung + lời nhắn | Khách nhận thông báo, có thể bổ sung (KH-55) | Cao |
| NV-23 | Duyệt yêu cầu | Duyệt (chưa cấu hình địa chỉ cửa hàng → bắt buộc ghi chú) | Thiếu ghi chú bị từ chối; có ghi chú thì duyệt thành công | Cao |
| NV-24 | Nhận hàng → gửi hãng → gửi trả | Đã duyệt → Đã nhận → Gửi hãng → Đang gửi trả | Trạng thái và lịch sử đúng; khi Đang gửi trả tồn kho trừ đúng 1 sản phẩm | Cao |
| NV-25 | Đổi hàng khi hết hàng | Đổi sản phẩm khi tồn kho = 0 | Báo "Không đủ tồn kho để đổi", không đổi trạng thái | Cao |
| NV-26 | Từ chối yêu cầu | Từ chối ở trạng thái Đã gửi; từ chối ở trạng thái Đã nhận | Đã gửi: từ chối được; Đã nhận: bắt buộc nhập thông tin gửi trả hàng | Cao |
| NV-27 | Gửi lời nhắn / ghi chú nội bộ | SendMessage, SaveNote | Lời nhắn xuất hiện trong lịch sử phía khách; ghi chú nội bộ khách không thấy | TB |
| NV-28 | Hoàn tất thay khách | Đang gửi trả → Hoàn tất | Yêu cầu Hoàn tất | TB |
| NV-30 | Trang thông báo nhân viên | `/Staff/Notifications` | Có thông báo đơn mới, yêu cầu mới, bổ sung bằng chứng, khách hủy | TB |

---

## 6. Test case – Chủ cửa hàng (Admin)

> Admin có toàn bộ quyền của nhân viên: chạy lại NV-10 → NV-30 bằng tài khoản Admin (ưu tiên TB).

### 6.1. Đăng nhập và tổng quan

| ID | Tình huống | Các bước chính | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| CH-01 | Đăng nhập Admin | `/admin/login` | Vào `/Admin/Dashboard` | Cao |
| CH-02 | Customer dùng trang `/admin/login` | Đăng nhập bằng tài khoản customer | Không vào được khu quản trị | Cao |
| CH-03 | Dashboard | Mở Dashboard sau khi có đơn hoàn tất | Doanh thu, số đơn khớp với Báo cáo cùng kỳ | TB |

### 6.2. Sản phẩm, danh mục, thương hiệu, thông số

| ID | Tình huống | Các bước chính | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| CH-05 | Tạo sản phẩm | Tạo sản phẩm → thêm phiên bản (SKU, giá, tồn kho) → tải ảnh, chọn ảnh chính | Sản phẩm chỉ hiện trên cửa hàng sau khi có phiên bản; ảnh chính hiện ở thẻ sản phẩm | Cao |
| CH-06 | Ẩn/hiện | Ẩn sản phẩm; ẩn danh mục; ẩn thương hiệu | Admin thấy "Đang ẩn" kèm lý do; cửa hàng không bán (KH-25) | Cao |
| CH-07 | Dữ liệu sai | Giá âm, SKU trùng, tên trống, ảnh không phải file ảnh | Báo lỗi từng trường, không lưu | Cao |
| CH-08 | Sửa sản phẩm / phiên bản | Đổi giá, tên | Cửa hàng hiển thị giá mới; đơn cũ giữ giá cũ | Cao |
| CH-09 | Xóa sản phẩm / phiên bản / ảnh | Xóa sản phẩm đã có đơn và chưa có đơn | Không làm hỏng đơn cũ (bị chặn hoặc chỉ ẩn); sản phẩm chưa có đơn xóa được | Cao |
| CH-10 | Nhập kho | Nhập số lượng kèm ghi chú; hủy một đơn để hoàn kho | Tồn kho tăng đúng, có dòng lịch sử "Nhập hàng" / "Hoàn kho do hủy đơn" | Cao |
| CH-11 | Thông số sản phẩm | Gán thông số ở trang Edit, đổi danh mục | Lọc thông số ở cửa hàng ra sản phẩm; đổi danh mục thì thông số cũ bị bỏ | TB |
| CH-12 | Danh mục / thương hiệu | Thêm, sửa, tìm, lọc trạng thái, xóa danh mục đang có sản phẩm | Xóa danh mục có sản phẩm bị chặn kèm thông báo | Cao |

### 6.3. Đơn hàng

| ID | Tình huống | Các bước chính | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| CH-15 | Quản lý đơn | `/Admin/Orders` — giống NV-10 → NV-14 | Kết quả giống quyền nhân viên | Cao |

### 6.4. Khách hàng và nhân viên

| ID | Tình huống | Các bước chính | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| CH-20 | Danh sách khách hàng | Tìm theo tên/SĐT/email, lọc trạng thái, xem chi tiết | Không lẫn tài khoản Admin/Staff; chi tiết có đơn hàng của khách | TB |
| CH-21 | Tạo nhân viên | Họ tên + Gmail + SĐT; thử email không phải Gmail, SĐT trùng | Tạo được; mật khẩu ban đầu = SĐT, bắt buộc đổi; email khác Gmail và SĐT trùng bị từ chối | Cao |
| CH-22 | Sửa nhân viên | Đổi tên, SĐT; thêm Gmail cho nhân viên seed | Nhân viên đăng nhập bằng thông tin mới | TB |
| CH-23 | Khóa / mở khóa khách hàng | Khóa kèm lý do → khách đăng nhập; mở khóa | Khóa: bị chặn, phiên đang mở bị đăng xuất; mở khóa: đăng nhập lại được | Cao |
| CH-24 | Khóa / mở khóa nhân viên | Như CH-23 với nhân viên | Như NV-03 | Cao |
| CH-25 | Cấp mật khẩu tạm | Khách hàng / nhân viên → Tạo mật khẩu tạm | Mật khẩu hiện 1 lần; người dùng bị bắt đổi (KH-12) | Cao |
| CH-26 | Xóa khách hàng | Khách chưa có đơn: nhập lại đúng SĐT → xóa; khách đã có đơn | Chưa có đơn: xóa được (giỏ hàng, thông báo xóa theo); đã có đơn hoặc nhập sai SĐT: bị chặn, gợi ý khóa | Cao |

### 6.5. Chính sách bảo hành và báo cáo

| ID | Tình huống | Các bước chính | Kết quả mong đợi | Ưu tiên |
|---|---|---|---|---|
| CH-30 | Số ngày đổi lỗi theo thương hiệu | Đổi số ngày của 1 thương hiệu | Hạn đổi lỗi trên `/Claims/Start` cập nhật | TB |
| CH-31 | Chính sách bảo hành thương hiệu × danh mục | Sửa số ngày bảo hành; nhập số âm | Hạn bảo hành của khách cập nhật; số âm bị từ chối | TB |
| CH-32 | Loại lỗi | Sửa phạm vi áp dụng (Bảo hành / Đổi lỗi / Không áp dụng) | Danh sách loại lỗi phía khách thay đổi tương ứng | TB |
| CH-40 | Báo cáo doanh thu | Chọn 7 ngày / 30 ngày / tháng này / tháng trước / năm nay / khoảng tự chọn | Doanh thu = tổng đơn hoàn tất theo ngày thanh toán (giờ Việt Nam); khoảng > 62 ngày gộp theo tháng | Cao |
| CH-41 | Đơn hủy không tính doanh thu | Có đơn hủy trong kỳ | Không cộng vào doanh thu | Cao |
| CH-42 | Ngày không hợp lệ | Từ ngày > đến ngày | Báo lỗi hoặc tự đảo, không lỗi 500 | TB |
| CH-43 | Xuất Excel | Bấm xuất | Tải file `.xlsx` mở được bằng Excel, số liệu khớp màn hình | Cao |

---

## 7. Kịch bản liên vai trò (end-to-end)

| ID | Kịch bản | Vai trò tham gia | Kiểm tra cuối |
|---|---|---|---|
| E2E-01 | Khách đăng ký → xác nhận email → mua 2 sản phẩm → nhân viên xử lý đến Hoàn tất → Admin xem báo cáo | Khách, Nhân viên, Admin | Tồn kho giảm đúng; đơn Đã thanh toán; doanh thu tăng đúng tổng đơn (đã gồm phí ship) |
| E2E-02 | Khách đặt → tự hủy khi Chờ xác nhận | Khách, Nhân viên | Tồn kho hoàn lại; nhân viên nhận thông báo; không tính doanh thu |
| E2E-03 | Khách đặt → nhân viên hủy khi Đang giao | Khách, Nhân viên | Tồn kho hoàn lại; khách nhận thông báo kèm lý do |
| E2E-04 | Khách tạo yêu cầu đổi lỗi → nhân viên yêu cầu bổ sung → khách bổ sung → duyệt → khách gửi hàng → nhận → đổi hàng → khách xác nhận nhận hàng | Khách, Nhân viên | Mỗi bước đều có thông báo; tồn kho trừ 1 khi gửi trả; yêu cầu Hoàn tất |
| E2E-05 | Yêu cầu bảo hành gửi hãng → hãng đổi → gửi trả khách | Khách, Nhân viên | Kết quả "Hãng đổi mới"; lịch sử trạng thái đầy đủ |
| E2E-06 | Admin khóa khách đang đăng nhập giữa chừng thanh toán | Admin, Khách | Khách bị đăng xuất trong ~1 phút, không tạo được đơn |
| E2E-07 | Admin ẩn thương hiệu khi khách đang có sản phẩm đó trong giỏ | Admin, Khách | Khách không thanh toán được sản phẩm đó |
| E2E-08 | Admin tạo nhân viên mới → nhân viên đăng nhập lần đầu, đổi mật khẩu → xử lý 1 đơn | Admin, Nhân viên | Nhân viên mới làm việc được, không vào được trang Admin |

## 8. Kiểm thử phi chức năng (mức cơ bản)

| ID | Hạng mục | Cách kiểm tra | Kỳ vọng |
|---|---|---|---|
| PF-01 | Phân quyền URL | Với mỗi vai trò (vãng lai, Customer, Staff), gọi trực tiếp toàn bộ URL `/Admin/*`, `/Staff/*` | Vãng lai → trang đăng nhập; vai trò thiếu quyền → AccessDenied |
| PF-02 | Chống CSRF | Gửi POST thiếu antiforgery token (UpdateStatus, Cancel, Lock…) | Bị từ chối (400) |
| PF-03 | Giao diện mobile | Duyệt các luồng KH-30 → KH-39, KH-51 ở 375px | Không tràn ngang, nút bấm được |
| PF-04 | Trình duyệt | Chrome và Edge cho E2E-01 | Hoạt động như nhau |
| PF-05 | Thời gian phản hồi | Trang danh sách sản phẩm, báo cáo năm nay | Dưới 2 giây trên máy dev |
| PF-06 | Trang lỗi | Mở id không tồn tại, URL sai | Trang 404/lỗi thân thiện, không lộ stack trace (build Release) |

## 9. Lịch thực hiện đề xuất

| Vòng | Nội dung | Thời lượng |
|---|---|---|
| 1 | Chuẩn bị môi trường, dữ liệu, rà test case | 0,5 ngày |
| 2 | Khách hàng (mục 4) | 1 ngày |
| 3 | Nhân viên (mục 5) | 1 ngày |
| 4 | Chủ cửa hàng (mục 6) | 1 ngày |
| 5 | End-to-end + phi chức năng (mục 7, 8) | 1 ngày |
| 6 | Sửa lỗi, kiểm thử lại (re-test) và kiểm thử hồi quy các ca Cao | 1 ngày |

## 10. Mẫu ghi nhận kết quả

| ID | Ngày chạy | Người chạy | Kết quả (Đạt / Không đạt / Bị chặn) | Mã lỗi | Ghi chú / ảnh chụp |
|---|---|---|---|---|---|
| KH-01 | | | | | |

**Mẫu báo lỗi:** Mã lỗi · Test case · Mức độ · Các bước tái hiện · Kết quả thực tế · Kết quả mong đợi · Ảnh chụp / log · Trạng thái (Mới / Đang sửa / Đã sửa / Đóng).
