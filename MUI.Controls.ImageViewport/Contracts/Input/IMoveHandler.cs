namespace MUI.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// ����ƶ��¼�����ӿڡ�
    /// </summary>
    public interface IMoveHandler
    {
        /// <summary>
        /// ��������ƶ��¼���
        /// </summary>
        bool OnMove(object sender, PointerEvent p);
    }
}