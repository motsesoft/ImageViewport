using System.Windows;
using System.Windows.Media;

using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Surfaces;

namespace MUI.Controls.ImageViewport.Surfaces.Primitives
{
    /// <summary>
    /// ����ͼ����Ⱦ�㣺�� ImageSource ��ͼ�����ؿռ�(0,0,w,h)���Ƶ���������ϵ��
    /// </summary>
    public sealed class ImageSurfaceRenderer : ISurfaceRenderer
    {
        public ImageSource? Source { get; set; }
        public PxRect ImageRectPx { get; set; } // �����<=0��SourceΪBitmapSource���Զ�ȡ���سߴ�

        public SurfaceMode TransformMode => SurfaceMode.Follow;

        public void Render(DrawingContext dc, in SurfaceRenderContext ctx)
        {
            if ((ImageRectPx.Width <= 0 || ImageRectPx.Height <= 0) && Source is System.Windows.Media.Imaging.BitmapSource bs)
            {
                ImageRectPx = new PxRect(0, 0, bs.PixelWidth, bs.PixelHeight);
            }
            if (Source is null || ImageRectPx.Width <= 0 || ImageRectPx.Height <= 0) return;

            var dest = new Rect(ImageRectPx.X, ImageRectPx.Y, ImageRectPx.Width, ImageRectPx.Height);
            dc.DrawImage(Source, dest);
        }
    }
}