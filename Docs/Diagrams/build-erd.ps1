param(
    [string]$Snapshot = (Join-Path $PSScriptRoot '..\..\Data\Migrations\PopPopDbContextModelSnapshot.cs'),
    [string]$OutDrawio = (Join-Path $PSScriptRoot '..\erd.drawio'),
    [string]$OutPng = (Join-Path $PSScriptRoot '..\erd.png')
)
# Sơ đồ ERD (mức vật lý) của CSDL PopPopShop, sinh từ model snapshot của EF Core nên tên bảng, cột, kiểu dữ liệu,
# khóa chính và khóa ngoại luôn khớp với CSDL thật. Bỏ qua 4 bảng phụ của Identity (claims, logins, tokens, role claims).
# Bảng chỉ ghi tên cột (PK gạch chân); kiểu dữ liệu xem trong snapshot hoặc bảng mô tả.
# Ký hiệu chân quạ: một đầu "một và chỉ một" ở bảng cha (PK), đầu "nhiều" ở bảng con (FK);
# Carts.UserId là duy nhất nên đầu bảng con là "không hoặc một". Đường nét đứt = liên kết logic không có khóa ngoại.
# PNG được chụp từ trình xem chính thức của draw.io (viewer.diagrams.net) chạy trong Edge ẩn, nên giống hệt khi mở file.
$utf8 = New-Object System.Text.UTF8Encoding($false)
function Esc([string]$s) { [System.Security.SecurityElement]::Escape($s) }

# ---------------------------------------------------------------- đọc snapshot
$src = [IO.File]::ReadAllText($Snapshot)
$tables = [ordered]@{}   # entity CLR name -> table
$fks = @()
$skip = 'IdentityRoleClaim', 'IdentityUserClaim', 'IdentityUserLogin', 'IdentityUserToken'
foreach ($m in [regex]::Matches($src, 'modelBuilder\.Entity\("([^"]+)", b =>\s*\{(.*?)\r?\n\s*\}\);', 'Singleline')) {
    $clr = $m.Groups[1].Value; $body = $m.Groups[2].Value
    if ($skip | Where-Object { $clr -like "*$_*" }) { continue }
    if (-not $tables.Contains($clr)) {
        $tables[$clr] = [pscustomobject]@{ Clr = $clr; Name = $null; Cols = New-Object System.Collections.ArrayList; Pk = @(); Unique = @() }
    }
    $t = $tables[$clr]
    $tm = [regex]::Match($body, 'b\.ToTable\("([^"]+)"'); if ($tm.Success) { $t.Name = $tm.Groups[1].Value }
    foreach ($p in [regex]::Matches($body, 'b\.Property<([^>]+)>\("([^"]+)"\)((?:\s*\.[A-Za-z]+\([^;]*?\))*);')) {
        $ty = [regex]::Match($p.Groups[3].Value, 'HasColumnType\("([^"]+)"\)').Groups[1].Value
        $req = $p.Groups[3].Value -match 'IsRequired' -or $p.Groups[1].Value -notmatch '\?$|^string$'
        [void]$t.Cols.Add([pscustomobject]@{ Name = $p.Groups[2].Value; Type = $ty; Null = -not $req })
    }
    $km = [regex]::Match($body, 'b\.HasKey\(([^)]+)\)'); if ($km.Success) { $t.Pk = @([regex]::Matches($km.Groups[1].Value, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value }) }
    foreach ($u in [regex]::Matches($body, 'b\.HasIndex\(([^)]+)\)\s*\.IsUnique\(\)')) {
        $cols = @([regex]::Matches($u.Groups[1].Value, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value }); if ($cols.Count -eq 1) { $t.Unique += $cols[0] }
    }
    foreach ($r in [regex]::Matches($body, 'b\.HasOne\("([^"]+)",[^)]*\)\s*\.With(?:Many|One)\([^)]*\)\s*\.HasForeignKey\("([^"]+)"\)\s*\.OnDelete\(DeleteBehavior\.(\w+)\)')) {
        $fks += [pscustomobject]@{ Child = $clr; Parent = $r.Groups[1].Value; Col = $r.Groups[2].Value; Delete = $r.Groups[3].Value; Logical = $false }
    }
}
# UserRoles: snapshot dùng tên lớp Identity
foreach ($t in $tables.Values) { if (-not $t.Name) { $t.Name = ($t.Clr -split '\.')[-1] } }
$byName = @{}; foreach ($t in $tables.Values) { $byName[$t.Name] = $t }
foreach ($f in $fks) { $f | Add-Member Child2 $tables[$f.Child].Name; $f | Add-Member Parent2 $tables[$f.Parent].Name }
# liên kết logic (lưu mã người dùng nhưng không có khóa ngoại)
$fks += [pscustomobject]@{ Child2 = 'WarrantyClaimMedia'; Parent2 = 'AspNetUsers'; Col = 'UploadedByUserId'; Delete = ''; Logical = $true }
$fks += [pscustomobject]@{ Child2 = 'WarrantyClaimHistory'; Parent2 = 'AspNetUsers'; Col = 'ActorUserId'; Delete = ''; Logical = $true }

