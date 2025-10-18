
using MUI.Controls.ImageViewport.Contracts.Abstractions;
using MUI.Controls.ImageViewport.Contracts.Input;
using MUI.Controls.ImageViewport.Contracts.Surfaces;
using MUI.Controls.ImageViewport.Handlers.Composite;
using MUI.Controls.ImageViewport.Runtime.Services;
using MUI.Controls.ImageViewport.Runtime.Transforms;

namespace MUI.Controls.ImageViewport.Facade
{
    /// <summary>
    /// ͨ�á�����չ�� Facade:���ɹ��� Surface �б�������·������
    /// - ���ھ۵����:�ؼ������� IViewportFacade;
    /// - �ɲ��:Part(Renderer + ��ѡ Router + ZIndex + Group + Visible + ReceivesInput)
    /// - Transforms:֧���ⲿ�Զ��幤�� + ������ + �汾����
    /// </summary>
    public sealed class ImageViewportFacade : IEditableViewportFacade, IDisposable
    {
        #region Part ����
        private sealed class Part
        {
            public string Key { get; }
            public string? Group { get; set; }
            public int ZIndex { get; set; }
            public bool Visible { get; set; } = true;
            public bool ReceivesInput { get; set; } = true;

            public ISurfaceRenderer Renderer { get; set; }
            public IInputRouter? Router { get; set; }

            public Part(string key, ISurfaceRenderer renderer, IInputRouter? router = null)
            {
                Key = key ?? throw new ArgumentNullException(nameof(key));
                Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
                Router = router;
            }
        }
        #endregion

        // ---------- ״̬ ----------
        private readonly List<Part> _parts = new();
        private readonly List<ISurfaceRenderer> _visibleCache = new();
        private IInputRouter? _composite;
        private bool _dirtySurfaces = true;
        private bool _dirtyInput = true;
        private int _batchDepth = 0;

        // ---------- �������ȼ�(��ѡ�ӿ�) ----------
        private static int GetPriority(IInputRouter r) => (r is IInputPrioritizable p) ? p.Priority : 0;

        // ---------- Transforms �����뻺�� ----------
        private readonly Func<ViewportInfo, IViewportTransforms> _transformsFactory;
        private ulong _lastVersion = ulong.MaxValue;
        private IViewportTransforms? _cachedTransforms;

        public ImageViewportFacade(
            IViewportService? service = null,
            Func<ViewportInfo, IViewportTransforms>? transformsFactory = null,
            IContextMenuProvider? contextMenu = null,
            IInputRouter? initialRouter = null)
        {
            Service = service ?? new BuiltInViewportService();
            _transformsFactory = transformsFactory ?? (info => new BuiltInViewportTransforms(info));
            ContextMenu = contextMenu;
            _composite = initialRouter;

            // ��ʼ�� current & ����
            UpdateCache(Service.Current);
        }

        #region IViewportFacade
        public IEnumerable<ISurfaceRenderer> Surfaces => GetVisibleSurfaces();
        public IViewportService Service { get; }
        public IInputRouter? InputRouter => GetCompositeRouter();
        public IContextMenuProvider? ContextMenu { get; private set; }

        public IViewportTransforms GetTransforms(in ViewportInfo info)
        {
            if (_cachedTransforms is null || _lastVersion != info.Version)
            {
                UpdateCache(info);
            }
            return _cachedTransforms!;
        }
        #endregion

        #region IEditableViewportFacade ���� Surface �б�(���������)
        // ��翴������"ֻ�����ɼ������"�Ŀɱ伯����ͼ��
        // �򻯴���:����һ��ͶӰ����(�޸�ͨ���·� API ���),����ֱ�Ӵ۸��ڲ� _parts��
        public IList<ISurfaceRenderer> SurfacesMutable => GetVisibleMutableView();

        private IList<ISurfaceRenderer> GetVisibleMutableView()
        {
            RebuildCachesIfDirty();
            // �ṩһ���ɱ༭������:�������޸�ת��Ϊ�·� API ����
            return new MutableSurfaceAdapter(this);
        }

        private sealed class MutableSurfaceAdapter : IList<ISurfaceRenderer>
        {
            private readonly ImageViewportFacade _f;
            public MutableSurfaceAdapter(ImageViewportFacade f) => _f = f;

            private List<ISurfaceRenderer> Snapshot => _f.GetVisibleSurfaces().ToList();

            public int Count => Snapshot.Count;
            public bool IsReadOnly => false;
            public ISurfaceRenderer this[int index] { get => Snapshot[index]; set => _f.MoveReplace(index, value); }

