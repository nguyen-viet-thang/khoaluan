# Đặc tả quy trình nghiệp vụ – Website quản lý và bán hàng PôpPôp

Sơ đồ đi kèm: QT01 ở `Docs/Quy-trinh-nghiep-vu.drawio`; từ QT02 trở đi mỗi quy trình một file riêng `Docs/quytrinhN.drawio` (QT02 → `quytrinh2.drawio`, QT03 → `quytrinh3.drawio`, …). Ký hiệu: sơ đồ hoạt động UML có làn (swimlane), trắng đen; hành động là hình chữ nhật góc vuông, điểm rẽ nhánh là hình thoi có ghi câu hỏi bên trong, nhánh ra ghi "Có" / "Không", chấm đen là bắt đầu, chấm đen có viền là kết thúc; các đường luồng chỉ gấp góc vuông, không cắt qua ô hay đường khác.

---

## QT01: Quy trình đặt hàng

**Chức năng đáp ứng:** FR11, FR12 (CN16 – Thêm sản phẩm vào giỏ hàng; CN17 – Xem giỏ hàng; CN18 – Cập nhật số lượng trong giỏ; CN19 – Xóa sản phẩm khỏi giỏ; CN20 – Đặt hàng).

**Tác nhân:** Khách hàng.

**Mô tả:** Cho phép khách hàng chọn sản phẩm vào giỏ hàng, nhập thông tin giao hàng và đặt đơn hàng với hình thức thanh toán khi nhận hàng (COD). Hệ thống kiểm tra tồn kho, tạo đơn hàng ở trạng thái Chờ xác nhận và thông báo cho nhân viên cửa hàng.

**Tiền điều kiện:** Khách hàng đã đăng nhập; sản phẩm đang được bán (sản phẩm, danh mục và thương hiệu đều đang hoạt động) và còn hàng.

**Hậu điều kiện:** Đơn hàng mới được tạo ở trạng thái Chờ xác nhận, tồn kho được trừ và giỏ hàng được làm trống; nhân viên nhận thông báo để xử lý đơn hàng.

**Sơ đồ hoạt động:** trang "QT01. Quy trình đặt hàng" trong `Docs/Quy-trinh-nghiep-vu.drawio`. Trang này đã chứa sẵn năm mục trên (Chức năng đáp ứng, Tác nhân, Mô tả, Tiền điều kiện, Hậu điều kiện) ở phía trên, tiếp theo là thanh tiêu đề "SƠ ĐỒ HOẠT ĐỘNG: QUY TRÌNH ĐẶT HÀNG (QT01)" và sơ đồ.

**Mô tả sơ đồ:**

Sơ đồ QT01 gồm hai làn: làn **Khách hàng** chứa các thao tác khách thực hiện trên website, làn **Hệ thống** chứa các bước website tự kiểm tra và xử lý. Các ô xếp từ trên xuống theo đúng thứ tự bước trong luồng sự kiện chính; mỗi hình thoi là một điểm rẽ nhánh ghi câu hỏi kiểm tra ("Còn bán và đủ tồn kho?", "Giỏ hàng hợp lệ?", "Thông tin và tồn kho hợp lệ?"), nhánh "Có" đi tiếp xuống dưới, nhánh "Không" quay lại bước trước của khách hàng.

Quy trình bắt đầu khi khách hàng chọn phiên bản và số lượng của một sản phẩm rồi bấm "Thêm vào giỏ" (bước 1–2). Hệ thống kiểm tra sản phẩm còn được bán và số lượng không vượt tồn kho (bước 3); nếu không đạt, khách được báo lỗi và chọn lại, nếu đạt, sản phẩm được thêm vào giỏ và giỏ hàng được hiển thị (bước 4).

Tại giỏ hàng, khách có thể điều chỉnh số lượng hoặc xóa sản phẩm, sau đó bấm "Thanh toán" (bước 5–6). Hệ thống kiểm tra giỏ không trống và vẫn đủ tồn kho (bước 7); nếu không đạt, khách quay lại giỏ hàng, nếu đạt, hệ thống hiển thị trang thanh toán với tổng tiền, phí vận chuyển và thông tin giao hàng được điền sẵn (bước 8).

Khách nhập hoặc kiểm tra thông tin người nhận rồi bấm "Đặt hàng" (bước 9–10). Hệ thống kiểm tra lại thông tin giao hàng và tồn kho (bước 11); nếu không đạt, khách sửa thông tin tại trang thanh toán. Nếu đạt, hệ thống tạo đơn hàng ở trạng thái Chờ xác nhận với hình thức COD, trừ tồn kho và làm trống giỏ (bước 12), gửi thông báo đơn mới cho nhân viên (bước 13) và hiển thị chi tiết đơn hàng (bước 14). Quy trình kết thúc khi khách xem đơn hàng vừa đặt (bước 15).

**Luồng sự kiện chính:**

| Bước | Thực hiện | Hành động |
|---|---|---|
| 1 | Khách hàng | Xem chi tiết sản phẩm, chọn phiên bản và số lượng. |
| 2 | Khách hàng | Chọn "Thêm vào giỏ". |
| 3 | Hệ thống | Kiểm tra sản phẩm còn được bán và số lượng không vượt tồn kho. |
| 4 | Hệ thống | Thêm sản phẩm vào giỏ hàng và hiển thị giỏ hàng. |
| 5 | Khách hàng | Xem giỏ hàng; có thể cập nhật số lượng hoặc xóa sản phẩm. |
| 6 | Khách hàng | Chọn "Thanh toán". |
| 7 | Hệ thống | Kiểm tra giỏ hàng không trống và đủ tồn kho. |
| 8 | Hệ thống | Hiển thị trang thanh toán: danh sách sản phẩm, tạm tính, phí vận chuyển, tổng tiền; điền sẵn thông tin giao hàng từ đơn gần nhất hoặc từ hồ sơ tài khoản. |
| 9 | Khách hàng | Nhập hoặc kiểm tra tên người nhận, số điện thoại, địa chỉ giao hàng, ghi chú. |
| 10 | Khách hàng | Chọn "Đặt hàng". |
| 11 | Hệ thống | Kiểm tra thông tin giao hàng; kiểm tra lại từng sản phẩm còn được bán và đủ tồn kho. |
| 12 | Hệ thống | Tạo đơn hàng (mã đơn, trạng thái Chờ xác nhận, COD), lưu giá bán tại thời điểm đặt, trừ tồn kho, ghi lịch sử xuất kho, làm trống giỏ hàng. |
| 13 | Hệ thống | Gửi thông báo đơn hàng mới cho nhân viên và quản trị viên. |
| 14 | Hệ thống | Hiển thị chi tiết đơn hàng kèm thông báo đặt hàng thành công. |
| 15 | Khách hàng | Xem đơn hàng vừa đặt. Kết thúc quy trình. |

**Luồng thay thế và ngoại lệ:**

| Tại bước | Tình huống | Xử lý của hệ thống |
|---|---|---|
| 2 | Khách chưa đăng nhập | Chuyển tới trang đăng nhập; đăng nhập xong quay lại trang trước đó. |
| 3 | Số lượng vượt tồn kho hoặc sản phẩm ngừng bán | Báo "Số lượng yêu cầu vượt quá tồn kho." hoặc "Sản phẩm hiện không khả dụng."; giỏ hàng không đổi; quay lại bước 1. |
| 7 | Giỏ hàng trống hoặc tồn kho đã thay đổi | Báo "Giỏ hàng đang trống." hoặc "Tồn kho đã thay đổi. Vui lòng kiểm tra lại giỏ hàng trước khi đặt hàng."; quay lại bước 5. |
| 11 | Thông tin giao hàng không hợp lệ | Báo lỗi tại từng trường; quay lại bước 9. |
| 11 | Có sản phẩm vừa ngừng bán hoặc không đủ tồn kho (ví dụ khách khác vừa mua hết) | Báo "Một sản phẩm trong giỏ hiện không còn khả dụng." hoặc "Sản phẩm … không đủ số lượng tồn kho."; không tạo đơn, không trừ kho; quay lại bước 9. |

