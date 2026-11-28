# Thiết kế Cơ sở dữ liệu

Xây dựng từ mã nguồn hiện có: các lớp thực thể trong `Data/Entities/` và cấu hình quan hệ trong `Data/PopPopDbContext.cs` (EF Core, SQL Server). Tên bảng vật lý ghi trong ngoặc ở cột Thực thể.

## Xác định các thực thể

| STT | Thực thể | Các thuộc tính chính |
|:---|:---|:---|
| 1 | Người dùng (AspNetUsers) | Mã người dùng, Tên đăng nhập (email hoặc số điện thoại), Email, Cờ đã xác nhận email, Số điện thoại, Mật khẩu đã băm, Họ và tên, Ảnh đại diện, Cờ bắt buộc đổi mật khẩu, Thời điểm khóa đến, Lý do khóa, Số lần đăng nhập sai, Thời điểm tạo. |
| 2 | Vai trò (AspNetRoles) | Mã vai trò, Tên vai trò (Admin – Quản trị viên, Staff – Nhân viên, Customer – Khách hàng), Mô tả. |
| 3 | Danh mục (Categories) | Mã danh mục, Tên danh mục (duy nhất), Mô tả, Trạng thái hoạt động, Thời điểm tạo, Thời điểm cập nhật. |
| 4 | Thương hiệu (Brands) | Mã thương hiệu, Tên thương hiệu (duy nhất), Mô tả, Số ngày đổi hàng lỗi khi nhận hàng, Trạng thái hoạt động, Thời điểm tạo, Thời điểm cập nhật. |
| 5 | Sản phẩm (Products) | Mã sản phẩm, Mã danh mục, Mã thương hiệu, Tên sản phẩm, Mô tả, Trạng thái hiển thị, Thời điểm tạo, Thời điểm cập nhật. |
| 6 | Phiên bản sản phẩm (ProductVariants) | Mã phiên bản, Mã sản phẩm, Mã SKU (duy nhất), Nhãn phiên bản (màu, size, trọng lượng…), Giá bán, Số lượng tồn kho, Trạng thái hoạt động. |
| 7 | Hình ảnh sản phẩm (ProductImages) | Mã hình ảnh, Mã sản phẩm, Đường dẫn ảnh, Cờ ảnh chính, Thứ tự hiển thị. |
| 8 | Thuộc tính thông số (SpecAttributes) | Mã thuộc tính, Mã danh mục, Tên thuộc tính (duy nhất trong danh mục), Phạm vi áp dụng (Sản phẩm/Phiên bản), Gợi ý, Thứ tự hiển thị. |
| 9 | Giá trị thông số (SpecOptions) | Mã giá trị, Mã thuộc tính, Giá trị (duy nhất trong thuộc tính), Gợi ý, Thứ tự hiển thị. |
| 10 | Thông số sản phẩm (ProductSpecValues) | Mã sản phẩm, Mã giá trị thông số (khóa chính ghép). |
| 11 | Thông số phiên bản (VariantSpecValues) | Mã phiên bản, Mã giá trị thông số (khóa chính ghép). |
| 12 | Giỏ hàng (Carts) | Mã giỏ hàng, Mã người dùng (duy nhất), Thời điểm tạo, Thời điểm cập nhật. |
| 13 | Chi tiết giỏ hàng (CartItems) | Mã dòng giỏ hàng, Mã giỏ hàng, Mã phiên bản, Số lượng, Đơn giá tại thời điểm thêm. |
| 14 | Đơn hàng (Orders) | Mã đơn hàng, Mã người đặt, Mã tra cứu đơn (PP-…, duy nhất), Trạng thái đơn (Chờ xác nhận, Đã xác nhận, Đang xử lý, Đang giao, Hoàn tất, Đã hủy), Phương thức thanh toán (COD), Trạng thái thanh toán (Chưa/Đã thanh toán), Thời điểm thanh toán, Thời điểm hoàn tồn kho, Tổng tiền, Phí vận chuyển, Ghi chú (ghi chú của khách khi đặt, lý do hủy), Tên người nhận, Số điện thoại người nhận, Địa chỉ giao hàng, Thời điểm tạo, Thời điểm cập nhật. |
| 15 | Chi tiết đơn hàng (OrderItems) | Mã dòng đơn hàng, Mã đơn hàng, Mã phiên bản, Tên sản phẩm lúc mua, Mã SKU lúc mua, Đơn giá, Số lượng, Thành tiền. |
| 16 | Giao dịch kho (InventoryTransactions) | Mã giao dịch, Mã phiên bản, Loại giao dịch (Nhập kho, Bán hàng, Điều chỉnh tăng, Điều chỉnh giảm, Xuất hàng thay thế bảo hành, Nhập lại hàng bảo hành, Hoàn kho do hủy đơn), Số lượng, Ghi chú, Thời điểm tạo. |
| 17 | Loại lỗi (DefectTypes) | Mã loại lỗi, Mã lỗi (duy nhất), Mã danh mục, Tên lỗi, Cách nhận biết, Hướng dẫn quay video minh chứng, Phạm vi xử lý (Bảo hành / Đổi hàng lỗi khi nhận / Không hỗ trợ), Nguồn tham khảo, Đường dẫn nguồn, Thứ tự hiển thị, Trạng thái hoạt động. |
| 18 | Chính sách bảo hành (WarrantyPolicies) | Mã chính sách, Mã thương hiệu, Mã danh mục (cặp thương hiệu – danh mục duy nhất), Số ngày bảo hành, Bộ phận được bảo hành, Ghi chú, Đường dẫn nguồn, Thời điểm cập nhật. |
| 19 | Yêu cầu bảo hành (WarrantyClaims) | Mã yêu cầu, Mã tra cứu (BH-…, duy nhất), Mã người gửi, Mã đơn hàng, Mã dòng đơn hàng, Mã loại lỗi, Loại yêu cầu (Đổi hàng lỗi khi nhận / Bảo hành), Trạng thái (Đã gửi, Cần bổ sung, Đã duyệt, Đã nhận hàng, Đã gửi hãng, Đang gửi trả khách, Hoàn tất, Từ chối, Đã hủy), Số lượng, Mô tả lỗi, Số serial, Hạn chót tiếp nhận, Hình thức giải quyết (Cửa hàng đổi mới / Hãng thay thế), Đơn vị vận chuyển và mã vận đơn khách gửi về, Thời điểm khách gửi, Đơn vị vận chuyển và mã vận đơn gửi trả khách, Thời điểm gửi trả, Ghi chú nội bộ, Thời điểm tạo, Thời điểm cập nhật, Thời điểm đóng. |
| 20 | Minh chứng yêu cầu bảo hành (WarrantyClaimMedia) | Mã minh chứng, Mã yêu cầu, Loại tệp (Video/Ảnh), Tên tệp lưu trữ, Tên tệp gốc, Kiểu nội dung, Dung lượng, Mã người tải lên, Cờ nhân viên tải lên, Thời điểm tải lên. |
| 21 | Lịch sử xử lý yêu cầu bảo hành (WarrantyClaimHistory) | Mã lịch sử, Mã yêu cầu, Trạng thái cũ, Trạng thái mới, Nội dung ghi chú / lời nhắn, Mã người thực hiện, Tên người thực hiện, Cờ do nhân viên thực hiện, Thời điểm tạo. |
| 22 | Thông báo (Notifications) | Mã thông báo, Mã người nhận, Tiêu đề, Nội dung, Đường dẫn liên quan, Cờ đã đọc, Thời điểm tạo, Thời điểm đọc. |

