Add-Type -AssemblyName System.Drawing

$size = 120
$bmp = New-Object System.Drawing.Bitmap($size, $size)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit

# Background - rounded rectangle (blue gradient feel)
$bgBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(0, 122, 204))
$path = New-Object System.Drawing.Drawing2D.GraphicsPath
$radius = 20
$path.AddArc(0, 0, $radius, $radius, 180, 90)
$path.AddArc($size - $radius, 0, $radius, $radius, 270, 90)
$path.AddArc($size - $radius, $size - $radius, $radius, $radius, 0, 90)
$path.AddArc(0, $size - $radius, $radius, $radius, 90, 90)
$path.CloseFigure()
$g.FillPath($bgBrush, $path)

# Checkmark
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 8)
$pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
$g.DrawLine($pen, 28, 58, 48, 78)
$g.DrawLine($pen, 48, 78, 92, 34)

# "QC" text bottom right
$font = New-Object System.Drawing.Font("Segoe UI", 18, [System.Drawing.FontStyle]::Bold)
$textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(180, 255, 255, 255))
$g.DrawString("QC", $font, $textBrush, 62, 82)

$g.Dispose()

$outPath = Join-Path $PSScriptRoot "app-icon.png"
$bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

Write-Host "Icon saved to: $outPath"
