# KIẾN TRÚC HỆ THỐNG

## 1. Tổng quan

Hệ thống được xây dựng theo mô hình **Client – Server**, dạng **ứng dụng web kết xuất phía máy chủ** (server-side rendering): người dùng chỉ cần trình duyệt web, mọi trang HTML được máy chủ ASP.NET Core dựng sẵn từ dữ liệu rồi gửi về. Toàn bộ logic nghiệp vụ và dữ liệu nằm tập trung ở máy chủ, trình duyệt không lưu dữ liệu nghiệp vụ.

Kiến trúc gồm **3 tầng**:

| Tầng | Vai trò | Công nghệ |
|:---|:---|:---|
| Tầng giao diện người dùng (Client Layer) | Hiển thị trang, nhận thao tác của người dùng, nhận thông báo đẩy | Trình duyệt web, Razor View (HTML), Bootstrap, jQuery, SignalR JavaScript client |
| Tầng ứng dụng & nghiệp vụ (Application Layer) | Tiếp nhận yêu cầu, xác thực – phân quyền, xử lý nghiệp vụ, truy cập dữ liệu, đẩy thông báo | ASP.NET Core 10 MVC + Razor Pages, ASP.NET Core Identity, SignalR, Entity Framework Core 10 |
| Tầng lưu trữ & dịch vụ ngoại vi (Data & External Services) | Lưu dữ liệu nghiệp vụ, lưu tệp ảnh/video, gửi email | SQL Server (CSDL `PopPopShop`), hệ thống tệp của máy chủ, máy chủ SMTP |

Toàn bộ mã nguồn nằm trong **một dự án duy nhất** `PopPop.csproj` (thư mục gốc của repo). Bên trong dự án, mã được tách theo kiến trúc **N-tier**: `Controller → Service → Repository → DbContext`, mỗi lớp chỉ gọi lớp ngay dưới nó thông qua interface (giao diện lập trình) và được nối với nhau bằng cơ chế Dependency Injection khai báo trong `Program.cs`.

![Sơ đồ kiến trúc hệ thống PôpPôp](kien-truc-he-thong.png)

*Hình 1. Sơ đồ kiến trúc tổng thể hệ thống website quản lý và bán hàng cửa hàng cầu lông PôpPôp*

## 2. Tầng giao diện người dùng (Client Layer)

### 2.1. Các phân hệ giao diện

Một website duy nhất phục vụ cả 4 tác nhân; giao diện được chia theo **khu vực (Area)** và vai trò đăng nhập:

| Phân hệ | Đường dẫn | Tác nhân | Chức năng chính |
|:---|:---|:---|:---|
| Cửa hàng trực tuyến | `/`, `/Products`, `/Cart`, `/Orders`, `/Claims`, `/Notifications` | Khách vãng lai, Khách hàng | Xem trang chủ; xem, tìm kiếm, lọc sản phẩm theo danh mục, thương hiệu, giá, thông số; giỏ hàng; đặt hàng thanh toán khi nhận hàng (COD); theo dõi và tự hủy đơn khi còn chờ xác nhận; gửi yêu cầu bảo hành / đổi hàng lỗi kèm video; xem thông báo |
| Tài khoản | `/login`, `/Identity/Account/...` | Tất cả | Đăng ký bằng email và xác nhận qua liên kết; đăng nhập; quên mật khẩu qua email; đổi mật khẩu; cập nhật hồ sơ |
| Khu vực nhân viên | `/Staff/...` | Nhân viên, Quản trị viên | Bảng điều khiển; xử lý đơn hàng theo trạng thái; xử lý yêu cầu bảo hành; danh sách thông báo |
| Khu vực quản trị | `/Admin/...`, đăng nhập tại `/admin/login` | Quản trị viên | Danh mục, thương hiệu, sản phẩm, phiên bản, ảnh, thông số, tồn kho; khách hàng; tài khoản nhân viên; chính sách bảo hành; bảng điều khiển, báo cáo doanh thu và xuất Excel |

Khách vãng lai chỉ xem được các trang công khai (trang chủ, danh sách, chi tiết sản phẩm, tìm kiếm). Mọi thao tác mua hàng, bảo hành, thông báo đều yêu cầu đăng nhập.

### 2.2. Thành phần mã nguồn phía giao diện

