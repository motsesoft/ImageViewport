namespace MUI.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// �������¼�����ӿڡ�
    /// </summary>
    public interface IWheelHandler
    {
        /// <summary>
        /// �����������¼���
        /// </summary>
        /// <param name="sender">�����ߣ�ͨ���� ImageViewport����</param>
        /// <param name="p">ָ���¼���Ϣ��</param>
        /// <returns>����¼��������򷵻� true�����򷵻� false��</returns>
        bool OnWheel(object sender, PointerEvent p);
    }
}