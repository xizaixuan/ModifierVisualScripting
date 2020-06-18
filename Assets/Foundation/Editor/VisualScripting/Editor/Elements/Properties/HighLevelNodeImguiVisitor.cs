using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Properties;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEditor.Modifier.VisualScripting.Runtime;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public abstract class ImguiVisitorBase : PropertyVisitor
    {
        static ImguiVisitorBase()
        {
        }

        protected virtual IEnumerable<IPropertyVisitorAdapter> Adapters
        {
            get
            {
                yield return new IMGUIPrimitivesAdapter();
                yield return new IMGUIMathematicsAdapter();
                yield return new TypeHandleVisitorAdapter(this);
            }
        }

        public ImguiVisitorBase()
        {
            foreach (var adapter in Adapters)
            {
                AddAdapter(adapter);
            }
        }

        protected override VisitStatus Visit<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
        {
            if (typeof(TValue).IsEnum)
            {
                EditorGUI.BeginChangeCheck();

                value = (TValue)(object)EditorGUILayout.EnumPopup(property.GetName(), (Enum)(object)value);

                if (EditorGUI.EndChangeCheck())
                    changeTracker.MarkChanged();
            }
            else
            {
                GUILayout.Label(property.GetName());
            }

            return VisitStatus.Handled;
        }

        public class IMGUIAdapter : IPropertyVisitorAdapter
        {
            protected static void DoField<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker, Func<GUIContent, TValue, TValue> drawer)
                where TProperty : IProperty<TContainer, TValue>
            {
                EditorGUI.BeginChangeCheck();

                value = drawer(new GUIContent(property.GetName()), value);

                if (EditorGUI.EndChangeCheck())
                {
                    changeTracker.MarkChanged();
                }
            }
        }

        public abstract class PickFromSearcherAdapter<T> : IMGUIAdapter
        {
            readonly ImguiVisitorBase m_HighLevelNodeImguiVisitor;
            INodeModel m_EditedModel;
            protected T m_Picked;

            public Stencil Stencil => m_HighLevelNodeImguiVisitor.model.GraphModel.Stencil;
            public INodeModel VisitorModel => m_HighLevelNodeImguiVisitor.model;

            protected PickFromSearcherAdapter(ImguiVisitorBase highLevelNodeImguiVisitor)
            {
                m_HighLevelNodeImguiVisitor = highLevelNodeImguiVisitor;
            }

            protected abstract T InvalidValue { get; }

            protected abstract string Title(T currentValue);

            protected abstract void ShowSearcher(Stencil stencil, Vector2 position, IProperty property, T currentValue);

            protected bool ExposeProperty(IProperty property, ref ChangeTracker changeTracker, ref T pickedType)
            {
                bool justPicked = false;
                if (m_EditedModel == m_HighLevelNodeImguiVisitor.model && !m_Picked.Equals(InvalidValue))
                {
                    pickedType = m_Picked;
                    justPicked = true;
                    m_Picked = InvalidValue;
                    m_EditedModel = null;
                    changeTracker.MarkChanged();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(property.GetName());

                if (GUILayout.Button(Title(pickedType)))
                {
                    m_EditedModel = m_HighLevelNodeImguiVisitor.model;
                    var mousePosition = Event.current.mousePosition;
                    if (m_HighLevelNodeImguiVisitor.CurrentContainer != null)
                        mousePosition = m_HighLevelNodeImguiVisitor.CurrentContainer.LocalToWorld(mousePosition);
                    ShowSearcher(Stencil, mousePosition, property, pickedType);
                }

                EditorGUILayout.EndHorizontal();
                return justPicked;
            }
        }

        public abstract class TypeHandleSearcherAdapter : PickFromSearcherAdapter<TypeHandle>
        {
            protected override string Title(TypeHandle _) => m_Picked.GetMetadata(Stencil).FriendlyName;

            protected TypeHandleSearcherAdapter(ImguiVisitorBase highLevelNodeImguiVisitor)
                : base(highLevelNodeImguiVisitor) { }

            protected override TypeHandle InvalidValue => default;

            protected override void ShowSearcher(Stencil stencil, Vector2 position, IProperty property, TypeHandle currentValue)
            {
                SearcherService.ShowTypes(stencil, position, (type, unknown) => m_Picked = type, MakeSearcherAdapterForProperty(property, VisitorModel));
            }

            protected virtual SearcherFilter MakeSearcherAdapterForProperty(IProperty property, INodeModel model)
            {
                var attribute = property.Attributes.GetAttribute<TypeSearcherAttribute>();
                if (attribute.FilteredType != null)
                {
                    var filter = attribute.FilteredType;
                    Assert.IsTrue(typeof(ISearcherFilter).IsAssignableFrom(filter),
                        "The filter is not type of ISearcherFilter");
                    return ((ISearcherFilter)Activator.CreateInstance(filter)).GetFilter(VisitorModel);
                }

                return null;
            }
        }

        class TypeHandleVisitorAdapter : TypeHandleSearcherAdapter, IVisitAdapter<TypeHandle>
        {
            public TypeHandleVisitorAdapter(ImguiVisitorBase highLevelNodeImguiVisitor)
                : base(highLevelNodeImguiVisitor)
            {
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref TypeHandle value, ref ChangeTracker changeTracker) where TProperty : IProperty<TContainer, TypeHandle>
            {
                if (!property.Attributes.HasAttribute<TypeSearcherAttribute>())
                    return VisitStatus.Unhandled;
                ExposeProperty(property, ref changeTracker, ref value);

                return VisitStatus.Handled;
            }
        }

        public INodeModel model { get; set; }
        public IMGUIContainer CurrentContainer { get; set; }

        class IMGUIPrimitivesAdapter : IMGUIAdapter,
            IVisitAdapterPrimitives
        {
            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref sbyte value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, sbyte>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => (sbyte)EditorGUILayout.IntField(label, val));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref short value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, short>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => (short)EditorGUILayout.IntField(label, val));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref int value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, int>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => EditorGUILayout.IntField(label, val));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref long value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, long>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => EditorGUILayout.LongField(label, val));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref byte value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, byte>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => (byte)EditorGUILayout.IntField(label, val));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref ushort value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, ushort>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => (ushort)EditorGUILayout.IntField(label, val));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref uint value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, uint>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => (uint)EditorGUILayout.LongField(label, val));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref ulong value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, ulong>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) =>
                {
                    EditorGUILayout.TextField(label, text: val.ToString());
                    return val;
                });
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref float value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, float>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => EditorGUILayout.FloatField(label, val));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref double value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, double>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => EditorGUILayout.DoubleField(label, val));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref bool value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, bool>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => EditorGUILayout.Toggle(label, val));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref char value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, char>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => EditorGUILayout.TextField(label, val.ToString()).FirstOrDefault());
                return VisitStatus.Handled;
            }
        }

        class IMGUIMathematicsAdapter : IMGUIAdapter
            , IVisitAdapter<Quaternion>
            , IVisitAdapter<Vector2>
            , IVisitAdapter<Vector3>
            , IVisitAdapter<Vector4>
            , IVisitAdapter<Matrix4x4>
        {
            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref Quaternion value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, Quaternion>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => Quaternion.Euler(EditorGUILayout.Vector3Field(label, val.eulerAngles)));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref Vector2 value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, Vector2>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => EditorGUILayout.Vector2Field(label, val));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref Vector3 value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, Vector3>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => EditorGUILayout.Vector3Field(label, val));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref Vector4 value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, Vector4>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) => EditorGUILayout.Vector4Field(label, val));
                return VisitStatus.Handled;
            }

            public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref Matrix4x4 value, ref ChangeTracker changeTracker)
                where TProperty : IProperty<TContainer, Matrix4x4>
            {
                DoField(property, ref container, ref value, ref changeTracker, (label, val) =>
                {
                    val.SetColumn(0, EditorGUILayout.Vector4Field(property.GetName(), val.GetColumn(0)));
                    val.SetColumn(1, EditorGUILayout.Vector4Field(" ", val.GetColumn(1)));
                    val.SetColumn(2, EditorGUILayout.Vector4Field(" ", val.GetColumn(2)));
                    val.SetColumn(3, EditorGUILayout.Vector4Field(" ", val.GetColumn(3)));

                    return val;
                });

                return VisitStatus.Handled;
            }
        }

        protected virtual bool SkipChildren<TProperty, TContainer, TValue>()
        {
            return false;
        }
    }

    class StencilImguiVisitor : ImguiVisitorBase
    {
        readonly HashSet<string> m_PropertiesVisibleInGraphInspector;

        public StencilImguiVisitor(IEnumerable<string> propertiesVisibleInGraphInspector)
        {
            m_PropertiesVisibleInGraphInspector = new HashSet<string>(propertiesVisibleInGraphInspector);
        }

        public override bool IsExcluded<TProperty, TContainer, TValue>(TProperty property, ref TContainer container)
        {
            return !m_PropertiesVisibleInGraphInspector.Contains(property.GetName());
        }
    }

    public class HighLevelNodeImguiVisitor : ImguiVisitorBase
    {
        const string k_ScriptPropertyName = "m_Script";
        static readonly HashSet<string> k_ExcludedPropertyNames =
            new HashSet<string>(
                typeof(NodeModel)
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(f => f.Name)
            )
        {
            k_ScriptPropertyName
        };

        public override bool IsExcluded<TProperty, TContainer, TValue>(TProperty property, ref TContainer container)
        {
            return k_ExcludedPropertyNames.Contains(property.GetName()) ||
                model is IPropertyVisitorNodeTarget target && target.IsExcluded(property.GetValue(ref container));
        }

        protected override VisitStatus BeginContainer<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
        {
            bool foldout;
            if (SkipChildren<TProperty, TContainer, TValue>())
                foldout = false;
            else
            {
                EditorGUILayout.LabelField(property.GetName(), new GUIStyle(EditorStyles.boldLabel) { fontStyle = FontStyle.Bold });
                foldout = true;
            }

            EditorGUI.indentLevel++;
            return foldout ? VisitStatus.Handled : VisitStatus.Override;
        }

        protected override void EndContainer<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
        {
            EditorGUI.indentLevel--;
        }

        protected override bool SkipChildren<TProperty, TContainer, TValue>()
        {
            return !typeof(TValue).IsValueType;
        }
    }
}