| Thư mục / tệp | Nội dung |
|:---|:---|
| `Views/` | Các trang Razor (`.cshtml`) của cửa hàng: `Home`, `Products`, `Cart`, `Orders`, `Claims`, `Notifications` |
| `Views/Shared/` | Bố cục chung: `_Layout.cshtml`, `_HeaderPartial.cshtml` (thanh menu, ô tìm kiếm, chuông thông báo), `_FooterPartial.cshtml`, thẻ sản phẩm dùng chung `_ProductCard.cshtml`, chi tiết đơn dùng chung cho Admin/Staff `_OrderManagementDetails.cshtml` |
| `Areas/Admin/Views/`, `Areas/Staff/Views/` | Trang của khu vực quản trị và nhân viên, với bố cục riêng `_AdminLayout.cshtml`, `_StaffLayout.cshtml` |
| `Areas/Identity/Pages/Account/` | Các trang tài khoản (Razor Pages): `Login`, `AdminLogin`, `Register`, `ConfirmEmail`, `ForgotPassword`, `ResetPassword`, `Manage/ChangePassword`… |
| `wwwroot/css/` | `storefront.css` (giao diện cửa hàng), `site.css` (khu vực quản trị/nhân viên), CSS của giao diện mẫu |
| `wwwroot/js/site.js` | Ô tìm kiếm ở đầu trang: tải các nút lọc theo danh mục và kết quả tìm kiếm trực tiếp (gọi `/Products/SearchFilters`, `/Products/Search`) |
| `wwwroot/js/notifications.js` | Chuông thông báo: mở/đóng bảng thông báo, kết nối SignalR, tăng số chưa đọc và hiện thông báo nổi (toast) khi có thông báo mới |
| `wwwroot/lib/` | Thư viện bên thứ ba: Bootstrap, jQuery, jQuery Validation, Chart.js (biểu đồ ở bảng điều khiển)… |

### 2.3. Giao thức giao tiếp giữa trình duyệt và máy chủ

- **HTTPS (HTTP request/response)**: dùng cho mọi thao tác. Trang được tải bằng yêu cầu `GET`; mọi thao tác thay đổi dữ liệu (thêm giỏ, đặt hàng, đổi trạng thái…) gửi bằng form `POST` kèm **mã chống giả mạo** (`[ValidateAntiForgeryToken]`). Sau khi xử lý, máy chủ chuyển hướng (redirect) về trang kết quả và hiển thị thông báo qua `TempData`. Người dùng được nhận diện qua **cookie đăng nhập** do ASP.NET Core Identity cấp.
- **Yêu cầu nền (AJAX)**: ô tìm kiếm ở đầu trang gọi `GET /Products/Search` và `GET /Products/SearchFilters`, máy chủ trả về đoạn HTML (partial view `_SearchResults`, `_SearchFilters`) để chèn vào trang mà không tải lại.
- **WebSocket qua SignalR** (`/hubs/notifications`): kênh một chiều từ máy chủ đến trình duyệt để đẩy thông báo tức thì. Trình duyệt không gọi phương thức nào trên hub; máy chủ phát sự kiện `NotificationReceived` tới đúng người nhận.

## 3. Tầng ứng dụng & nghiệp vụ (Application Layer)

### 3.1. Cấu trúc thư mục dự án

| Thư mục / tệp | Nội dung |
|:---|:---|
| `PopPop.csproj` | Dự án ASP.NET Core 10 (net10.0) |
| `Program.cs` | Điểm khởi động: đăng ký dịch vụ (DI), cấu hình Identity, pipeline xử lý request, định tuyến |
| `appsettings.json` | Cấu hình: chuỗi kết nối CSDL, SMTP, phí vận chuyển (Shipping), thông tin cửa hàng (Store) |
| `Controllers/` | Controller của cửa hàng: Home, Products, Cart, Orders, Claims, Notifications |
| `Areas/Admin/Controllers/` | Products, Categories, Brands, Customers, StaffAccounts, Orders, Reports, WarrantyPolicies, Dashboard |
| `Areas/Staff/Controllers/` | Orders, Claims, Notifications, Dashboard |
| `Areas/Identity/Pages/Account/` | Đăng nhập, đăng ký, xác nhận email, quên/đặt lại mật khẩu, quản lý tài khoản |
| `Middleware/` | ForcePasswordChangeMiddleware (bắt buộc đổi mật khẩu) |
| `Hubs/` | NotificationHub (SignalR) |
| `Services/Interfaces/` | Hợp đồng của lớp nghiệp vụ (IOrderService, ICartService…) |
| `Services/Implementations/` | Cài đặt nghiệp vụ và tiện ích (ShopClock, ShortCodeGenerator, XlsxWriter, SmtpEmailSender…) |
| `Repositories/Interfaces/` | Hợp đồng truy cập dữ liệu (IRepository, IOrderRepository, IUnitOfWork…) |
| `Repositories/Implementations/` | Truy vấn EF Core, CatalogFiltering, UnitOfWork |
| `Models/` | ViewModel (dữ liệu đưa ra/nhận vào từ trang) và lớp cấu hình (ShippingOptions, SmtpOptions, StoreOptions) |
| `Data/PopPopDbContext.cs` | Ánh xạ thực thể ↔ bảng, ràng buộc, dữ liệu khởi tạo (HasData) |
| `Data/Entities/` | Thực thể (Product, Order, WarrantyClaim…) và kiểu liệt kê (OrderStatus, ClaimStatus…) |
| `Data/Migrations/` | Lịch sử thay đổi lược đồ CSDL (EF Core Migrations) |
| `Data/Seed/` | Dữ liệu mẫu: danh mục, thương hiệu, 46 sản phẩm, thông số, loại lỗi, chính sách bảo hành, tài khoản mẫu |
| `wwwroot/` | Tài nguyên tĩnh (CSS, JS, ảnh) và thư mục ảnh tải lên `uploads/products` |
| `App_Data/claims/` | Video/ảnh minh chứng bảo hành (không công khai) |
| `Docs/` | Tài liệu khóa luận |

