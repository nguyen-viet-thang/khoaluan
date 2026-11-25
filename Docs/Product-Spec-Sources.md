# Nguồn thông số sản phẩm (dữ liệu mẫu)

Thông số dùng cho bộ lọc tìm kiếm nằm trong `Data/Seed/SpecSeedData.cs`.

- **Thông số theo sản phẩm** (điểm cân bằng, độ cứng thân vợt, đường kính cước...) được gán thủ công theo bảng bên dưới.
- **Thông số theo phiên bản** (trọng lượng, cỡ cán, size, màu, tốc độ cầu) được ghép tự động từ tên phiên bản trong `CatalogSeedData`. Nếu một phiên bản không khớp được thì quá trình build model sẽ báo lỗi.

Hãng không công bố đầy đủ một bảng thông số thống nhất. Các giá trị dưới đây là cách phân loại phổ biến nhất trên trang của các đại lý chính hãng, đã đối chiếu với mô tả sản phẩm trong catalog. Với mẫu mà các nguồn ghi khác nhau, cột "Ghi chú" nói rõ lựa chọn và lý do.

## Vợt cầu lông

| Sản phẩm | Điểm cân bằng | Độ cứng thân | Ghi chú |
|---|---|---|---|
| Yonex Astrox 99 Pro | Nặng đầu | Cứng | Dòng Astrox thiên công |
| Yonex Astrox 88D Pro | Nặng đầu | Cứng | |
| Yonex Nanoflare 800 Pro | Nhẹ đầu | Cứng | |
| Yonex Arcsaber 11 Pro | Cân bằng | Cứng | |
| Yonex Nanoflare 1000Z | Nhẹ đầu | Cứng | |
| Yonex Astrox 77 Play | Nặng đầu | Trung bình | Bản "Play" có thân mềm hơn bản Pro |
| Yonex Nanoflare 800 Play | Nhẹ đầu | Trung bình | Đa số đại lý ghi Medium (khoảng 285 mm) |
| Victor Thruster Ryuga II Pro | Nặng đầu | Cứng | |
| Victor Auraspeed 90K II | Nhẹ đầu | Cứng | Đại lý ghi Head Light / Stiff |
| Victor Brave Sword 12 | Cân bằng | Trung bình | Nguồn ghi từ Medium đến Slightly Stiff; chọn Trung bình |
| Li-Ning Axforce 80 | Nặng đầu | Trung bình | Điểm cân bằng khoảng 300–304 mm |
| Li-Ning Aeronaut 9000C | Nặng đầu | Trung bình | Bản Combat, khoảng 298 mm |
| Li-Ning Halbertec 8000 | Cân bằng | Cứng | Khoảng 301 mm, nguồn ghi từ "even" đến "hơi nặng đầu"; Halbertec là dòng toàn diện |
| Apacs Feather Weight 55 | Nặng đầu | Dẻo | Khoảng 335 mm, thân dẻo (Flexible) |

Nguồn đối chiếu (trang đại lý):

- Nanoflare 800 Play: [Central Sports](https://centralsports.co.uk/products/yonex-nanoflare-800-play-deep-green-4u5), [Badminton HQ](https://badmintonhq.co.uk/products/yonex-nanoflare-800-play-4u-badminton-racket-deep-green)
- Auraspeed 90K II: [Racquet Guys](https://racquetguys.ca/products/victor-auraspeed-90kii), [Joy Badminton](https://joybadminton.com/products/victor-auraspeed-90k-ii-ars-90k-ii-shock-blue)
- Brave Sword 12: [Victor Canada](https://ca.victorsport.com/product/brave-sword-12-n), [Badminton HQ](https://badmintonhq.co.uk/products/victor-bravesword-12-badminton-racket)
- Axforce 80: [Central Sports](https://centralsports.co.uk/products/li-ning-axforce-80-aypt271), [Badminton HQ](https://badmintonhq.co.uk/products/li-ning-axforce-80-5u-badminton-racket-unstrung)
- Aeronaut 9000C: [Central Sports](https://centralsports.co.uk/products/li-ning-aeronaut-9000c-aypt405), [Badminton HQ](https://badmintonhq.co.uk/products/li-ning-aeronaut-9000-combat-badminton-racket)
- Halbertec 8000: [Central Sports](https://centralsports.co.uk/products/li-ning-halbertec-8000-aupt369), [Badminton HQ](https://badmintonhq.co.uk/products/li-ning-halbertec-8000-4u-badminton-racket-capri-breeze-neon-iridescent-pink)
- Feather Weight 55: [Central Sports](https://centralsports.co.uk/products/apacs-feather-weight-55), [BadmintonBay](https://www.badmintonbay.com/apacs-feather-weight-55-pink-badminton-racket.html)

## Cước đan vợt (đường kính do hãng công bố)

| Sản phẩm | Đường kính |
|---|---|
| Yonex BG65 | 0,70 mm |
| Yonex BG66 Ultimax | 0,65 mm |
| Yonex BG80 | 0,68 mm |
| Yonex Exbolt 63 | 0,63 mm |
| Victor VBS-66N | 0,66 mm |
| Li-Ning No.1 | 0,65 mm ([BadmintonBay](https://badmintonbay.com/li-ning-no-1-badminton-string.html)) |

## Các nhóm còn lại

Giới tính, màu, loại (quần short/váy, túi/balo, quấn cán PU/khăn, tất/băng cổ tay/lưới) lấy theo tên và mô tả sản phẩm đã có trong catalog.