**Quy tắc nghiệp vụ:**
- Hình thức thanh toán duy nhất là thanh toán khi nhận hàng (COD).
- Phí vận chuyển 30.000đ; miễn phí cho đơn có tạm tính từ 500.000đ (cấu hình được).
- Thông tin giao hàng: tên người nhận 2–256 ký tự; số điện thoại di động Việt Nam hợp lệ; địa chỉ 10–500 ký tự (số nhà, đường, phường/xã, quận/huyện, tỉnh/thành phố).
- Đơn hàng lưu giá bán tại thời điểm đặt; thay đổi giá sau đó không ảnh hưởng đơn đã đặt.
- Kiểm tra tồn kho, tạo đơn, trừ kho và làm trống giỏ thực hiện trong cùng một giao dịch: lỗi ở bất kỳ bước nào thì không thay đổi dữ liệu.

---

## QT02: Quy trình xử lý đơn hàng

**Chức năng đáp ứng:** FR18, FR19 (CN24 – Xem danh sách đơn hàng; CN25 – Tìm kiếm, lọc đơn hàng; CN26 – Xem chi tiết đơn hàng (cửa hàng); CN27 – Cập nhật trạng thái đơn hàng; CN28 – Hủy đơn hàng (cửa hàng)).

**Tác nhân:** Nhân viên.

**Mô tả:** Cho phép nhân viên tìm đơn hàng cần xử lý và chuyển đơn lần lượt qua các trạng thái Đã xác nhận, Đang xử lý, Đang giao, Đã giao thành công, hoặc hủy đơn khi hàng chưa giao cho đơn vị vận chuyển. Hệ thống kiểm tra thao tác hợp lệ, hoàn lại tồn kho khi hủy, ghi nhận đã thanh toán COD khi giao thành công và thông báo cho khách hàng.

**Tiền điều kiện:** Nhân viên đã đăng nhập trang quản lý; có đơn hàng chưa kết thúc (Chờ xác nhận, Đã xác nhận, Đang xử lý hoặc Đang giao).

**Hậu điều kiện:** Đơn hàng được chuyển sang trạng thái mới (hủy đơn thì tồn kho được hoàn lại, giao thành công thì được ghi nhận đã thanh toán); khách hàng nhận thông báo về trạng thái đơn hàng.

**Sơ đồ hoạt động:** file `Docs/quytrinh2.drawio` (trang "QT02. Quy trình xử lý đơn hàng"). Trang này đã chứa sẵn năm mục trên (Chức năng đáp ứng, Tác nhân, Mô tả, Tiền điều kiện, Hậu điều kiện) ở phía trên, tiếp theo là thanh tiêu đề "SƠ ĐỒ HOẠT ĐỘNG: QUY TRÌNH XỬ LÝ ĐƠN HÀNG (QT02)" và sơ đồ.

**Mô tả sơ đồ:**

Sơ đồ QT02 gồm hai làn: làn **Nhân viên** chứa các thao tác nhân viên thực hiện trên trang quản lý đơn hàng, làn **Hệ thống** chứa các bước website tự kiểm tra và xử lý. Các ô xếp từ trên xuống theo đúng thứ tự bước trong luồng sự kiện chính. Hình thoi "Thao tác hợp lệ?" có nhánh "Có" đi tiếp xuống dưới, nhánh "Không" quay lại bước hiển thị chi tiết đơn hàng; hình thoi "Hủy đơn?" có nhánh "Có" đi qua bước hoàn tồn kho, nhánh "Không" bỏ qua bước này và đi thẳng tới bước cập nhật trạng thái.

Quy trình bắt đầu khi nhân viên mở danh sách đơn hàng, tìm kiếm hoặc lọc theo trạng thái rồi chọn một đơn cần xử lý (bước 1–2). Hệ thống hiển thị chi tiết đơn cùng các thao tác được phép ở trạng thái hiện tại (bước 3). Nhân viên chọn thao tác tiếp theo, nếu là hủy đơn thì nhập lý do, rồi chọn "Lưu trạng thái" (bước 4).

Hệ thống kiểm tra thao tác có hợp lệ với trạng thái hiện tại của đơn hay không (bước 5); nếu không hợp lệ (ví dụ khách vừa tự hủy đơn hoặc nhân viên khác vừa cập nhật), hệ thống báo lỗi và hiển thị lại chi tiết đơn. Nếu hợp lệ và thao tác là hủy đơn, hệ thống hoàn lại tồn kho, ghi lịch sử nhập xuất kho và lưu lý do hủy (bước 6). Sau đó hệ thống cập nhật trạng thái đơn, nếu đơn được giao thành công thì ghi nhận đã thanh toán COD (bước 7), gửi thông báo cho khách hàng (bước 8) và đưa nhân viên về danh sách đơn hàng kèm thông báo cập nhật thành công (bước 9). Quy trình kết thúc khi nhân viên thấy đơn ở trạng thái mới (bước 10). Mỗi lần chuyển trạng thái là một lượt của quy trình; đơn được xử lý lặp lại như vậy cho tới khi ở trạng thái Đã giao thành công hoặc Đã hủy.

**Luồng sự kiện chính:**

| Bước | Thực hiện | Hành động |
|---|---|---|
| 1 | Nhân viên | Mở trang quản lý đơn hàng, xem danh sách đơn; tìm kiếm theo mã đơn, tên hoặc số điện thoại người nhận, tên hoặc tài khoản khách hàng; lọc theo trạng thái. |
| 2 | Nhân viên | Chọn đơn hàng cần xử lý. |
| 3 | Hệ thống | Hiển thị chi tiết đơn: trạng thái, thanh toán COD, khách hàng, thông tin giao hàng, ghi chú, danh sách sản phẩm, phí giao hàng, tổng tiền và các thao tác được phép theo trạng thái hiện tại. |
| 4 | Nhân viên | Chọn thao tác tiếp theo ("Xác nhận đơn", "Chuẩn bị hàng (đang xử lý)", "Giao cho đơn vị vận chuyển", "Đã giao thành công (đã thu tiền COD)" hoặc "Hủy đơn và hoàn kho"); nếu hủy đơn thì nhập lý do hủy và xác nhận; chọn "Lưu trạng thái". |
| 5 | Hệ thống | Kiểm tra thao tác đã chọn có hợp lệ với trạng thái hiện tại của đơn. |
| 6 | Hệ thống | Nếu thao tác là hủy đơn: cộng lại tồn kho từng phiên bản theo số lượng trong đơn, ghi lịch sử nhập xuất kho, lưu lý do hủy vào ghi chú đơn. |
| 7 | Hệ thống | Cập nhật trạng thái đơn; nếu chuyển sang Đã giao thành công thì ghi nhận thanh toán COD đã thanh toán và thời điểm thanh toán. |
| 8 | Hệ thống | Gửi thông báo trạng thái mới của đơn hàng cho khách hàng (kèm lý do nếu đơn bị hủy). |
| 9 | Hệ thống | Hiển thị lại danh sách đơn hàng kèm thông báo "Đã chuyển đơn … sang …". |
| 10 | Nhân viên | Xem đơn hàng ở trạng thái mới. Kết thúc quy trình. |

**Luồng thay thế và ngoại lệ:**

| Tại bước | Tình huống | Xử lý của hệ thống |
|---|---|---|
| 1 | Không có đơn phù hợp với từ khóa hoặc trạng thái đã lọc | Hiển thị danh sách trống; nhân viên đổi điều kiện tìm kiếm. |
| 3 | Đơn ở trạng thái Đã giao thành công hoặc Đã hủy | Chỉ hiển thị chi tiết, không có thao tác cập nhật; kết thúc quy trình. |
| 4 | Nhân viên chọn hủy đơn nhưng không đồng ý ở hộp xác nhận "Hủy đơn hàng … và hoàn lại tồn kho?" | Không gửi yêu cầu; đơn giữ nguyên trạng thái; quay lại bước 4. |
| 5 | Thao tác không còn hợp lệ (khách vừa tự hủy đơn, nhân viên khác vừa cập nhật, hoặc chọn hủy khi đơn đang giao) | Báo "Không thể chuyển đơn hàng sang trạng thái đã chọn."; không thay đổi dữ liệu; quay lại bước 3. |
| 5 | Đơn hàng không tồn tại | Báo không tìm thấy đơn hàng; kết thúc quy trình. |

