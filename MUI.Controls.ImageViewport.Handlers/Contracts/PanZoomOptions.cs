using System;

using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace MUI.Controls.ImageViewport.Handlers.Contracts
{
    public sealed class PanZoomOptions : IPanZoomOptions
    {
        /// <summary>������קƽ�Ƶ�������Ĭ�� Middle����</summary>
        public PanButton PanButton { get; set; } = PanButton.Middle;

        /// <summary>���������Ƿ���Ҫ Ctrl��Ĭ�� false����</summary>
        public bool RequireCtrlForWheelZoom { get; set; } = false;

        public bool UseImageCoordinateZoom => true;

        /// <summary>�����������᣺Center / Mouse / Custom��Ĭ�� Mouse����</summary>
        public ZoomPivotMode WheelPivot { get; set; } = ZoomPivotMode.Mouse;

        public double ScaleFactor { get; set; } = 1.1;

        public double MinScale { get; set; } = 0.01;

        public double MaxScale { get; set; } = 1000.0;

        /// <summary>�� WheelPivot = Custom ʱʹ�õĴ������ص��ṩ�ߡ�</summary>
        public Func<PxPoint>? CustomPivotWindowPxProvider { get; set; }
    }
}