# Generate app.ico: solid blue rounded square + white arrow, multi-size 32-bit
Add-Type -AssemblyName System.Drawing

$base = 256
$bmp = [System.Drawing.Bitmap]::new($base, $base)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

function RoundPath([single]$x, [single]$y, [single]$w, [single]$h, [single]$r) {
    $p = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $d = $r * 2
    $p.AddArc($x, $y, $d, $d, 180, 90)
    $p.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $p.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $p.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $p.CloseFigure()
    return $p
}

$blue = [System.Drawing.Color]::FromArgb(47, 106, 227)
$brush = [System.Drawing.SolidBrush]::new($blue)
$path = RoundPath 0 0 $base $base 56
$g.FillPath($brush, $path)

$pen = [System.Drawing.Pen]::new([System.Drawing.Color]::White, [single]24)
$pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$g.DrawLine($pen, 92, 164, 164, 92)

$tri = [System.Drawing.PointF[]]@(
    [System.Drawing.PointF]::new(124, 72),
    [System.Drawing.PointF]::new(184, 72),
    [System.Drawing.PointF]::new(184, 132)
)
$white = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::White)
$g.FillPolygon($white, $tri)
$g.Dispose()

# Per-size PNG data for the ico
$sizes = @(256, 48, 32, 16)
$pngs = @()
foreach ($s in $sizes) {
    if ($s -eq $base) {
        $b = $bmp
    }
    else {
        $b = [System.Drawing.Bitmap]::new($s, $s)
        $gg = [System.Drawing.Graphics]::FromImage($b)
        $gg.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $gg.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $gg.DrawImage($bmp, 0, 0, $s, $s)
        $gg.Dispose()
    }
    $ms = [System.IO.MemoryStream]::new()
    $b.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngs += , $ms.ToArray()
    $ms.Close()
    if ($s -ne $base) { $b.Dispose() }
}

# High-res logo PNG (64px) for the window title bar, scaled with high quality at draw time
$b64 = [System.Drawing.Bitmap]::new(64, 64)
$g64 = [System.Drawing.Graphics]::FromImage($b64)
$g64.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g64.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g64.DrawImage($bmp, 0, 0, 64, 64)
$g64.Dispose()
$b64.Save("$PSScriptRoot\logo64.png", [System.Drawing.Imaging.ImageFormat]::Png)
$b64.Dispose()

$bmp.Dispose()

# Write ico: ICONDIR + entries + PNG payloads
$bw = [System.IO.BinaryWriter]::new([System.IO.File]::Create("$PSScriptRoot\app.ico"))
$bw.Write([uint16]0); $bw.Write([uint16]1); $bw.Write([uint16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $s = $sizes[$i]
    $dim = if ($s -ge 256) { [byte]0 } else { [byte]$s }
    $bw.Write($dim); $bw.Write($dim); $bw.Write([byte]0); $bw.Write([byte]0)
    $bw.Write([uint16]1); $bw.Write([uint16]32)
    $bw.Write([uint32]$pngs[$i].Length); $bw.Write([uint32]$offset)
    $offset += $pngs[$i].Length
}
foreach ($p in $pngs) { $bw.Write($p) }
$bw.Close()
Write-Host "app.ico generated (multi-size 32bit) + logo64.png"
