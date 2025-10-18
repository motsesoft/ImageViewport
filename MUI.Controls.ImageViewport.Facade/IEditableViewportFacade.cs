
// File: MUI.Controls.ImageViewport.Facade.Abstractions/IEditableViewportFacade.cs
using MUI.Controls.ImageViewport.Contracts.Input;
using MUI.Controls.ImageViewport.Contracts.Surfaces;

namespace MUI.Controls.ImageViewport.Facade
{
    /// <summary>
    /// ���򡰿ɱ༭����ͨ�� Facade ϸ���ӿڣ����ı���� IViewportFacade ��Լ��
    /// </summary>
    public interface IEditableViewportFacade : Contracts.Facade.IViewportFacade
    {
        // ---- Surface ����˳�� Z ��Խ����Խ�ϲ㣩 ----
        /// <summary>�ɱ༭�� Surface �б��� Facade �ڲ�����ͬ������</summary>
        IList<ISurfaceRenderer> SurfacesMutable { get; }

        /// <summary>���뵽���㣨ĩβ����</summary>
        bool AddSurface(ISurfaceRenderer surface);

        /// <summary>�� index ���루0 Ϊ��ײ㣩��</summary>
        bool InsertSurface(int index, ISurfaceRenderer surface);

        /// <summary>��ʵ���Ƴ���</summary>
        bool RemoveSurface(ISurfaceRenderer surface);

        /// <summary>�������������غ���ȾҲ���������룩��</summary>
        bool SetSurfaceVisible(ISurfaceRenderer surface, bool visible);

        /// <summary>Z �������</summary>
        bool BringToFront(ISurfaceRenderer surface);
        bool SendToBack(ISurfaceRenderer surface);
        bool MoveTo(ISurfaceRenderer surface, int index);

        /// <summary>Ϊ Surface �趨/������������������������ͷֲ㳡���л�����</summary>
        bool SetSurfaceGroup(ISurfaceRenderer surface, string? group);

        // ---- ���������ݣ�----
        int HideGroup(string group);
        int ShowGroup(string group);
        int ClearGroup(string group);

        // ---- ����·��ƴ�� ----
        void SetInputRouter(IInputRouter? router);            // ����
        void PrependInputRouter(IInputRouter router);         // ͷ��ƴ��
        void AppendInputRouter(IInputRouter router);          // β��ƴ��

        // ---- �Ҽ��˵� ----
        void SetContextMenu(IContextMenuProvider? provider);
    }
}
