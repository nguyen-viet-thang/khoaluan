param([string]$Md = (Join-Path $PSScriptRoot '..\Thiet-ke-CSDL.md'), [string]$Docx = (Join-Path $PSScriptRoot '..\Thiet-ke-CSDL.docx'), [string]$Pdf = (Join-Path $env:TEMP 'Thiet-ke-CSDL.pdf'))
# Chuyển Thiet-ke-CSDL.md -> HTML (Times New Roman) -> Word (.docx) qua Word COM, rồi xuất PDF để kiểm tra.
$lines = [IO.File]::ReadAllLines($Md, [Text.Encoding]::UTF8)
function Enc([string]$s) {
    $s = [Net.WebUtility]::HtmlEncode($s)
    $s = [regex]::Replace($s, '`([^`]+)`', '<span style="font-family:Consolas;font-size:11pt">$1</span>')
    $s -replace '&lt;br&gt;', '<br>'
}
$sb = New-Object Text.StringBuilder
[void]$sb.Append(@'
<html><head><meta charset="utf-8"><style>
body { font-family: "Times New Roman"; font-size: 13pt; }
p { margin: 0 0 6pt 0; line-height: 1.3; text-align: justify; }
h1 { font-family: "Times New Roman"; font-size: 14pt; font-weight: bold; margin: 0 0 8pt 0; }
h2 { font-family: "Times New Roman"; font-size: 13pt; font-weight: bold; margin: 12pt 0 6pt 0; }
table { border-collapse: collapse; width: 100%; }
td, th { border: 1px solid #000; padding: 3pt 4pt; font-family: "Times New Roman"; font-size: 12pt; vertical-align: top; text-align: left; }
th { font-weight: bold; text-align: center; vertical-align: middle; background: #D9D9D9; }
li { font-size: 13pt; margin-bottom: 4pt; text-align: left; }
</style></head><body>
'@)
$i = 0; $inList = $false
while ($i -lt $lines.Count) {
    $l = $lines[$i]
    if ($l -match '^\|') {
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
            $c = 0
            foreach ($cell in $cells) {
                $c++
                $al = if ($tag -eq 'td' -and ($c -eq 1 -or ($cells.Count -eq 6 -and $c -eq 5))) { ' style="text-align:center"' } else { '' }
                [void]$sb.Append("<$tag$al>$(Enc $cell)</$tag>")
            }
            [void]$sb.Append('</tr>')
        }
        [void]$sb.Append('</table>')
        continue
    }
    if ($l -match '^!\[[^\]]*\]\(([^)]+)\)') { [void]$sb.Append("<p>[[IMG:$(Join-Path (Split-Path $Md) $Matches[1])]]</p>"); $i++; continue }
    if ($l -match '^- ') { if (-not $inList) { [void]$sb.Append('<ul>'); $inList = $true }; [void]$sb.Append("<li>$(Enc $l.Substring(2))</li>"); $i++; continue }
    if ($inList) { [void]$sb.Append('</ul>'); $inList = $false }
    if ($l -match '^# (.*)') { [void]$sb.Append("<h1>$(Enc $Matches[1])</h1>") }
    elseif ($l -match '^## (.*)') { [void]$sb.Append("<h2>$(Enc $Matches[1])</h2>") }
    elseif ($l.Trim()) { [void]$sb.Append("<p>$(Enc $l)</p>") }
    $i++
}
if ($inList) { [void]$sb.Append('</ul>') }
[void]$sb.Append('</body></html>')
$html = [IO.Path]::ChangeExtension($Pdf, '.html')
[IO.File]::WriteAllText($html, $sb.ToString(), (New-Object Text.UTF8Encoding($true)))

$word = New-Object -ComObject Word.Application
$word.Visible = $false
try {
    $doc = $word.Documents.Open($html, $false, $true)
    $word.ActiveWindow.View.Type = 3   # print layout
    foreach ($p in $doc.Paragraphs) { if ($p.OutlineLevel -le 2) { $p.KeepWithNext = -1 } }
    $ps = $doc.PageSetup
    $ps.PaperSize = 7                  # A4
    $ps.TopMargin = $word.CentimetersToPoints(2); $ps.BottomMargin = $word.CentimetersToPoints(2)
    $ps.LeftMargin = $word.CentimetersToPoints(3); $ps.RightMargin = $word.CentimetersToPoints(2)
    foreach ($t in $doc.Tables) {
        $t.Borders.Enable = $true
        $t.AutoFitBehavior(2)          # fit window
        $t.Rows.Item(1).HeadingFormat = -1   # lặp lại dòng tiêu đề khi sang trang
        $t.Rows.AllowBreakAcrossPages = 0
        $t.Range.ParagraphFormat.SpaceAfter = 0
        $t.Range.ParagraphFormat.LineSpacingRule = 0
        $t.Range.ParagraphFormat.Alignment = 0
        foreach ($r in 1..$t.Rows.Count) { if ($r -eq 1) { $t.Rows.Item($r).Range.ParagraphFormat.Alignment = 1 } }
        $nc = $t.Columns.Count
        $w = $ps.PageWidth - $ps.LeftMargin - $ps.RightMargin
        if ($nc -eq 3) { $pct = @(0.08, 0.27, 0.65) } else { $pct = @(0.07, 0.17, 0.15, 0.17, 0.10, 0.34) }
        for ($c = 1; $c -le $nc; $c++) { $t.Columns.Item($c).PreferredWidthType = 3; $t.Columns.Item($c).PreferredWidth = $w * $pct[$c - 1] }
        for ($r = 2; $r -le $t.Rows.Count; $r++) {
            $t.Cell($r, 1).Range.ParagraphFormat.Alignment = 1
            if ($nc -eq 6) { $t.Cell($r, 5).Range.ParagraphFormat.Alignment = 1 }
        }
    }
    # ảnh: trang nằm ngang riêng, ảnh vừa bề rộng trang (đoạn tiêu đề ngay trước ảnh cũng sang trang ngang)
    $usable = $ps.PageHeight - $word.CentimetersToPoints(4)   # chiều rộng khi xoay ngang, lề 2 cm
    $paras = @($doc.Paragraphs | Where-Object { $_.Range.Text -like '`[`[IMG:*' })
    foreach ($p in $paras) {
        $path = ([regex]::Match($p.Range.Text, '\[\[IMG:(.*?)\]\]')).Groups[1].Value
        $start = $p.Range
        $k = $p
        while ($k.Previous() -and $k.Previous().OutlineLevel -gt 2) { $k = $k.Previous() }
        if ($k.Previous()) { $k = $k.Previous() }   # tiêu đề mục
        $k.Range.InsertBreak(2)   # ngắt section, trang mới
        $sec = $doc.Sections.Item($doc.Sections.Count)
        $sec.PageSetup.Orientation = 1
        $sec.PageSetup.LeftMargin = $word.CentimetersToPoints(2); $sec.PageSetup.RightMargin = $word.CentimetersToPoints(2)
        $sec.PageSetup.TopMargin = $word.CentimetersToPoints(2); $sec.PageSetup.BottomMargin = $word.CentimetersToPoints(2)
        $r = $p.Range; $r.MoveEnd(1, -1) | Out-Null; $r.Text = ''
        $pic = $doc.InlineShapes.AddPicture($path, $false, $true, $r)
        $wmax = $sec.PageSetup.PageWidth - $sec.PageSetup.LeftMargin - $sec.PageSetup.RightMargin
        $pic.LockAspectRatio = -1; $pic.Width = $wmax
        $p.Range.ParagraphFormat.Alignment = 1
    }
    $doc.SaveAs2($Docx, 16)
    $doc.ExportAsFixedFormat($Pdf, 17)
    $doc.Close($false)
} finally { $word.Quit() }
Remove-Item $html
"ok"