### 3.2. Đường đi của một yêu cầu (request pipeline)

Khi trình duyệt gửi một yêu cầu, ASP.NET Core cho yêu cầu đi lần lượt qua các bước được khai báo trong `Program.cs`:

1. `UseForwardedHeaders` → `UseHttpsRedirection`: chuyển mọi truy cập sang HTTPS.
2. `UseRouting`: xác định yêu cầu thuộc controller/action hoặc trang Razor nào. Có 3 kiểu tuyến:
   - Khu vực: `{area}/{controller=Dashboard}/{action=Index}/{id?}` (ví dụ `/Staff/Orders/Details/5`).
   - Mặc định: `{controller=Home}/{action=Index}/{id?}` (ví dụ `/Products/Details/12`).
   - Razor Pages của Identity (ví dụ `/login`, `/admin/login`) và hub SignalR `/hubs/notifications`.
3. `UseStaticFiles` / `MapStaticAssets`: trả trực tiếp tệp CSS, JS, ảnh (kể cả ảnh sản phẩm tải lên lúc chạy).
4. `UseAuthentication`: đọc cookie đăng nhập, dựng thông tin người dùng (mã người dùng, vai trò, cờ bắt buộc đổi mật khẩu).
5. `ForcePasswordChangeMiddleware`: nếu tài khoản có cờ `MustChangePassword` (nhân viên mới, mật khẩu do quản trị viên đặt lại) thì mọi trang đều bị chuyển về `/Identity/Account/Manage/ChangePassword` cho đến khi đổi mật khẩu.
6. `UseAuthorization`: kiểm tra thuộc tính `[Authorize]` trên controller. Chưa đăng nhập thì chuyển tới `/login` (cửa hàng) hoặc `/admin/login` (khu vực `/Admin`, `/Staff`); sai vai trò thì tới trang `AccessDenied`.
7. **Controller** nhận tham số (model binding), gọi **Service**; Service gọi **Repository**; Repository dùng **PopPopDbContext** (EF Core) truy vấn SQL Server.
8. Service trả về **ViewModel**; Controller đưa ViewModel vào **View** (`.cshtml`) để dựng HTML trả về trình duyệt.

### 3.3. Phân lớp Controller → Service → Repository

| Lớp | Trách nhiệm | Không được làm | Ví dụ |
|:---|:---|:---|:---|
| Controller (`Controllers/`, `Areas/*/Controllers/`) | Nhận request, lấy mã người dùng từ cookie, kiểm tra dữ liệu form (`ModelState`), gọi service, chọn View hoặc chuyển hướng, bắt lỗi nghiệp vụ để hiện thông báo | Truy vấn CSDL, chứa quy tắc nghiệp vụ | `CartController.Add` gọi `ICartService.AddItemAsync` |
| Service (`Services/Implementations/`) | Toàn bộ quy tắc nghiệp vụ (kiểm tra tồn kho, tính phí vận chuyển, chuyển trạng thái đơn, tính hạn bảo hành…), mở giao dịch, chuyển Entity thành ViewModel, phát thông báo | Phụ thuộc vào HTML/HTTP | `OrderService.PlaceOrderAsync` |
| Repository (`Repositories/Implementations/`) | Viết truy vấn EF Core (lọc, nạp bảng liên quan bằng `Include`, phân trang), thêm/sửa/xóa thực thể | Ra quyết định nghiệp vụ | `OrderRepository.GetDetailsForManagementAsync` |
| UnitOfWork (`UnitOfWork.cs`) | `SaveChangesAsync` và `ExecuteInTransactionAsync`: chạy một nhóm thao tác trong **một giao dịch mức Serializable**, thành công thì commit, lỗi thì rollback | | Đặt hàng, hủy đơn, xử lý bảo hành |
| DbContext (`Data/PopPopDbContext.cs`) | Ánh xạ thực thể ↔ bảng, khóa, chỉ mục duy nhất, quan hệ, dữ liệu khởi tạo | | |

