using System.Windows.Input;

using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace MUI.Controls.ImageViewport.Contracts.Input
{
    public sealed class PointerEvent
    {
        public DateTime Timestamp { get; init; }
        public PxPoint WindowPx { get; init; }
        public PxPoint ImagePx { get; init; }
        public bool IsLeftDown { get; init; }
        public bool IsRightDown { get; init; }
        public bool IsMiddleDown { get; init; }
        public double WheelDelta { get; init; }
        public int ClickCount { get; init; }
        public ModifierKeys Modifiers { get; init; }
        public bool CurrentLeftPressed { get; init; }
        public bool CurrentRightPressed { get; init; }
        public bool CurrentMiddlePressed { get; init; }
    }
}
