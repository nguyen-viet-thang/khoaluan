param([string]$Data, [string]$OutDrawio, [int]$FontSize = 22)
# Context-level DFD (one process circle, external entities in the four corners).
# Data lines (UTF-8):  CENTER|title|subtitle   ENTITY|TL/TR/BL/BR|name|subtitle   OUT|pos|label (entity -> system)   IN|pos|label (system -> entity)
# Geometry is computed for the top-left corner and mirrored for the others, so connectors never cross:
#   OUT: leaves the entity's inner side, runs horizontally, then vertically onto the circle's top/bottom arc.
#   IN : leaves the circle's left/right arc horizontally, then vertically into the entity's top/bottom edge.
# Everything is sized from the font size and the real label lengths, as compact as possible, so the text stays
# large once the diagram is scaled down to the page width in Word (user request: biggest readable font).
# Black & white, Times New Roman, labels sit on the horizontal segment with a white background.
$utf8 = New-Object System.Text.UTF8Encoding($false)
function Esc([string]$s) { [System.Security.SecurityElement]::Escape($s) }

$fs = $FontSize
$charW = 0.53 * $fs            # average glyph width of Times New Roman with Vietnamese marks (measured on the 14 px build)
$s = [math]::Ceiling(1.6 * $fs) # pitch between two parallel labelled lines
$pad = [math]::Ceiling(0.6 * $fs)
$labelH = [math]::Ceiling(1.3 * $fs)
$font = 'fontFamily=Times New Roman;'
$boxStyle = "rounded=0;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#000000;strokeWidth=2;fontSize=$fs;spacing=8;$font"
$circleStyle = "ellipse;whiteSpace=wrap;html=1;aspect=fixed;fillColor=#ffffff;strokeColor=#000000;strokeWidth=2;fontSize=$fs;spacing=20;$font"
$edgeStyle = 'edgeStyle=none;rounded=0;html=1;endArrow=classic;endFill=1;endSize=10;strokeColor=#000000;strokeWidth=2;'
$labelStyle = "text;html=1;align=center;verticalAlign=middle;fillColor=#ffffff;strokeColor=none;spacing=0;fontSize=$fs;$font"
function Width([string]$t) { [math]::Ceiling($t.Length * $charW) + 6 }

$center = $null; $entities = [ordered]@{}; $outs = @{}; $ins = @{}
foreach ($line in [IO.File]::ReadAllLines($Data, $utf8)) {
    if (-not $line.Trim()) { continue }
    $f = $line.Split('|')
    switch ($f[0]) {
        'CENTER' { $center = $f }
        'ENTITY' { $entities[$f[1]] = $f; $outs[$f[1]] = @(); $ins[$f[1]] = @() }
        'OUT'    { $outs[$f[1]] += $f[2] }
        'IN'     { $ins[$f[1]] += $f[2] }
    }
}
$nMax = ($outs.Values | ForEach-Object { $_.Count } | Measure-Object -Maximum).Maximum
$mMax = ($ins.Values | ForEach-Object { $_.Count } | Measure-Object -Maximum).Maximum

# ---- sizes (top-left reference corner)
$ex = 20; $ey = 20
$ew = [math]::Max(10 * $fs, ($mMax + 1) * 1.4 * $fs)
$eh = ($nMax + 1) * $s
$r = [math]::Ceiling([math]::Max($s * [math]::Max($mMax - 1, 1) / 0.55, 8 * $fs))
function OutT([int]$i, [int]$n) { if ($n -gt 1) { $i / ($n - 1) } else { 0.5 } }
# smallest cx that leaves room for every label in every corner
$cx = 0
foreach ($pos in $entities.Keys) {
    $n = $outs[$pos].Count
    for ($i = 0; $i -lt $n; $i++) {
        $need = $ex + $ew + (Width $outs[$pos][$i]) + 2 * $pad + $r * (0.1 + 0.65 * (OutT $i $n))
        $cx = [math]::Max($cx, $need)
    }
    $m = $ins[$pos].Count
    for ($j = 0; $j -lt $m; $j++) {
        $x = $ex + ($j + 1) * $ew / ($m + 1); $t = OutT $j $m
        $need = $x + (Width $ins[$pos][$j]) + 2 * $pad + $r * [math]::Sqrt(1 - [math]::Pow(0.05 + 0.55 * $t, 2))
        $cx = [math]::Max($cx, $need)
    }
}
$cx = [math]::Ceiling($cx)
$cy = [math]::Ceiling($ey + $eh + $s + 0.6 * $r)