Các lớp dùng chung đáng chú ý:

- `Repository<TEntity, TKey>`: thao tác cơ bản (lấy theo khóa, thêm, sửa, xóa) cho mọi thực thể; các repository riêng kế thừa và bổ sung truy vấn đặc thù.
- `CatalogGroupRepository<T>` / `CatalogGroupService` / `CatalogGroupController`: một bộ mã dùng chung cho cả **Danh mục** và **Thương hiệu** (cùng dạng tên – mô tả – trạng thái).
- `CatalogFiltering.cs`: toàn bộ điều kiện lọc sản phẩm ngoài cửa hàng (danh mục, thương hiệu, giá, từ khóa, thông số; trong cùng một thuộc tính là "HOẶC", giữa các thuộc tính là "VÀ").
- `ShopClock`: quy đổi giờ UTC lưu trong CSDL sang giờ Việt Nam để chia ngày/tháng trong báo cáo.
- `ShortCodeGenerator`: sinh mã tra cứu `PP-yyMMdd-xxxxxx` (đơn hàng) và `BH-yyMMdd-xxxxxx` (yêu cầu bảo hành) bằng bộ sinh số ngẫu nhiên mật mã, kiểm tra trùng trong CSDL.

Mọi lớp được đăng ký trong `Program.cs` (`AddScoped`: mỗi request một thể hiện; `AddSingleton` cho lớp không giữ trạng thái như `ProductImageStorage`, `ClaimMediaStorage`, `SmtpEmailSender`).

### 3.4. Xác thực và phân quyền

- **ASP.NET Core Identity** quản lý tài khoản (bảng `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`…), băm mật khẩu, khóa tài khoản khi đăng nhập sai nhiều lần, sinh mã xác nhận email/đặt lại mật khẩu.
- **3 vai trò**: `Admin` (Quản trị viên), `Staff` (Nhân viên), `Customer` (Khách hàng). Phân quyền đặt trực tiếp trên controller:
  - `Areas/Admin/*`: `[Authorize(Roles = "Admin")]`.
  - `Areas/Staff/*`: `[Authorize(Roles = "Admin,Staff")]` (quản trị viên cũng xử lý được đơn và bảo hành).
  - `Cart`, `Orders`, `Claims`, `Notifications` của cửa hàng: `[Authorize]` (mọi tài khoản đã đăng nhập).
- **Tên đăng nhập**: khách hàng đăng ký bằng email (phải xác nhận qua liên kết, do lớp `EmailUserConfirmation` kiểm tra); nhân viên mới được tạo bằng Gmail; tài khoản quản trị và tài khoản mẫu dùng số điện thoại.
- `PopPopClaimsPrincipalFactory` thêm cờ `MustChangePassword` vào cookie để `ForcePasswordChangeMiddleware` dùng.
- Khi quản trị viên khóa tài khoản, "security stamp" của tài khoản được đổi; Identity kiểm tra lại mỗi **1 phút**, nên người bị khóa bị đăng xuất gần như ngay lập tức.
- **Kiểm tra quyền sở hữu dữ liệu**: ngoài vai trò, service luôn lọc theo mã người dùng hiện tại (ví dụ `GetDetailsForUserAsync(orderId, userId)`), nên khách hàng không xem được đơn hay yêu cầu bảo hành của người khác dù đoán được mã.

### 3.5. Giao dịch và toàn vẹn dữ liệu

Các nghiệp vụ thay đổi nhiều bảng cùng lúc đều chạy trong `IUnitOfWork.ExecuteInTransactionAsync` (mức cô lập **Serializable**):

- **Đặt hàng**: kiểm tra tồn kho, tạo đơn và chi tiết đơn, trừ tồn kho, ghi giao dịch kho, xóa giỏ hàng — tất cả thành công hoặc không có gì thay đổi. Mức Serializable ngăn hai khách cùng mua món cuối cùng làm tồn kho âm.
- **Hủy đơn**: hoàn tồn kho và ghi giao dịch kho `OrderCancelRestock`. Cột `StockReleasedAt` đảm bảo một đơn chỉ được hoàn kho **một lần**.
- **Mọi biến động kho** đều để lại một dòng trong `InventoryTransactions` (nhập kho, bán hàng, điều chỉnh tăng/giảm, xuất hàng thay thế bảo hành, nhập lại hàng bảo hành, hoàn kho do hủy đơn) để truy vết.
- **Đơn hàng lưu "ảnh chụp" giá**: `OrderItems` sao chép tên sản phẩm, SKU, đơn giá tại thời điểm mua, nên sửa giá sau này không làm sai đơn cũ.
- **Tệp minh chứng bảo hành**: nếu giao dịch tạo yêu cầu thất bại, các tệp đã ghi xuống đĩa bị xóa lại.

