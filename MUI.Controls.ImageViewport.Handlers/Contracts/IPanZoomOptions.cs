using System;

using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace MUI.Controls.ImageViewport.Handlers.Contracts
{
    public interface IPanZoomOptions
    {
        // ��קƽ��ʹ����һ������
        PanButton PanButton { get; }

        // ���������Ƿ���Ҫ Ctrl
        bool RequireCtrlForWheelZoom { get; }

        // �Ƿ�ʹ��ͼ����������
        bool UseImageCoordinateZoom { get; }

        // ������������
        ZoomPivotMode WheelPivot { get; }

        // ��������
        double ScaleFactor { get; }

        // ��С����
        double MinScale { get; }

        // �������
        double MaxScale { get; }

        // �� WheelPivot = Custom ʱ�ṩ�Զ��崰�����ص�
        Func<PxPoint>? CustomPivotWindowPxProvider { get; }
    }
}