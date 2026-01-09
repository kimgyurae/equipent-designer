using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EquipmentDesigner.Models.ProcessEditor;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace EquipmentDesigner.Converters
{
    /// <summary>
    /// Attached behavior for managing drawboard cursor based on selected tool.
    /// Handles custom cursors and eraser ghost trail effect.
    /// </summary>
    public static class DrawboardCursorBehavior
    {
        private static readonly Dictionary<FrameworkElement, GhostTrailManager> GhostTrailManagers = new Dictionary<FrameworkElement, GhostTrailManager>();
        private static Cursor _eraserCursor;
        private static Cursor _crossCursor;

        #region Attached Properties

        public static readonly DependencyProperty CursorTypeProperty =
            DependencyProperty.RegisterAttached(
                "CursorType",
                typeof(DrawboardToolCursorType),
                typeof(DrawboardCursorBehavior),
                new PropertyMetadata(DrawboardToolCursorType.Default, OnCursorTypeChanged));

        public static DrawboardToolCursorType GetCursorType(DependencyObject obj)
        {
            return (DrawboardToolCursorType)obj.GetValue(CursorTypeProperty);
        }

        public static void SetCursorType(DependencyObject obj, DrawboardToolCursorType value)
        {
            obj.SetValue(CursorTypeProperty, value);
        }

        public static readonly DependencyProperty GhostTrailCanvasProperty =
            DependencyProperty.RegisterAttached(
                "GhostTrailCanvas",
                typeof(Canvas),
                typeof(DrawboardCursorBehavior),
                new PropertyMetadata(null));

        public static Canvas GetGhostTrailCanvas(DependencyObject obj)
        {
            return (Canvas)obj.GetValue(GhostTrailCanvasProperty);
        }

        public static void SetGhostTrailCanvas(DependencyObject obj, Canvas value)
        {
            obj.SetValue(GhostTrailCanvasProperty, value);
        }

        #endregion

        #region Cursor Type Changed Handler

        private static void OnCursorTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement element)) return;

            var cursorType = (DrawboardToolCursorType)e.NewValue;
            var previousType = (DrawboardToolCursorType)e.OldValue;

            // Cleanup previous eraser state
            if (previousType == DrawboardToolCursorType.Eraser)
            {
                CleanupEraserBehavior(element);
            }

            // Apply new cursor
            switch (cursorType)
            {
                case DrawboardToolCursorType.Default:
                    element.Cursor = Cursors.Arrow;
                    break;

                case DrawboardToolCursorType.Hand:
                    element.Cursor = Cursors.Hand;
                    break;

                case DrawboardToolCursorType.Cross:
                    element.Cursor = GetCrossCursor();
                    break;

                case DrawboardToolCursorType.Eraser:
                    element.Cursor = GetEraserCursor();
                    SetupEraserBehavior(element);
                    break;
            }
        }

        #endregion

        #region Eraser Cursor Creation

        private static Cursor GetEraserCursor()
        {
            if (_eraserCursor != null) return _eraserCursor;

            try
            {
                // Match the size with AddGhostDot's Ellipse (TrailDotSize = 16, StrokeThickness = 1.5)
                const double strokeThickness = 1.5;
                const int dotSize = 16; // Same as TrailDotSize in GhostTrailManager
                const int size = dotSize + 4; // Add margin for anti-aliasing
                const int hotspot = size / 2;

                // Calculate radius to match WPF Ellipse behavior
                // WPF Ellipse with Width/Height = dotSize draws stroke inside the bounds
                // For DrawEllipse, we need to calculate the fill radius such that
                // the outer edge of the stroke matches the WPF Ellipse bounds
                double radius = (dotSize - strokeThickness) / 2;

                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen())
                {
                    // White fill with black stroke circle - matches AddGhostDot style
                    dc.DrawEllipse(
                        Brushes.White,
                        new Pen(Brushes.Black, strokeThickness),
                        new Point(hotspot, hotspot),
                        radius,
                        radius);
                }

                var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(visual);

                _eraserCursor = CursorHelper.CreateCursor(bitmap, hotspot, hotspot);
                return _eraserCursor;
            }
            catch (Exception e)
            {
                // Fallback to built-in cursor
                return Cursors.Cross;
            }
        }

        private static Cursor GetCrossCursor()
        {
            if (_crossCursor != null) return _crossCursor;

            try
            {
                // Create a small crosshair cursor
                const int size = 16;
                const int hotspot = size / 2;
                const int lineLength = 5;

                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen())
                {
                    var pen = new Pen(Brushes.Black, 1);

                    // Horizontal line
                    dc.DrawLine(pen,
                        new Point(hotspot - lineLength, hotspot),
                        new Point(hotspot + lineLength, hotspot));

                    // Vertical line
                    dc.DrawLine(pen,
                        new Point(hotspot, hotspot - lineLength),
                        new Point(hotspot, hotspot + lineLength));
                }

                var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(visual);

                _crossCursor = CursorHelper.CreateCursor(bitmap, hotspot, hotspot);
                return _crossCursor;
            }
            catch
            {
                // Fallback to built-in cursor
                return Cursors.Cross;
            }
        }

        #endregion

        #region Eraser Ghost Trail

        private static void SetupEraserBehavior(FrameworkElement element)
        {
            var canvas = GetGhostTrailCanvas(element);
            if (canvas == null) return;

            if (!GhostTrailManagers.ContainsKey(element))
            {
                var manager = new GhostTrailManager(element, canvas);
                GhostTrailManagers[element] = manager;
            }

            GhostTrailManagers[element].Enable();
        }

        private static void CleanupEraserBehavior(FrameworkElement element)
        {
            if (GhostTrailManagers.TryGetValue(element, out var manager))
            {
                manager.Disable();
            }
        }

        #endregion

        #region Ghost Trail Manager

        private class GhostTrailManager
        {
            private readonly FrameworkElement _element;
            private readonly Canvas _canvas;
            private bool _isEnabled;
            private bool _isMouseDown;
            private Point _prevPoint;
            private Point _lastPoint;
            private bool _hasEnoughPoints;
            private const double TrailStrokeWidth = 8;

            public GhostTrailManager(FrameworkElement element, Canvas canvas)
            {
                _element = element;
                _canvas = canvas;
            }

            public void Enable()
            {
                if (_isEnabled) return;
                _isEnabled = true;

                _element.MouseLeftButtonDown += OnMouseLeftButtonDown;
                _element.MouseLeftButtonUp += OnMouseLeftButtonUp;
                _element.MouseMove += OnMouseMove;
                _element.MouseLeave += OnMouseLeave;
            }

            public void Disable()
            {
                if (!_isEnabled) return;
                _isEnabled = false;

                _element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                _element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
                _element.MouseMove -= OnMouseMove;
                _element.MouseLeave -= OnMouseLeave;

                _isMouseDown = false;
                _hasEnoughPoints = false;
            }

            private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                _isMouseDown = true;
                _hasEnoughPoints = false;
                _lastPoint = e.GetPosition(_canvas);
                _prevPoint = _lastPoint;
            }

            private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                _isMouseDown = false;
                _hasEnoughPoints = false;
            }

            private void OnMouseMove(object sender, MouseEventArgs e)
            {
                if (!_isMouseDown) return;

                var currentPoint = e.GetPosition(_canvas);

                if (_hasEnoughPoints)
                {
                    // Draw Bezier curve using midpoints for smoothness
                    var startPoint = GetMidPoint(_prevPoint, _lastPoint);
                    var endPoint = GetMidPoint(_lastPoint, currentPoint);
                    AddGhostCurve(startPoint, _lastPoint, endPoint);
                }
                else
                {
                    _hasEnoughPoints = true;
                }

                _prevPoint = _lastPoint;
                _lastPoint = currentPoint;
            }

            private void OnMouseLeave(object sender, MouseEventArgs e)
            {
                _isMouseDown = false;
                _hasEnoughPoints = false;
            }

            private static Point GetMidPoint(Point p1, Point p2)
            {
                return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
            }

            private void AddGhostCurve(Point start, Point control, Point end)
            {
                var pathFigure = new PathFigure
                {
                    StartPoint = start,
                    IsClosed = false
                };

                pathFigure.Segments.Add(new QuadraticBezierSegment(control, end, true));

                var pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(pathFigure);

                var strokeBrush = Application.Current.TryFindResource("Color.Gray.300") is Color c
                    ? new SolidColorBrush(c)
                    : new SolidColorBrush(Color.FromRgb(180, 180, 180));

                var path = new Path
                {
                    Data = pathGeometry,
                    Stroke = strokeBrush,
                    StrokeThickness = TrailStrokeWidth,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    IsHitTestVisible = false
                };

                _canvas.Children.Add(path);

                // StrokeThickness animation - curve gets thinner (pointed tail effect)
                var thicknessAnimation = new DoubleAnimation
                {
                    From = TrailStrokeWidth,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new PowerEase { EasingMode = EasingMode.EaseIn, Power = 3 }
                };

                thicknessAnimation.Completed += (s, e) =>
                {
                    _canvas.Children.Remove(path);
                };

                path.BeginAnimation(Shape.StrokeThicknessProperty, thicknessAnimation);
            }
        }

        #endregion
    }

    /// <summary>
    /// Helper class for creating custom cursors from bitmaps.
    /// Uses WPF native APIs only (no System.Drawing dependency).
    /// </summary>
    internal static class CursorHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BitmapInfoHeader
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, IntPtr lpvBits);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDIBSection(IntPtr hdc, ref BitmapInfoHeader pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

        private const uint BI_RGB = 0;
        private const uint DIB_RGB_COLORS = 0;

        public static Cursor CreateCursor(BitmapSource bitmapSource, int xHotspot, int yHotspot)
        {
            // Ensure BGRA32 format for consistent pixel handling
            var source = bitmapSource;
            if (bitmapSource.Format != PixelFormats.Bgra32)
            {
                source = new FormatConvertedBitmap(bitmapSource, PixelFormats.Bgra32, null, 0);
            }

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4; // 4 bytes per pixel (BGRA32)

            // Copy pixel data from BitmapSource
            byte[] pixels = new byte[height * stride];
            source.CopyPixels(pixels, stride, 0);

            // Create color bitmap using CreateDIBSection (WPF native, no System.Drawing)
            var bmi = new BitmapInfoHeader
            {
                biSize = (uint)Marshal.SizeOf<BitmapInfoHeader>(),
                biWidth = width,
                biHeight = -height, // Negative for top-down DIB
                biPlanes = 1,
                biBitCount = 32,
                biCompression = BI_RGB,
                biSizeImage = 0,
                biXPelsPerMeter = 0,
                biYPelsPerMeter = 0,
                biClrUsed = 0,
                biClrImportant = 0
            };

            IntPtr ppvBits;
            IntPtr hBitmap = CreateDIBSection(IntPtr.Zero, ref bmi, DIB_RGB_COLORS, out ppvBits, IntPtr.Zero, 0);

            if (hBitmap == IntPtr.Zero || ppvBits == IntPtr.Zero)
            {
                return Cursors.Arrow;
            }

            // Copy pixel data to the DIB section
            Marshal.Copy(pixels, 0, ppvBits, pixels.Length);

            // Create a monochrome mask bitmap (all zeros = fully opaque using color bitmap's alpha)
            IntPtr hMask = CreateBitmap(width, height, 1, 1, IntPtr.Zero);

            if (hMask == IntPtr.Zero)
            {
                DeleteObject(hBitmap);
                return Cursors.Arrow;
            }

            try
            {
                var iconInfo = new IconInfo
                {
                    fIcon = false, // Cursor, not icon
                    xHotspot = xHotspot,
                    yHotspot = yHotspot,
                    hbmMask = hMask,
                    hbmColor = hBitmap
                };

                var cursorHandle = CreateIconIndirect(ref iconInfo);

                if (cursorHandle != IntPtr.Zero)
                {
                    return CursorInteropHelper.Create(new SafeIconHandle(cursorHandle));
                }
            }
            finally
            {
                DeleteObject(hBitmap);
                DeleteObject(hMask);
            }

            return Cursors.Arrow;
        }
    }

    /// <summary>
    /// Safe handle wrapper for cursor/icon handles.
    /// </summary>
    internal class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);

        public SafeIconHandle(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return DestroyIcon(handle);
        }
    }
}