### 3.6. Thông báo thời gian thực

Thành phần: `Services/Implementations/NotificationService.cs`, `Hubs/NotificationHub.cs`, `Repositories/Implementations/NotificationRepository.cs`, `wwwroot/js/notifications.js`, `Services/Implementations/HeaderBadgeService.cs`.

1. Sau khi nghiệp vụ đã **commit** (đặt hàng, đổi trạng thái đơn, xử lý bảo hành…), service gọi `NotificationService`.
2. `NotificationService` lưu thông báo vào bảng `Notifications` (để người nhận vẫn thấy khi đang offline), sau đó gọi `IHubContext<NotificationHub>.Clients.User(userId).SendAsync("NotificationReceived", …)` để đẩy tới mọi tab đang mở của **đúng người nhận**.
   - `NotifyCustomerAsync`: gửi cho khách hàng sở hữu đơn/yêu cầu.
   - `NotifyStaffAsync`: gửi cho từng nhân viên và quản trị viên chưa bị khóa (mỗi người một bản ghi riêng để trạng thái "đã đọc" độc lập).
3. Trong trình duyệt, `notifications.js` đã kết nối tới `/hubs/notifications`; khi nhận sự kiện, nó tăng số trên chuông, chèn thông báo vào đầu danh sách và hiện thông báo nổi 7 giây.
4. Khi tải trang, `HeaderBadgeService` đọc số thông báo chưa đọc và 5 thông báo gần nhất để dựng sẵn chuông. Bấm vào thông báo đi qua `/Notifications/Open/{id}` để đánh dấu đã đọc rồi chuyển tới trang liên quan.
5. Lỗi khi gửi thông báo chỉ được ghi log, **không** làm hỏng nghiệp vụ đã thành công.

Thư viện SignalR client được nạp trong bố cục khu vực Quản trị, Nhân viên (`_AdminLayout.cshtml`, `_StaffLayout.cshtml`) và trong giao diện cửa hàng khi người dùng đã đăng nhập (`Views/Shared/_MainScriptPartial.cshtml`); khách vãng lai không mở kết nối vì hub chỉ phục vụ người đã đăng nhập.

## 4. Tầng lưu trữ & dịch vụ ngoại vi

### 4.1. Cơ sở dữ liệu SQL Server

- CSDL `PopPopShop` (khi phát triển dùng SQL Server LocalDB, chuỗi kết nối `ConnectionStrings:DefaultConnection` trong `appsettings.json`).
- **22 thực thể** (chi tiết trong `Docs/Thiet-ke-CSDL.md`, sơ đồ `Docs/erd.png`), chia thành các nhóm:

| Nhóm | Bảng |
|:---|:---|
| Tài khoản | `AspNetUsers`, `AspNetRoles` (và các bảng phụ của Identity) |
| Hàng hóa | `Categories`, `Brands`, `Products`, `ProductVariants`, `ProductImages` |
| Thông số kỹ thuật | `SpecAttributes`, `SpecOptions`, `ProductSpecValues`, `VariantSpecValues` |
| Mua hàng | `Carts`, `CartItems`, `Orders`, `OrderItems` |
| Kho | `InventoryTransactions` |
| Bảo hành | `DefectTypes`, `WarrantyPolicies`, `WarrantyClaims`, `WarrantyClaimMedia`, `WarrantyClaimHistory` |
| Thông báo | `Notifications` |

- Lược đồ được quản lý bằng **EF Core Migrations** (`Data/Migrations/`); dữ liệu mẫu ban đầu khai báo trong `Data/Seed/` và được chèn khi chạy `dotnet ef database update`.
- Quy ước: giá bán và tồn kho nằm ở **phiên bản** (`ProductVariants`, mỗi phiên bản một SKU), không nằm ở sản phẩm. Một sản phẩm chỉ được bán khi chính nó, danh mục và thương hiệu của nó đều đang hoạt động (`Product.IsOnSale()`).

### 4.2. Lưu trữ tệp

| Loại tệp | Vị trí | Lớp xử lý | Cách truy cập |
|:---|:---|:---|:---|
| Ảnh sản phẩm do quản trị viên tải lên | `wwwroot/uploads/products/` (tên tệp ngẫu nhiên, tối đa 5 MB) | `ProductImageStorage` | Công khai, trình duyệt tải trực tiếp; CSDL chỉ lưu đường dẫn trong `ProductImages` |
| Ảnh sản phẩm mẫu | `wwwroot/img/product/catalog/` | — | Công khai |
| Video/ảnh minh chứng bảo hành | `App_Data/claims/{mã yêu cầu}/` | `ClaimMediaStorage` (kiểm tra đuôi tệp, chữ ký đầu tệp và dung lượng) | **Không công khai**: chỉ tải qua action `Claims/Media` (khách hàng sở hữu) hoặc `Staff/Claims/Media`, có hỗ trợ tua video; CSDL lưu thông tin tệp trong `WarrantyClaimMedia` |