**Quy tắc nghiệp vụ:**
- Trạng thái đơn chỉ đi một chiều, mỗi lần một bước: Chờ xác nhận → Đã xác nhận → Đang xử lý → Đang giao → Đã giao thành công.
- Cửa hàng được hủy đơn ở trạng thái Chờ xác nhận, Đã xác nhận hoặc Đang xử lý; đơn Đang giao không được hủy. Đơn Đã giao thành công hoặc Đã hủy không thay đổi được nữa.
- Lý do hủy không bắt buộc, tối đa 300 ký tự; được ghi vào ghi chú đơn cùng tên người hủy và gửi kèm thông báo cho khách.
- Hoàn tồn kho khi hủy chỉ thực hiện một lần cho mỗi đơn (không hoàn kho hai lần dù đơn bị hủy lại).
- Hình thức thanh toán là COD: đơn được ghi nhận đã thanh toán tại thời điểm chuyển sang Đã giao thành công; doanh thu tính theo ngày giao thành công.
- Danh sách đơn ưu tiên đơn cần xử lý: sắp theo trạng thái (Chờ xác nhận trước), đơn chưa kết thúc xếp cũ trước, đơn đã kết thúc xếp mới trước.
- Kiểm tra, hoàn kho và cập nhật trạng thái thực hiện trong cùng một giao dịch: lỗi ở bất kỳ bước nào thì không thay đổi dữ liệu; thông báo cho khách chỉ gửi sau khi cập nhật thành công.
- Quản trị viên có cùng quyền xử lý đơn hàng như nhân viên.

---

## QT03: Quy trình bảo hành và đổi hàng lỗi

**Chức năng đáp ứng:** FR15, FR16, FR20 (CN29 – Tạo yêu cầu bảo hành / đổi hàng lỗi; CN30 – Xem danh sách và chi tiết yêu cầu bảo hành; CN31 – Bổ sung bằng chứng; CN32 – Khai báo thông tin gửi hàng; CN33 – Hủy yêu cầu bảo hành; CN34 – Xác nhận đã nhận sản phẩm; CN35 – Xem danh sách yêu cầu bảo hành; CN36 – Xem chi tiết yêu cầu và bằng chứng; CN37 – Xử lý yêu cầu bảo hành).

**Tác nhân:** Khách hàng, Nhân viên.

**Mô tả:** Cho phép khách hàng gửi yêu cầu đổi hàng lỗi khi nhận hoặc bảo hành cho sản phẩm đã mua kèm video minh chứng; nhân viên xem xét, yêu cầu bổ sung, duyệt hoặc từ chối, nhận sản phẩm khách gửi về, gửi hãng thẩm định khi cần và gửi sản phẩm thay thế cho khách. Cửa hàng không hoàn tiền: mỗi yêu cầu kết thúc bằng một sản phẩm thay thế hoặc bị từ chối.

**Tiền điều kiện:** Khách hàng đã đăng nhập; đơn hàng ở trạng thái Đã giao thành công; sản phẩm còn trong thời hạn đổi hàng lỗi hoặc thời hạn bảo hành và chưa có yêu cầu khác đang xử lý.

**Hậu điều kiện:** Khách hàng nhận được sản phẩm thay thế và yêu cầu ở trạng thái Hoàn tất (hoặc yêu cầu bị từ chối kèm lý do); lịch sử xử lý và tồn kho được cập nhật.

**Sơ đồ hoạt động:** file `Docs/quytrinh3.drawio` (trang "QT03. Quy trình bảo hành và đổi hàng lỗi"). Trang này đã chứa sẵn năm mục trên (Chức năng đáp ứng, Tác nhân, Mô tả, Tiền điều kiện, Hậu điều kiện) ở phía trên, tiếp theo là thanh tiêu đề "SƠ ĐỒ HOẠT ĐỘNG: QUY TRÌNH BẢO HÀNH VÀ ĐỔI HÀNG LỖI (QT03)" và sơ đồ.

**Mô tả sơ đồ:**

Sơ đồ QT03 gồm ba làn: làn **Khách hàng** chứa các thao tác khách thực hiện trên website, làn **Nhân viên** chứa các thao tác xử lý của nhân viên cửa hàng, làn **Hệ thống** chứa các bước website tự kiểm tra, lưu trạng thái và gửi thông báo. Hãng sản xuất không phải tác nhân của hệ thống; việc gửi hãng thẩm định do nhân viên thực hiện và ghi nhận trên website. Đổi hàng lỗi khi nhận và bảo hành qua hãng dùng chung một sơ đồ, chỉ khác nhau tại hình thoi "Cần gửi hãng thẩm định?".

Quy trình bắt đầu khi khách chọn đơn hàng đã giao và sản phẩm cần xử lý, chọn loại lỗi, nhập mô tả, tải video minh chứng và gửi yêu cầu (bước 1–3). Hệ thống kiểm tra thời hạn, loại lỗi, số lượng và tệp minh chứng (bước 4); nếu không hợp lệ, khách sửa lại thông tin. Nếu hợp lệ, hệ thống tạo yêu cầu ở trạng thái Chờ duyệt và thông báo cho nhân viên (bước 5).

Nhân viên xem chi tiết yêu cầu và video minh chứng (bước 6). Nếu minh chứng chưa đủ rõ, nhân viên yêu cầu bổ sung; khách nhận thông báo, tải thêm video hoặc ảnh, và yêu cầu quay lại bước nhân viên xem xét (bước 7). Nếu không chấp nhận, nhân viên từ chối kèm lý do, hệ thống chuyển yêu cầu sang Từ chối và thông báo cho khách; quy trình kết thúc. Nếu chấp nhận, nhân viên duyệt và hướng dẫn khách gửi sản phẩm về (bước 8).

Khách gửi sản phẩm về cửa hàng và khai báo đơn vị vận chuyển, mã vận đơn (bước 9). Nhân viên nhận và kiểm tra sản phẩm (bước 10). Với yêu cầu bảo hành cần hãng thẩm định, nhân viên gửi sản phẩm cho hãng và nhận kết quả (bước 11); với đổi hàng lỗi khi nhận, bước này được bỏ qua. Nhân viên chọn cách giải quyết, gửi sản phẩm thay thế và nhập thông tin vận chuyển (bước 12–13); hệ thống trừ tồn kho nếu cửa hàng đổi sản phẩm mới, chuyển yêu cầu sang Đang gửi hàng cho khách và thông báo cho khách (bước 14). Khách nhận sản phẩm và chọn "Đã nhận được hàng" (bước 15); hệ thống chuyển yêu cầu sang Hoàn tất và thông báo cho nhân viên (bước 16). Quy trình kết thúc.

**Luồng sự kiện chính:**

