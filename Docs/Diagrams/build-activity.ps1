param([string]$OutDrawio, [string[]]$Pages)
# Builds UML activity diagrams with swimlanes (one draw.io page per data file).
# Data file lines (UTF-8):
#   PAGE|page name          TITLE|title bar text        INFO|field|text   (spec block above the diagram)
#   LANE|lane title         N|id|start/end/action/decision|lane|row|text   E|src|dst|guard|f/loop/skip/lloop
# Style: black & white, square action boxes, questions inside decisions, orthogonal connectors.
# A layout check fails the page if a connector passes through a box or crosses another connector.
$utf8 = New-Object System.Text.UTF8Encoding($false)
function Esc([string]$s) { [System.Security.SecurityElement]::Escape($s) }

$laneW = 520; $laneHeader = 50; $titleH = 50; $rowPitch = 125
$size = @{ action = @(400, 80); decision = @(220, 100); start = @(34, 34); end = @(38, 38) }
$font = 'fontFamily=Times New Roman;'
# Font size 14 everywhere (user request 2026-11-26)
$fs = 14
# Border widths: 1px borders vanish when the diagram is scaled down into Word (user saw boxes losing edges), so draw them thicker.
$sw = 2; $laneSw = 1.5
$style = @{
    info     = "text;html=1;whiteSpace=wrap;align=left;verticalAlign=top;fontSize=$fs;spacing=0;$font"
    title    = "rounded=0;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#000000;strokeWidth=$sw;fontSize=$fs;fontStyle=1;$font"
    lane     = "swimlane;html=1;startSize=$laneHeader;horizontal=1;fontSize=$fs;fontStyle=1;fillColor=#ffffff;strokeColor=#000000;strokeWidth=$laneSw;swimlaneFillColor=#ffffff;$font"
    action   = "rounded=0;whiteSpace=wrap;html=1;fontSize=$fs;fillColor=#ffffff;strokeColor=#000000;strokeWidth=$sw;$font"
    decision = "rhombus;whiteSpace=wrap;html=1;fontSize=$fs;fillColor=#ffffff;strokeColor=#000000;strokeWidth=$sw;spacing=12;$font"
    start    = 'ellipse;html=1;fillColor=#000000;strokeColor=#000000;'
    end      = 'ellipse;html=1;shape=endState;fillColor=#000000;strokeColor=#000000;'
    edge     = "edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;endArrow=open;endSize=12;strokeColor=#000000;strokeWidth=1.5;fontSize=$fs;labelBackgroundColor=#ffffff;$font"
}