            public int IndexOf(ISurfaceRenderer item) => Snapshot.IndexOf(item);
            public void Insert(int index, ISurfaceRenderer item) => _f.InsertSurface(index, item);
            public void RemoveAt(int index) { var s = Snapshot[index]; _f.RemoveSurface(s); }
            public void Add(ISurfaceRenderer item) => _f.AddSurface(item);
            public void Clear() { foreach (var it in Snapshot.ToList()) _f.RemoveSurface(it); }
            public bool Contains(ISurfaceRenderer item) => Snapshot.Contains(item);
            public void CopyTo(ISurfaceRenderer[] array, int arrayIndex) => Snapshot.CopyTo(array, arrayIndex);
            public bool Remove(ISurfaceRenderer item) => _f.RemoveSurface(item);
            public IEnumerator<ISurfaceRenderer> GetEnumerator() => Snapshot.GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public bool AddSurface(ISurfaceRenderer surface)
        {
            if (surface is null) return false;
            var z = _parts.Count == 0 ? 0 : _parts.Max(p => p.ZIndex) + 1;
            _parts.Add(new Part(Guid.NewGuid().ToString("N"), surface) { ZIndex = z });
            MarkDirty();
            return true;
        }

        public bool InsertSurface(int index, ISurfaceRenderer surface)
        {
            if (surface is null) return false;
            // ���ɼ���ͼ�� index ӳ����ڲ� ZIndex:�򵥲���=ֱ�Ӹ�ֵ index,�������ȶ�����
            var ordered = _parts.OrderBy(p => p.ZIndex).ToList();
            var newPart = new Part(Guid.NewGuid().ToString("N"), surface) { ZIndex = index };
            ordered.Insert(Math.Clamp(index, 0, ordered.Count), newPart);
            // ���±��,��֤ ZIndex ����
            for (int i = 0; i < ordered.Count; i++) ordered[i].ZIndex = i;
            _parts.Clear(); _parts.AddRange(ordered);
            MarkDirty();
            return true;
        }

        public bool RemoveSurface(ISurfaceRenderer surface)
        {
            var idx = _parts.FindIndex(p => ReferenceEquals(p.Renderer, surface));
            if (idx < 0) return false;
            _parts.RemoveAt(idx);
            MarkDirty();
            return true;
        }

        public bool SetSurfaceVisible(ISurfaceRenderer surface, bool visible)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;
            if (p.Visible == visible) return true;
            p.Visible = visible;
            MarkDirty();
            return true;
        }

        public bool SetSurfaceGroup(ISurfaceRenderer surface, string? group)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;
            if (p.Group == group) return true;
            p.Group = group;
            // ��������Ӱ����Ⱦ˳�������,���� MarkDirty
            return true;
        }

