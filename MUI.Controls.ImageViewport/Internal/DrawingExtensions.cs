using System.Windows;
using System.Windows.Media;

namespace MUI.Controls.ImageViewport.Internal
{
    internal static class DrawingExtensions
    {
        public static void DrawLine(this DrawingContext dc, Pen pen, Point p0, Point p1)
        {
            dc.DrawGeometry(null, pen, new LineGeometry(p0, p1));
        }
    }
}
