namespace MUI.Controls.ImageViewport.Contracts.Abstractions.Events
{
    public sealed class WindowPixelSizeChangedEventArgs : EventArgs
    {
        public WindowPixelSizeChangedEventArgs(PxSize newSize)
        {
            NewSize = newSize;
        }
        public PxSize NewSize { get; }
    }
}
