using System.ComponentModel;
using System.Windows.Media;

using MUI.Controls.ImageViewport.Handlers.Contracts;

namespace ImageViewport.DemoApp.ViewModels
{
    using System.Runtime.CompilerServices;

    using MUI.Controls.ImageViewport;
    using MUI.Controls.ImageViewport.Defaults;
    using MUI.Controls.ImageViewport.Surfaces.Primitives;

    public class ViewportViewModel : INotifyPropertyChanged
    {
        private ImageSource? _imageSource;
        private DefaultImageViewportFacade? _facade;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ViewportViewModel()
        {
            _facade = new DefaultImageViewportFacade(new ImageViewport(), new PanZoomOptions(), null);
        }

        public ImageSource? ImageSource
        {
            get => _imageSource;
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    OnPropertyChanged(nameof(ImageSource));
                    _facade?.SetSource(value);
                }
            }
        }

        public DefaultImageViewportFacade? Facade => _facade;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // UI toggles
        private bool _showRuler = true;
        public bool ShowRuler { get => _showRuler; set { if (_showRuler != value) { _showRuler = value; OnPropertyChanged(); } } }
        private bool _showHud = true;
        public bool ShowHud { get => _showHud; set { if (_showHud != value) { _showHud = value; OnPropertyChanged(); } } }

        private RulerUnits _rulerUnits = RulerUnits.Pixels;
        public RulerUnits RulerUnits { get => _rulerUnits; set { if (_rulerUnits != value) { _rulerUnits = value; OnPropertyChanged(); } } }

        private TickMode _rulerTickMode = TickMode.Decimal;
        public TickMode RulerTickMode { get => _rulerTickMode; set { if (_rulerTickMode != value) { _rulerTickMode = value; OnPropertyChanged(); } } }

        private bool _showGrid = true;
        public bool ShowGrid { get => _showGrid; set { if (_showGrid != value) { _showGrid = value; OnPropertyChanged(); } } }

        private bool _showCrosshair = true;
        public bool ShowCrosshair { get => _showCrosshair; set { if (_showCrosshair != value) { _showCrosshair = value; OnPropertyChanged(); } } }

        private bool _showSelectionOverlay = true;
        public bool ShowSelectionOverlay { get => _showSelectionOverlay; set { if (_showSelectionOverlay != value) { _showSelectionOverlay = value; OnPropertyChanged(); } } }

        private bool _showMeasurementOverlay = true;
        public bool ShowMeasurementOverlay { get => _showMeasurementOverlay; set { if (_showMeasurementOverlay != value) { _showMeasurementOverlay = value; OnPropertyChanged(); } } }

    }
}
