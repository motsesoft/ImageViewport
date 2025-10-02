using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using MUI.Controls.ImageViewport.Contracts;

namespace ImageViewport.Demo.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;

        // 可选：显示鼠标坐标（HUD）
        Viewport.MouseMove += OnViewportMouseMove;
        Viewport.MouseLeave += (_, __) => { Viewport.Facade.Clear(LayerKind.Hud); Viewport.Facade.Invalidate(); };
    }

    void OnLoaded(object sender, RoutedEventArgs e)
    {
        DrawTestLayers();
        FitToRect(new Rect(0, 0, 1200, 900), 40); // 让世界范围合适可见
    }

    void DrawTestLayers()
    {
        // 1) Background（世界层）：底图网格
        using (var dc = Viewport.Facade.Open(LayerKind.Background, clear: true))
        {
            dc.DrawRectangle(Brushes.Black, null, new Rect(0, 0, 1200, 900));
            var minor = new Pen(new SolidColorBrush(Color.FromRgb(40, 40, 40)), 0.5); minor.Freeze();
            var major = new Pen(new SolidColorBrush(Color.FromRgb(70, 70, 70)), 1.0); major.Freeze();
            for (int x = 0; x <= 1200; x += 50) dc.DrawLine((x % 250 == 0) ? major : minor, new Point(x, 0), new Point(x, 900));
            for (int y = 0; y <= 900; y += 50) dc.DrawLine((y % 250 == 0) ? major : minor, new Point(0, y), new Point(1200, y));
        }

        // 2) Active（世界层）：几个世界坐标的图形
        using (var dc = Viewport.Facade.Open(LayerKind.Active, clear: true))
        {
            var brushA = new SolidColorBrush(Color.FromArgb(180, 0, 170, 255)); brushA.Freeze();
            var brushB = new SolidColorBrush(Color.FromArgb(160, 255, 120, 0)); brushB.Freeze();
            var penW = new Pen(Brushes.White, 2); penW.Freeze();

            // 世界矩形（会随缩放/平移）
            dc.DrawRectangle(brushA, null, new Rect(100, 100, 200, 150));

            // 世界圆形（会随缩放/平移）
            dc.DrawEllipse(brushB, null, new Point(800, 400), 120, 120);

            // 世界文字（会随缩放/平移而变大/变小）
            var ft = new FormattedText("World Label @ (800,400)",
                CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface("Consolas"), 16, Brushes.Yellow, 1.25);
            dc.DrawText(ft, new Point(760, 540));

            // 世界十字定位（跟随缩放/平移）
            dc.DrawLine(penW, new Point(800 - 140, 400), new Point(800 + 140, 400));
            dc.DrawLine(penW, new Point(800, 400 - 140), new Point(800, 400 + 140));
        }

        // 3) OverlayWorld（世界层）：世界叠加标注（半透明框）
        using (var dc = Viewport.Facade.Open(LayerKind.OverlayWorld, clear: true))
        {
            var region = new Rect(500, 200, 220, 160);
            var fill = new SolidColorBrush(Color.FromArgb(70, 0, 255, 0)); fill.Freeze();
            var pen = new Pen(Brushes.Lime, 2); pen.Freeze();
            dc.DrawRectangle(fill, pen, region);
        }

        // 4) HUD（屏幕层）：不随缩放/平移的 UI 元素
        using (var dc = Viewport.Facade.Open(LayerKind.Hud, clear: true))
        {
            // 左上角固定标题
            var ft = new FormattedText("HUD: fixed to screen, not scaled",
                CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface("Consolas"), 12, Brushes.White, 1.25);
            var box = new Rect(new Point(8, 8), new Size(ft.Width + 16, ft.Height + 10));
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)), null, box, 4, 4);
            dc.DrawText(ft, new Point(box.X + 8, box.Y + 5));

            // 居中 HUD 十字线（屏幕中心不随缩放/平移）
            var vp = new Rect(new Point(0, 0), Viewport.RenderSize);
            var cx = vp.Width / 2; var cy = vp.Height / 2;
            var pen = new Pen(new SolidColorBrush(Color.FromArgb(160, 255, 64, 64)), 1); pen.Freeze();
            dc.DrawLine(pen, new Point(cx - 40, cy), new Point(cx + 40, cy));
            dc.DrawLine(pen, new Point(cx, cy - 40), new Point(cx, cy + 40));
        }

        Viewport.Facade.Invalidate();
    }

    // 显示鼠标坐标（HUD，不随缩放）
    void OnViewportMouseMove(object sender, MouseEventArgs e)
    {
        var s = e.GetPosition(Viewport);
        var w = Viewport.Facade.ScreenToWorld(s);
        using (var dc = Viewport.Facade.Open(LayerKind.Hud, clear: false)) // 不清 HUD 其它内容
        {
            // 右下角
            var msg = FormattableString.Invariant($"Screen {s.X:F3},{s.Y:F3} | World {w.X:F3},{w.Y:F3}");
            var ft = new FormattedText(msg, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                       new Typeface("Consolas"), 12, Brushes.White, 1.25);
            var vp = new Rect(new Point(0, 0), Viewport.RenderSize);
            var p = new Point(vp.Width - ft.Width - 14, vp.Height - ft.Height - 10);
            var box = new Rect(p, new Size(ft.Width + 10, ft.Height + 6));
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)), null, box, 4, 4);
            dc.DrawText(ft, new Point(p.X + 5, p.Y + 3));
        }
        Viewport.Facade.Invalidate();
    }

    // 适配函数（让某个世界矩形完整可见）
    void FitToRect(Rect worldRect, double margin)
    {
        if (worldRect.IsEmpty) return;
        var vp = new Rect(new Point(0, 0), Viewport.RenderSize);
        if (vp.IsEmpty) return;

        vp.Inflate(-margin, -margin);
        if (vp.Width <= 0 || vp.Height <= 0) return;

        var s = Math.Min(vp.Width / worldRect.Width, vp.Height / worldRect.Height);
        var wc = new Point(worldRect.X + worldRect.Width / 2, worldRect.Y + worldRect.Height / 2);
        var sc = new Point(vp.X + vp.Width / 2, vp.Y + vp.Height / 2);

        var m = Matrix.Identity;
        m.Translate(-wc.X, -wc.Y);
        m.Scale(s, s);
        m.Translate(sc.X, sc.Y);

        Viewport.ViewMatrix = m;
    }
}
