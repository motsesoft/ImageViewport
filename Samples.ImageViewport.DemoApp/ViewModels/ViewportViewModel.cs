using MUI.Controls.ImageViewport.Surfaces;
using MUI.Controls.ImageViewport.Contracts.Input;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace Samples.ImageViewport.DemoApp.ViewModels
{
    public interface IViewportController
    {
        void FitImageRect(PxRect imageRect);
        void ZoomAtWindowPx(double factor, PxPoint pivotWindowPx);
        PxRect WindowRectToImageRect(PxRect winRect);
    }

    public class ViewportViewModel : INotifyPropertyChanged
    {
        private double _targetScale = double.NaN;
        private double _minScale = 0.02;
        private double _maxScale = 40;
        private double _scaleStep = 1.1;
        private PanButton _panButton = PanButton.Left;
        private bool _requireCtrlForWheelZoom = false;

        public IViewportController? Controller { get; set; }

        public double TargetScale { get => _targetScale; set { if (_targetScale != value){ _targetScale = value; OnPropertyChanged(); } } }
        public double MinScale { get => _minScale; set { if (_minScale != value){ _minScale = value; OnPropertyChanged(); } } }
        public double MaxScale { get => _maxScale; set { if (_maxScale != value){ _maxScale = value; OnPropertyChanged(); } } }
        [System.Obsolete("Use ScaleFactor instead.")]
        public double ScaleFactor { get => ScaleFactor; set => ScaleFactor = value; } } }

        public PanButton PanButton { get => _panButton; set { if (_panButton != value){ _panButton = value; OnPropertyChanged(); } } }
        public bool RequireCtrlForWheelZoom { get => _requireCtrlForWheelZoom; set { if (_requireCtrlForWheelZoom != value){ _requireCtrlForWheelZoom = value; OnPropertyChanged(); } } }

        // Readonly mirrors (optional)
        private double _currentScale;
        
private double _scaleFactor = 1.1;
public double ScaleFactor { get => _scaleFactor; set { if (System.Math.Abs(_scaleFactor - value) > double.Epsilon){ _scaleFactor = value; OnPropertyChanged(); } } }

public double CurrentScale { get => _currentScale; set { if (Math.Abs(_currentScale - value) > double.Epsilon){ _currentScale = value; OnPropertyChanged(); } } }
        private PxRect _currentView;
        public PxRect CurrentView { get => _currentView; set { if (!_currentView.Equals(value)){ _currentView = value; OnPropertyChanged(); } } }

        // Selection (window & image space)
        private bool _selectionActive;
        public bool SelectionActive { get => _selectionActive; set { if (_selectionActive != value){ _selectionActive = value; OnPropertyChanged(); } } }
        private PxRect _selectionWinRect;
        public PxRect SelectionWindowRect { get => _selectionWinRect; set { _selectionWinRect = value; OnPropertyChanged(); } }
        private PxRect _selectionImgRect;
        public PxRect SelectionImageRect { get => _selectionImgRect; set { _selectionImgRect = value; OnPropertyChanged(); } }

        // Measurement demo
        public bool HasMeasurement { get; set; }
        public PxPoint MeasureP1 { get; set; }
        public PxPoint MeasureP2 { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PxPoint MouseWindow { get; set; }
        public PxPoint MouseScene { get; set; }
        

// UI toggles
private bool _showRuler = true;
public bool ShowRuler { get => _showRuler; set { if (_showRuler != value){ _showRuler = value; OnPropertyChanged(); } } }
private bool _showHud = true;
public bool ShowHud { get => _showHud; set { if (_showHud != value){ _showHud = value; OnPropertyChanged(); } } }

        private RulerUnits _rulerUnits = RulerUnits.Pixels;
        public RulerUnits RulerUnits { get => _rulerUnits; set { if (_rulerUnits != value){ _rulerUnits = value; OnPropertyChanged(); } } }

        private TickMode _rulerTickMode = TickMode.Decimal;
        public TickMode RulerTickMode { get => _rulerTickMode; set { if (_rulerTickMode != value){ _rulerTickMode = value; OnPropertyChanged(); } } }

private bool _showGrid = true;
public bool ShowGrid { get => _showGrid; set { if (_showGrid != value){ _showGrid = value; OnPropertyChanged(); } } }

private bool _showCrosshair = true;
public bool ShowCrosshair { get => _showCrosshair; set { if (_showCrosshair != value){ _showCrosshair = value; OnPropertyChanged(); } } }

        private bool _showSelectionOverlay = true;
        public bool ShowSelectionOverlay { get => _showSelectionOverlay; set { if (_showSelectionOverlay != value){ _showSelectionOverlay = value; OnPropertyChanged(); } } }

        private bool _showMeasurementOverlay = true;
        public bool ShowMeasurementOverlay { get => _showMeasurementOverlay; set { if (_showMeasurementOverlay != value){ _showMeasurementOverlay = value; OnPropertyChanged(); } } }


        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}