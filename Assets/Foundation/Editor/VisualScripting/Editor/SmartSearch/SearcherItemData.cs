using System;
using System.Reflection;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;

namespace UnityEditor.Modifier.VisualScripting.Editor.SmartSearch
{
    public enum SearcherItemTarget
    {
        Constant,
        UnaryOperator,
        BinaryOperator,
        Stack,
        StickyNote,
        Node,
        Variable,
        GraphModel,
        Type
    }

    public interface ISearcherItemData
    {
        SearcherItemTarget Target { get; }
    }

    public struct TypeSearcherItemData : ISearcherItemData
    {
        public SearcherItemTarget Target { get; }
        public TypeHandle Type { get; }

        public TypeSearcherItemData(TypeHandle type, SearcherItemTarget target)
        {
            Type = type;
            Target = target;
        }
    }

    public struct UnaryOperatorSearcherItemData : ISearcherItemData
    {
        public SearcherItemTarget Target => SearcherItemTarget.UnaryOperator;
        public UnaryOperatorKind Kind { get; }

        public UnaryOperatorSearcherItemData(UnaryOperatorKind kind)
        {
            Kind = kind;
        }
    }

    public struct BinaryOperatorSearcherItemData : ISearcherItemData
    {
        public SearcherItemTarget Target => SearcherItemTarget.BinaryOperator;
        public BinaryOperatorKind Kind { get; }

        public BinaryOperatorSearcherItemData(BinaryOperatorKind kind)
        {
            Kind = kind;
        }
    }

    public struct NodeSearcherItemData : ISearcherItemData
    {
        public SearcherItemTarget Target => SearcherItemTarget.Node;
        public Type Type { get; }

        public NodeSearcherItemData(Type type)
        {
            Type = type;
        }
    }

    public struct GraphAssetSearcherItemData : ISearcherItemData
    {
        public SearcherItemTarget Target => SearcherItemTarget.GraphModel;
        public IGraphAssetModel GraphAssetModel { get; }

        public GraphAssetSearcherItemData(IGraphAssetModel graphAssetModel)
        {
            GraphAssetModel = graphAssetModel;
        }
    }

    public struct SearcherItemData : ISearcherItemData
    {
        public SearcherItemTarget Target { get; }

        public SearcherItemData(SearcherItemTarget target)
        {
            Target = target;
        }
    }
}