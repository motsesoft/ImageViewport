using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace MUI.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// ���ڳߴ�仯�������ӿڡ�
    /// </summary>
    public interface IWindowSizeHandler
    {
        bool OnWindowSizeChanged(object sender, PxSize newSize);
    }
}