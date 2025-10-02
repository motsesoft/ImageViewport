namespace MUI.Controls.ImageViewport.Contracts
{
    /// <summary>
    /// 视口层枚举（仅对外可绘制层；内部网格与十字线使用独立内部层）
    /// </summary>
    public enum LayerKind
    {
        Background,
        Active,
        Preview,
        OverlayWorld,
        Hud
    }
}
