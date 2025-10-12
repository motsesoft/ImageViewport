namespace MUI.Controls.ImageViewport.Contracts.Input
{
    public interface IInputRouter
    {
        bool OnMove(object sender, PointerEvent p);
        bool OnWheel(object sender, PointerEvent p);
        bool OnMouseDown(object sender, PointerEvent p) => false;
        bool OnMouseUp(object sender, PointerEvent p) => false;
        bool OnLeftDown(object sender, PointerEvent p);
        bool OnLeftUp(object sender, PointerEvent p);
        bool OnRightDown(object sender, PointerEvent p);
        bool OnRightUp(object sender, PointerEvent p);
        bool OnMiddleDown(object sender, PointerEvent p);
        bool OnMiddleUp(object sender, PointerEvent p);
    }
}