$diagrams = New-Object System.Text.StringBuilder
$pageNo = 0
foreach ($file in $Pages) {
    $pageNo++; $p = "q$pageNo"
    $name = ''; $title = ''; $info = @(); $lanes = @(); $rawNodes = @(); $edges = @()
    foreach ($line in [IO.File]::ReadAllLines($file, $utf8)) {
        if (-not $line.Trim()) { continue }
        $f = $line.Split('|')
        switch ($f[0]) {
            'PAGE'  { $name = $f[1] }
            'TITLE' { $title = $f[1] }
            'INFO'  { $info += [pscustomobject]@{ Field = $f[1]; Text = $f[2] } }
            'LANE'  { $lanes += $f[1] }
            'N'     { $rawNodes += ,$f }
            'E'     { $edges += [pscustomobject]@{ Src = $f[1]; Dst = $f[2]; Label = $f[3]; Kind = $f[4] } }
        }
    }
    $pageW = $lanes.Count * $laneW

    # ---- vertical stack: spec block, title bar, lanes
    $sb = New-Object System.Text.StringBuilder
    $y = 0
    if ($info.Count) {
        # characters per line at this font size across the page width (~95 per 1040px at 18pt), 1.5 line height, 8px between fields
        $perLine = [math]::Floor(0.9 * 95 * 18 / $fs * $pageW / 1040)   # 0.9: keep a margin so text never overflows
        $lineCount = ($info | ForEach-Object { [math]::Ceiling(("$($_.Field): $($_.Text)").Length / $perLine) } | Measure-Object -Sum).Sum
        $infoH = [math]::Ceiling($lineCount * $fs * 1.5) + ($info.Count - 1) * 8
        $html = ($info | ForEach-Object { "<b>$(Esc $_.Field):</b> $(Esc $_.Text)" }) -join '<div style="height:8px"></div>'
        [void]$sb.AppendLine("        <mxCell id=""$p-info"" value=""$(Esc $html)"" style=""$($style.info)"" vertex=""1"" parent=""1""><mxGeometry x=""0"" y=""0"" width=""$pageW"" height=""$infoH"" as=""geometry"" /></mxCell>")
        $y = $infoH + 20
    }
    if ($title) {
        [void]$sb.AppendLine("        <mxCell id=""$p-title"" value=""$(Esc $title)"" style=""$($style.title)"" vertex=""1"" parent=""1""><mxGeometry x=""0"" y=""$y"" width=""$pageW"" height=""$titleH"" as=""geometry"" /></mxCell>")
        $y += $titleH
    }
    $lanesTop = $y; $row0 = $lanesTop + $laneHeader + 50

    $nodes = [ordered]@{}
    foreach ($f in $rawNodes) {
        $w = $size[$f[2]][0]; $h = $size[$f[2]][1]
        $cx = [int]$f[3] * $laneW + $laneW / 2; $cy = $row0 + [int]$f[4] * $rowPitch
        $nodes[$f[1]] = [pscustomobject]@{ Id = $f[1]; Kind = $f[2]; Lane = [int]$f[3]; Row = [int]$f[4]; Text = $f[5]
            X = $cx - $w / 2; Y = $cy - $h / 2; W = $w; H = $h; Cx = $cx; Cy = $cy }
    }
    $maxRow = ($nodes.Values | Measure-Object -Property Row -Maximum).Maximum
    $laneH = $row0 + $maxRow * $rowPitch + 60 - $lanesTop
    $pageH = $lanesTop + $laneH

    for ($i = 0; $i -lt $lanes.Count; $i++) {
        [void]$sb.AppendLine("        <mxCell id=""$p-lane$i"" value=""$(Esc $lanes[$i])"" style=""$($style.lane)"" vertex=""1"" parent=""1""><mxGeometry x=""$($i * $laneW)"" y=""$lanesTop"" width=""$laneW"" height=""$laneH"" as=""geometry"" /></mxCell>")
    }
    foreach ($n in $nodes.Values) {
        [void]$sb.AppendLine("        <mxCell id=""$p-$($n.Id)"" value=""$(Esc $n.Text)"" style=""$($style[$n.Kind])"" vertex=""1"" parent=""1""><mxGeometry x=""$($n.X)"" y=""$($n.Y)"" width=""$($n.W)"" height=""$($n.H)"" as=""geometry"" /></mxCell>")
    }

    # ---- connectors
    $k = 0; $polys = @()
    foreach ($e in ($edges | Sort-Object { $_.Kind -ne 'f' })) {
        $k++; $s = $nodes[$e.Src]; $d = $nodes[$e.Dst]; $pts = @(); $label = ''
        if (-not $s -or -not $d) { throw "Unknown node in edge $($e.Src) -> $($e.Dst)" }
        if ($e.Kind -in 'loop', 'skip') {
            # loop = back to an earlier step, skip = jump forward past steps in the same lane (a "Không" branch
            # that bypasses optional steps): leave the source on the right, run along the right margin,
            # then go left along the target's row (that row must be empty in the other lane)
            $busy = @($nodes.Values | Where-Object { $_.Lane -ne $d.Lane -and $_.Row -eq $d.Row })
            if ($busy.Count) { throw "Loop $($e.Src)->$($e.Dst): row $($d.Row) is not empty in the other lane" }
            $marginX = $pageW - 25
            $ends = 'exitX=1;exitY=0.5;exitDx=0;exitDy=0;entryX=1;entryY=0.5;entryDx=0;entryDy=0;'
            $pts = @(@($marginX, $s.Cy), @($marginX, $d.Cy))
            $a = @(($s.X + $s.W), $s.Cy); $b = @(($d.X + $d.W), $d.Cy)
            $label = '<mxGeometry x="-0.85" y="0" relative="1" as="geometry"><mxPoint y="-16" as="offset" />'
        } elseif ($e.Kind -eq 'lloop') {
            # back to an earlier step along the LEFT margin: leave the source on the left, run up the left margin,
            # then go right along the target's row (lanes left of the target must be empty on that row)
            $busy = @($nodes.Values | Where-Object { $_.Lane -lt $d.Lane -and $_.Row -eq $d.Row })
            if ($busy.Count) { throw "Loop $($e.Src)->$($e.Dst): row $($d.Row) is not empty left of the target" }
            $marginX = 25
            $ends = 'exitX=0;exitY=0.5;exitDx=0;exitDy=0;entryX=0;entryY=0.5;entryDx=0;entryDy=0;'
            $pts = @(@($marginX, $s.Cy), @($marginX, $d.Cy))
            $a = @($s.X, $s.Cy); $b = @($d.X, $d.Cy)
            $label = '<mxGeometry x="-0.85" y="0" relative="1" as="geometry"><mxPoint y="-16" as="offset" />'
        } elseif ($s.Row -eq $d.Row) {
            if ($d.Lane -gt $s.Lane) {
                $ends = 'exitX=1;exitY=0.5;exitDx=0;exitDy=0;entryX=0;entryY=0.5;entryDx=0;entryDy=0;'
                $a = @(($s.X + $s.W), $s.Cy); $b = @($d.X, $d.Cy)
            } else {
                $ends = 'exitX=0;exitY=0.5;exitDx=0;exitDy=0;entryX=1;entryY=0.5;entryDx=0;entryDy=0;'
                $a = @($s.X, $s.Cy); $b = @(($d.X + $d.W), $d.Cy)
            }
        } elseif ($s.Lane -eq $d.Lane) {
            $ends = 'exitX=0.5;exitY=1;exitDx=0;exitDy=0;entryX=0.5;entryY=0;entryDx=0;entryDy=0;'
            $a = @($s.Cx, ($s.Y + $s.H)); $b = @($d.Cx, $d.Y)
            if ($e.Label) { $label = '<mxGeometry x="0" y="0" relative="1" as="geometry"><mxPoint x="30" as="offset" />' }
        } else {
            $exit = if ($d.Lane -lt $s.Lane) { 'exitX=0' } else { 'exitX=1' }
            $ends = "$exit;exitY=0.5;exitDx=0;exitDy=0;entryX=0.5;entryY=0;entryDx=0;entryDy=0;"
            $pts = @(,@($d.Cx, $s.Cy))   # unary comma: keep it an array of ONE point
            $a = @($(if ($d.Lane -lt $s.Lane) { $s.X } else { $s.X + $s.W }), $s.Cy); $b = @($d.Cx, $d.Y)
        }
        $polys += [pscustomobject]@{ Name = "$($e.Src)->$($e.Dst)"; Src = $s.Id; Dst = $d.Id; Pts = @(,$a) + $pts + @(,$b) }
        if (-not $label) { $label = '<mxGeometry relative="1" as="geometry">' }
        $ptsXml = if ($pts.Count) { '<Array as="points">' + (($pts | ForEach-Object { "<mxPoint x=""$($_[0])"" y=""$($_[1])"" />" }) -join '') + '</Array>' } else { '' }
        [void]$sb.AppendLine("        <mxCell id=""$p-e$k"" value=""$(Esc $e.Label)"" style=""$($style.edge)$ends"" edge=""1"" parent=""1"" source=""$p-$($s.Id)"" target=""$p-$($d.Id)"">$label$ptsXml</mxGeometry></mxCell>")
    }

    # ---- layout check: no connector may pass through a box, run diagonally, or cross another connector
    $problems = @()
    $segs = foreach ($pl in $polys) { for ($j = 0; $j -lt $pl.Pts.Count - 1; $j++) { [pscustomobject]@{ E = $pl; A = $pl.Pts[$j]; B = $pl.Pts[$j + 1] } } }
    foreach ($sg in $segs) {
        if ($sg.A[0] -ne $sg.B[0] -and $sg.A[1] -ne $sg.B[1]) { $problems += "diagonal segment in $($sg.E.Name)" }
        $x1 = [math]::Min($sg.A[0], $sg.B[0]); $x2 = [math]::Max($sg.A[0], $sg.B[0]); $y1 = [math]::Min($sg.A[1], $sg.B[1]); $y2 = [math]::Max($sg.A[1], $sg.B[1])
        foreach ($n in $nodes.Values) {
            if ($n.Id -eq $sg.E.Src -or $n.Id -eq $sg.E.Dst) { continue }
            if ($x2 -gt $n.X -and $x1 -lt $n.X + $n.W -and $y2 -gt $n.Y -and $y1 -lt $n.Y + $n.H) { $problems += "$($sg.E.Name) passes through $($n.Id)" }
        }
    }
    for ($i = 0; $i -lt $segs.Count; $i++) { for ($j = $i + 1; $j -lt $segs.Count; $j++) {
        $sp = $segs[$i]; $sq = $segs[$j]; if ($sp.E -eq $sq.E) { continue }
        $pv = $sp.A[0] -eq $sp.B[0]; $qv = $sq.A[0] -eq $sq.B[0]
        if ($pv -eq $qv) {
            # parallel: two connectors must not run on top of each other (overlap longer than a point)
            $axis = if ($pv) { 0 } else { 1 }; $other = 1 - $axis
            if ($sp.A[$axis] -eq $sq.A[$axis]) {
                $lo = [math]::Max([math]::Min($sp.A[$other], $sp.B[$other]), [math]::Min($sq.A[$other], $sq.B[$other]))
                $hi = [math]::Min([math]::Max($sp.A[$other], $sp.B[$other]), [math]::Max($sq.A[$other], $sq.B[$other]))
                if ($hi -gt $lo) { $problems += "$($sp.E.Name) overlaps $($sq.E.Name)" }
            }
            continue
        }
        $v = if ($pv) { $sp } else { $sq }; $h = if ($pv) { $sq } else { $sp }
        $vx = $v.A[0]; $hy = $h.A[1]
        if ($vx -gt [math]::Min($h.A[0], $h.B[0]) -and $vx -lt [math]::Max($h.A[0], $h.B[0]) -and $hy -gt [math]::Min($v.A[1], $v.B[1]) -and $hy -lt [math]::Max($v.A[1], $v.B[1])) {
            $problems += "$($sp.E.Name) crosses $($sq.E.Name)"
        }
    } }
    $empty = @($nodes.Values | Where-Object { $_.Kind -eq 'decision' -and -not $_.Text })
    if ($empty) { $problems += "decision without a question: $($empty.Id -join ', ')" }
    if ($problems) { $problems | ForEach-Object { Write-Output "LAYOUT PROBLEM (${p}): $_" } } else { Write-Output "${p}: layout check passed" }

    [void]$diagrams.Append(@"
  <diagram id="$p" name="$(Esc $name)">
    <mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="$pageW" pageHeight="$pageH" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />
$($sb.ToString().TrimEnd())
      </root>
    </mxGraphModel>
  </diagram>

"@)
    Write-Output ("{0}: {1} lanes, {2} nodes, {3} edges, {4}x{5}" -f $p, $lanes.Count, $nodes.Count, $edges.Count, $pageW, $pageH)
}
[IO.File]::WriteAllText($OutDrawio, "<mxfile host=""app.diagrams.net"" type=""device"">`n$($diagrams.ToString())</mxfile>`n", $utf8)
[xml]$check = [IO.File]::ReadAllText($OutDrawio, $utf8)
Write-Output "XML OK: $(@($check.mxfile.diagram).Count) page(s)"
