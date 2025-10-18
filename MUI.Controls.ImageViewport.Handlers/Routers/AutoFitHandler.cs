using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Input;
using MUI.Controls.ImageViewport.Handlers.Contracts;

namespace MUI.Controls.ImageViewport.Handlers.Routers
{
    /// <summary>
    /// �Զ����䴦����(������״̬����Ͳ����ж�)��
    /// </summary>
    public sealed class AutoFitHandler : IWindowSizeHandler
    {
        /// <summary>
        /// �Զ�����ģʽ��
        /// </summary>
        public AutoFitMode Mode { get; set; } = AutoFitMode.Disabled;

        private PxRect? _cachedRect;
        private PxSize _lastWindowSize;

        /// <summary>
        /// ����Ҫ�����ͼ�����(������)��
        /// </summary>
        public void SetCachedRect(PxRect rect)
        {
            _cachedRect = rect;
        }

        /// <summary>
        /// ��ȡ��ǰ�����������Ρ�
        /// </summary>
        public PxRect? GetCachedRect() => _cachedRect;

        /// <summary>
        /// �������ľ��Ρ�
        /// </summary>
        public void ClearCache()
        {
            _cachedRect = null;
        }

        /// <summary>
        /// ���ڳߴ�仯ʱ�Ĵ���(IWindowSizeHandler ʵ��)��
        /// </summary>
        /// <param name="sender">������(ͨ���� ImageViewport �ؼ�)��</param>
        /// <param name="newSize">�µĴ��ڳߴ硣</param>
        /// <returns>�Ƿ����˸��¼���</returns>
        public bool OnWindowSizeChanged(object sender, PxSize newSize)
        {
            if (!_cachedRect.HasValue || Mode == AutoFitMode.Disabled)
            {
                _lastWindowSize = newSize;
                return false;
            }

            bool shouldFit = Mode switch
            {
                AutoFitMode.Always => true,
                AutoFitMode.OnWindowGrow => newSize.Width > _lastWindowSize.Width ||
                                           newSize.Height > _lastWindowSize.Height,
                AutoFitMode.OnWindowShrink => newSize.Width < _lastWindowSize.Width ||
                                              newSize.Height < _lastWindowSize.Height,
                _ => false
            };

            _lastWindowSize = newSize;

            // �����Ƿ���Ҫ��������
            return shouldFit;
        }

        /// <summary>
        /// �жϴ��ڳߴ�仯ʱ�Ƿ�Ӧ���������䡣
        /// </summary>
        /// <param name="newSize">�µĴ��ڳߴ硣</param>
        /// <param name="rectToFit">�����Ҫ����,����Ҫ����ľ���;���򷵻� null��</param>
        /// <returns>�Ƿ�Ӧ��ִ�����䡣</returns>
        public bool ShouldRefitOnWindowSizeChange(PxSize newSize, out PxRect? rectToFit)
        {
            rectToFit = null;

            bool shouldFit = OnWindowSizeChanged(this, newSize);

            if (shouldFit && _cachedRect.HasValue)
            {
                rectToFit = _cachedRect.Value;
                return true;
            }

            return false;
        }
    }
}