### 4.3. Dịch vụ bên ngoài

- **Máy chủ SMTP** (`SmtpEmailSender`, `AccountEmailService` trong `Services/Implementations/SmtpEmailSender.cs`; cấu hình mục `Smtp`): gửi email xác nhận đăng ký và email đặt lại mật khẩu. Nếu `Smtp:Host` để trống, hệ thống chỉ ghi nội dung email ra log (ở môi trường phát triển, liên kết xác nhận được hiện trên trang).
- **CDN** (jsDelivr, cdnjs): cung cấp thư viện SignalR client và Popper.js cho trình duyệt.
- Thanh toán: chỉ hỗ trợ **COD**; không tích hợp cổng thanh toán trực tuyến.

## 5. Luồng dữ liệu của các chức năng chính

Mỗi luồng dưới đây đi từ thao tác của người dùng xuống CSDL rồi quay lại giao diện, ghi rõ tệp xử lý ở từng bước.

### 5.1. Xem và tìm kiếm sản phẩm

1. Người dùng mở `/Products?categoryId=…&brandId=…&spec=…&q=…&minPrice=…&sort=…` (hoặc gõ vào ô tìm kiếm ở đầu trang).
2. `ProductsController.Index` (hoặc `Search` cho kết quả trực tiếp) → `ProductService.GetCatalogAsync`.
3. `ProductRepository` áp dụng `CatalogFiltering`: chỉ lấy sản phẩm đang bán, lọc theo điều kiện, đếm số sản phẩm cho từng nút lọc, phân trang.
4. Service chuyển kết quả thành `ProductCatalogViewModel` / `ProductCardViewModel` → View `Views/Products/Index.cshtml` dựng lưới sản phẩm.
5. Trang chi tiết `/Products/Details/{id}` → `ProductService.GetDetailsAsync` trả về ảnh, các phiên bản (giá, tồn kho từng phiên bản) và thông số.

Bảng đọc: `Products`, `ProductVariants`, `ProductImages`, `Categories`, `Brands`, `SpecAttributes`, `SpecOptions`, `ProductSpecValues`, `VariantSpecValues`.

### 5.2. Thêm vào giỏ hàng

1. Khách hàng chọn phiên bản, số lượng, bấm "Thêm vào giỏ" → `POST /Cart/Add` (`CartController.Add`).
2. `CartService.AddItemAsync`: lấy phiên bản còn bán (`ProductRepository.GetAvailableVariantAsync`), kiểm tra số lượng ≤ tồn kho; tạo giỏ nếu khách chưa có; cộng dồn nếu phiên bản đã có trong giỏ.
3. `UnitOfWork.SaveChangesAsync` ghi `Carts`, `CartItems`.
4. Chuyển hướng về `/Cart` kèm thông báo thành công hoặc lỗi.

### 5.3. Đặt hàng (COD)

1. `GET /Orders/Checkout` (`OrdersController.Checkout`): hiển thị giỏ, phí vận chuyển (30.000 ₫, miễn phí từ 500.000 ₫ — cấu hình `ShippingOptions`) và điền sẵn người nhận từ đơn gần nhất.
2. Khách điền người nhận, số điện thoại, địa chỉ → `POST /Orders/Checkout`.
3. `OrderService.PlaceOrderAsync` mở **một giao dịch**:
   - đọc giỏ (`CartRepository`), kiểm tra từng dòng còn bán và đủ tồn kho;
   - sinh mã đơn `PP-…` (`ShortCodeGenerator`);
   - tạo `Orders` (trạng thái *Chờ xác nhận*, thanh toán *Chưa thanh toán*) và `OrderItems` (sao chép tên, SKU, đơn giá);
   - trừ `ProductVariants.StockQuantity`, ghi `InventoryTransactions` loại *Bán hàng*;
   - tính phí vận chuyển và tổng tiền, xóa các dòng giỏ hàng;
   - commit.
4. Sau commit, `NotificationService.NotifyStaffAsync` báo "Đơn hàng mới" tới mọi nhân viên/quản trị viên (chuông cập nhật tức thì).
5. Khách được chuyển tới `/Orders/Details/{id}`.

### 5.4. Nhân viên xử lý đơn hàng

