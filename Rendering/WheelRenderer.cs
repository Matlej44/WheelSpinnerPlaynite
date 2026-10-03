using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using WheelSpinner.Models;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Path = System.Windows.Shapes.Path;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace WheelSpinner.Rendering
{
     
    public class WheelRenderer
    {
        private const double MinAngleForLabel = 4;
        
        private const double Saturation = 0.8;
        private const double Brightness = 0.92;
        
        private readonly Canvas _canvas;
        private readonly List<double> _sliceBoundaries = new List<double>();

        public WheelRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public bool HasSlices => _sliceBoundaries.Count > 0;

         
        public int GetSliceIndex(double angle)
        {
            for (var i = _sliceBoundaries.Count - 1; i >= 0; i--)
            {
                if (angle >= _sliceBoundaries[i])
                    return i;
            }

            return -1;
        }

        public void Draw(IReadOnlyList<WheelItem> items)
        {
            _canvas.Children.Clear();
            _sliceBoundaries.Clear();

            if (items.Count == 0)
                return;

            var radius = Math.Min(_canvas.Width, _canvas.Height) / 2;
            var cx = _canvas.Width / 2;
            var cy = _canvas.Height / 2;

            _canvas.Clip = new EllipseGeometry(new Point(cx, cy), radius, radius);

            DrawSlices(items, cx, cy, radius);
            DrawOutline(cx, cy, radius);
        }

        private static Color GetSliceColor(int index, int total)
        {
            if (total == 0)
                return ColorFromHsv(0, Saturation, Brightness);
            var hue = 360*index/total;
            return ColorFromHsv(hue, Saturation, Brightness);
        }

        private static Color ColorFromHsv(double h, double s, double v)
        {
            h = ((h % 360) + 360) % 360;
            var c = v * s;
            var x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            var m = v - c;
            double r, g, b;
            if (h < 60) { r = c; g = x; b = 0; }
            else if (h < 120) { r = x; g = c; b = 0; }
            else if (h < 180) { r = 0; g = c; b = x; }
            else if (h < 240) { r = 0; g = x; b = c; }
            else if (h < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }
            return Color.FromRgb((byte)((r+m) * 255), (byte)((g+m) * 255), (byte)((b+m) * 255));
        }

        private void DrawSlices(IReadOnlyList<WheelItem> items, double cx, double cy, double radius)
        {
            var total = items.Aggregate<WheelItem, double>(0, (current, item) => current + item.Weight);

            double cursor = 0;
            for (var i = 0; i < items.Count; i++)
            {
                var sliceAngle = (items[i].Weight / total) * 360;
                var startAngle = cursor;
                var endAngle = cursor + sliceAngle;
                _sliceBoundaries.Add(startAngle);
                
                var color = GetSliceColor(i, items.Count);

                _canvas.Children.Add(CreateSlice(cx, cy, radius, startAngle, endAngle, color));

                if (sliceAngle >= MinAngleForLabel)
                {
                    var label = CreateLabel(items[i].Game.Name, cx, cy, radius, startAngle, sliceAngle);
                    _canvas.Children.Add(label);
                }

                cursor = endAngle;
            }
        }

        private void DrawOutline(double cx, double cy, double radius)
        {
            var outline = new System.Windows.Shapes.Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Fill = null,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(outline, cx - radius);
            Canvas.SetTop(outline, cy - radius);
            _canvas.Children.Add(outline);
        }

        private static Path CreateSlice(double cx, double cy, double radius, double startAngle, double endAngle,
            Color color)
        {
            var startPoint = PointOnCircle(cx, cy, radius, startAngle);
            var endPoint = PointOnCircle(cx, cy, radius, endAngle);
            var isLargeArc = (endAngle - startAngle) > 180;
            var angle = endAngle - startAngle;

            var figure = new PathFigure { StartPoint = new Point(cx, cy), IsClosed = true };
            figure.Segments.Add(new LineSegment(startPoint, true));
            figure.Segments.Add(new ArcSegment(endPoint, new Size(radius, radius), 0, isLargeArc,
                SweepDirection.Clockwise, true));

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return new Path
            {
                Data = geometry,
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Colors.SlateGray),
                StrokeThickness = angle>2 ? 1 : 0
            };
        }

        private static TextBlock CreateLabel(string text, double cx, double cy, double radius, double startAngle,
            double sliceAngle)
        {
            var midAngle = startAngle + sliceAngle / 2;

            var innerRadius = radius * 0.22;
            var outerRadius = radius * 0.92;
            var maxWidth = outerRadius - innerRadius;
            var midRadius = (innerRadius + outerRadius) / 2;

            var tb = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Segoe UI Semibold"),
                FontSize = CalculateFontSize(radius, sliceAngle),
                MaxWidth = maxWidth,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextAlignment = TextAlignment.Center,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 3,
                    ShadowDepth = 0,
                    Opacity = 0.9
                }
            };

            var anchor = PointOnCircle(cx, cy, midRadius, midAngle);

            tb.Measure(new Size(maxWidth, double.PositiveInfinity));
            var width = Math.Min(tb.DesiredSize.Width, maxWidth);
            var height = tb.DesiredSize.Height;

            Canvas.SetLeft(tb, anchor.X - width / 2);
            Canvas.SetTop(tb, anchor.Y - height / 2);

            tb.RenderTransformOrigin = new Point(0.5, 0.5);
            tb.RenderTransform = new RotateTransform(GetReadableLabelRotation(midAngle));
            return tb;
        }

        private static double CalculateFontSize(double radius, double sliceAngle)
        {
            var scaled = radius * 0.06 * (sliceAngle / 30);
            var min = Math.Max(8, radius * 0.03);
            var max = radius * 0.09;
            return Math.Min(Math.Max(scaled, min), max);
        }

         
        private static double GetReadableLabelRotation(double midAngle)
        {
            var screenAngle = midAngle - 90;
            var normalized = ((screenAngle + 180) % 360 + 360) % 360 - 180;
            if (normalized > 90) normalized -= 180;
            else if (normalized < -90) normalized += 180;
            return normalized;
        }

        private static Point PointOnCircle(double cx, double cy, double radius, double angleDegrees)
        {
            var rad = (Math.PI / 180) * (angleDegrees - 90);
            return new Point(cx + (radius * Math.Cos(rad)), cy + (radius * Math.Sin(rad)));
        }
    }
}
