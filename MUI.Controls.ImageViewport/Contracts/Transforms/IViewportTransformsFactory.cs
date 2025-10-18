using MUI.Controls.ImageViewport.Contracts.Abstractions;

namespace MUI.Controls.ImageViewport.Contracts.Transforms
{
    /// <summary>
    /// �����ɱ�� ViewportInfo ���չ�������� Version һ�µ� IViewportTransforms��
    /// ���ϲ㣨���÷�/��Ŀ��ע�룬���� Facade ǿ�����ʵ�֡�
    /// </summary>
    public interface IViewportTransformsFactory
    {
        IViewportTransforms Create(in ViewportInfo info);
    }
}