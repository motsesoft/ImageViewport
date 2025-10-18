
using System.ComponentModel;

using MUI.Controls.ImageViewport.Defaults;
using MUI.Controls.ImageViewport.Handlers.Contracts;

namespace ImageViewport.DemoApp.ViewModels
{
    using MUI.Controls.ImageViewport;

    public class MainViewModel : INotifyPropertyChanged
    {
        private DefaultImageViewportFacade? _facade;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            // Initialize the ImageViewportFacade with demo settings
            _facade = new DefaultImageViewportFacade(new ImageViewport(), new PanZoomOptions(), null);
        }

        public DefaultImageViewportFacade? Facade
        {
            get => _facade;
            set
            {
                if (_facade != value)
                {
                    _facade = value;
                    OnPropertyChanged(nameof(Facade));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