| Bước | Thực hiện | Hành động |
|---|---|---|
| 1 | Khách hàng | Mở trang bảo hành / đổi hàng, chọn đơn hàng đã giao thành công và sản phẩm cần xử lý (hệ thống hiển thị hạn đổi hàng lỗi và hạn bảo hành của từng sản phẩm). |
| 2 | Khách hàng | Chọn loại lỗi, số lượng, nhập mô tả tình trạng (số serial nếu có), tải lên ít nhất một video minh chứng (có thể kèm ảnh). |
| 3 | Khách hàng | Chọn "Gửi yêu cầu". |
| 4 | Hệ thống | Kiểm tra đơn đã giao, loại lỗi áp dụng cho sản phẩm và thuộc diện được xử lý, còn thời hạn, số lượng hợp lệ, sản phẩm chưa có yêu cầu đang xử lý, tệp minh chứng đúng định dạng và dung lượng. Loại yêu cầu (đổi hàng lỗi khi nhận hoặc bảo hành) được xác định theo loại lỗi. |
| 5 | Hệ thống | Tạo yêu cầu (mã bắt đầu bằng BH) ở trạng thái Chờ duyệt, lưu minh chứng và lịch sử xử lý; gửi thông báo yêu cầu mới cho nhân viên. |
| 6 | Nhân viên | Xem danh sách yêu cầu, mở chi tiết yêu cầu, xem video và ảnh minh chứng. |
| 7 | Nhân viên | Nếu minh chứng chưa đủ: chọn "Yêu cầu khách bổ sung minh chứng" kèm nội dung cần bổ sung; khách nhận thông báo, tải thêm video hoặc ảnh; yêu cầu quay lại Chờ duyệt và nhân viên được thông báo (quay lại bước 6). |
| 8 | Nhân viên | Chọn "Duyệt – hướng dẫn khách gửi hàng về"; hệ thống chuyển yêu cầu sang Đã duyệt – chờ gửi hàng về và thông báo cho khách kèm địa chỉ cửa hàng. |
| 9 | Khách hàng | Gửi sản phẩm về cửa hàng, khai báo đơn vị vận chuyển và mã vận đơn (hoặc chọn mang trực tiếp đến cửa hàng). |
| 10 | Nhân viên | Nhận sản phẩm, kiểm tra và chọn "Đã nhận được hàng khách gửi"; yêu cầu chuyển sang Cửa hàng đã nhận hàng. |
| 11 | Nhân viên | Với yêu cầu bảo hành cần hãng thẩm định: chọn "Gửi hãng thẩm định bảo hành", gửi sản phẩm cho hãng và nhận kết quả; yêu cầu ở trạng thái Đang gửi hãng thẩm định. |
| 12 | Nhân viên | Chọn cách giải quyết: cửa hàng đổi sản phẩm mới (đổi hàng lỗi khi nhận chỉ có cách này) hoặc hãng thay thế sản phẩm. |
| 13 | Nhân viên | Chọn "Gửi sản phẩm thay thế cho khách", nhập đơn vị vận chuyển và mã vận đơn. |
| 14 | Hệ thống | Nếu cửa hàng đổi mới: kiểm tra và trừ tồn kho, ghi lịch sử xuất kho; chuyển yêu cầu sang Đang gửi hàng cho khách, lưu thông tin vận chuyển và thông báo cho khách. |
| 15 | Khách hàng | Nhận sản phẩm thay thế và chọn "Đã nhận được hàng". |
| 16 | Hệ thống | Chuyển yêu cầu sang Hoàn tất, ghi thời điểm đóng yêu cầu và thông báo cho nhân viên. Kết thúc quy trình. |

**Luồng thay thế và ngoại lệ:**

| Tại bước | Tình huống | Xử lý của hệ thống |
|---|---|---|
| 1 | Sản phẩm đã hết hạn đổi hàng lỗi và hết hạn bảo hành, hoặc đang có yêu cầu chưa xử lý xong | Sản phẩm không chọn được để gửi yêu cầu. |
| 4 | Loại lỗi không thuộc diện bảo hành hoặc đổi hàng | Báo "… không thuộc diện bảo hành hoặc đổi hàng (căn cứ: …)."; quay lại bước 2. |
| 4 | Quá thời hạn hoặc sản phẩm không có chính sách bảo hành của hãng | Báo "Đã quá thời hạn báo lỗi khi nhận hàng …", "Sản phẩm đã hết thời hạn bảo hành …" hoặc "… không có chính sách bảo hành của hãng."; quay lại bước 2. |
| 4 | Không có video, quá số tệp hoặc tệp sai định dạng, quá dung lượng | Báo "Vui lòng tải lên ít nhất một video minh chứng tình trạng sản phẩm." hoặc "Mỗi lần gửi tối đa 3 video và 5 ảnh."; không lưu tệp nào; quay lại bước 2. |
| 6–8 | Từ chối yêu cầu | Nhân viên bắt buộc ghi lý do; yêu cầu chuyển sang Từ chối, khách được thông báo kèm lý do. Kết thúc quy trình. |
| 7 | Yêu cầu bổ sung nhưng không ghi nội dung | Báo "Vui lòng ghi rõ khách cần bổ sung minh chứng gì."; giữ nguyên trạng thái. |
| 5–9 | Khách hủy yêu cầu (khi Chờ duyệt, Cần bổ sung minh chứng, hoặc Đã duyệt nhưng chưa khai báo gửi hàng) | Yêu cầu chuyển sang Khách đã hủy, nhân viên được thông báo. Kết thúc quy trình. Sau khi đã khai báo gửi hàng thì khách không tự hủy được. |
| 10–11 | Sau khi kiểm tra (hoặc theo kết quả của hãng), lỗi không được chấp nhận | Nhân viên chọn "Từ chối", ghi lý do và nhập thông tin vận chuyển gửi trả sản phẩm cho khách; yêu cầu chuyển sang Từ chối, khách được thông báo. Kết thúc quy trình. |
| 14 | Cửa hàng đổi mới nhưng không đủ tồn kho | Báo "Không đủ tồn kho để đổi (còn …). Hãy nhập thêm hàng rồi gửi sản phẩm mới cho khách (cửa hàng không hoàn tiền)."; không thay đổi dữ liệu; quay lại bước 12 sau khi nhập thêm hàng. |
| 14 | Đổi hàng lỗi khi nhận mà sản phẩm khách trả về còn bán được (ví dụ giao nhầm, còn nguyên niêm phong) | Nhân viên chọn nhập lại kho; hệ thống cộng tồn kho và ghi lịch sử nhập kho. |
| 15 | Khách không bấm xác nhận dù đã nhận hàng | Nhân viên chọn "Hoàn tất xử lý"; yêu cầu chuyển sang Hoàn tất. |

**Quy tắc nghiệp vụ:**
- Chỉ gửi yêu cầu cho sản phẩm thuộc đơn hàng Đã giao thành công; mỗi sản phẩm trong đơn chỉ có một yêu cầu đang xử lý tại một thời điểm.
- Thời hạn tính theo ngày giao thành công (giờ Việt Nam, tính trọn ngày cuối): thời hạn đổi hàng lỗi khi nhận theo thương hiệu; thời hạn bảo hành theo thương hiệu và danh mục (do quản trị viên cấu hình).
- Loại yêu cầu do loại lỗi quyết định: lỗi khi nhận hàng → đổi hàng lỗi; lỗi thuộc bảo hành của hãng → bảo hành; lỗi không thuộc diện chỉ để giải thích, không gửi được.
- Mô tả tình trạng 20–2000 ký tự; mỗi lần gửi tối đa 3 video (mỗi video ≤ 100 MB) và 5 ảnh (mỗi ảnh ≤ 5 MB); lần gửi đầu bắt buộc có video.
- Trạng thái yêu cầu: Chờ duyệt → (Cần bổ sung minh chứng) → Đã duyệt – chờ gửi hàng về → Cửa hàng đã nhận hàng → (Đang gửi hãng thẩm định) → Đang gửi hàng cho khách → Hoàn tất; có thể kết thúc ở Từ chối hoặc Khách đã hủy.
- Cửa hàng không hoàn tiền. Đổi hàng lỗi khi nhận chỉ giải quyết bằng sản phẩm mới từ kho cửa hàng; bảo hành giải quyết bằng sản phẩm hãng thay thế hoặc cửa hàng đổi mới.
- Từ chối sau khi đã nhận sản phẩm thì bắt buộc gửi trả sản phẩm cho khách (nhập đơn vị vận chuyển và mã vận đơn, trừ khi giao trực tiếp tại cửa hàng).
- Mọi thay đổi trạng thái được ghi vào lịch sử xử lý (người thực hiện, thời điểm, ghi chú); khách thấy người xử lý là "Cửa hàng PôpPôp". Thông báo chỉ gửi sau khi thay đổi đã được lưu.
- Quản trị viên có cùng quyền xử lý yêu cầu như nhân viên.

---

## QT04: Quy trình nhập kho

**UC04:** Quy trình nhập kho.

**Chức năng đáp ứng:** FR23 (CN52 – Nhập kho; CN53 – Xem lịch sử nhập xuất kho).

**Tác nhân:** Quản trị viên.

