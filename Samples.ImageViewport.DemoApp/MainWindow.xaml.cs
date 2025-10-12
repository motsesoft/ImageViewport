using Microsoft.Win32;

using System.Windows;

using Samples.ImageViewport.DemoApp.ViewModels;

using MUI.Controls.ImageViewport.Contracts.Abstractions;

using MUI.Controls.ImageViewport.Contracts.Input;

using System.Windows.Media.Imaging;

using System.Windows.Media;



namespace Samples.ImageViewport.DemoApp
{
    public partial class MainWindow : Window
    {
        private sealed class ViewportControllerAdapter : IViewportController
        {
            private readonly MUI.Controls.ImageViewport.ImageViewport _vp;
            public ViewportControllerAdapter(MUI.Controls.ImageViewport.ImageViewport vp) { _vp = vp; }
            public void FitImageRect(PxRect imageRect) => _vp.FitImageRect(imageRect);
            public void ZoomAtWindowPx(double factor, PxPoint pivotWindowPx) => _vp.ZoomAtWindowPx(factor, pivotWindowPx);
            public PxRect WindowRectToImageRect(PxRect winRect) => _vp.WindowRectToImageRect(winRect);
        }

        public MainWindow()
        {
            InitializeComponent();
            var vm = (MainViewModel)DataContext;
            vm.Viewport.Controller = new ViewportControllerAdapter(viewport);
            viewport.Facade = new DemoFacade(vm.Viewport, r => ((IViewportController)vm.Viewport.Controller!).WindowRectToImageRect(r));

            viewport.ScaleChanged += (s, e) => vm.Viewport.CurrentScale = e.NewScale;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current.TryFindResource("DemoImage") is ImageSource img)
            {
                int w = 1024, h = 768;
                if (img is BitmapSource bs) { w = bs.PixelWidth; h = bs.PixelHeight; }
                viewport.FitImageRect(new PxRect(0, 0, w, h));
            }
        }

        private void OpenMultiImage_Click(object sender, RoutedEventArgs e) => new MultiImageWindow().Show();
        private void OpenGallery_Click(object sender, RoutedEventArgs e) => new GalleryWindow().Show();

        private void ScenarioBasic_Click(object sender, RoutedEventArgs e) => ((MainViewModel)DataContext).Scenario = DemoScenario.Basic;
        private void ScenarioWheel_Click(object sender, RoutedEventArgs e) => ((MainViewModel)DataContext).Scenario = DemoScenario.DragWithWheel;
        private void ScenarioPrecision_Click(object sender, RoutedEventArgs e) => ((MainViewModel)DataContext).Scenario = DemoScenario.PrecisionZoom;
        private void ScenarioBoxZoom_Click(object sender, RoutedEventArgs e) => ((MainViewModel)DataContext).Scenario = DemoScenario.BoxZoom;
        private void ScenarioMeasure_Click(object sender, RoutedEventArgs e) => ((MainViewModel)DataContext).Scenario = DemoScenario.Measure;

        private void PanLeft_Click(object sender, RoutedEventArgs e) { ((MainViewModel)DataContext).Viewport.PanButton = PanButton.Left; }
        private void PanMiddle_Click(object sender, RoutedEventArgs e) { ((MainViewModel)DataContext).Viewport.PanButton = PanButton.Middle; }
        private void PanRight_Click(object sender, RoutedEventArgs e) { ((MainViewModel)DataContext).Viewport.PanButton = PanButton.Right; }

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff" };
            if (dlg.ShowDialog() == true)
            {
                var bmp = new BitmapImage(new System.Uri(dlg.FileName));
                Application.Current.Resources["DemoImage"] = bmp;
                viewport.FitImageRect(new PxRect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            }
        }

        private void RulerUnitsPixels_Click(object sender, RoutedEventArgs e)  => ((MainViewModel)DataContext).Viewport.RulerUnits = MUI.Controls.ImageViewport.Surfaces.RulerUnits.Pixels;
        private void RulerUnitsMillimeters_Click(object sender, RoutedEventArgs e) => ((MainViewModel)DataContext).Viewport.RulerUnits = MUI.Controls.ImageViewport.Surfaces.RulerUnits.Millimeters;
        private void RulerUnitsInches_Click(object sender, RoutedEventArgs e) => ((MainViewModel)DataContext).Viewport.RulerUnits = MUI.Controls.ImageViewport.Surfaces.RulerUnits.Inches;
        private void RulerModeDecimal_Click(object sender, RoutedEventArgs e) => ((MainViewModel)DataContext).Viewport.RulerTickMode = MUI.Controls.ImageViewport.Surfaces.TickMode.Decimal;
        private void RulerModeBinary_Click(object sender, RoutedEventArgs e) => ((MainViewModel)DataContext).Viewport.RulerTickMode = MUI.Controls.ImageViewport.Surfaces.TickMode.Binary;
    }
}