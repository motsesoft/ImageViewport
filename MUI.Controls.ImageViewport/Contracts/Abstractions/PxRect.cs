namespace MUI.Controls.ImageViewport.Contracts.Abstractions
{
    /// <summary>
    /// ��ʾһ��������Ϊ��λ�ľ�������
    /// </summary>
    public readonly record struct PxRect(double X, double Y, double Width, double Height)
    {
        /// <summary>
        /// ��ȡһ��ֵ��ָʾ�þ����Ƿ�Ϊ�գ���Ȼ�߶�С�ڵ���0����
        /// </summary>
        public bool IsEmpty => Width <= 0 || Height <= 0;

        /// <summary>
        /// ��ȡ���ε����Ͻǵ㡣
        /// </summary>
        public PxPoint TopLeft => new(X, Y);

        /// <summary>
        /// ��ȡ���ε����ĵ㡣
        /// </summary>
        public PxPoint Center => new(X + Width / 2, Y + Height / 2);
    }
}