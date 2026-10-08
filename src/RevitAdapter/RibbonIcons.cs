using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RevitQualityChecker.RevitAdapter
{
    internal static class RibbonIcons
    {
        public static BitmapSource CreateCheckIcon(int size)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                // Blue rounded rectangle background
                var bg = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
                dc.DrawRoundedRectangle(bg, null, new Rect(0, 0, size, size), size * 0.15, size * 0.15);

                // White checkmark
                var pen = new Pen(Brushes.White, size * 0.1) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
                double m = size * 0.22;
                dc.DrawLine(pen, new Point(m, size * 0.52), new Point(size * 0.4, size * 0.72));
                dc.DrawLine(pen, new Point(size * 0.4, size * 0.72), new Point(size - m, size * 0.3));
            }

            return Render(visual, size);
        }

        public static BitmapSource CreateSettingsIcon(int size)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                // Dark rounded rectangle background
                var bg = new SolidColorBrush(Color.FromRgb(0x3E, 0x3E, 0x3E));
                dc.DrawRoundedRectangle(bg, null, new Rect(0, 0, size, size), size * 0.15, size * 0.15);

                // Gear (simplified: circle with inner circle)
                double cx = size / 2.0, cy = size / 2.0;
                var pen = new Pen(new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)), size * 0.06);
                dc.DrawEllipse(null, pen, new Point(cx, cy), size * 0.28, size * 0.28);
                dc.DrawEllipse(null, pen, new Point(cx, cy), size * 0.12, size * 0.12);

                // Four notches
                double notch = size * 0.35;
                for (int i = 0; i < 4; i++)
                {
                    double angle = i * Math.PI / 2;
                    dc.DrawLine(pen,
                        new Point(cx + Math.Cos(angle) * size * 0.22, cy + Math.Sin(angle) * size * 0.22),
                        new Point(cx + Math.Cos(angle) * notch, cy + Math.Sin(angle) * notch));
                }
            }

            return Render(visual, size);
        }

        public static BitmapSource CreateAboutIcon(int size)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                var bg = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                dc.DrawRoundedRectangle(bg, null, new Rect(0, 0, size, size), size * 0.15, size * 0.15);

                var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
                var text = new FormattedText("i", System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, typeface, size * 0.55, Brushes.White,
                    VisualTreeHelper.GetDpi(visual).PixelsPerDip);
                dc.DrawText(text, new Point((size - text.Width) / 2, (size - text.Height) / 2));
            }

            return Render(visual, size);
        }

        private static BitmapSource Render(DrawingVisual visual, int size)
        {
            var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }
    }
}
