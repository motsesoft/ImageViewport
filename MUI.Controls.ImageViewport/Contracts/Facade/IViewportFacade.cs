using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Input;
using MUI.Controls.ImageViewport.Contracts.Surfaces;

namespace MUI.Controls.ImageViewport.Contracts.Facade
{
    public interface IViewportFacade
    {
        /// <summary>���裺��˷��񣨿ؼ�ͨ������ȡ Current�����ı仯�ȣ���</summary>
        IViewportService Service { get; }

        /// <summary>���裺������Ƶ� Surface����->������</summary>
        IEnumerable<ISurfaceRenderer> Surfaces { get; }

        /// <summary>��ѡ������·������</summary>
        IInputRouter? InputRouter { get; }

        /// <summary>��ѡ���Ҽ��˵���</summary>
        IContextMenuProvider? ContextMenu { get; }

        /// <summary>
        /// ���ݵ������ṩ�Ĳ��ɱ���գ��������� Version һ�µ� IViewportTransforms��
        /// ÿ֡/ÿ����Ⱦ���ؼ������õ�һ�������ٵ�������
        /// </summary>
        IViewportTransforms GetTransforms(in ViewportInfo info);

        /// <summary>������չλ����ѡ����</summary>
        IViewportTransforms? TryGetTransforms(string domain) => null;
    }
}