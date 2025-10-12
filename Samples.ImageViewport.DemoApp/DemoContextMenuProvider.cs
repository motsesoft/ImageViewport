using System.Windows.Controls;
using MUI.Controls.ImageViewport.Contracts.Input;
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using Samples.ImageViewport.DemoApp.ViewModels;

namespace Samples.ImageViewport.DemoApp
{
    public sealed class DemoContextMenuProvider : IContextMenuProvider
    {
        private readonly MainViewModel _vm;
        public DemoContextMenuProvider(MainViewModel vm){ _vm = vm; }

        public ContextMenu? BuildContextMenu(PointerEvent p)
        {
            var menu = new ContextMenu();
            menu.Items.Add(new MenuItem { Header = "Fit To Window", Command = _vm.FitToWindowCommand });
            var fitSel = new MenuItem { Header = "Fit To Selection", Command = _vm.FitToSelectionCommand };
            fitSel.IsEnabled = _vm.Viewport.SelectionActive && _vm.Viewport.SelectionImageRect.Width > 0 && _vm.Viewport.SelectionImageRect.Height > 0;
            menu.Items.Add(fitSel);
            menu.Items.Add(new Separator());
            menu.Items.Add(new MenuItem { Header = "Reset View", Command = _vm.ResetViewCommand });
            return menu;
        }
    }
}