1. Nhân viên mở `/Staff/Orders` (lọc theo trạng thái, tìm theo mã/tên/số điện thoại) → `OrderService.GetManagementOrdersAsync`.
2. Ở chi tiết đơn, chọn trạng thái kế tiếp → `POST /Staff/Orders/UpdateStatus` (quản trị viên dùng `/Admin/Orders/UpdateStatus`, cùng service).
3. `OrderService.UpdateOrderStatusAsync` trong giao dịch:
   - chỉ cho phép bước hợp lệ: *Chờ xác nhận → Đã xác nhận → Đang xử lý → Đang giao → Đã giao thành công*; được hủy khi chưa *Đang giao*;
   - nếu hủy: hoàn tồn kho, ghi giao dịch kho, ghi lý do vào ghi chú đơn;
   - nếu *Đã giao thành công*: đặt thanh toán = *Đã thanh toán*, ghi `PaidAt` (COD thu tiền khi giao).
4. Sau commit, `NotifyOrderStatusChangedAsync` gửi thông báo cho khách hàng.

Khách hàng tự hủy (`POST /Orders/Cancel` → `OrderService.CancelByCustomerAsync`) chỉ được khi đơn còn *Chờ xác nhận*; kho được hoàn và nhân viên nhận thông báo.

### 5.5. Bảo hành và đổi hàng lỗi

1. Khách mở `/Claims/Start`: `WarrantyClaimService.GetStartAsync` liệt kê các đơn đã giao và hạn đổi lỗi/bảo hành của từng sản phẩm.
2. Chọn sản phẩm → `/Claims/Create`: chọn loại lỗi (`DefectTypes` theo danh mục), mô tả, tải lên ít nhất một video.
3. `WarrantyClaimService.CreateAsync`:
   - kiểm tra đơn thuộc khách và đã giao, sản phẩm chưa có yêu cầu đang mở;
   - loại lỗi quyết định loại yêu cầu: *Đổi hàng lỗi khi nhận* (hạn = `Brands.ReceiptDefectDays`) hoặc *Bảo hành* (hạn = `WarrantyPolicies` theo thương hiệu × danh mục); quá hạn thì từ chối ngay;
   - `ClaimMediaStorage` kiểm tra và lưu tệp vào `App_Data/claims/{id}`;
   - trong giao dịch: tạo `WarrantyClaims` (mã `BH-…`, trạng thái *Đã gửi*), `WarrantyClaimHistory`, `WarrantyClaimMedia`.
4. Nhân viên nhận thông báo, xử lý ở `/Staff/Claims/Details/{id}` → `DecideAsync`: *Cần bổ sung*, *Đã duyệt*, *Đã nhận hàng*, *Đã gửi hãng*, *Đang gửi trả khách*, *Hoàn tất*, *Từ chối*. Mỗi lần đổi trạng thái ghi một dòng lịch sử; khi gửi hàng thay thế, kho bị trừ (`ClaimReplacement`); hàng lỗi nhập lại ghi `ClaimRestock`. Khách nhận thông báo sau mỗi bước.
5. Khách có thể bổ sung minh chứng, khai báo vận đơn gửi hàng về, xác nhận đã nhận hàng thay thế, hoặc hủy yêu cầu trước khi gửi hàng. Hệ thống **không hoàn tiền**: yêu cầu kết thúc bằng sản phẩm mới hoặc bị từ chối.

### 5.6. Đăng ký, xác nhận email, quên mật khẩu

1. `/Identity/Account/Register` (`Register.cshtml.cs`): tạo `AspNetUsers` với tên đăng nhập = email, gán vai trò `Customer`, sinh mã xác nhận và gửi liên kết qua `AccountEmailService` → SMTP.
2. Khách bấm liên kết → `ConfirmEmail` đặt `EmailConfirmed = true`. Trước đó, đăng nhập đúng mật khẩu vẫn bị chặn và hệ thống gửi lại liên kết.
3. `/login` (`Login.cshtml.cs`): nhận email hoặc số điện thoại, `SignInManager` kiểm tra mật khẩu, trạng thái khóa, xác nhận email, rồi cấp cookie.
4. `ForgotPassword` gửi liên kết đặt lại mật khẩu (hiệu lực 24 giờ, dùng một lần) → `ResetPassword` đặt mật khẩu mới.

### 5.7. Quản trị sản phẩm và kho

- `Areas/Admin/Controllers/ProductsController` → `ProductManagementService` → `ProductManagementRepository`: tạo/sửa/xóa sản phẩm, phiên bản (SKU, giá, tồn kho), gán thông số, tải ảnh (`ProductImageStorage`), chọn ảnh chính.
- Điều chỉnh tồn kho (`AdjustStock`): cập nhật `ProductVariants.StockQuantity` và ghi `InventoryTransactions` (nhập kho, điều chỉnh tăng/giảm) kèm ghi chú.
- Danh mục, thương hiệu: `CategoriesController` / `BrandsController` dùng chung `CatalogGroupController`; tắt danh mục hoặc thương hiệu sẽ ẩn toàn bộ sản phẩm thuộc nó khỏi cửa hàng.

