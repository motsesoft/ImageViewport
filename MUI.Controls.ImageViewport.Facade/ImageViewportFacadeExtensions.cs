
using MUI.Controls.ImageViewport.Contracts.Input;
using MUI.Controls.ImageViewport.Contracts.Surfaces;

namespace MUI.Controls.ImageViewport.Facade
{
    /// <summary>
    /// �����չ��ֻ���� IEditableViewportFacade �Ĺ���������������ʵ����˽�г�Ա��
    /// </summary>
    public static class ImageViewportFacadeExtensions
    {
        // ---------- ������� ----------
        public static IEditableViewportFacade AddSurfaces(
            this IEditableViewportFacade facade,
            IEnumerable<ISurfaceRenderer> surfaces)
        {
            if (facade is null) throw new ArgumentNullException(nameof(facade));
            if (surfaces is null) return facade;

            using (BeginBatch(facade))
            {
                foreach (var s in surfaces) facade.AddSurface(s);
            }
            return facade;
        }

        // ---------- �������� ----------
        public static int ShowGroup(this IEditableViewportFacade facade, string group, IEnumerable<ISurfaceRenderer>? members = null)
            => SetGroupVisibleInternal(facade, group, true, members);

        public static int HideGroup(this IEditableViewportFacade facade, string group, IEnumerable<ISurfaceRenderer>? members = null)
            => SetGroupVisibleInternal(facade, group, false, members);

        /// <summary>�ڲ�ͳһʵ�֣�������ɼ��ԡ��������ýӿ��Դ��� ShowGroup/HideGroup��û�г�Ա�б�Ͱ������������ṩ members ʱ������á�</summary>
        private static int SetGroupVisibleInternal(IEditableViewportFacade facade, string group, bool visible, IEnumerable<ISurfaceRenderer>? members)
        {
            if (facade is null) throw new ArgumentNullException(nameof(facade));
            if (string.IsNullOrWhiteSpace(group)) return 0;

            int changed = 0;

            // ���ʵ����֧���鼶����������ǽӿ������У���ֱ����
            if (members is null)
            {
                changed = visible ? facade.ShowGroup(group) : facade.HideGroup(group);
                return changed;
            }

            // ����Ը�����Ա������ÿɼ���
            using (BeginBatch(facade))
            {
                foreach (var s in members)
                {
                    // �޷��ж� s �����ĸ��飨�ӿ�δ��¶�������������������
                    if (facade.SetSurfaceVisible(s, visible)) changed++;
                }
            }
            return changed;
        }

        // ---------- Z ����� ----------
        public static IEditableViewportFacade BringToFront(this IEditableViewportFacade facade, ISurfaceRenderer surface)
        {
            if (facade.BringToFront(surface)) return facade;
            return facade;
        }

        public static IEditableViewportFacade SendToBack(this IEditableViewportFacade facade, ISurfaceRenderer surface)
        {
            if (facade.SendToBack(surface)) return facade;
            return facade;
        }

        public static IEditableViewportFacade MoveTo(this IEditableViewportFacade facade, ISurfaceRenderer surface, int index)
        {
            if (facade.MoveTo(surface, index)) return facade;
            return facade;
        }

        // ---------- ����·��ƴ�� ----------
        public static IEditableViewportFacade UseRouter(this IEditableViewportFacade facade, IInputRouter router)
        {
            facade.SetInputRouter(router);
            return facade;
        }

        public static IEditableViewportFacade AppendRouter(this IEditableViewportFacade facade, IInputRouter router)
        {
            facade.AppendInputRouter(router);
            return facade;
        }

        public static IEditableViewportFacade PrependRouter(this IEditableViewportFacade facade, IInputRouter router)
        {
            facade.PrependInputRouter(router);
            return facade;
        }

        // ---------- �˵� ----------
        public static IEditableViewportFacade WithContextMenu(this IEditableViewportFacade facade, IContextMenuProvider provider)
        {
            facade.SetContextMenu(provider);
            return facade;
        }

        // ---------- �������������� ----------
        private static IDisposable BeginBatch(IEditableViewportFacade facade)
        {
            // �Գ���ʵ�֣��籾�ĵ� ImageViewportFacade���ɷ��䳢���� BatchUpdate��
            // ���ʵ��û�� BatchUpdate���ɷ��ؿն���
            var mi = facade.GetType().GetMethod("BatchUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (mi != null && mi.GetParameters().Length == 0 && mi.ReturnType == typeof(IDisposable))
            {
                return (IDisposable)mi.Invoke(facade, null)!;
            }
            return new NullScope();
        }

        private sealed class NullScope : IDisposable { public void Dispose() { } }
    }
}