**Mô tả:** Cho phép quản trị viên nhập thêm hàng cho từng phiên bản sản phẩm bằng cách chọn phiên bản, nhập số lượng và ghi chú. Hệ thống cộng tồn kho và ghi lịch sử nhập xuất kho; hàng xuất kho do bán hàng, đổi bảo hành và hàng hoàn kho do hủy đơn được hệ thống tự ghi.

**Tiền điều kiện:** Quản trị viên đã đăng nhập trang quản trị; sản phẩm cần nhập đã có ít nhất một phiên bản.

**Hậu điều kiện:** Tồn kho của phiên bản được tăng thêm số lượng nhập; lịch sử nhập xuất kho ghi nhận giao dịch nhập hàng.

**Sơ đồ hoạt động:** file `Docs/quytrinh4.drawio` (trang "QT04. Quy trình nhập kho"). Trang này đã chứa sẵn các mục trên (UC04, Chức năng đáp ứng, Tác nhân, Mô tả, Tiền điều kiện, Hậu điều kiện) ở phía trên, tiếp theo là thanh tiêu đề "SƠ ĐỒ HOẠT ĐỘNG: QUY TRÌNH NHẬP KHO (QT04)" và sơ đồ.

**Mô tả sơ đồ:**

Sơ đồ QT04 gồm hai làn: làn **Quản trị viên** chứa các thao tác quản trị viên thực hiện trên trang quản trị sản phẩm, làn **Hệ thống** chứa các bước website tự kiểm tra và xử lý. Các ô xếp từ trên xuống theo đúng thứ tự bước trong luồng sự kiện chính; hình thoi "Thông tin hợp lệ?" có nhánh "Có" đi tiếp xuống dưới, nhánh "Không" quay lại bước quản trị viên chọn phiên bản và nhập số lượng.

Quy trình bắt đầu khi quản trị viên mở danh sách sản phẩm, tìm và chọn sản phẩm cần nhập hàng (bước 1–2). Hệ thống hiển thị chi tiết sản phẩm cùng các phiên bản kèm số tồn kho hiện tại và lịch sử kho gần đây (bước 3). Quản trị viên chọn phiên bản, nhập số lượng, ghi chú rồi chọn "Nhập kho" (bước 4–5).

Hệ thống kiểm tra phiên bản và số lượng nhập (bước 6); nếu không hợp lệ, hệ thống báo lỗi và quản trị viên nhập lại. Nếu hợp lệ, hệ thống cộng số lượng nhập vào tồn kho của phiên bản (bước 7), ghi một dòng lịch sử kho loại Nhập hàng (bước 8) và hiển thị lại trang sản phẩm với tồn kho, lịch sử kho mới cùng thông báo cập nhật thành công (bước 9). Quy trình kết thúc khi quản trị viên xem tồn kho và lịch sử kho đã cập nhật (bước 10).

**Luồng sự kiện chính:**

| Bước | Thực hiện | Hành động |
|---|---|---|
| 1 | Quản trị viên | Mở trang quản lý sản phẩm, tìm sản phẩm cần nhập hàng. |
| 2 | Quản trị viên | Chọn sản phẩm cần nhập kho. |
| 3 | Hệ thống | Hiển thị chi tiết sản phẩm: danh sách phiên bản (mã SKU, tên phiên bản, tồn kho hiện tại), khung nhập kho và lịch sử kho gần đây. |
| 4 | Quản trị viên | Chọn phiên bản, nhập số lượng nhập và ghi chú (ví dụ "Nhập lô hàng tháng 11"). |
| 5 | Quản trị viên | Chọn "Nhập kho". |
| 6 | Hệ thống | Kiểm tra đã chọn phiên bản, phiên bản tồn tại và số lượng trong khoảng cho phép. |
| 7 | Hệ thống | Cộng số lượng nhập vào tồn kho của phiên bản, cập nhật thời điểm sửa sản phẩm. |
| 8 | Hệ thống | Ghi lịch sử kho: loại Nhập hàng, số lượng (+), ghi chú (để trống thì ghi "Nhập hàng"), thời điểm. |
| 9 | Hệ thống | Hiển thị lại trang sản phẩm với tồn kho và lịch sử kho mới, kèm thông báo "Đã cập nhật tồn kho.". |
| 10 | Quản trị viên | Xem tồn kho và lịch sử kho đã cập nhật. Kết thúc quy trình. |

**Luồng thay thế và ngoại lệ:**

| Tại bước | Tình huống | Xử lý của hệ thống |
|---|---|---|
| 3 | Sản phẩm chưa có phiên bản nào | Không hiển thị khung nhập kho, báo "Hãy thêm phiên bản trước khi nhập kho."; kết thúc quy trình. |
| 6 | Chưa chọn phiên bản | Báo "Vui lòng chọn phiên bản."; không thay đổi tồn kho; quay lại bước 4. |
| 6 | Số lượng ngoài khoảng 1–100.000 hoặc ghi chú quá 500 ký tự | Báo "Số lượng phải từ 1 đến 100.000." hoặc "Ghi chú không được vượt quá 500 ký tự."; không thay đổi tồn kho; quay lại bước 4. |
| 6 | Phiên bản không còn tồn tại (vừa bị xóa) | Báo không tìm thấy; kết thúc quy trình. |

**Quy tắc nghiệp vụ:**
- Quản trị viên chỉ nhập thêm hàng (số lượng dương); không có thao tác trừ kho thủ công.
- Hàng xuất kho và hoàn kho được hệ thống ghi tự động: bán hàng khi đặt đơn (QT01), hoàn kho khi hủy đơn (QT02), xuất kho đổi hàng và nhập lại hàng khách trả còn bán được (QT03).
- Mỗi lần nhập kho tạo đúng một dòng lịch sử kho gắn với phiên bản; lịch sử kho trên trang sản phẩm hiển thị 30 giao dịch gần nhất, mới nhất trước.
- Cộng tồn kho và ghi lịch sử thực hiện trong cùng một giao dịch: lỗi ở bất kỳ bước nào thì không thay đổi dữ liệu.

---

## QT05: Quy trình đăng ký và xác nhận email

**UC05:** Quy trình đăng ký và xác nhận email.

**Chức năng đáp ứng:** FR01 (CN01 – Đăng ký tài khoản; CN02 – Xác nhận email; CN03 – Gửi lại email xác nhận).

**Tác nhân:** Khách vãng lai.

**Mô tả:** Cho phép khách vãng lai đăng ký tài khoản bằng họ tên, email và mật khẩu. Hệ thống tạo tài khoản ở trạng thái chưa xác thực và gửi liên kết xác thực về email; khách mở liên kết để kích hoạt tài khoản, có thể yêu cầu gửi lại liên kết khi liên kết hết hạn hoặc không nhận được email.

**Tiền điều kiện:** Khách chưa đăng nhập; email dùng để đăng ký chưa có tài khoản trong hệ thống.

**Hậu điều kiện:** Tài khoản khách hàng mới được tạo lập và xác thực email; người dùng có thể đăng nhập và bắt đầu sử dụng hệ thống.

**Sơ đồ hoạt động:** file `Docs/quytrinh5.drawio` (trang "QT05. Quy trình đăng ký và xác nhận email"). Trang này đã chứa sẵn các mục trên (UC05, Chức năng đáp ứng, Tác nhân, Mô tả, Tiền điều kiện, Hậu điều kiện) ở phía trên, tiếp theo là thanh tiêu đề "SƠ ĐỒ HOẠT ĐỘNG: QUY TRÌNH ĐĂNG KÝ VÀ XÁC NHẬN EMAIL (QT05)" và sơ đồ.

**Mô tả sơ đồ:**

Sơ đồ QT05 gồm hai làn: làn **Khách vãng lai** chứa các thao tác của người chưa có tài khoản (trên website và trong hộp thư email), làn **Hệ thống** chứa các bước website tự kiểm tra, tạo tài khoản và gửi email. Hình thoi "Thông tin hợp lệ?" có nhánh "Không" quay lại bước nhập thông tin đăng ký; hình thoi "Liên kết hợp lệ?" có nhánh "Không" dẫn tới bước khách yêu cầu gửi lại email xác thực, sau đó quay lại bước hệ thống gửi email.

