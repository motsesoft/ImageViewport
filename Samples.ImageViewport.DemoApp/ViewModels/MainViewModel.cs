using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace Samples.ImageViewport.DemoApp.ViewModels
{
    public enum DemoScenario { Basic, DragWithWheel, PrecisionZoom, BoxZoom, Measure }

    public class MainViewModel : INotifyPropertyChanged
    {
        private DemoScenario _scenario = DemoScenario.Basic;
        public DemoScenario Scenario { get => _scenario; set { if (_scenario != value){ _scenario = value; OnPropertyChanged(); ApplyScenario(); } } }

        public ViewportViewModel Viewport { get; } = new ViewportViewModel();

        public ICommand FitToWindowCommand { get; }
        public ICommand ResetViewCommand { get; }
        public ICommand FitToSelectionCommand { get; }

        public MainViewModel()
        {
            FitToWindowCommand = new RelayCommand(_ => FitToWindow());
            ResetViewCommand   = new RelayCommand(_ => ResetView());
            FitToSelectionCommand = new RelayCommand(_ => FitToSelection(), _ => Viewport.SelectionActive && Viewport.SelectionImageRect.Width > 0 && Viewport.SelectionImageRect.Height > 0);
            ApplyScenario();
        }

        private void FitToWindow()
        {
            // demo：直接将当前可见窗口作为目标（如果有选择，则优先选择）
            var rect = Viewport.SelectionActive && Viewport.SelectionImageRect.Width > 0 && Viewport.SelectionImageRect.Height > 0
                ? Viewport.SelectionImageRect
                : Viewport.CurrentView;
            if (Viewport.Controller != null && rect.Width > 0 && rect.Height > 0)
                Viewport.Controller.FitImageRect(rect);
        }

        private void ResetView()
        {
            // demo：重置到一个标准矩形（0,0,100,100）
            if (Viewport.Controller != null)
                Viewport.Controller.FitImageRect(new PxRect(0,0,100,100));
        }

        private void FitToSelection()
        {
            if (Viewport.Controller != null && Viewport.SelectionActive)
            {
                Viewport.Controller.FitImageRect(Viewport.SelectionImageRect);
                Viewport.SelectionActive = false;
            }
        }

        private void ApplyScenario()
        {
            switch (_scenario)
            {
                case DemoScenario.Basic:
                    Viewport.PanButton = PanButton.Left;
                    Viewport.RequireCtrlForWheelZoom = false;
                    Viewport.ScaleFactor = 1.1;
                    Viewport.HasMeasurement = false;
                    break;
                case DemoScenario.DragWithWheel:
                    Viewport.PanButton = PanButton.Middle;        // 中键拖拽平移
                    Viewport.RequireCtrlForWheelZoom = false;
                    Viewport.ScaleFactor = 1.1;
                    Viewport.HasMeasurement = false;
                    break;
                case DemoScenario.PrecisionZoom:
                    Viewport.PanButton = PanButton.Left;
                    Viewport.RequireCtrlForWheelZoom = true;      // 需按住Ctrl才缩放
                    Viewport.ScaleFactor = 1.05;                    // 更细腻步进
                    Viewport.HasMeasurement = false;
                    break;
                case DemoScenario.BoxZoom:
                    Viewport.PanButton = PanButton.Left;
                    Viewport.RequireCtrlForWheelZoom = false;
                    Viewport.ScaleFactor = 1.1;
                    Viewport.HasMeasurement = false;
                    break;
                case DemoScenario.Measure:
                    Viewport.PanButton = PanButton.Left;
                    Viewport.RequireCtrlForWheelZoom = false;
                    Viewport.ScaleFactor = 1.1;
                    // 随便放两个点作为示例
                    Viewport.HasMeasurement = true;
                    Viewport.MeasureP1 = new PxPoint(10,10);
                    Viewport.MeasureP2 = new PxPoint(250,130);
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null){ _execute = execute; _canExecute = canExecute; }
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}