        public bool BringToFront(ISurfaceRenderer surface)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;
            p.ZIndex = (_parts.Count == 0 ? 0 : _parts.Max(x => x.ZIndex)) + 1;
            MarkDirty();
            return true;
        }

        public bool SendToBack(ISurfaceRenderer surface)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;
            p.ZIndex = (_parts.Count == 0 ? 0 : _parts.Min(x => x.ZIndex)) - 1;
            MarkDirty();
            return true;
        }

        public bool MoveTo(ISurfaceRenderer surface, int index)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;

            var ordered = _parts.OrderBy(x => x.ZIndex).ToList();
            ordered.Remove(p);
            ordered.Insert(Math.Clamp(index, 0, ordered.Count), p);
            for (int i = 0; i < ordered.Count; i++) ordered[i].ZIndex = i;

            _parts.Clear(); _parts.AddRange(ordered);
            MarkDirty();
            return true;
        }

        private void MoveReplace(int index, ISurfaceRenderer newRenderer)
        {
            // ���ɼ���ͼ index ӳ�����ʵ Part,���滻 Renderer(ͨ���Ƴ��ɵġ������µķ�ʽ)
            var ordered = _parts.Where(p => p.Visible).OrderBy(p => p.ZIndex).ToList();
            if (index < 0 || index >= ordered.Count) return;

            var target = ordered[index];
            // �һ�ԭ Part
            var real = _parts.First(p => ReferenceEquals(p, target));
            var z = real.ZIndex;
            var group = real.Group;
            var visible = real.Visible;
            var receivesInput = real.ReceivesInput;
            var router = real.Router;

            // ����ͬλ�ò����� renderer ��ɾ���ɵ�
            _parts.Remove(real);
            _parts.Add(new Part(Guid.NewGuid().ToString("N"), newRenderer)
            {
                ZIndex = z,
                Group = group,
                Visible = visible,
                ReceivesInput = receivesInput,
                Router = router
            });
            MarkDirty();
        }

        public int HideGroup(string group)
        {
            int n = 0;
            foreach (var p in _parts.Where(p => p.Group == group).ToList())
            {
                if (p.Visible) { p.Visible = false; n++; }
            }
            if (n > 0) MarkDirty();
            return n;
        }

        public int ShowGroup(string group)
        {
            int n = 0;
            foreach (var p in _parts.Where(p => p.Group == group).ToList())
            {
                if (!p.Visible) { p.Visible = true; n++; }
            }
            if (n > 0) MarkDirty();
            return n;
        }

        public int ClearGroup(string group)
        {
            int n = _parts.RemoveAll(p => p.Group == group);
            if (n > 0) MarkDirty();
            return n;
        }

        public IDisposable BatchUpdate() => new Batch(this);
        private sealed class Batch : IDisposable
        {
            private readonly ImageViewportFacade _f;
            private bool _disposed;
            public Batch(ImageViewportFacade f) { _f = f; _f._batchDepth++; }
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                if (--_f._batchDepth == 0) _f.RebuildCachesIfDirty();
            }
        }
        #endregion

        #region IEditableViewportFacade ���� ����·�� & �˵�
        public void SetInputRouter(IInputRouter? router)
        {
            _composite = router;
            _dirtyInput = false; // �滻����ʱ,����Ҫ�ؽ�
        }

        public void PrependInputRouter(IInputRouter router)
        {
            var current = GetCompositeRouter();
            _composite = (current is null)
                ? router
                : new CompositeInputRouter(new[] { router, current });
        }

        public void AppendInputRouter(IInputRouter router)
        {
            var current = GetCompositeRouter();
            _composite = (current is null)
                ? router
                : new CompositeInputRouter(new[] { current, router });
        }

        public void SetContextMenu(IContextMenuProvider? provider) => ContextMenu = provider;
        #endregion

        #region �ڲ�:�ɼ� Surface �б� & ����ۺ�
        private IEnumerable<ISurfaceRenderer> GetVisibleSurfaces()
        {
            RebuildCachesIfDirty();
            return _visibleCache;
        }

        private IInputRouter? GetCompositeRouter()
        {
            RebuildCachesIfDirty();
            return _composite;
        }

        private void RebuildCachesIfDirty()
        {
            if (_batchDepth > 0) return;

            if (_dirtySurfaces)
            {
                _visibleCache.Clear();
                foreach (var p in _parts.Where(p => p.Visible).OrderBy(p => p.ZIndex))
                    _visibleCache.Add(p.Renderer);
                _dirtySurfaces = false;
            }

            if (_dirtyInput)
            {
                var routers = _parts
                    .Where(p => p.Visible && p.ReceivesInput && p.Router != null)
                    .Select(p => (router: p.Router!, z: p.ZIndex, pri: GetPriority(p.Router!)))
                    .ToList();

                routers.Sort((a, b) =>
                {
                    int c = b.pri.CompareTo(a.pri);
                    return (c != 0) ? c : b.z.CompareTo(a.z); // ���ȼ�����ǰ;ͬ���ȼ� Z ����ǰ
                });

                _composite = routers.Count switch
                {
                    0 => _composite, // �����ⲿָ���� router(����)
                    1 => (_composite != null)
                        ? new CompositeInputRouter(new[] { _composite, routers[0].router })
                        : routers[0].router,
                    _ => new CompositeInputRouter(
                        (_composite != null
                            ? new[] { _composite }.Concat(routers.Select(x => x.router))
                            : routers.Select(x => x.router)).ToArray())
                };
                _dirtyInput = false;
            }
        }

        private void MarkDirty()
        {
            _dirtySurfaces = true;
            _dirtyInput = true;
            if (_batchDepth == 0) RebuildCachesIfDirty();
        }
        #endregion

        #region ����
        private void UpdateCache(ViewportInfo info)
        {
            // ���贴�� + �汾����;���������ϲ�ע��(��ǿ�� BuiltIn)
            _cachedTransforms = _transformsFactory(info);
            _lastVersion = info.Version;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            _parts.Clear();
            _visibleCache.Clear();
            _composite = null;
            _dirtyInput = _dirtySurfaces = false;
        }
        #endregion
    }
}