Quy trình bắt đầu khi khách mở trang đăng ký, nhập họ tên, email, mật khẩu, xác nhận mật khẩu và chọn "Đăng ký" (bước 1–2). Hệ thống kiểm tra thông tin và kiểm tra email chưa được dùng cho tài khoản nào (bước 3); nếu không hợp lệ, khách sửa lại thông tin. Nếu hợp lệ, hệ thống tạo tài khoản khách hàng ở trạng thái chưa xác thực email (bước 4), gửi email chứa liên kết xác thực (bước 5) và hiển thị trang hướng dẫn kiểm tra hộp thư (bước 6).

Khách mở email và chọn liên kết xác thực (bước 7). Hệ thống kiểm tra liên kết (bước 8); nếu liên kết không hợp lệ hoặc đã hết hạn, khách chọn "Gửi lại email xác thực", nhập email và hệ thống gửi một liên kết mới. Nếu hợp lệ, hệ thống kích hoạt tài khoản và báo xác thực thành công (bước 9). Quy trình kết thúc khi khách chọn "Đăng nhập ngay" và đăng nhập bằng email, mật khẩu vừa đăng ký (bước 10).

**Luồng sự kiện chính:**

| Bước | Thực hiện | Hành động |
|---|---|---|
| 1 | Khách vãng lai | Mở trang đăng ký, nhập họ và tên, email, mật khẩu, xác nhận mật khẩu. |
| 2 | Khách vãng lai | Chọn "Đăng ký". |
| 3 | Hệ thống | Kiểm tra các trường bắt buộc, định dạng email, độ dài mật khẩu, mật khẩu xác nhận khớp; kiểm tra email chưa được dùng cho tài khoản nào. |
| 4 | Hệ thống | Tạo tài khoản với vai trò khách hàng: email (chuyển về chữ thường) dùng làm tên đăng nhập, lưu họ tên, mật khẩu lưu dạng băm, trạng thái chưa xác thực email. |
| 5 | Hệ thống | Tạo liên kết xác thực (hiệu lực 24 giờ) và gửi email chứa liên kết tới địa chỉ khách đã đăng ký. |
| 6 | Hệ thống | Hiển thị trang thông báo đã gửi email, hướng dẫn khách kiểm tra hộp thư. |
| 7 | Khách vãng lai | Mở email và chọn liên kết xác thực. |
| 8 | Hệ thống | Kiểm tra liên kết: tài khoản tồn tại và mã xác thực đúng, còn hiệu lực. |
| 9 | Hệ thống | Đánh dấu email đã xác thực (kích hoạt tài khoản), hiển thị "Xác thực thành công! Tài khoản … đã được kích hoạt." kèm nút "Đăng nhập ngay". |
| 10 | Khách vãng lai | Chọn "Đăng nhập ngay", đăng nhập bằng email và mật khẩu. Kết thúc quy trình. |

**Luồng thay thế và ngoại lệ:**

| Tại bước | Tình huống | Xử lý của hệ thống |
|---|---|---|
| 3 | Thông tin không hợp lệ (thiếu trường, email sai định dạng, mật khẩu dưới 6 ký tự, mật khẩu xác nhận không khớp) | Báo lỗi tại từng trường, ví dụ "Email không hợp lệ.", "Mật khẩu xác nhận không khớp."; quay lại bước 1. |
| 3 | Email đã có tài khoản đã xác thực | Báo "Email này đã được đăng ký. Vui lòng đăng nhập."; không tạo tài khoản. |
| 3 | Email đã đăng ký nhưng chưa xác thực | Báo "Email này đã đăng ký nhưng chưa xác thực. Hãy kiểm tra hộp thư hoặc dùng "Gửi lại email xác thực"."; không tạo tài khoản mới. |
| 5 | Không gửi được email (máy chủ thư lỗi) | Tài khoản vẫn được tạo; trang ở bước 6 báo chưa gửi được email và hướng dẫn dùng "Gửi lại email xác thực". |
| 8 | Liên kết sai hoặc đã quá 24 giờ | Báo "Liên kết xác thực không hợp lệ hoặc đã hết hạn (liên kết chỉ có hiệu lực 24 giờ)." kèm nút "Gửi lại email xác thực"; khách nhập email, hệ thống gửi liên kết mới (quay lại bước 5). |
| 8 | Email đã được xác thực trước đó (mở lại liên kết cũ) | Báo "Email … đã được xác thực trước đó." kèm nút "Đăng nhập ngay"; tiếp tục bước 10. |
| 10 | Khách đăng nhập khi chưa xác thực email (nhập đúng mật khẩu) | Không cho đăng nhập; báo email chưa xác thực và tự gửi lại liên kết xác thực (quay lại bước 7). |

**Quy tắc nghiệp vụ:**
- Chỉ đăng ký bằng email; email được chuyển về chữ thường và dùng làm tên đăng nhập, mỗi email chỉ có một tài khoản.
- Họ và tên 2–100 ký tự; email tối đa 256 ký tự; mật khẩu 6–100 ký tự.
- Tài khoản tạo từ trang đăng ký luôn có vai trò khách hàng; tài khoản nhân viên do quản trị viên tạo (QT07).
- Tài khoản chưa xác thực email không đăng nhập được. Mở liên kết xác thực không tự đăng nhập: khách đăng nhập lại bằng mật khẩu.
- Liên kết xác thực có hiệu lực 24 giờ; mỗi lần gửi lại tạo một liên kết mới.
- Trang "Gửi lại email xác thực" luôn trả lời giống nhau dù email có tồn tại hay không, để không lộ email nào đã đăng ký; khi đăng nhập, hệ thống chỉ báo "chưa xác thực" nếu mật khẩu đúng.

---

## QT06: Quy trình lấy lại mật khẩu qua email

**UC06:** Quy trình lấy lại mật khẩu qua email.

**Chức năng đáp ứng:** FR02, FR04 (CN04 – Đăng nhập; CN08 – Đổi mật khẩu).

**Tác nhân:** Khách hàng, Nhân viên.

**Mô tả:** Cho phép người dùng quên mật khẩu tự đặt lại mật khẩu qua email của tài khoản. Hệ thống gửi liên kết đặt lại mật khẩu (dùng một lần, hiệu lực 24 giờ) tới email; người dùng mở liên kết, đặt mật khẩu mới và đăng nhập lại.

**Tiền điều kiện:** Người dùng đã có tài khoản đăng nhập bằng email (khách hàng) hoặc Gmail (nhân viên) và truy cập được hộp thư đó.

**Hậu điều kiện:** Mật khẩu của tài khoản được đặt lại theo mật khẩu mới người dùng chọn; người dùng có thể đăng nhập lại và tiếp tục sử dụng hệ thống.

**Sơ đồ hoạt động:** file `Docs/quytrinh6.drawio` (trang "QT06. Quy trình lấy lại mật khẩu qua email"). Trang này đã chứa sẵn các mục trên (UC06, Chức năng đáp ứng, Tác nhân, Mô tả, Tiền điều kiện, Hậu điều kiện) ở phía trên, tiếp theo là thanh tiêu đề "SƠ ĐỒ HOẠT ĐỘNG: QUY TRÌNH LẤY LẠI MẬT KHẨU QUA EMAIL (QT06)" và sơ đồ.

**Mô tả sơ đồ:**

Sơ đồ QT06 gồm hai làn: làn **Khách hàng, Nhân viên** chứa các thao tác của người quên mật khẩu (trên website và trong hộp thư email), làn **Hệ thống** chứa các bước website tự kiểm tra, gửi email và lưu mật khẩu. Hình thoi "Email hợp lệ?" có nhánh "Không" quay lại bước nhập email; hình thoi "Liên kết hợp lệ?" có nhánh "Không" dẫn tới bước người dùng yêu cầu gửi lại email, sau đó quay lại bước nhập email; hình thoi "Mật khẩu hợp lệ?" có nhánh "Không" quay lại bước nhập mật khẩu mới.

