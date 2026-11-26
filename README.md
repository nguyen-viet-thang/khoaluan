# PopPop
<div align="center">

![PopPop Logo](wwwroot/img/logo.png)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Version](https://img.shields.io/badge/.NET-10-512BD4?logo=.net)](https://dotnet.microsoft.com/)
![Status](https://img.shields.io/badge/Status-Active%20Development-brightgreen)

[Về dự án](#-về-dự-án) | [Cài đặt](#-cài-đặt) | [Cấu trúc dự án](#-cấu-trúc-dự-án) | [Luồng hoạt động](#-luồng-hoạt-động-các-tầng-nghiệp-vụ) | [Đóng góp](#-đóng-góp) | [Liên hệ](#-liên-hệ) | [Giấy phép](#-giấy-phép)

</div>

---

## 📋 Về dự án

**PopPop** là dự án được phát triển cho đề tài khóa luận tốt nghiệp **Xây dựng hệ thống website quản lý và bán hàng trực tuyến cho cửa hàng cầu lông PôpPôp**, dự án được xây dựng nhằm mục đích học tập và nghiên cứu sát với thực tế, và không phục vụ mục đích thương mại.

## 🚀 Cài đặt

### Yêu cầu

- **.NET 10 SDK** - [Tải xuống](https://dotnet.microsoft.com/download/dotnet/10.0)
- **SQL Server LocalDB** - có sẵn khi cài Visual Studio 2022+ (workload *ASP.NET and web development*), hoặc dùng [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- **Git** - [Tải xuống](https://git-scm.com/download/win)

### Các bước cài đặt (PowerShell trên Windows)

#### 1. Clone repository (nhánh `thang`)

```powershell
git clone -b thang https://github.com/nguyen-viet-thang/khoaluan.git
cd khoaluan
```

#### 2. Tạo file `appsettings.json`

File này không có trong repo (để tránh lộ mật khẩu), mỗi máy tự tạo. Chạy nguyên khối lệnh sau trong thư mục dự án:

```powershell
@'
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=PopPopShop;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Smtp": { "Host": "", "Port": 587, "EnableSsl": true, "UserName": "", "FromAddress": "", "FromName": "PopPop" },
  "Shipping": { "Fee": 30000, "FreeShippingThreshold": 500000 },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}
'@ | Set-Content -Path "appsettings.json" -Encoding utf8
```

- Dùng SQL Server Express thì đổi `(localdb)\\MSSQLLocalDB` thành `.\\SQLEXPRESS`.
- `Smtp.Host` để trống: không gửi email thật, link xác nhận/đặt lại mật khẩu hiện ngay trên trang (môi trường Development).
- `Shipping` (không bắt buộc): phí giao hàng và mức miễn phí giao hàng. Hệ thống chỉ hỗ trợ thanh toán khi nhận hàng (COD).

#### 3. Tạo cơ sở dữ liệu (kèm dữ liệu mẫu)

```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update
```

Hoặc trong Visual Studio: `Package Manager Console > Update-Database`.

#### 4. Chạy ứng dụng

```powershell
dotnet run
```

Mở trình duyệt tại http://localhost:5263. Hoặc mở `PopPop.slnx` trong Visual Studio và nhấn **F5**.

#### 5. Tài khoản mẫu

| Vai trò | Trang đăng nhập | Tài khoản | Mật khẩu |
|---|---|---|---|
| Admin | `/admin/login` | 0900000000 | Admin@123 |
| Nhân viên | `/admin/login` | 0911222301 | PopPop@123 |
| Khách hàng | `/Identity/Account/Login` | 0901234501 | PopPop@123 |

> Chỉ dùng cho môi trường chạy thử trên máy cá nhân.

#### Cập nhật code mới

```powershell
git pull
dotnet ef database update
```

## 📁 Cấu trúc dự án

```
PopPop/
├── Areas/
│   ├── Admin/                      # khu vực quản trị
│   ├── Staff/                      # khu vực nhân viên
│   └── Identity/                   # khu vực xác thực, phân quyền và quản lý người dùng
├── Controllers/                    # controllers cho khách hàng
├── Data/                           # cơ sở dữ liệu
│   ├── Entities/                   # thực thể
├── Models/                         # dtos,viewmodels request và response
├── Services/                       # tầng logic xử lý nghiệp vụ
├── Repositories/                   # tầng truy vấn cơ sở dữ liệu
├── Views/                          # giao diện người dùng
├── wwwroot/                        # tài nguyên tĩnh
├── Doc/                            # tài liệu
├── appsettings.json                # cấu hình ứng dụng
├── Program.cs                      # entry point
```

## 🔄 Luồng hoạt động các tầng nghiệp vụ

Dự án PopPop sử dụng **N-Tier Architecture** với 5 tầng chính:

```
📱 Views/Razor Pages (Presentation)
         ↓ (HTTP Request)
🎮 Controllers (Application Layer)
         ↓ (Service Call)
💼 Services (Business Logic)
         ↓ (Repository Call)
📊 Repositories (Data Access)
         ↓ (EF Core Query)
🗄️  SQL Server (Database)
```

## 💻 Đóng góp

Chúng tôi rất hoan nghênh các đóng góp!

**1.** Fork repository và Pull request

**2.** Liên hệ với chúng tôi nếu bạn muốn tham gia vào Repo

## 📞 Liên hệ
- **Email:** `linhphuonglinh0321@gmail.com`

## 📄 Giấy phép

Dự án này được cấp phép dưới **License MIT** - xem file [LICENSE](LICENSE) để biết chi tiết.

---

<div align="center">

**[⬆ Quay lại đầu trang](#poppop)**

Made with ❤️ by TuanVu

</div>
