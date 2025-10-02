using System;
using System.Windows.Media;

namespace UI.Controls.Contracts
{
    /// <summary>
    /// 只读：视口矩阵访问与变更通知
    /// </summary>
    public interface IViewportMatrix
    {
        Matrix ViewMatrix { get; }
        Matrix InverseViewMatrix { get; }
        event EventHandler ViewMatrixChanged;
    }
}
