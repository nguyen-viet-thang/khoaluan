# Creates sample DELIVERED (Completed, COD paid) orders to test the warranty/claim flow.
# Every order is tagged with Note = '[Dữ liệu mẫu bảo hành]'. Re-running first deletes the previous sample orders
# that have no warranty claim yet. Stock / inventory history are NOT touched (test data only).
# Usage (from the project folder):  powershell -ExecutionPolicy Bypass -File Docs\Seed-DeliveredOrders.ps1
param([string]$Server = '(localdb)\MSSQLLocalDB', [string]$Database = 'PopPopShop')

$tag = '[Dữ liệu mẫu bảo hành]'
$alphabet = 'ABCDEFGHJKMNPQRSTUVWXYZ23456789'   # same alphabet as ShortCodeGenerator (no 0/O/1/I/L)
$rng = New-Object System.Random

# DaysAgo = delivery date relative to today. Lines: variant id + quantity.
#  1 day   : every receipt window (3-7 days) still open, racket + shoes warranty open
#  5 days  : only Victor (7-day receipt window) open; Apacs racket has no warranty -> cannot claim
#  20 days : receipt windows closed; Li-Ning racket/bag (90) and shoes (30) still under warranty
#  120 days: everything expired
$scenarios = @(
    @{ User = '0901234501'; DaysAgo = 1;   Lines = @(@(1, 1), @(82, 2), @(24, 1)) },
    @{ User = '0901234501'; DaysAgo = 5;   Lines = @(@(13, 1), @(23, 1), @(33, 1)) },
    @{ User = '0901234501'; DaysAgo = 20;  Lines = @(@(18, 1), @(81, 1), @(43, 1), @(72, 2)) },
    @{ User = '0901234501'; DaysAgo = 120; Lines = @(@(2, 1), @(13, 1)) },
    @{ User = '0912345602'; DaysAgo = 2;   Lines = @(@(13, 1), @(90, 3)) },
    @{ User = 'ringringbananaphone375@gmail.com'; DaysAgo = 1;  Lines = @(@(1, 1), @(24, 1)) },
    @{ User = 'ringringbananaphone375@gmail.com'; DaysAgo = 20; Lines = @(@(18, 1), @(43, 1)) }
)
$addresses = @(
    '12 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh',
    '45 Trần Phú, Phường Điện Biên, Quận Ba Đình, Hà Nội',
    '78 Lê Duẩn, Phường Thạch Thang, Quận Hải Châu, Đà Nẵng'
)

$sql = New-Object System.Text.StringBuilder
[void]$sql.AppendLine("SET NOCOUNT ON; SET XACT_ABORT ON; BEGIN TRAN;")
[void]$sql.AppendLine("DELETE oi FROM OrderItems oi JOIN Orders o ON o.Id = oi.OrderId WHERE o.Note = N'$tag' AND NOT EXISTS (SELECT 1 FROM WarrantyClaims c WHERE c.OrderId = o.Id);")
[void]$sql.AppendLine("DELETE o FROM Orders o WHERE o.Note = N'$tag' AND NOT EXISTS (SELECT 1 FROM WarrantyClaims c WHERE c.OrderId = o.Id) AND NOT EXISTS (SELECT 1 FROM OrderItems oi WHERE oi.OrderId = o.Id);")
[void]$sql.AppendLine("DECLARE @u nvarchar(450), @name nvarchar(256), @phone nvarchar(32), @o int, @delivered datetime2, @sub decimal(18,2);")

$i = 0
foreach ($s in $scenarios) {
    $delivered = (Get-Date).ToUniversalTime().AddDays(-$s.DaysAgo).AddHours(-2)
    $created = $delivered.AddDays(-3)
    $code = 'PP-' + $created.AddHours(7).ToString('yyMMdd') + '-' + (-join (1..6 | ForEach-Object { $alphabet[$rng.Next($alphabet.Length)] }))
    $address = $addresses[$i % $addresses.Count]; $i++
    $d = $delivered.ToString('yyyy-MM-ddTHH:mm:ss'); $c = $created.ToString('yyyy-MM-ddTHH:mm:ss')
    [void]$sql.AppendLine(@"
SELECT @u = Id, @name = ISNULL(FullName, UserName), @phone = ISNULL(PhoneNumber, '0901234567') FROM AspNetUsers WHERE UserName = N'$($s.User)';
IF @u IS NULL PRINT N'Bỏ qua: không có tài khoản $($s.User)'
ELSE BEGIN
  INSERT INTO Orders (UserId, OrderCode, Status, TotalAmount, ShippingFee, Note, RecipientName, RecipientPhone, ShippingAddress, CreatedAt, UpdatedAt, PaidAt, PaymentMethod, PaymentStatus)
  VALUES (@u, '$code', 5, 0, 0, N'$tag', @name, @phone, N'$address', '$c', '$d', '$d', 1, 3);
  SET @o = SCOPE_IDENTITY();
"@)
    foreach ($line in $s.Lines) {
        [void]$sql.AppendLine("  INSERT INTO OrderItems (OrderId, ProductVariantId, ProductName, SKU, UnitPrice, Quantity, TotalPrice) SELECT @o, v.Id, p.Name, v.SKU, v.Price, $($line[1]), v.Price * $($line[1]) FROM ProductVariants v JOIN Products p ON p.Id = v.ProductId WHERE v.Id = $($line[0]);")
    }
    [void]$sql.AppendLine(@"
  SELECT @sub = SUM(TotalPrice) FROM OrderItems WHERE OrderId = @o;
  UPDATE Orders SET ShippingFee = CASE WHEN @sub >= 500000 THEN 0 ELSE 30000 END,
                    TotalAmount = @sub + CASE WHEN @sub >= 500000 THEN 0 ELSE 30000 END WHERE Id = @o;
  PRINT N'$code  ($($s.User), giao $($s.DaysAgo) ngày trước)';
END
SET @u = NULL;
"@)
}
[void]$sql.AppendLine("COMMIT;")

$file = Join-Path ([IO.Path]::GetTempPath()) 'poppop-seed-delivered-orders.sql'
[IO.File]::WriteAllText($file, $sql.ToString(), (New-Object Text.UTF8Encoding $true))
sqlcmd -S $Server -d $Database -I -b -f 65001 -i $file
Remove-Item $file
