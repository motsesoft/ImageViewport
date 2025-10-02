using System.Windows;
using System.Windows.Media;

namespace MUI.Controls.ImageViewport.Contracts
{
    /// <summary>
    /// 可写：获取绘图上下文
    /// </summary>
    public interface IViewportSurfaces
    {
        DrawingContext Open(LayerKind layer, bool clear = true);

        Rect ViewportBounds { get; }       // 屏幕域像素矩形（DIP）
        Rect WorldVisibleBounds { get; }   // 当前可视世界矩形（粗略由反矩阵映射 ViewportBounds）

        void Invalidate();                 // 请求重绘（不清空）
        void Clear(LayerKind layer);       // 清除指定层
        void ClearAll();                   // 清除所有对外层

        event EventHandler SurfacesInvalidated;
    }
}