## Xác định mối liên kết giữa các thực thể

| STT | Thực thể 1 | Tên liên kết | Thực thể 2 | Bản số | Ý nghĩa nghiệp vụ |
|:---|:---|:---|:---|:---|:---|
| 1 | Người dùng | Đảm nhận | Vai trò | N - N | Mỗi tài khoản được gán vai trò Khách hàng, Nhân viên hoặc Quản trị viên; một vai trò có nhiều tài khoản (bảng trung gian AspNetUserRoles). |
| 2 | Danh mục | Phân loại | Sản phẩm | 1 - N | Mỗi sản phẩm thuộc đúng một danh mục; không xóa được danh mục còn sản phẩm. |
| 3 | Thương hiệu | Sản xuất | Sản phẩm | 1 - N | Mỗi sản phẩm thuộc đúng một thương hiệu; không xóa được thương hiệu còn sản phẩm. |
| 4 | Sản phẩm | Có | Phiên bản sản phẩm | 1 - N | Một sản phẩm có nhiều phiên bản (màu, size, trọng lượng), mỗi phiên bản có giá và tồn kho riêng. |
| 5 | Sản phẩm | Có | Hình ảnh sản phẩm | 1 - N | Một sản phẩm có nhiều ảnh, trong đó một ảnh được chọn làm ảnh chính. |
| 6 | Danh mục | Định nghĩa | Thuộc tính thông số | 1 - N | Mỗi danh mục có bộ thuộc tính thông số kỹ thuật riêng (ví dụ vợt: độ cứng, điểm cân bằng). |
| 7 | Thuộc tính thông số | Gồm | Giá trị thông số | 1 - N | Mỗi thuộc tính có danh sách giá trị được phép chọn. |
| 8 | Sản phẩm | Được gán | Giá trị thông số | N - N | Một sản phẩm được gán nhiều giá trị thông số; một giá trị dùng cho nhiều sản phẩm (bảng trung gian Thông số sản phẩm). |
| 9 | Phiên bản sản phẩm | Được gán | Giá trị thông số | N - N | Thông số riêng của từng phiên bản (ví dụ trọng lượng 3U/4U), dùng để lọc sản phẩm (bảng trung gian Thông số phiên bản). |
| 10 | Người dùng | Sở hữu | Giỏ hàng | 1 - 1 (0,1) | Mỗi khách hàng có tối đa một giỏ hàng; xóa tài khoản thì xóa giỏ hàng. |
| 11 | Giỏ hàng | Chứa | Chi tiết giỏ hàng | 1 - N | Giỏ hàng gồm nhiều dòng, mỗi dòng là một phiên bản với số lượng. |
| 12 | Phiên bản sản phẩm | Được thêm vào | Chi tiết giỏ hàng | 1 - N | Một phiên bản có thể nằm trong giỏ của nhiều khách hàng. |
| 13 | Người dùng | Đặt | Đơn hàng | 1 - N | Một khách hàng đặt nhiều đơn hàng; không xóa được tài khoản đã có đơn hàng. |
| 14 | Đơn hàng | Gồm | Chi tiết đơn hàng | 1 - N | Mỗi đơn hàng có nhiều dòng; dòng đơn lưu lại tên, SKU và đơn giá tại thời điểm mua. |
| 15 | Phiên bản sản phẩm | Được bán trong | Chi tiết đơn hàng | 1 - N | Một phiên bản được bán trong nhiều đơn hàng; không xóa được phiên bản đã phát sinh đơn. |
| 16 | Phiên bản sản phẩm | Ghi nhận | Giao dịch kho | 1 - N | Mọi lần nhập kho, bán hàng, hoàn kho do hủy đơn, điều chỉnh và xuất/nhập hàng bảo hành đều được ghi lại theo phiên bản. |
| 17 | Danh mục | Áp dụng | Loại lỗi | 1 - N | Mỗi loại lỗi thuộc một danh mục sản phẩm (lỗi của vợt khác lỗi của giày). |
| 18 | Thương hiệu | Áp dụng | Chính sách bảo hành | 1 - N | Mỗi thương hiệu có các chính sách bảo hành theo từng danh mục. |
| 19 | Danh mục | Áp dụng | Chính sách bảo hành | 1 - N | Mỗi cặp thương hiệu – danh mục có tối đa một chính sách bảo hành (thời hạn tính theo ngày). |
| 20 | Người dùng | Gửi | Yêu cầu bảo hành | 1 - N | Khách hàng gửi nhiều yêu cầu bảo hành / đổi hàng lỗi; không xóa được tài khoản đã có yêu cầu. |
| 21 | Đơn hàng | Phát sinh | Yêu cầu bảo hành | 1 - N | Yêu cầu chỉ được tạo từ đơn hàng đã hoàn tất (đã giao) của chính khách hàng đó. |
| 22 | Chi tiết đơn hàng | Được yêu cầu xử lý | Yêu cầu bảo hành | 1 - N | Mỗi yêu cầu gắn với đúng một sản phẩm (dòng đơn) trong đơn hàng; hạn chót tính từ ngày giao theo chính sách. |
| 23 | Loại lỗi | Được khai báo trong | Yêu cầu bảo hành | 1 - N | Khách chọn loại lỗi khi tạo yêu cầu; phạm vi của loại lỗi quyết định yêu cầu là bảo hành hay đổi hàng lỗi. |
| 24 | Yêu cầu bảo hành | Có | Minh chứng yêu cầu bảo hành | 1 - N | Mỗi yêu cầu có ít nhất một video minh chứng, có thể bổ sung thêm video/ảnh. |
| 25 | Người dùng | Tải lên | Minh chứng yêu cầu bảo hành | 1 - N | Khách hàng hoặc nhân viên tải tệp minh chứng (lưu mã người tải, không ràng buộc khóa ngoại). |
| 26 | Yêu cầu bảo hành | Ghi nhận | Lịch sử xử lý yêu cầu bảo hành | 1 - N | Mỗi lần đổi trạng thái hoặc gửi lời nhắn được ghi thành một dòng lịch sử. |
| 27 | Người dùng | Thực hiện | Lịch sử xử lý yêu cầu bảo hành | 1 - N | Khách hàng, nhân viên hoặc quản trị viên là người thực hiện bước xử lý (lưu mã và tên người thực hiện, không ràng buộc khóa ngoại). |
| 28 | Người dùng | Nhận | Thông báo | 1 - N | Mỗi người dùng nhận nhiều thông báo về đơn hàng, yêu cầu bảo hành; xóa tài khoản thì xóa thông báo. |

