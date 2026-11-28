param(
    [string]$Md = (Join-Path $PSScriptRoot '..\Kien-truc-he-thong.md'),
    [string]$Docx = (Join-Path $PSScriptRoot '..\Kien-truc-he-thong.docx'),
    [string]$Pdf = (Join-Path $env:TEMP 'Kien-truc-he-thong.pdf')
)
# Chuyển Kien-truc-he-thong.md -> HTML (Times New Roman) -> Word (.docx) qua Word COM, xuất kèm PDF để kiểm tra.
# Hỗ trợ: tiêu đề #/##/###, đoạn văn, **đậm**, *nghiêng*, `mã`, danh sách 1./- (và - lồng 1 cấp), bảng, khối ```mã```, ảnh.
$ErrorActionPreference = 'Stop'
$lines = [IO.File]::ReadAllLines($Md, [Text.Encoding]::UTF8)
function Enc([string]$s) {
    $s = [Net.WebUtility]::HtmlEncode($s)
    # mã: cho phép ngắt dòng sau "/" để tên tệp dài không làm tràn bảng
    $s = [regex]::Replace($s, '`([^`]+)`', { param($m) '<span style="font-family:Consolas;font-size:10.5pt">' + ($m.Groups[1].Value -replace '/', '/&#8203;') + '</span>' })
    $s = [regex]::Replace($s, '\*\*(.+?)\*\*', '<b>$1</b>')
    $s = [regex]::Replace($s, '(?<![\w*])\*([^*]+)\*(?![\w*])', '<i>$1</i>')
    $s -replace '&lt;br&gt;', '<br>'
}
$sb = New-Object Text.StringBuilder
[void]$sb.Append(@'
<html><head><meta charset="utf-8"><style>
body { font-family: "Times New Roman"; font-size: 13pt; }
p { margin: 0 0 6pt 0; line-height: 1.3; text-align: justify; }
h1 { font-family: "Times New Roman"; font-size: 16pt; font-weight: bold; text-align: center; margin: 0 0 10pt 0; }
h2 { font-family: "Times New Roman"; font-size: 14pt; font-weight: bold; margin: 14pt 0 6pt 0; }
h3 { font-family: "Times New Roman"; font-size: 13pt; font-weight: bold; font-style: italic; margin: 10pt 0 4pt 0; }
table { border-collapse: collapse; width: 100%; margin-bottom: 8pt; }
td, th { border: 1px solid #000; padding: 3pt 4pt; font-family: "Times New Roman"; font-size: 12pt; vertical-align: top; text-align: left; }
th { font-weight: bold; text-align: center; vertical-align: middle; background: #D9D9D9; }
li { font-size: 13pt; margin-bottom: 3pt; text-align: justify; }
pre { font-family: Consolas; font-size: 9.5pt; background: #F2F2F2; border: 1px solid #BFBFBF; padding: 4pt; margin: 0 0 8pt 0; }
p.caption { text-align: center; font-style: italic; font-size: 12pt; }
</style></head><body>
'@)
$stack = New-Object System.Collections.Stack   # các thẻ danh sách đang mở: 'ol'/'ul'
function CloseLists([int]$depth) {
    while ($stack.Count -gt $depth) { [void]$sb.Append("</li></$($stack.Pop())>") }
}
$i = 0
while ($i -lt $lines.Count) {
    $l = $lines[$i]
    if ($l -match '^```') {
        CloseLists 0
        $i++; $code = @()
        while ($i -lt $lines.Count -and $lines[$i] -notmatch '^```') { $code += [Net.WebUtility]::HtmlEncode($lines[$i]); $i++ }
        [void]$sb.Append("<pre>$($code -join "`n")</pre>"); $i++; continue
    }
    if ($l -match '^\|') {
        CloseLists 0
        $rows = @()
        while ($i -lt $lines.Count -and $lines[$i] -match '^\|') { $rows += $lines[$i]; $i++ }
        [void]$sb.Append('<table>')
        $r = 0
        foreach ($row in $rows) {
            $r++
            if ($r -eq 2) { continue }
            $cells = $row.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() }
            $tag = if ($r -eq 1) { 'th' } else { 'td' }
            [void]$sb.Append('<tr>')
            foreach ($cell in $cells) { [void]$sb.Append("<$tag>$(Enc $cell)</$tag>") }
            [void]$sb.Append('</tr>')
        }
        [void]$sb.Append('</table>')
        continue
    }
    if ($l -match '^!\[[^\]]*\]\(([^)]+)\)') { CloseLists 0; [void]$sb.Append("<p>[[IMG:$(Join-Path (Split-Path $Md) $Matches[1])]]</p>"); $i++; continue }
    if ($l -match '^( *)(\d+\.|-) (.*)') {
        $depth = [Math]::Min(2, [int][Math]::Floor($Matches[1].Length / 2) + 1)
        $kind = if ($Matches[2] -eq '-') { 'ul' } else { 'ol' }
        $text = $Matches[3]
        if ($stack.Count -ge $depth) {
            CloseLists $depth
            if ($stack.Peek() -ne $kind) { CloseLists ($depth - 1); [void]$sb.Append("<$kind>"); $stack.Push($kind) }
            else { [void]$sb.Append('</li>') }
        } else {
            while ($stack.Count -lt $depth) { [void]$sb.Append("<$kind>"); $stack.Push($kind) }
        }
        [void]$sb.Append("<li>$(Enc $text)"); $i++; continue
    }
    if (-not $l.Trim()) { $i++; continue }
    CloseLists 0
    if ($l -match '^# (.*)') { [void]$sb.Append("<h1>$(Enc $Matches[1])</h1>") }
    elseif ($l -match '^## (.*)') { [void]$sb.Append("<h2>$(Enc $Matches[1])</h2>") }
    elseif ($l -match '^### (.*)') { [void]$sb.Append("<h3>$(Enc $Matches[1])</h3>") }
    elseif ($l -match '^\*([^*].*)\*$') { [void]$sb.Append("<p class=""caption"">$(Enc $Matches[1])</p>") }
    else { [void]$sb.Append("<p>$(Enc $l)</p>") }
    $i++
}
CloseLists 0
[void]$sb.Append('</body></html>')
$html = [IO.Path]::ChangeExtension($Pdf, '.html')
[IO.File]::WriteAllText($html, $sb.ToString(), (New-Object Text.UTF8Encoding($true)))

$word = New-Object -ComObject Word.Application
$word.Visible = $false
try {
    $doc = $word.Documents.Open($html, $false, $true)
    $word.ActiveWindow.View.Type = 3   # bố cục trang in
    foreach ($p in $doc.Paragraphs) { if ($p.OutlineLevel -le 3) { $p.KeepWithNext = -1 } }
    $ps = $doc.PageSetup
    $ps.PaperSize = 7                  # A4
    $ps.TopMargin = $word.CentimetersToPoints(2); $ps.BottomMargin = $word.CentimetersToPoints(2)
    $ps.LeftMargin = $word.CentimetersToPoints(3); $ps.RightMargin = $word.CentimetersToPoints(2)
    $w = $ps.PageWidth - $ps.LeftMargin - $ps.RightMargin
    foreach ($t in $doc.Tables) {
        $t.Borders.Enable = $true
        $t.AutoFitBehavior(2)          # vừa bề rộng trang
        $t.PreferredWidthType = 2; $t.PreferredWidth = 100   # 100% bề rộng, không tràn lề
        if ($t.Columns.Count -eq 5) {  # bảng tra cứu: chia cột cố định
            $pct = @(0.18, 0.22, 0.22, 0.20, 0.18)
            for ($c = 1; $c -le 5; $c++) { $t.Columns.Item($c).PreferredWidthType = 2; $t.Columns.Item($c).PreferredWidth = 100 * $pct[$c - 1] }
            $t.AllowAutoFit = $false   # cột cố định: tên dài được ngắt dòng thay vì đẩy bảng tràn lề
            $t.Range.Font.Size = 11
            foreach ($cell in $t.Range.Cells) { foreach ($run in $cell.Range.Words) { if ($run.Font.Name -eq 'Consolas') { $run.Font.Size = 9 } } }
        }
        $t.Rows.Item(1).HeadingFormat = -1   # lặp dòng tiêu đề khi sang trang
        $t.Range.ParagraphFormat.SpaceAfter = 0
        $t.Range.ParagraphFormat.LineSpacingRule = 0
        $t.Range.ParagraphFormat.Alignment = 0
        $t.Rows.Item(1).Range.ParagraphFormat.Alignment = 1
    }
    # Ảnh sơ đồ: đặt trên trang nằm ngang riêng để chữ trong sơ đồ đủ lớn, rồi quay lại trang dọc.
    $paras = @($doc.Paragraphs | Where-Object { $_.Range.Text -like '`[`[IMG:*' })
    foreach ($p in $paras) {
        $path = ([regex]::Match($p.Range.Text, '\[\[IMG:(.*?)\]\]')).Groups[1].Value
        $caption = $p.Next()
        $p.Range.InsertBreak(2)        # section mới trước ảnh
        $caption.Range.InsertParagraphAfter()
        $caption.Next().Range.InsertBreak(2)  # section mới sau chú thích
        $n = $doc.Sections.Count
        $imgSec = $null
        foreach ($s in $doc.Sections) { if ($s.Range.Start -le $p.Range.Start -and $s.Range.End -ge $p.Range.End) { $imgSec = $s } }
        $imgSec.PageSetup.Orientation = 1
        $imgSec.PageSetup.LeftMargin = $word.CentimetersToPoints(1.5); $imgSec.PageSetup.RightMargin = $word.CentimetersToPoints(1.5)
        $imgSec.PageSetup.TopMargin = $word.CentimetersToPoints(1.5); $imgSec.PageSetup.BottomMargin = $word.CentimetersToPoints(1.5)
        $r = $p.Range; $r.MoveEnd(1, -1) | Out-Null; $r.Text = ''
        $pic = $doc.InlineShapes.AddPicture($path, $false, $true, $r)
        $pic.LockAspectRatio = -1
        $pic.Width = $imgSec.PageSetup.PageWidth - $imgSec.PageSetup.LeftMargin - $imgSec.PageSetup.RightMargin
        $maxH = $imgSec.PageSetup.PageHeight - $imgSec.PageSetup.TopMargin - $imgSec.PageSetup.BottomMargin - 40
        if ($pic.Height -gt $maxH) { $pic.Height = $maxH }
        $p.Range.ParagraphFormat.Alignment = 1
        $p.Range.ParagraphFormat.KeepWithNext = -1
    }
    $doc.SaveAs2($Docx, 16)
    $doc.ExportAsFixedFormat($Pdf, 17)
    $doc.Close($false)
} finally { $word.Quit() }
Remove-Item $html
"docx: $Docx"
"pdf (kiểm tra): $Pdf"
