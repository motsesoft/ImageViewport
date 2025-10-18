using System;
using System.Windows;
using System.Windows.Input;

using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Input;
using MUI.Controls.ImageViewport.Handlers.Contracts;

namespace MUI.Controls.ImageViewport.Handlers.Routers
{
    /// <summary>
    /// ����קƽ�ƴ�������
    /// ֧��������ק���������/�м�/�Ҽ�����
    /// </summary>
    public class PanHandler : IMouseButtonHandler, IMoveHandler
    {
        /// <summary>
        /// ��̬���õ���ק�������� PanButtonProvider Ϊ null ʱʹ�ã���
        /// </summary>
        public PanButton PanButton { get; set; } = PanButton.Left;

        /// <summary>
        /// ��̬��ȡ PanButton ��ί�У����ȼ����� PanButton ���ԣ���
        /// </summary>
        public Func<PanButton>? PanButtonProvider { get; set; }

        // ҵ��״̬���Ƿ�������ק
        private bool _isDragging;
        private PxPoint _lastWindowPos;

        public bool OnMouseDown(object sender, PointerEvent p)
        {
            if (!IsPanButtonPressed(p))
                return false;

            _isDragging = true;
            _lastWindowPos = p.WindowPx;

            // ������õ���ק���� Right���ͽ�ֹ���˵�
            var button = PanButtonProvider?.Invoke() ?? PanButton;
            if (button == PanButton.Right)
            {
                p.SuppressContextMenu = true;
            }

            if (sender is IInputElement el)
            {
                el.CaptureMouse();
            }

            return true;
        }

        public bool OnMove(object sender, PointerEvent p)
        {
            if (!_isDragging)
                return false;

            // ������õ���ק���� Right���ͽ�ֹ���˵�
            var button = PanButtonProvider?.Invoke() ?? PanButton;
            if (button == PanButton.Right)
            {
                p.SuppressContextMenu = true;
            }

            // ���������״̬����ֹ�첽���⣺������Ƴ����ں��ͷţ�
            if (!IsPanButtonPressed(p))
            {
                _isDragging = false;

                if (sender is IInputElement el && Mouse.Captured == el)
                {
                    el.ReleaseMouseCapture(); // �ͷ�
                }

                return false;
            }

            if (sender is not ImageViewport vp)
                return false;

            var dx = p.WindowPx.X - _lastWindowPos.X;
            var dy = p.WindowPx.Y - _lastWindowPos.Y;

            if (Math.Abs(dx) > double.Epsilon || Math.Abs(dy) > double.Epsilon)
            {
                vp.PanWindowPx(dx, dy);
                _lastWindowPos = p.WindowPx;
                return true;
            }

            return false;
        }

        public bool OnMouseUp(object sender, PointerEvent p)
        {
            if (!_isDragging)
                return false;

            // ������õ���ק���� Right���ͽ�ֹ���˵�
            var button = PanButtonProvider?.Invoke() ?? PanButton;
            if (button == PanButton.Right)
            {
                p.SuppressContextMenu = true;
            }

            _isDragging = false;
            if (sender is IInputElement el && Mouse.Captured == el)
            {
                el.ReleaseMouseCapture(); // �ͷ�
            }

            return true;
        }

        /// <summary>
        /// ������õ���ק�����Ƿ񱻰��£�����״̬����
        /// </summary>
        private bool IsPanButtonPressed(PointerEvent p)
        {
            // ����ʹ��ί��,��̬��ȡ��������
            var button = PanButtonProvider?.Invoke() ?? PanButton;

            return button switch
            {
                PanButton.Left => p.CurrentLeftPressed,
                PanButton.Middle => p.CurrentMiddlePressed,
                PanButton.Right => p.CurrentRightPressed,
                _ => false
            };
        }
    }
}