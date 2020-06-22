using System.Collections.Generic;
using UnityEngine;

namespace Unity.Modifier.GraphToolsFoundation.Model
{
    public interface IHasTitle
    {
        string Title { get; set; }
    }

    public interface IHasSingleInputPort
    {
        IGTFPortModel GTFInputPort { get; }
    }

    public interface IHasSingleOutputPort
    {
        IGTFPortModel GTFOutputPort { get; }
    }

    public interface IHasPorts
    {
        IEnumerable<IGTFPortModel> InputPorts { get; }
        IEnumerable<IGTFPortModel> OutputPorts { get; }
    }

    public interface IHasProgress
    {
        bool HasProgress { get; }
    }

    public interface ISelectable
    {
    }

    public interface ICollapsible
    {
        bool Collapsed { get; set; }
    }

    public interface IResizable
    {
        Rect PositionAndSize { get; set; }
    }

    public interface IPositioned
    {
        Vector2 Position { get; set; }
        void Move(Vector2 delta);
    }

    public interface ICopiable
    {
        bool IsCopiable { get; }
    }

    public interface IDeletable
    {
        bool IsDeletable { get; }
    }

    public interface IDroppable
    {
        bool IsDroppable { get; }
    }

    public interface IRenamable
    {
        bool IsRenamable { get; }
        void Rename(string newName);
    }

    public interface IAscendable
    {
    }

    public interface IModifiable
    {
        bool IsModifiable { get; }
    }

    public interface IGhostEdge
    {
        Vector2 EndPoint { get; }
    }
}