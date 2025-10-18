namespace MUI.Controls.ImageViewport.Facade
{
    internal static class ListUtil
    {
        public static void StableSort<T>(this List<T> list, Comparison<T> cmp)
        {
            // List<T>.Sort �����Ƿ��ȶ��ģ����������ȶ����򣨼�ʵ�֣�
            var indexed = new List<(T item, int idx)>(list.Count);
            for (int i = 0; i < list.Count; i++) indexed.Add((list[i], i));
            indexed.Sort((a, b) =>
            {
                int c = cmp(a.item, b.item);
                return (c != 0) ? c : a.idx.CompareTo(b.idx);
            });
            for (int i = 0; i < indexed.Count; i++) list[i] = indexed[i].item;
        }
    }
}