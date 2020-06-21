using System;

namespace UnityEditor.Modifier.VisualScripting.Editor.SmartSearch
{
    public static class SearcherFilterExtensions
    {
        public static void RegisterConstant(this SearcherFilter filter, Func<TypeSearcherItemData, bool> func)
        {
            filter.Register(func, SearcherItemTarget.Constant);
        }

        public static void RegisterUnaryOperator(this SearcherFilter filter, Func<UnaryOperatorSearcherItemData, bool> func)
        {
            filter.Register(func, SearcherItemTarget.UnaryOperator);
        }

        public static void RegisterBinaryOperator(this SearcherFilter filter, Func<BinaryOperatorSearcherItemData, bool> func)
        {
            filter.Register(func, SearcherItemTarget.BinaryOperator);
        }

        public static void RegisterStack(this SearcherFilter filter, Func<SearcherItemData, bool> func)
        {
            filter.Register(func, SearcherItemTarget.Stack);
        }

        public static void RegisterStickyNote(this SearcherFilter filter, Func<SearcherItemData, bool> func)
        {
            filter.Register(func, SearcherItemTarget.StickyNote);
        }

        public static void RegisterNode(this SearcherFilter filter, Func<NodeSearcherItemData, bool> func)
        {
            filter.Register(func, SearcherItemTarget.Node);
        }

        public static void RegisterVariable(this SearcherFilter filter, Func<TypeSearcherItemData, bool> func)
        {
            filter.Register(func, SearcherItemTarget.Variable);
        }

        public static void RegisterGraphAsset(this SearcherFilter filter, Func<GraphAssetSearcherItemData, bool> func)
        {
            filter.Register(func, SearcherItemTarget.GraphModel);
        }

        public static void RegisterType(this SearcherFilter filter, Func<TypeSearcherItemData, bool> func)
        {
            filter.Register(func, SearcherItemTarget.Type);
        }
    }
}