# ---------------------------------------------------------------- bố cục: cột (0..6) và thứ tự bảng trong cột
# Mỗi liên kết đi vuông góc qua "làn" riêng trong khe giữa hai cột; liên kết nối hai cột không kề nhau
# đi vòng qua hành lang phía trên/dưới toàn bộ các bảng, nên không đường nào cắt ngang qua bảng.
$layout = [ordered]@{
    0 = 'AspNetRoles', 'AspNetUserRoles', 'Notifications'
    1 = , 'AspNetUsers'
    2 = 'Carts', 'Orders', 'WarrantyClaims'
    3 = 'CartItems', 'OrderItems', 'WarrantyClaimMedia', 'WarrantyClaimHistory'
    4 = 'ProductVariants', 'InventoryTransactions', 'VariantSpecValues', 'DefectTypes'
    5 = 'Products', 'ProductImages', 'ProductSpecValues', 'SpecOptions'
    6 = 'Categories', 'Brands', 'SpecAttributes', 'WarrantyPolicies'
}
# phía đặt làn cho liên kết giữa hai bảng cùng cột
$sameSide = @{ 0 = 'R'; 1 = 'R'; 2 = 'L'; 3 = 'R'; 4 = 'R'; 5 = 'R'; 6 = 'R' }
$fs = 14; $rowH = 26; $headH = 34; $keyW = 56; $W = 300; $GAP = 190; $VGAP = 60; $TOP = 140
$pitch = $W + $GAP
$pos = [ordered]@{}; $colOf = @{}
foreach ($c in $layout.Keys) {
    $y = $TOP
    foreach ($n in $layout[$c]) {
        if (-not $byName[$n]) { throw "unknown table $n" }
        $pos[$n] = @((40 + $c * $pitch), $y); $colOf[$n] = $c
        $y += $headH + $rowH * $byName[$n].Cols.Count + $VGAP
    }
}
$bottom = 0; foreach ($n in $pos.Keys) { $b = $pos[$n][1] + $headH + $rowH * $byName[$n].Cols.Count; if ($b -gt $bottom) { $bottom = $b } }

