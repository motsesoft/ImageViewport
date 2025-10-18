namespace MUI.Controls.ImageViewport.Contracts.Abstractions
{
    /// <summary>
    /// �ṩ�ӿڣ�Viewport����ز����ķ���ӿڣ�֧�ֿ��ա����š�ƽ�ơ���Ӧͼ�����򡢴��ڳߴ�� DPI ���õȹ��ܡ�
    /// </summary>
    public interface IViewportService
    {
        /// <summary>
        /// ��ȡ��ǰ�ӿڵĿ�����Ϣ��ÿ�ε��þ������µ�ʵ����
        /// </summary>
        /// <returns>������ǰ�ӿ�״̬�� <see cref="ViewportInfo"/> ʵ����</returns>
        ViewportInfo Snapshot();

        /// <summary>
        /// ͳһ�Ĳ��ɱ���գ��κοɼ������ ++Version �������¿���
        /// ͬһ Version �ڼ䷵��ͬһʵ��
        /// </summary>
        ViewportInfo Current { get; }

        /// <summary>
        /// �Դ�������Ϊ��׼����ָ����������Ų�����
        /// </summary>
        /// <param name="scaleFactor">�������ӡ�</param>
        /// <param name="windowPx">�����������꣬��Ϊ�������ġ�</param>
        void ZoomAtWindowPx(double scaleFactor, PxPoint windowPx);

        /// <summary>
        /// �Դ�������Ϊ��λ����ƽ�Ʋ�����
        /// </summary>
        /// <param name="dx">ˮƽ����ƽ�Ƶ���������</param>
        /// <param name="dy">��ֱ����ƽ�Ƶ���������</param>
        void PanWindowPx(double dx, double dy);

        /// <summary>
        /// ʹ�ӿ���Ӧָ����ͼ����������
        /// </summary>
        /// <param name="imagePxRect">��Ҫ��Ӧ��ͼ����������</param>
        void FitImageRect(PxRect imagePxRect);

        /// <summary>
        /// �����ӿڵĴ��ڳߴ磨������Ϊ��λ����
        /// </summary>
        /// <param name="size">�������سߴ硣</param>
        void SetWindowSize(PxSize size);

        /// <summary>
        /// �����ӿڵ� DPI ���ű�����
        /// </summary>
        /// <param name="x">X ����� DPI ���ű�����</param>
        /// <param name="y">Y ����� DPI ���ű�����</param>
        void SetDpi(double x, double y);
    }
}