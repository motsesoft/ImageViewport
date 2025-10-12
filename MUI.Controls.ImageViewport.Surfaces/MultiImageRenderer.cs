using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Surfaces;

namespace MUI.Controls.ImageViewport.Surfaces
{
    /// <summary>
    /// 通用多图渲染器：每张图定义 Local->Scene 矩阵与本地范围，统一绘制到 Window。
    /// </summary>
    public sealed class MultiImageRenderer : ISurfaceRenderer
    {
        public sealed class ImageEntry
        {
            public ImageSource? Source { get; set; }
            public PxRect LocalExtent { get; set; }  // e.g. 0,0,w,h in local space
            public Matrix LocalToScene { get; set; } // scale/translate/rotate allowed
        }

        public List<ImageEntry> Images { get; } = new();

        public void Render(DrawingContext dc, Rect windowRect, ViewportInfo view, IViewportTransforms tf, object[] surfaces)
        {
            foreach (var img in Images)
            {
                if (img?.Source is null || img.LocalExtent.Width <= 0 || img.LocalExtent.Height <= 0)
                    continue;

                // Map local rect corners -> scene -> window
                var p00 = Transform(img.LocalToScene, img.LocalExtent.TopLeft);
                var p11 = Transform(img.LocalToScene, new PxPoint(img.LocalExtent.X + img.LocalExtent.Width, img.LocalExtent.Y + img.LocalExtent.Height));
                var w0 = tf.ImageToWindow(p00);
                var w1 = tf.ImageToWindow(p11);
                var winRect = new Rect(new Point(w0.X, w0.Y), new Point(w1.X, w1.Y));

                dc.DrawImage(img.Source, winRect);
            }
        }

        private static PxPoint Transform(Matrix m, PxPoint p)
            => new PxPoint(m.M11*p.X + m.M21*p.Y + m.OffsetX, m.M12*p.X + m.M22*p.Y + m.OffsetY);
    }
}