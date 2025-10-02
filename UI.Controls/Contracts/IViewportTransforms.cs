using System.Windows;

namespace UI.Controls.Contracts
{
    /// <summary>
    /// 只读：坐标变换
    /// </summary>
    public interface IViewportTransforms
    {
        Point ScreenToWorld(Point p);
        Point WorldToScreen(Point p);
        Rect ScreenToWorld(Rect r);
        Rect WorldToScreen(Rect r);
    }
}
