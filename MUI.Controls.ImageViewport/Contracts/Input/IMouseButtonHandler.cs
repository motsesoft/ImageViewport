namespace MUI.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// ��갴���¼�����ӿڡ�
    /// </summary>
    public interface IMouseButtonHandler
    {
        /// <summary>
        /// ������갴���¼������ⰴ������
        /// </summary>
        bool OnMouseDown(object sender, PointerEvent p);

        /// <summary>
        /// ��������ͷ��¼������ⰴ������
        /// </summary>
        bool OnMouseUp(object sender, PointerEvent p);
    }
}