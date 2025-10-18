namespace MUI.Controls.ImageViewport.Contracts.Abstractions
{
    /// <summary>
    /// �ṩ�ӿڵ������Ϣ��
    /// </summary>
    public sealed class ViewportInfo
    {
        /// <summary>�汾�ţ��κ�Ӱ�����/ӳ��Ŀɼ��仯����������</summary>
        public ulong Version { get; init; }

        /// <summary>
        /// ��ȡ�����ô��ڵ����سߴ硣
        /// </summary>
        public PxSize WindowPixelSize { get; init; }

        /// <summary>
        /// ��ȡ������ͼ����ͼ��ͼ�������е�����
        /// </summary>
        public PxRect ViewportRectInImage { get; init; }

        /// <summary>
        /// ��ȡ���������ű�����
        /// </summary>
        public double Scale { get; init; }

        /// <summary>
        /// ��ȡ������ X ����� DPI ���ű�����
        /// </summary>
        public double DpiScaleX { get; init; } = 1.0;

        /// <summary>
        /// ��ȡ������ Y ����� DPI ���ű�����
        /// </summary>
        public double DpiScaleY { get; init; } = 1.0;
    }
}