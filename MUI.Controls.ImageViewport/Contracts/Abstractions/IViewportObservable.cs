namespace MUI.Controls.ImageViewport.Contracts.Abstractions
{
    /// <summary>
    /// ��ѡ���ӿ��¼�Դ�ӿڣ�������ʵ�ָýӿڣ���������ӿڱ仯�¼�����
    /// </summary>
    public interface IViewportObservable
    {
        /// <summary>
        /// �ӿ�״̬�����仯ʱ�������¼���
        /// </summary>
        event EventHandler<ViewportInfo> ViewportChanged;
    }
}