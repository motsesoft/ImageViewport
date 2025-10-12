namespace MUI.Controls.ImageViewport.Contracts.Abstractions
{
    public readonly record struct PxRect(double X, double Y, double Width, double Height)
    {
        public bool IsEmpty => Width <= 0 || Height <= 0;
        public PxPoint TopLeft => new(X, Y);
        public PxPoint Center => new(X + Width / 2, Y + Height / 2);
    }
}