Quy trình bắt đầu khi người dùng chọn "Quên mật khẩu?" tại trang đăng nhập (trang đăng nhập của khách hàng hoặc trang đăng nhập nội bộ), nhập email của tài khoản và chọn "Gửi email đặt lại mật khẩu" (bước 1–3). Hệ thống kiểm tra email đã nhập (bước 4); nếu không hợp lệ, người dùng nhập lại. Nếu hợp lệ và email thuộc một tài khoản, hệ thống tạo liên kết đặt lại mật khẩu và gửi tới email đó (bước 5), rồi hiển thị trang hướng dẫn kiểm tra email (bước 6).

Người dùng mở email và chọn "Đặt mật khẩu mới" (bước 7). Hệ thống kiểm tra liên kết (bước 8); nếu liên kết không hợp lệ, đã hết hạn hoặc đã dùng, người dùng chọn "Gửi lại email đặt lại mật khẩu" để nhận liên kết mới. Nếu hợp lệ, hệ thống hiển thị trang đặt mật khẩu mới (bước 9). Người dùng nhập mật khẩu mới, xác nhận và chọn "Lưu mật khẩu mới" (bước 10–11). Hệ thống kiểm tra mật khẩu mới và liên kết (bước 12); nếu không hợp lệ, người dùng nhập lại. Nếu hợp lệ, hệ thống lưu mật khẩu mới và báo đã đổi mật khẩu (bước 13). Quy trình kết thúc khi người dùng đăng nhập bằng mật khẩu mới (bước 14).

**Luồng sự kiện chính:**

| Bước | Thực hiện | Hành động |
|---|---|---|
| 1 | Khách hàng, Nhân viên | Tại trang đăng nhập, chọn "Quên mật khẩu?" (trang đăng nhập nội bộ ghi "Quên mật khẩu? (đặt lại qua Gmail)"). |
| 2 | Khách hàng, Nhân viên | Nhập email của tài khoản (khách hàng: email đã đăng ký; nhân viên: Gmail do quản trị viên khai báo). |
| 3 | Khách hàng, Nhân viên | Chọn "Gửi email đặt lại mật khẩu". |
| 4 | Hệ thống | Kiểm tra email bắt buộc, đúng định dạng, tối đa 256 ký tự. |
| 5 | Hệ thống | Tìm tài khoản theo email; nếu có, tạo liên kết đặt lại mật khẩu (dùng một lần, hiệu lực 24 giờ) và gửi email "PôpPôp – Đặt lại mật khẩu" chứa nút "Đặt mật khẩu mới". |
| 6 | Hệ thống | Hiển thị trang "Kiểm tra email của bạn": nếu email là email của một tài khoản PôpPôp thì một liên kết đã được gửi tới đó. |
| 7 | Khách hàng, Nhân viên | Mở email và chọn "Đặt mật khẩu mới". |
| 8 | Hệ thống | Kiểm tra liên kết: tài khoản tồn tại, mã đặt lại mật khẩu đúng, còn hiệu lực và chưa được dùng. |
| 9 | Hệ thống | Hiển thị trang "Đặt mật khẩu mới" kèm email của tài khoản. |
| 10 | Khách hàng, Nhân viên | Nhập mật khẩu mới và xác nhận mật khẩu mới. |
| 11 | Khách hàng, Nhân viên | Chọn "Lưu mật khẩu mới". |
| 12 | Hệ thống | Kiểm tra mật khẩu mới đạt quy định, khớp với ô xác nhận và liên kết vẫn còn hiệu lực. |
| 13 | Hệ thống | Lưu mật khẩu mới; bỏ yêu cầu bắt buộc đổi mật khẩu (nếu có), đánh dấu email đã xác thực, xóa số lần đăng nhập sai; hiển thị "Mật khẩu của bạn đã được đặt lại. Hãy đăng nhập bằng mật khẩu mới." kèm liên kết tới trang đăng nhập phù hợp (khách hàng hoặc nội bộ). |
| 14 | Khách hàng, Nhân viên | Đăng nhập bằng mật khẩu mới. Kết thúc quy trình. |

**Luồng thay thế và ngoại lệ:**

| Tại bước | Tình huống | Xử lý của hệ thống |
|---|---|---|
| 4 | Email để trống hoặc sai định dạng | Báo "Vui lòng nhập email." hoặc "Email không hợp lệ."; quay lại bước 2. |
| 5 | Email không thuộc tài khoản nào | Không gửi email nhưng vẫn hiển thị trang ở bước 6 như bình thường, để không lộ email nào đã đăng ký. |
| 5 | Không gửi được email (máy chủ thư lỗi) | Trang ở bước 6 báo "Hiện chưa gửi được email đặt lại mật khẩu. Vui lòng thử lại sau ít phút."; người dùng quay lại bước 2. |
| 6 | Không thấy email | Người dùng kiểm tra thư mục Spam/Quảng cáo hoặc chọn "Gửi lại email"; quay lại bước 2. |
| 8 | Liên kết sai, quá 24 giờ hoặc đã được dùng | Báo "Liên kết đặt lại mật khẩu không hợp lệ, đã hết hạn hoặc đã được sử dụng. Vui lòng yêu cầu một liên kết mới." kèm "Gửi lại email đặt lại mật khẩu"; quay lại bước 2. |
| 12 | Mật khẩu mới không đạt quy định hoặc xác nhận không khớp | Báo lỗi (ví dụ "Mật khẩu xác nhận không khớp."); mật khẩu không đổi; quay lại bước 10. |
| 12 | Liên kết vừa hết hạn hoặc đã được dùng trong lúc nhập | Báo liên kết không hợp lệ như tại bước 8; quay lại bước 2. |
| 14 | Tài khoản đang bị quản trị viên khóa | Mật khẩu vẫn được đặt lại nhưng không đăng nhập được cho tới khi quản trị viên mở khóa. |

**Quy tắc nghiệp vụ:**
- Áp dụng cho mọi tài khoản đăng nhập bằng email: khách hàng (email đã đăng ký) và nhân viên (Gmail); hệ thống không lấy lại mật khẩu qua số điện thoại.
- Liên kết đặt lại mật khẩu chỉ dùng được một lần và có hiệu lực 24 giờ; mỗi lần yêu cầu tạo một liên kết mới.
- Trang yêu cầu đặt lại mật khẩu luôn trả lời giống nhau dù email có tồn tại hay không; nếu một email trùng với nhiều tài khoản thì hệ thống không gửi để tránh đặt lại nhầm tài khoản.
- Mật khẩu mới: 6–100 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt.
- Đặt lại thành công thì tài khoản không còn bị bắt buộc đổi mật khẩu (người dùng đã tự chọn mật khẩu) và email được xem là đã xác thực (vì người dùng đã mở được liên kết trong hộp thư).
- Ngoài quy trình này, quản trị viên vẫn có thể cấp mật khẩu tạm cho khách hàng hoặc đặt lại mật khẩu nhân viên; khi đó người dùng bị bắt buộc đổi mật khẩu ở lần đăng nhập sau.

---

## QT07: Quy trình tạo tài khoản nhân viên

**UC07:** Quy trình tạo tài khoản nhân viên.

**Chức năng đáp ứng:** FR26, FR03, FR04 (CN60 – Thêm nhân viên; CN05 – Đăng nhập trang quản lý; CN08 – Đổi mật khẩu).

**Tác nhân:** Quản trị viên, Nhân viên.

**Mô tả:** Cho phép quản trị viên tạo tài khoản cho nhân viên bằng họ tên, Gmail và số điện thoại; mật khẩu ban đầu là số điện thoại của nhân viên. Nhân viên đăng nhập trang quản lý bằng Gmail và bắt buộc đổi sang mật khẩu của riêng mình ở lần đăng nhập đầu tiên trước khi sử dụng các chức năng khác.

**Tiền điều kiện:** Quản trị viên đã đăng nhập trang quản trị; Gmail và số điện thoại của nhân viên chưa thuộc tài khoản nào khác.

**Hậu điều kiện:** Tài khoản nhân viên mới được tạo lập và nhân viên đã đặt mật khẩu riêng; nhân viên có thể bắt đầu sử dụng trang nhân viên.

