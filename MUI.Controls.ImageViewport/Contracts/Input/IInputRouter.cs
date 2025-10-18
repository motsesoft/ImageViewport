namespace MUI.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// ����������·�����ӿڣ�����������봦��������
    /// ���� Facade ������ַ���
    /// </summary>
    public interface IInputRouter : IWheelHandler, IMoveHandler, IMouseButtonHandler
    {
        /// <summary>��������¼���</summary>
        bool OnLeftDown(object sender, PointerEvent p) => false;

        /// <summary>����ͷ��¼���</summary>
        bool OnLeftUp(object sender, PointerEvent p) => false;

        /// <summary>�Ҽ������¼���</summary>
        bool OnRightDown(object sender, PointerEvent p) => false;

        /// <summary>�Ҽ��ͷ��¼���</summary>
        bool OnRightUp(object sender, PointerEvent p) => false;

        /// <summary>�м������¼���</summary>
        bool OnMiddleDown(object sender, PointerEvent p) => false;

        /// <summary>�м��ͷ��¼���</summary>
        bool OnMiddleUp(object sender, PointerEvent p) => false;
    }
}