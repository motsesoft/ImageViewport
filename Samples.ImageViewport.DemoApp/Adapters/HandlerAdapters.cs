using System;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Input;
using Samples.ImageViewport.DemoApp.ViewModels;

namespace Samples.ImageViewport.DemoApp.Adapters
{
    public sealed class VmPanZoomConfig : IPanZoomConfig
    {
        private readonly ViewportViewModel _vm;
        public VmPanZoomConfig(ViewportViewModel vm){ _vm = vm; }
        public PanButton PanButton => _vm.PanButton;public bool RequireCtrlForWheelZoom => _vm.RequireCtrlForWheelZoom;
        public double? ScaleFactorOverride => _vm.ScaleFactor > 0 ? _vm.ScaleFactor : null;
    }

    public sealed class VmSelectionSink : ISelectionSink
    {
        private readonly ViewportViewModel _vm;
        private readonly Func<PxRect, PxRect> _winToImg;
        public VmSelectionSink(ViewportViewModel vm, Func<PxRect, PxRect> windowToImageRect)
        {
            _vm = vm; _winToImg = windowToImageRect;
        }

        public void OnSelectionChanging(PxRect selectionWindowRect)
        {
            _vm.SelectionActive = true;
            _vm.SelectionWindowRect = selectionWindowRect;
            _vm.SelectionImageRect = _winToImg(selectionWindowRect);
        }

        public void OnSelectionCommitted(PxRect selectionWindowRect, PxRect selectionImageRect)
        {
            _vm.SelectionActive = true;
            _vm.SelectionWindowRect = selectionWindowRect;
            _vm.SelectionImageRect = selectionImageRect;
        }

        public void OnSelectionCanceled()
        {
            _vm.SelectionActive = false;
            _vm.SelectionWindowRect = new PxRect();
            _vm.SelectionImageRect = new PxRect();
        }
    }
}