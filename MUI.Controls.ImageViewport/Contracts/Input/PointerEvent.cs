using System.Windows.Input;

using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace MUI.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// ��ʾָ�루��꣩�¼�����Ϣ��
    /// </summary>
    public sealed class PointerEvent
    {
        /// <summary>
        /// ��ȡ�������¼�������ʱ�����
        /// </summary>
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// ��ȡ������ָ���ڴ����е��������ꡣ
        /// </summary>
        public PxPoint WindowPx { get; init; }

        /// <summary>
        /// ��ȡ������ָ����ͼ���е��������ꡣ
        /// </summary>
        public PxPoint ImagePx { get; init; }

        /// <summary>
        /// ��ȡ�����������ֵ�������
        /// </summary>
        public double WheelDelta { get; init; }

        /// <summary>
        /// ��ȡ�����õ��������
        /// </summary>
        public int ClickCount { get; init; }

        /// <summary>
        /// ��ȡ���������μ�״̬��
        /// </summary>
        public ModifierKeys Modifiers { get; init; }

        /// <summary>
        /// ��ȡ�����õ�ǰ����Ƿ��ڰ���״̬��
        /// ��ֱֵ�ӷ�ӳ�˿���������������ʵ״̬��ͨ���� MouseEventArgs.LeftButton == Pressed �õ�����
        /// ����ʵʱ�ж��������״̬��
        /// </summary>
        public bool CurrentLeftPressed { get; init; }

        /// <summary>
        /// ��ȡ�����õ�ǰ�Ҽ��Ƿ��ڰ���״̬��
        /// ��ֱֵ�ӷ�ӳ�˿���������Ҽ�����ʵ״̬��ͨ���� MouseEventArgs.RightButton == Pressed �õ�����
        /// ����ʵʱ�ж��������״̬��
        /// </summary>
        public bool CurrentRightPressed { get; init; }

        /// <summary>
        /// ��ȡ�����õ�ǰ�м��Ƿ��ڰ���״̬��
        /// ��ֱֵ�ӷ�ӳ�˿���������м�����ʵ״̬��ͨ���� MouseEventArgs.MiddleButton == Pressed �õ�����
        /// ����ʵʱ�ж��������״̬��
        /// </summary>
        public bool CurrentMiddlePressed { get; init; }

        /// <summary>
        /// ��·�������ã�Ҫ��ؼ���Ҫ���������Ĳ˵���
        /// Ĭ��Ϊ false��ÿ���¼�����ʱ�������á�
        /// </summary>
        public bool SuppressContextMenu { get; set; }
    }
}