using System.Windows.Input;

using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Input;
using MUI.Controls.ImageViewport.Handlers.Contracts;

namespace MUI.Controls.ImageViewport.Handlers.Routers
{
    /// <summary>
    /// ���ʽ����������קƽ�� + �������š�
    /// �ڲ���� PanHandler �� ZoomHandler��
    /// </summary>
    public sealed class PanZoomHandler : IInputRouter
    {
        private readonly PanHandler _panHandler;
        private readonly ZoomHandler _zoomHandler;
        private readonly IPanZoomOptions _options;

        public PanZoomHandler(IPanZoomOptions? options = null)
        {
            _options = options ?? new PanZoomOptions();

            // ��ʼ���Ӵ�����
            _panHandler = new PanHandler
            {
                PanButtonProvider = () => _options.PanButton
            };

            _zoomHandler = new ZoomHandler
            {
                ScaleFactorProvider = () => _options.ScaleFactor,
                MinScaleProvider = () => _options.MinScale,
                MaxScaleProvider = () => _options.MaxScale,
                PivotModeProvider = () => _options.WheelPivot,
                CustomPivotProvider = () => _options.CustomPivotWindowPxProvider?.Invoke() ?? new PxPoint()
            };
        }

        public bool OnWheel(object sender, PointerEvent p)
        {
            // ��� Ctrl ���μ�Ҫ��
            if (_options.RequireCtrlForWheelZoom && !p.Modifiers.HasFlag(ModifierKeys.Control))
                return false;

            return _zoomHandler.OnWheel(sender, p);
        }

        public bool OnMouseDown(object sender, PointerEvent p)
        {
            // ��� Shift ���μ�����ֹ��ק��
            if (p.Modifiers.HasFlag(ModifierKeys.Shift))
                return false;

            return _panHandler.OnMouseDown(sender, p);
        }

        public bool OnMove(object sender, PointerEvent p)
        {
            // ���ȴ���ƽ�ƣ�Ȼ���������ê��
            var handled = _panHandler.OnMove(sender, p);
            _zoomHandler.OnMove(sender, p); // �����أ�������λ��
            return handled;
        }

        public bool OnMouseUp(object sender, PointerEvent p)
        {
            return _panHandler.OnMouseUp(sender, p);
        }

        // IInputRouter ��Ĭ��ʵ�֣����ɽӿ��ṩ��
        // OnLeftDown, OnLeftUp, OnRightDown, OnRightUp, OnMiddleDown, OnMiddleUp
        // ��ʹ��Ĭ��ʵ�֣����� false��
    }
}