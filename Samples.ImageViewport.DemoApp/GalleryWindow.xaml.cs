using System.Windows;
using Samples.ImageViewport.DemoApp.ViewModels;
using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace Samples.ImageViewport.DemoApp
{
    public partial class GalleryWindow : Window
    {
        private sealed class ViewportControllerAdapter : IViewportController
        {
            private readonly MUI.Controls.ImageViewport.ImageViewport _vp;
            public ViewportControllerAdapter(MUI.Controls.ImageViewport.ImageViewport vp){ _vp = vp; }
            public void FitImageRect(PxRect imageRect) => _vp.FitImageRect(imageRect);
            public void ZoomAtWindowPx(double factor, PxPoint pivotWindowPx) => _vp.ZoomAtWindowPx(factor, pivotWindowPx);
            public PxRect WindowRectToImageRect(PxRect winRect) => _vp.WindowRectToImageRect(winRect);
        }

        public GalleryWindow()
        {
            InitializeComponent();
            var vm = (MainViewModel)DataContext;
            vm.Viewport.Controller = new ViewportControllerAdapter(vpA);
            vpA.Facade = new DemoFacade(vm.Viewport, r => ((IViewportController)vm.Viewport.Controller!).WindowRectToImageRect(r));

            var vm2 = new MainViewModel();
            vm2.Viewport.Controller = new ViewportControllerAdapter(vpB);
            vpB.Facade = new DemoFacade(vm2.Viewport, r => ((IViewportController)vm2.Viewport.Controller!).WindowRectToImageRect(r));
        }
    }
}