### 5.8. Báo cáo doanh thu

1. `/Admin/Reports?preset=…` hoặc `from`/`to` → `ReportService.GetReportAsync` → `ReportRepository`.
2. Doanh thu = tổng tiền các đơn *Đã giao thành công*, tính theo ngày thanh toán; ngày được quy đổi sang giờ Việt Nam bằng `ShopClock`; khoảng thời gian dài hơn 62 ngày được gộp theo tháng.
3. `/Admin/Reports/Export` → `ReportService.ExportAsync` dựng tệp `.xlsx` bằng `XlsxWriter` và trả về để tải xuống.
4. Bảng điều khiển `/Admin` (`OrderService.GetAdminDashboardAsync`) dùng cùng truy vấn để vẽ biểu đồ 7 ngày gần nhất và số đơn theo trạng thái.

### 5.9. Quản lý tài khoản

- Khách hàng (`/Admin/Customers` → `CustomerManagementService`, `AccountLockService`, `AccountPasswordService`): xem, khóa/mở khóa kèm lý do, cấp mật khẩu tạm, xóa khi khách chưa có đơn và yêu cầu bảo hành.
- Nhân viên (`/Admin/StaffAccounts` → `StaffAccountService`): tạo tài khoản bằng Gmail + số điện thoại (mật khẩu ban đầu = số điện thoại, bắt buộc đổi ở lần đăng nhập đầu), sửa thông tin, đặt lại mật khẩu, khóa/mở khóa.

## 6. Bảng tra cứu nhanh: chức năng → mã nguồn → dữ liệu

| Chức năng | Controller | Service | Repository | Bảng chính |
|:---|:---|:---|:---|:---|
| Trang chủ, danh sách, chi tiết, tìm kiếm sản phẩm | `HomeController`, `ProductsController` | `ProductService` | `ProductRepository`, `CatalogFiltering`, `SpecRepository` | Products, ProductVariants, ProductImages, Spec* |
| Giỏ hàng | `CartController` | `CartService` | `CartRepository` | Carts, CartItems |
| Đặt hàng, xem/hủy đơn (khách) | `OrdersController` | `OrderService` | `OrderRepository`, `CartRepository` | Orders, OrderItems, InventoryTransactions |
| Xử lý đơn hàng | `Staff/OrdersController`, `Admin/OrdersController` | `OrderService` | `OrderRepository` | Orders, InventoryTransactions |
| Bảo hành (khách) | `ClaimsController` | `WarrantyClaimService` | `WarrantyClaimRepository` | WarrantyClaims, WarrantyClaimMedia, WarrantyClaimHistory |
| Bảo hành (nhân viên) | `Staff/ClaimsController` | `WarrantyClaimService` | `WarrantyClaimRepository` | như trên + InventoryTransactions |
| Chính sách bảo hành, loại lỗi | `Admin/WarrantyPoliciesController` | `WarrantyPolicyService` | `WarrantyPolicyRepository` | WarrantyPolicies, DefectTypes, Brands |
| Thông báo | `NotificationsController`, `Staff/NotificationsController` | `NotificationService`, `HeaderBadgeService` | `NotificationRepository` | Notifications |
| Sản phẩm, phiên bản, ảnh, kho | `Admin/ProductsController` | `ProductManagementService`, `SpecManagementService` | `ProductManagementRepository`, `SpecRepository` | Products, ProductVariants, ProductImages, InventoryTransactions |
| Danh mục, thương hiệu | `Admin/CategoriesController`, `Admin/BrandsController` | `CategoryManagementService`, `BrandManagementService` | `CatalogGroupRepository<T>` | Categories, Brands |
| Khách hàng | `Admin/CustomersController` | `CustomerManagementService`, `AccountLockService`, `AccountPasswordService` | `CustomerRepository` | AspNetUsers |
| Tài khoản nhân viên | `Admin/StaffAccountsController` | `StaffAccountService` | (UserManager của Identity) | AspNetUsers, AspNetUserRoles |
| Báo cáo, bảng điều khiển | `Admin/ReportsController`, `Admin/DashboardController`, `Staff/DashboardController` | `ReportService`, `OrderService` | `ReportRepository`, `OrderRepository` | Orders, OrderItems |
| Đăng ký, đăng nhập, mật khẩu | Razor Pages `Areas/Identity/Pages/Account` | `AccountEmailService`, `SmtpEmailSender` | (UserManager / SignInManager) | AspNetUsers, AspNetRoles |
