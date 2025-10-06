using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace RXDKNeighborhood.Controls
{
    public class DriveSpacePieControl : Control
    {
        public static readonly StyledProperty<ulong> UsedBytesProperty = AvaloniaProperty.Register<DriveSpacePieControl, ulong>(nameof(UsedBytes));

        public static readonly StyledProperty<ulong> FreeBytesProperty = AvaloniaProperty.Register<DriveSpacePieControl, ulong>(nameof(FreeBytes));

        public static readonly StyledProperty<Color> UsedBytesColorProperty = AvaloniaProperty.Register<DriveSpacePieControl, Color>(nameof(UsedBytesColor));

        public static readonly StyledProperty<Color> FreeBytesColorProperty = AvaloniaProperty.Register<DriveSpacePieControl, Color>(nameof(FreeBytesColor));

        public ulong UsedBytes
        {
            get => GetValue(UsedBytesProperty);
            set => SetValue(UsedBytesProperty, value);
        }

        public ulong FreeBytes
        {
            get => GetValue(FreeBytesProperty);
            set => SetValue(FreeBytesProperty, value);
        }

        public Color UsedBytesColor
        {
            get => GetValue(UsedBytesColorProperty);
            set => SetValue(UsedBytesColorProperty, value);
        }

        public Color FreeBytesColor
        {
            get => GetValue(FreeBytesColorProperty);
            set => SetValue(FreeBytesColorProperty, value);
        }

        private static Color DarkenColor(Color color, double factor)
        {
            return Color.FromArgb(
                color.A,
                (byte)(color.R * factor),
                (byte)(color.G * factor),
                (byte)(color.B * factor)
            );
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (FreeBytes == 0 && UsedBytes == 0)
                return;

            var total = (float)(UsedBytes + FreeBytes);
            float usedPercentage = UsedBytes / total;
            float freePercentage = FreeBytes / total;
            float[] percentages = { usedPercentage, freePercentage };
            var colors = new Color[] { UsedBytesColor, FreeBytesColor };

            var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
            double centerX = bounds.Width / 2;
            double centerY = (bounds.Height / 2) - 10;
            double radiusX = bounds.Width / 2;
            double radiusY = (bounds.Height / 2) - 10;

            double startAngle = 180.0;

            for (int i = 0; i < 2; i++)
            {
                double offsetY = (10 - (i * 10)) + 10;
                for (int j = 0; j < percentages.Length; j++)
                {
                    double sweepAngle = percentages[j] * 360;
                    var path = new StreamGeometry();
                    using (var ctx = path.Open())
                    {
                        var center = new Point(centerX, centerY + offsetY);
                        var arcStart = GetArcPoint(centerX, centerY + offsetY, radiusX, radiusY, startAngle);
                        var arcEnd = GetArcPoint(centerX, centerY + offsetY, radiusX, radiusY, startAngle + sweepAngle);
                        ctx.BeginFigure(center, true);
                        ctx.LineTo(arcStart);
                        ctx.ArcTo(arcEnd, new Size(radiusX, radiusY), 0, sweepAngle > 180, SweepDirection.Clockwise);
                        ctx.LineTo(center);
                        ctx.EndFigure(true);
                    }
                    var darkened = DarkenColor(colors[j], 0.6);
                    context.DrawGeometry(new SolidColorBrush(i == 0 ? darkened : colors[j]), new Pen(Brushes.Black, 1), path);
                    startAngle += sweepAngle;
                }
            }
        }

        private static Point GetArcPoint(double centerX, double centerY, double radiusX, double radiusY, double angleDegrees)
        {
            double radians = Math.PI * angleDegrees / 180.0;
            return new Point(
                centerX + radiusX * Math.Cos(radians),
                centerY + radiusY * Math.Sin(radians)
            );
        }
    }
}