$cells = New-Object System.Text.StringBuilder
$segs = @(); $labels = @(); $problems = @(); $k = 0
function Add-Cell([string]$xml) { [void]$cells.AppendLine("        $xml") }
function MX([string]$pos, [double]$x) { if ($pos[1] -eq 'R') { 2 * $cx - $x } else { $x } }
function MY([string]$pos, [double]$y) { if ($pos[0] -eq 'B') { 2 * $cy - $y } else { $y } }

Add-Cell "<mxCell id=""sys"" value=""$(Esc "<b>$($center[1])</b><br>$($center[2])")"" style=""$circleStyle"" vertex=""1"" parent=""1""><mxGeometry x=""$($cx - $r)"" y=""$($cy - $r)"" width=""$(2 * $r)"" height=""$(2 * $r)"" as=""geometry"" /></mxCell>"

foreach ($pos in $entities.Keys) {
    $e = $entities[$pos]
    $x0 = [math]::Min((MX $pos $ex), (MX $pos ($ex + $ew))); $y0 = [math]::Min((MY $pos $ey), (MY $pos ($ey + $eh)))
    Add-Cell "<mxCell id=""ent$pos"" value=""$(Esc "<b>$($e[2])</b><br>$($e[3])")"" style=""$boxStyle"" vertex=""1"" parent=""1""><mxGeometry x=""$x0"" y=""$y0"" width=""$ew"" height=""$eh"" as=""geometry"" /></mxCell>"

    # ---- entity -> system
    $n = $outs[$pos].Count
    for ($i = 0; $i -lt $n; $i++) {
        $y = $ey + ($i + 1) * $eh / ($n + 1)
        $X = $cx - $r * (0.1 + 0.65 * (OutT $i $n))              # top flow -> innermost, lower flows nest outside
        $arcY = $cy - [math]::Sqrt($r * $r - ($X - $cx) * ($X - $cx))
        if ($y -ge $arcY - $pad) { $problems += "OUT $pos #$($i+1) cannot reach the circle from above" }
        if ([math]::Abs($y - $cy) -lt $r -and $X -gt $cx - [math]::Sqrt($r * $r - ($y - $cy) * ($y - $cy))) { $problems += "OUT $pos #$($i+1) runs into the circle" }
        $pts = @(@(($ex + $ew), $y), @($X, $y), @($X, $arcY)) | ForEach-Object { ,@([math]::Round((MX $pos $_[0]), 1), [math]::Round((MY $pos $_[1]), 1)) }
        $text = $outs[$pos][$i]; $lw = Width $text
        $labels += [pscustomobject]@{ Text = $text; Cx = (MX $pos (($ex + $ew + $X) / 2)); Cy = (MY $pos $y); W = $lw }
        $segs += [pscustomobject]@{ Name = "OUT $pos #$($i+1)"; Pts = $pts }
    }
    # ---- system -> entity
    $m = $ins[$pos].Count
    for ($j = 0; $j -lt $m; $j++) {
        $x = $ex + ($j + 1) * $ew / ($m + 1)
        $Y = $cy - $r * (0.05 + 0.55 * (OutT $j $m))             # leftmost arrow -> lowest line
        $arcX = $cx - [math]::Sqrt($r * $r - ($Y - $cy) * ($Y - $cy))
        $pts = @(@($arcX, $Y), @($x, $Y), @($x, ($ey + $eh))) | ForEach-Object { ,@([math]::Round((MX $pos $_[0]), 1), [math]::Round((MY $pos $_[1]), 1)) }
        $text = $ins[$pos][$j]; $lw = Width $text
        $labels += [pscustomobject]@{ Text = $text; Cx = (MX $pos (($x + $arcX) / 2)); Cy = (MY $pos $Y); W = $lw }
        $segs += [pscustomobject]@{ Name = "IN $pos #$($j+1)"; Pts = $pts }
    }
}