# ---------------------------------------------------------------- draw.io
$cells = New-Object System.Text.StringBuilder
function Add([string]$x) { [void]$cells.AppendLine("        $x") }
$font = 'fontFamily=Arial;'
$rowId = @{}; $rowY = @{}
foreach ($name in $pos.Keys) {
    $t = $byName[$name]
    $x, $y = $pos[$name]
    $fkCols = @($fks | Where-Object { $_.Child2 -eq $name -and -not $_.Logical } | ForEach-Object { $_.Col })
    $ordered = @($t.Pk | ForEach-Object { $c = $_; $t.Cols | Where-Object Name -eq $c }) +
               @($t.Cols | Where-Object { $t.Pk -notcontains $_.Name -and $fkCols -contains $_.Name }) +
               @($t.Cols | Where-Object { $t.Pk -notcontains $_.Name -and $fkCols -notcontains $_.Name })
    $h = $headH + $rowH * $ordered.Count
    $tid = "t_$name"
    Add "<mxCell id=""$tid"" value=""$(Esc $name)"" style=""shape=table;startSize=$headH;container=1;collapsible=0;childLayout=tableLayout;fixedRows=1;rowLines=0;fontStyle=1;align=center;resizeLast=1;html=1;fillColor=#E7E6E6;strokeColor=#000000;strokeWidth=1.5;fontSize=15;$font"" vertex=""1"" parent=""1""><mxGeometry x=""$x"" y=""$y"" width=""$W"" height=""$h"" as=""geometry"" /></mxCell>"
    $i = 0
    foreach ($c in $ordered) {
        $isPk = $t.Pk -contains $c.Name; $isFk = $fkCols -contains $c.Name
        $key = if ($isPk -and $isFk) { 'PK, FK' } elseif ($isPk) { 'PK' } elseif ($isFk) { 'FK' } else { '' }
        $rid = "r_$($name)_$($c.Name)"; $rowId["$name.$($c.Name)"] = $rid; $rowY["$name.$($c.Name)"] = $y + $headH + $rowH * ($i + 0.5)
        $line = if ($i -eq $t.Pk.Count - 1 -and $t.Pk.Count) { 'bottom=1;' } else { 'bottom=0;' }
        $ry = $headH + $rowH * $i
        Add "<mxCell id=""$rid"" value="""" style=""shape=tableRow;horizontal=0;startSize=0;swimlaneHead=0;swimlaneBody=0;fillColor=none;collapsible=0;dropTarget=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;top=0;left=0;right=0;${line}strokeColor=#000000;html=1;"" vertex=""1"" parent=""$tid""><mxGeometry y=""$ry"" width=""$W"" height=""$rowH"" as=""geometry"" /></mxCell>"
        $kStyle = "shape=partialRectangle;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;fontStyle=1;overflow=hidden;whiteSpace=wrap;html=1;fontSize=$fs;$font"
        $nStyle = "shape=partialRectangle;connectable=0;fillColor=none;top=0;left=0;bottom=0;right=0;align=left;spacingLeft=8;overflow=hidden;whiteSpace=wrap;html=1;fontSize=$fs;$font" + $(if ($isPk) { 'fontStyle=4;' } else { '' })
        Add "<mxCell id=""${rid}_k"" value=""$key"" style=""$kStyle"" vertex=""1"" parent=""$rid""><mxGeometry width=""$keyW"" height=""$rowH"" as=""geometry""><mxRectangle width=""$keyW"" height=""$rowH"" as=""alternateBounds"" /></mxGeometry></mxCell>"
        Add "<mxCell id=""${rid}_n"" value=""$(Esc $c.Name)"" style=""$nStyle"" vertex=""1"" parent=""$rid""><mxGeometry x=""$keyW"" width=""$($W - $keyW)"" height=""$rowH"" as=""geometry""><mxRectangle width=""$($W - $keyW)"" height=""$rowH"" as=""alternateBounds"" /></mxGeometry></mxCell>"
        $i++
    }
}

# ---------------------------------------------------------------- định tuyến
function ColX($c) { 40 + $c * $pitch }
$routes = @()
foreach ($f in $fks) {
    $pk = $byName[$f.Parent2].Pk[0]
    $r = [pscustomobject]@{ F = $f; Src = $rowId["$($f.Child2).$($f.Col)"]; Dst = $rowId["$($f.Parent2).$pk"]
        Yc = $rowY["$($f.Child2).$($f.Col)"]; Yp = $rowY["$($f.Parent2).$pk"]; Cc = $colOf[$f.Child2]; Cp = $colOf[$f.Parent2]
        SideC = ''; SideP = ''; Gap1 = 0; Gap2 = $null; Lane1 = 0; Lane2 = 0; Corr = $null }
    if (-not $r.Src -or -not $r.Dst) { throw "missing row $($f.Child2).$($f.Col)" }
    if ($r.Cc -eq $r.Cp) {
        $s = $sameSide[$r.Cc]; $r.SideC = $s; $r.SideP = $s; $r.Gap1 = if ($s -eq 'R') { $r.Cc } else { $r.Cc - 1 }
    } elseif ([math]::Abs($r.Cc - $r.Cp) -eq 1) {
        if ($r.Cp -gt $r.Cc) { $r.SideC = 'R'; $r.SideP = 'L'; $r.Gap1 = $r.Cc } else { $r.SideC = 'L'; $r.SideP = 'R'; $r.Gap1 = $r.Cp }
    } else {
        if ($r.Cp -gt $r.Cc) { $r.SideC = 'R'; $r.SideP = 'L'; $r.Gap1 = $r.Cc; $r.Gap2 = $r.Cp - 1 } else { $r.SideC = 'L'; $r.SideP = 'R'; $r.Gap1 = $r.Cc - 1; $r.Gap2 = $r.Cp }
    }
    $routes += $r
}
# hành lang trên/dưới cho liên kết xa
$nTop = 0; $nBot = 0
foreach ($r in ($routes | Where-Object { $null -ne $_.Gap2 })) {
    if (($r.Yc + $r.Yp) / 2 -gt ($TOP + $bottom) / 2) { $r.Corr = $bottom + 40 + 22 * $nBot; $nBot++ } else { $r.Corr = $TOP - 40 - 22 * $nTop; $nTop++ }
}
# làn trong từng khe: mỗi đoạn dọc một làn riêng, xếp theo khoảng dọc để giảm chỗ cắt
$uses = @()
foreach ($r in $routes) {
    if ($null -eq $r.Gap2) { $uses += [pscustomobject]@{ R = $r; Which = 1; Gap = $r.Gap1; Y1 = [math]::Min($r.Yc, $r.Yp); Y2 = [math]::Max($r.Yc, $r.Yp) } }
    else {
        $uses += [pscustomobject]@{ R = $r; Which = 1; Gap = $r.Gap1; Y1 = [math]::Min($r.Yc, $r.Corr); Y2 = [math]::Max($r.Yc, $r.Corr) }
        $uses += [pscustomobject]@{ R = $r; Which = 2; Gap = $r.Gap2; Y1 = [math]::Min($r.Yp, $r.Corr); Y2 = [math]::Max($r.Yp, $r.Corr) }
    }
}
foreach ($g in ($uses | Group-Object Gap)) {
    $list = @($g.Group | Sort-Object { $_.Y2 - $_.Y1 })
    $n = $list.Count; $gx = (ColX ([int]$g.Name)) + $W
    $step = [math]::Min(16, ($GAP - 40) / [math]::Max(1, $n))
    $x0 = $gx + $GAP / 2 - ($n - 1) * $step / 2
    # làn ngắn nằm giữa; xen kẽ hai bên để các đoạn ngang ngắn ít cắt nhau
    $order = @(); for ($k = 0; $k -lt $n; $k++) { $order += $(if ($k % 2) { [math]::Floor($n / 2) + [math]::Ceiling($k / 2) } else { [math]::Floor($n / 2) - $k / 2 }) }
    for ($k = 0; $k -lt $n; $k++) {
        $slot = [math]::Max(0, [math]::Min($n - 1, $order[$k]))
        $x = [math]::Round($x0 + $slot * $step, 1)
        if ($list[$k].Which -eq 1) { $list[$k].R.Lane1 = $x } else { $list[$k].R.Lane2 = $x }
    }
}
$k = 0
foreach ($r in $routes) {
    $k++; $f = $r.F
    $pts = if ($null -eq $r.Gap2) { @(@($r.Lane1, $r.Yc), @($r.Lane1, $r.Yp)) } else { @(@($r.Lane1, $r.Yc), @($r.Lane1, $r.Corr), @($r.Lane2, $r.Corr), @($r.Lane2, $r.Yp)) }
    $ex = if ($r.SideC -eq 'R') { 1 } else { 0 }; $en = if ($r.SideP -eq 'R') { 1 } else { 0 }
    $childEnd = if ($byName[$f.Child2].Unique -contains $f.Col) { 'ERzeroToOne' } else { 'ERmany' }
    $dash = if ($f.Logical) { 'dashed=1;' } else { '' }
    $mid = '<Array as="points">' + (($pts | ForEach-Object { "<mxPoint x=""$($_[0])"" y=""$($_[1])"" />" }) -join '') + '</Array>'
    Add "<mxCell id=""e$k"" style=""edgeStyle=none;rounded=0;html=1;jumpStyle=arc;jumpSize=10;exitX=$ex;exitY=0.5;exitDx=0;exitDy=0;entryX=$en;entryY=0.5;entryDx=0;entryDy=0;startArrow=$childEnd;startFill=0;endArrow=ERmandOne;endFill=0;startSize=12;endSize=12;strokeColor=#000000;strokeWidth=1.3;$dash"" edge=""1"" parent=""1"" source=""$($r.Src)"" target=""$($r.Dst)""><mxGeometry relative=""1"" as=""geometry"">$mid</mxGeometry></mxCell>"
}
$pageW = (ColX 6) + $W + $GAP
$pageH = $bottom + 60 + 22 * $nBot
$xml = @"
<mxfile host="app.diagrams.net" type="device">
  <diagram id="erd" name="ERD PopPopShop">
    <mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="$pageW" pageHeight="$pageH" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />
$($cells.ToString().TrimEnd())
      </root>
    </mxGraphModel>
  </diagram>
</mxfile>
"@
[IO.File]::WriteAllText($OutDrawio, $xml, $utf8)
[xml]$null = $xml
Write-Output ("erd: {0} bảng, {1} liên kết ({2} khóa ngoại), {3}x{4}" -f $pos.Count, $fks.Count, @($fks | Where-Object { -not $_.Logical }).Count, $pageW, $pageH)
$missing = @($byName.Keys | Where-Object { -not $pos.Contains($_) }); if ($missing) { Write-Output "NOT PLACED: $($missing -join ', ')" }

# ---------------------------------------------------------------- PNG qua trình xem draw.io
if ($OutPng) {
    $edge = @("${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe", "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1
    $json = @{ xml = $xml; toolbar = ''; highlight = 'none'; nav = $false; 'auto-fit' = $false } | ConvertTo-Json -Compress
    $html = "<!doctype html><html><head><meta charset='utf-8'><style>body{margin:0;background:#fff}</style></head><body><div class='mxgraph' data-mxgraph=""$([Net.WebUtility]::HtmlEncode($json))""></div><script src='https://viewer.diagrams.net/js/viewer-static.min.js'></script></body></html>"
    $tmp = Join-Path $env:TEMP 'erd-view.html'
    [IO.File]::WriteAllText($tmp, $html, $utf8)
    & $edge --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=2 --virtual-time-budget=20000 "--screenshot=$OutPng" "--window-size=$pageW,$pageH" ([Uri]$tmp).AbsoluteUri 2>$null | Out-Null
    Write-Output "png: $OutPng"
}
