using System.Windows;
using System.Windows.Media;

namespace UI.Controls.Internal
{
    internal static class DrawingExtensions
    {
        public static void DrawLine(this DrawingContext dc, Pen pen, Point p0, Point p1)
        {
            dc.DrawGeometry(null, pen, new LineGeometry(p0, p1));
        }
    }
}
