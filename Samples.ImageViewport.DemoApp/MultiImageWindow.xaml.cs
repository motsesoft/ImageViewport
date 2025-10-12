using System.Windows;
using System.Windows.Media;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Surfaces;

namespace Samples.ImageViewport.DemoApp
{
    public partial class MultiImageWindow : Window
    {
        public MultiImageWindow()
        {
            InitializeComponent();

            // Prepare two images with different resolutions
            var imgA = (ImageSource)Application.Current.FindResource("DemoImage"); // 1024x768
            var imgB = (ImageSource)Application.Current.FindResource("DemoImageB"); // 2048x1024 (generated)

            var renderer = new MultiImageRenderer();
            // A: identity to Scene
            renderer.Images.Add(new MultiImageRenderer.ImageEntry
            {
                Source = imgA,
                LocalExtent = new PxRect(0,0,1024,768),
                LocalToScene = Matrix.Identity
            });
            // B: scale down to half, and place to the right of A (with 64px gap in Scene units)
            var s = 0.5;
            var m = new Matrix(s,0,0,s, 1024 + 64, 0); // scale then translate
            renderer.Images.Add(new MultiImageRenderer.ImageEntry
            {
                Source = imgB,
                LocalExtent = new PxRect(0,0,2048,1024),
                LocalToScene = m
            });

            vp.Facade = new SimpleFacade(renderer);
            // Fit both into view
            vp.FitImageRect(new PxRect(0,0,1024 + 64 + 2048*s, System.Math.Max(768, 1024*s)));
        }
    }

    internal sealed class SimpleFacade : MUI.Controls.ImageViewport.Contracts.Facade.IViewportFacade
    {
        public MUI.Controls.ImageViewport.Contracts.Surfaces.ISurfaceRenderer Renderer { get; }
        public MUI.Controls.ImageViewport.Contracts.Input.IInputRouter? InputRouter { get; } = null;
        public MUI.Controls.ImageViewport.Contracts.Input.IContextMenuProvider? ContextMenu { get; } = null;
        public SimpleFacade(MUI.Controls.ImageViewport.Contracts.Surfaces.ISurfaceRenderer renderer){ Renderer = renderer; }
    }
}