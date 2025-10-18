using System;

using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Input;

namespace MUI.Controls.ImageViewport.Handlers.Routers
{
    /// <summary>
    /// ���������Ŵ�������
    /// ֧�ֲ�ͬ������ê��ģʽ�����ŷ�Χ���ơ�
    /// </summary>
    public class ZoomHandler : IWheelHandler, IMoveHandler
    {
        // ��̬����(����Ӧ�� Provider Ϊ null ʱʹ��)
        public double ScaleFactor { get; set; } = 1.1;
        public double MinScale { get; set; } = 0.01;
        public double MaxScale { get; set; } = 100.0;
        public ZoomPivotMode PivotMode { get; set; } = ZoomPivotMode.Mouse;
        public PxPoint CustomPivot { get; set; }
        public bool UseImageCoordinate { get; set; } = true;

        // ��̬����ί��(���ȼ����ھ�̬����)
        public Func<double>? ScaleFactorProvider { get; set; }
        public Func<double>? MinScaleProvider { get; set; }
        public Func<double>? MaxScaleProvider { get; set; }
        public Func<ZoomPivotMode>? PivotModeProvider { get; set; }
        public Func<PxPoint>? CustomPivotProvider { get; set; }
        public Func<bool>? UseImageCoordinateProvider { get; set; }

        private PxPoint _lastMousePosWindow;
        private PxPoint _lastMousePosImage;

        public virtual bool OnWheel(object sender, PointerEvent p)
        {
            if (sender is not ImageViewport vp) return false;

            // ��̬��ȡ����
            var scaleFactor = ScaleFactorProvider?.Invoke() ?? ScaleFactor;
            var minScale = MinScaleProvider?.Invoke() ?? MinScale;
            var maxScale = MaxScaleProvider?.Invoke() ?? MaxScale;
            var useImageCoordinate = UseImageCoordinateProvider?.Invoke() ?? UseImageCoordinate;

            // ������������
            double factor = p.WheelDelta > 0 ? scaleFactor : 1.0 / scaleFactor;

            // ��ȡ��ǰ���Ų�Ӧ�÷�Χ����
            var current = vp.Scale <= 0 ? 1.0 : vp.Scale;
            var desired = current * factor;

            var minS = minScale > 0 ? minScale : 1e-6;
            var maxS = maxScale > minS ? maxScale : minS;
            desired = Math.Max(minS, Math.Min(maxS, desired));

            var actualFactor = desired / current;
            if (Math.Abs(actualFactor - 1.0) < 1e-9) return false; // �ޱ仯

            // ����ģʽѡ��ʹ�ô��������ͼ������
            if (useImageCoordinate)
            {
                vp.ZoomAtImagePx(actualFactor, _lastMousePosImage);
            } else
            {
                var pivotMode = PivotModeProvider?.Invoke() ?? PivotMode;
                var customPivot = CustomPivotProvider?.Invoke() ?? CustomPivot;

                PxPoint pivotWindow = pivotMode switch
                {
                    ZoomPivotMode.Mouse => _lastMousePosWindow,
                    ZoomPivotMode.Custom => customPivot,
                    ZoomPivotMode.Center => new PxPoint(
                        vp.WindowPixelSize.Width / 2.0,
                        vp.WindowPixelSize.Height / 2.0),
                    _ => _lastMousePosWindow
                };

                vp.ZoomAtWindowPx(actualFactor, pivotWindow);
            }

            return true;
        }

        public virtual bool OnMove(object sender, PointerEvent p)
        {
            // ׷�����λ��
            _lastMousePosWindow = p.WindowPx;
            _lastMousePosImage = p.ImagePx;
            return false; // �������ƶ��¼�
        }
    }
}