Ghi chú:
- "Không xóa được …" tương ứng ràng buộc `OnDelete(Restrict)`; "xóa … thì xóa …" tương ứng `OnDelete(Cascade)` trong `PopPopDbContext`.
- Các bảng phụ của ASP.NET Core Identity (AspNetUserClaims, AspNetUserLogins, AspNetUserTokens, AspNetRoleClaims) do thư viện tạo sẵn, không phải thực thể nghiệp vụ nên không liệt kê.
- Trạng thái và loại (đơn hàng, thanh toán, giao dịch kho, yêu cầu bảo hành…) được lưu dạng số nguyên theo các enum trong `Data/Entities/Enums/`.

## Sơ đồ ERD

Sơ đồ quan hệ thực thể mức vật lý của cơ sở dữ liệu PopPopShop (sinh từ model snapshot bằng `Docs/Diagrams/build-erd.ps1`, file gốc `Docs/erd.drawio`). Ký hiệu chân quạ: đầu hai gạch ở bảng cha (khóa chính), đầu chân quạ ở bảng con (khóa ngoại, quan hệ nhiều); đầu vòng tròn – gạch là quan hệ không hoặc một (Carts.UserId là duy nhất); đường nét đứt là liên kết logic không có khóa ngoại (UploadedByUserId, ActorUserId).

![Sơ đồ ERD](erd.png)