foreach ($sg in $segs) {
    $k++; $p = $sg.Pts
    $mid = '<Array as="points">' + (($p[1..($p.Count - 2)] | ForEach-Object { "<mxPoint x=""$($_[0])"" y=""$($_[1])"" />" }) -join '') + '</Array>'
    Add-Cell "<mxCell id=""f$k"" style=""$edgeStyle"" edge=""1"" parent=""1""><mxGeometry relative=""1"" as=""geometry""><mxPoint x=""$($p[0][0])"" y=""$($p[0][1])"" as=""sourcePoint"" /><mxPoint x=""$($p[-1][0])"" y=""$($p[-1][1])"" as=""targetPoint"" />$mid</mxGeometry></mxCell>"
}
$k = 0
foreach ($l in $labels) {
    $k++
    Add-Cell "<mxCell id=""l$k"" value=""$(Esc $l.Text)"" style=""$labelStyle"" vertex=""1"" parent=""1""><mxGeometry x=""$([math]::Round($l.Cx - $l.W / 2, 1))"" y=""$([math]::Round($l.Cy - $labelH / 2, 1))"" width=""$($l.W)"" height=""$labelH"" as=""geometry"" /></mxCell>"
}

# ---- checks: connectors never cross or overlap, labels never overlap each other
$lines = foreach ($sg in $segs) { for ($i = 0; $i -lt $sg.Pts.Count - 1; $i++) { [pscustomobject]@{ N = $sg.Name; A = $sg.Pts[$i]; B = $sg.Pts[$i + 1] } } }
for ($i = 0; $i -lt $lines.Count; $i++) { for ($j = $i + 1; $j -lt $lines.Count; $j++) {
    $a = $lines[$i]; $b = $lines[$j]; if ($a.N -eq $b.N) { continue }
    $ax1 = [math]::Min($a.A[0], $a.B[0]); $ax2 = [math]::Max($a.A[0], $a.B[0]); $ay1 = [math]::Min($a.A[1], $a.B[1]); $ay2 = [math]::Max($a.A[1], $a.B[1])
    $bx1 = [math]::Min($b.A[0], $b.B[0]); $bx2 = [math]::Max($b.A[0], $b.B[0]); $by1 = [math]::Min($b.A[1], $b.B[1]); $by2 = [math]::Max($b.A[1], $b.B[1])
    if ($ax1 -le $bx2 -and $bx1 -le $ax2 -and $ay1 -le $by2 -and $by1 -le $ay2) { $problems += "$($a.N) touches $($b.N)" }
} }
for ($i = 0; $i -lt $labels.Count; $i++) { for ($j = $i + 1; $j -lt $labels.Count; $j++) {
    $a = $labels[$i]; $b = $labels[$j]
    if ([math]::Abs($a.Cx - $b.Cx) * 2 -lt ($a.W + $b.W) -and [math]::Abs($a.Cy - $b.Cy) -lt $labelH) { $problems += "labels overlap: '$($a.Text)' / '$($b.Text)'" }
} }
if ($problems) { $problems | ForEach-Object { Write-Output "PROBLEM: $_" } } else { Write-Output 'check passed: no crossings, no overlapping labels' }

$W = 2 * $cx; $H = 2 * $cy
$xml = @"
<mxfile host="app.diagrams.net" type="device">
  <diagram id="dfd0" name="DFD mức ngữ cảnh">
    <mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="$W" pageHeight="$H" math="0" shadow="0">
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
[xml]$check = [IO.File]::ReadAllText($OutDrawio, $utf8)
Write-Output ("{0}: font {1}, {2} flows, {3}x{4} (font/width = 1/{5})" -f (Split-Path $OutDrawio -Leaf), $fs, $segs.Count, $W, $H, [math]::Round($W / $fs))
