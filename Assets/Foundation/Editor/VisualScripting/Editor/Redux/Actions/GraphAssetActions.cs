
using System;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class CreateGraphAssetAction : IAction
    {
        public readonly Type StencilType;
        public readonly Type GraphType;
        public readonly Type AssetType;
        public readonly string Name;
        public readonly string AssetPath;
        public readonly bool WriteOnDisk;
        public readonly IGraphTemplate GraphTemplate;

        public CreateGraphAssetAction(Type stencilType, string name = "", string assetPath = "", GameObject instance = null, bool writeOnDisk = true, IGraphTemplate graphTemplate = null)
            : this(stencilType, typeof(VSGraphModel), typeof(VSGraphAssetModel), name, assetPath, instance, writeOnDisk, graphTemplate)
        {

        }

        public CreateGraphAssetAction(Type stencilType, Type graphType, Type assetType, string name = "", string assetPath = "", GameObject instance = null, bool writeOnDisk = true, IGraphTemplate graphTemplate = null)
        {
            StencilType = stencilType;
            GraphType = graphType;
            AssetType = assetType;
            Name = name;
            AssetPath = assetPath;
            WriteOnDisk = writeOnDisk;
            GraphTemplate = graphTemplate;
        }
    }
}