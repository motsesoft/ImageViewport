using System.Windows;
using System.Windows.Media;

using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace MUI.Controls.ImageViewport.Contracts.Surfaces
{
    /// <summary>
    /// ��ʾͼ���ӿ���Ⱦ�����е���������Ϣ����������任����������DPI���ŵ���Ⱦ������������ݡ�
    /// </summary>
    /// <remarks>
    /// �˽ṹ���ǲ��ɱ�ģ���������Ⱦ�ܵ��д�����������Ϣ��ȷ���̰߳�ȫ�������Ż���
    /// </remarks>
    public readonly struct SurfaceRenderContext
    {
        /// <summary>
        /// ��ȡ���ڵľ�������
        /// </summary>
        /// <value>��ʾ���ڱ߽�� <see cref="Rect"/> ����</value>
        public Rect WindowRect { get; }

        /// <summary>
        /// ��ȡ�ӿ���Ϣ��
        /// </summary>
        /// <value>�����ӿ���ϸ��Ϣ�� <see cref="ViewportInfo"/> ����</value>
        public ViewportInfo View { get; }

        /// <summary>
        /// ��ȡ�ӿڱ任�����Ľӿڡ�
        /// </summary>
        /// <value>�ṩ����任���ܵ� <see cref="IViewportTransforms"/> �ӿ�ʵ����</value>
        public IViewportTransforms Transforms { get; }

        /// <summary>
        /// ��ȡ��ͼ������ϵ����������ϵ�ı任����
        /// </summary>
        /// <value>���ڽ�ͼ������ת��Ϊ��������� <see cref="Matrix"/> ����</value>
        public Matrix ViewportMatrix { get; }   // image -> window

        /// <summary>
        /// ��ȡ�Ӵ�������ϵ��ͼ������ϵ����任����
        /// </summary>
        /// <value>���ڽ���������ת��Ϊͼ������� <see cref="Matrix"/> ����</value>
        public Matrix ViewportMatrixInverse { get; }   // window -> image

        /// <summary>
        /// ��ȡˮƽ����� DPI �������ӡ�
        /// </summary>
        /// <value>ˮƽ����� DPI ���ű�����</value>
        public double DpiScaleX { get; }

        /// <summary>
        /// ��ȡ��ֱ����� DPI �������ӡ�
        /// </summary>
        /// <value>��ֱ����� DPI ���ű�����</value>
        public double DpiScaleY { get; }

        /// <summary>
        /// ��ȡ���ӵ��غ����ݼ��ϡ�
        /// </summary>
        /// <value>�����������ݵ�ֻ���б����û���ṩ������Ϊ�ռ��ϡ�</value>
        public IReadOnlyList<object> Payload { get; }

        /// <summary>
        /// ��ȡ�����ṩ�ߣ���������ע������λ��
        /// </summary>
        /// <value>�����ṩ��ʵ��������Ϊ null��</value>
        public IServiceProvider? Services { get; }

        /// <summary>
        /// ��ȡͨ�ñ�ǩ�������ڴ洢�û��Զ������ݡ�
        /// </summary>
        /// <value>�û�����ı�ǩ���󣬿���Ϊ null��</value>
        public object? Tag { get; }

        /// <summary>
        /// ��ʼ�� <see cref="SurfaceRenderContext"/> �ṹ�����ʵ����
        /// </summary>
        /// <param name="windowRect">���ڵľ�������</param>
        /// <param name="view">�ӿ���Ϣ��</param>
        /// <param name="transforms">�ӿڱ任�����ӿڡ�</param>
        /// <param name="viewportMatrix">��ͼ������ϵ����������ϵ�ı任����</param>
        /// <param name="viewportMatrixInverse">�Ӵ�������ϵ��ͼ������ϵ����任����</param>
        /// <param name="dpiX">ˮƽ����� DPI �������ӡ�</param>
        /// <param name="dpiY">��ֱ����� DPI �������ӡ�</param>
        /// <param name="payload">��ѡ�ĸ����غ����ݼ��ϡ�</param>
        /// <param name="services">��ѡ�ķ����ṩ�ߡ�</param>
        /// <param name="tag">��ѡ��ͨ�ñ�ǩ����</param>
        public SurfaceRenderContext(
            Rect windowRect,
            ViewportInfo view,
            IViewportTransforms transforms,
            Matrix viewportMatrix,
            Matrix viewportMatrixInverse,
            double dpiX,
            double dpiY,
            IReadOnlyList<object>? payload = null,
            IServiceProvider? services = null,
            object? tag = null)
        {
            WindowRect = windowRect;
            View = view;
            Transforms = transforms;
            ViewportMatrix = viewportMatrix;
            ViewportMatrixInverse = viewportMatrixInverse;
            DpiScaleX = dpiX;
            DpiScaleY = dpiY;
            Payload = payload ?? Array.Empty<object>();
            Services = services;
            Tag = tag;
        }
    }
}