**Sơ đồ hoạt động:** file `Docs/quytrinh7.drawio` (trang "QT07. Quy trình tạo tài khoản nhân viên"). Trang này đã chứa sẵn các mục trên (UC07, Chức năng đáp ứng, Tác nhân, Mô tả, Tiền điều kiện, Hậu điều kiện) ở phía trên, tiếp theo là thanh tiêu đề "SƠ ĐỒ HOẠT ĐỘNG: QUY TRÌNH TẠO TÀI KHOẢN NHÂN VIÊN (QT07)" và sơ đồ.

**Mô tả sơ đồ:**

Sơ đồ QT07 gồm ba làn: làn **Quản trị viên** chứa các thao tác tạo và bàn giao tài khoản, làn **Hệ thống** chứa các bước website tự kiểm tra và xử lý, làn **Nhân viên** chứa các thao tác của nhân viên ở lần đăng nhập đầu tiên. Hình thoi "Thông tin hợp lệ?" có nhánh "Không" quay lại bước quản trị viên nhập thông tin; hình thoi "Mật khẩu hợp lệ?" có nhánh "Không" quay lại bước nhân viên nhập mật khẩu.

Quy trình bắt đầu khi quản trị viên mở trang quản lý nhân viên, chọn "Tạo tài khoản nhân viên", nhập họ tên, Gmail, số điện thoại và chọn "Tạo tài khoản" (bước 1–3). Hệ thống kiểm tra thông tin và kiểm tra Gmail, số điện thoại chưa thuộc tài khoản khác (bước 4); nếu không hợp lệ, quản trị viên sửa lại. Nếu hợp lệ, hệ thống tạo tài khoản nhân viên với mật khẩu ban đầu là số điện thoại và đánh dấu bắt buộc đổi mật khẩu (bước 5), rồi hiển thị mật khẩu ban đầu cho quản trị viên (bước 6). Quản trị viên bàn giao Gmail và mật khẩu ban đầu cho nhân viên (bước 7).

Nhân viên đăng nhập trang quản lý bằng Gmail và mật khẩu ban đầu (bước 8). Hệ thống kiểm tra đăng nhập và chuyển nhân viên tới trang đổi mật khẩu (bước 9). Nhân viên nhập mật khẩu hiện tại, mật khẩu mới, xác nhận và chọn "Cập nhật mật khẩu" (bước 10). Hệ thống kiểm tra mật khẩu (bước 11); nếu không hợp lệ, nhân viên nhập lại. Nếu hợp lệ, hệ thống lưu mật khẩu mới, bỏ yêu cầu đổi mật khẩu và chuyển nhân viên tới trang nhân viên (bước 12). Quy trình kết thúc khi nhân viên bắt đầu sử dụng trang nhân viên (bước 13).

**Luồng sự kiện chính:**

| Bước | Thực hiện | Hành động |
|---|---|---|
| 1 | Quản trị viên | Mở trang quản lý nhân viên, chọn "Tạo tài khoản nhân viên". |
| 2 | Quản trị viên | Nhập họ và tên, Gmail và số điện thoại của nhân viên. |
| 3 | Quản trị viên | Chọn "Tạo tài khoản". |
| 4 | Hệ thống | Kiểm tra các trường bắt buộc, Gmail đúng định dạng (@gmail.com), số điện thoại gồm 10 chữ số bắt đầu bằng 0; kiểm tra Gmail chưa thuộc tài khoản khách hàng hoặc nhân viên nào và số điện thoại chưa thuộc tài khoản nhân viên, quản trị viên khác. |
| 5 | Hệ thống | Tạo tài khoản: Gmail (chữ thường) làm tên đăng nhập, email được xem là đã xác thực, vai trò nhân viên, mật khẩu ban đầu là số điện thoại (lưu dạng băm), đánh dấu bắt buộc đổi mật khẩu. |
| 6 | Hệ thống | Quay về danh sách nhân viên, báo "Đã tạo tài khoản nhân viên …" và hiển thị mật khẩu ban đầu một lần cho quản trị viên. |
| 7 | Quản trị viên | Bàn giao Gmail và mật khẩu ban đầu cho nhân viên (trực tiếp, ngoài hệ thống). |
| 8 | Nhân viên | Mở trang đăng nhập nội bộ, đăng nhập bằng Gmail và mật khẩu ban đầu. |
| 9 | Hệ thống | Kiểm tra thông tin đăng nhập; vì tài khoản đang bắt buộc đổi mật khẩu nên chuyển nhân viên tới trang đổi mật khẩu kèm thông báo "Mật khẩu hiện tại do quản trị viên cấp…". |
| 10 | Nhân viên | Nhập mật khẩu hiện tại (mật khẩu ban đầu), mật khẩu mới, xác nhận mật khẩu mới; chọn "Cập nhật mật khẩu". |
| 11 | Hệ thống | Kiểm tra mật khẩu hiện tại đúng, mật khẩu mới đạt quy định và khớp với ô xác nhận. |
| 12 | Hệ thống | Lưu mật khẩu mới, bỏ đánh dấu bắt buộc đổi mật khẩu, cấp lại phiên đăng nhập và chuyển nhân viên tới trang nhân viên. |
| 13 | Nhân viên | Bắt đầu sử dụng trang nhân viên (xử lý đơn hàng, yêu cầu bảo hành). Kết thúc quy trình. |

**Luồng thay thế và ngoại lệ:**

| Tại bước | Tình huống | Xử lý của hệ thống |
|---|---|---|
| 4 | Thiếu thông tin, Gmail không phải địa chỉ Gmail hợp lệ, số điện thoại sai định dạng | Báo "Vui lòng nhập địa chỉ Gmail hợp lệ của nhân viên (ví dụ: ten.nhanvien@gmail.com)." hoặc "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0 (VD: 0901234567)."; không tạo tài khoản; quay lại bước 2. |
| 4 | Gmail hoặc số điện thoại đã thuộc tài khoản khác | Báo "Gmail … đã thuộc một tài khoản khác (khách hàng hoặc nhân viên)." hoặc "Số điện thoại … đã thuộc một tài khoản khác (khách hàng hoặc nhân viên)."; quay lại bước 2. |
| 8 | Sai Gmail hoặc mật khẩu, hoặc tài khoản đã bị khóa | Không cho đăng nhập, báo lỗi; nhân viên nhập lại hoặc liên hệ quản trị viên. |
| 9–10 | Nhân viên mở trang khác khi chưa đổi mật khẩu | Hệ thống luôn chuyển về trang đổi mật khẩu; chỉ được đăng xuất. |
| 11 | Mật khẩu hiện tại sai, mật khẩu mới không đạt quy định hoặc xác nhận không khớp | Báo lỗi tương ứng (ví dụ "Mật khẩu xác nhận không khớp."); mật khẩu không đổi; quay lại bước 10. |
| 8–13 | Nhân viên quên mật khẩu | Nhân viên tự đặt lại qua Gmail theo QT06 ("Quên mật khẩu? (đặt lại qua Gmail)" ở trang đăng nhập nội bộ); hoặc quản trị viên chọn đặt lại mật khẩu: mật khẩu quay về số điện thoại, tài khoản lại bị bắt buộc đổi mật khẩu và các phiên đăng nhập cũ bị đăng xuất, tiếp tục từ bước 7. |

**Quy tắc nghiệp vụ:**
- Chỉ quản trị viên được tạo tài khoản nhân viên; nhân viên không tự đăng ký.
- Nhân viên đăng nhập bằng Gmail của mình; mỗi Gmail chỉ thuộc một tài khoản (khách hàng hoặc nhân viên), số điện thoại không trùng với nhân viên, quản trị viên khác.
- Mật khẩu ban đầu (và mật khẩu sau khi đặt lại) là số điện thoại của nhân viên, chỉ hiển thị một lần cho quản trị viên.
- Tài khoản có mật khẩu do quản trị viên cấp bắt buộc đổi mật khẩu trước khi dùng bất kỳ chức năng nào.
- Mật khẩu mới: 6–100 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt.
- Tài khoản nhân viên không cần xác thực email như tài khoản khách hàng (QT05) vì do quản trị viên khai báo.
