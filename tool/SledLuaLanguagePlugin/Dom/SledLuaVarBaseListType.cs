/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua.Dom
{
    public abstract class SledLuaVarBaseListType<T> : DomNodeAdapter, IItemView, ITreeListView, IObservableContext, IHierarchicalInsertionContext, IValidationContext
        where T : class, ISledLuaVarBaseType
    {
        public virtual string Name
        {
            get { return GetAttribute<string>(NameAttributeInfo); }
            set { SetAttribute(NameAttributeInfo, value); }
        }

        public virtual IList<T> Variables
        {
            get { return GetChildList<T>(VariablesChildInfo); }
        }

        #region IItemView Interface

        public virtual void GetInfo(object item, ItemInfo info)
        {
            if (ReferenceEquals(item, this))
            {
                info.Label = Name;
                info.Description = Description;
                info.ImageIndex = info.GetImageIndex(Atf.Resources.FolderImage);

                return;
            }

            var itemView = item.As<IItemView>();
            if ((itemView == null) || ReferenceEquals(itemView, this))
                return;

            itemView.GetInfo(item, info);
        }

        #endregion

        #region ITreeListView Interface

        public virtual IEnumerable<object> GetChildren(object parent)
        {
            if (parent == null)
                yield break;

            var node = parent.As<DomNode>();
            if (node == null)
                yield break;

            foreach (var child in node.Children)
            {
                if (!child.Is<T>())
                    continue;

                // Respect variable filtering
                var converted = child.As<T>();
                if (converted.Visible)
                    yield return converted;
            }
        }

        public virtual IEnumerable<object> Roots
        {
            get
            {
                // Respect variable filtering
                return Variables.Where(r => r.Visible);
            }
        }

        public virtual string[] ColumnNames
        {
            get { return TheColumnNames; }
        }

        #endregion

        #region IObservableContext Interface

        public virtual event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        public virtual event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        public virtual event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

        public virtual event EventHandler Reloaded { add { } remove { } } // Cheap trick to avoid compiler warning

        #endregion

        #region IHierarchicalInsertionContext Interface

        public bool CanInsert(object parent, object child)
        {
            // Figure out what's being drag-and-drop'd
            ISledLuaVarBaseType childVar;
            if (!TryGetLuaVar(child.As<IDataObject>(), out childVar))
                return false;

            // Dropping onto Lua variable...
            if (parent.Is<ISledLuaVarBaseType>())
            {
                // Can't drag-and-drop onto same GUI
                var parentVar = parent.As<ISledLuaVarBaseType>();
                if (ReferenceEquals(parentVar.DomNode.GetRoot(), childVar.DomNode.GetRoot()))
                    return false;

                // Can only drop onto watch GUI and then
                // only if item isn't already watched
                return
                    (parentVar.DomNode.GetRoot().Type == SledLuaSchema.SledLuaVarWatchListType.Type) &&
                    !m_luaWatchedVariableService.Get.IsLuaVarWatched(childVar);
            }

            // Dropping onto Lua editor...
            var editor = parent.As<SledLuaTreeListViewEditor>();
            if (editor == null)
                return false;

            if (!editor.View.Is<SledLuaVarWatchListType>())
                return false;

            return !m_luaWatchedVariableService.Get.IsLuaVarWatched(childVar);
        }

        public void Insert(object parent, object child)
        {
            // Figure out what's being drag-and-drop'd
            ISledLuaVarBaseType childVar;
            if (!TryGetLuaVar(child.As<IDataObject>(), out childVar))
                return;

            m_luaWatchedVariableService.Get.AddWatchedLuaVar(childVar);
        }

        #endregion

        #region IValidationContext Interface

        public event EventHandler Beginning;

        public event EventHandler Cancelled { add { } remove { } } // Cheap trick to avoid compiler warning

        public event EventHandler Ending;

        public event EventHandler Ended;

        #endregion

        public virtual IEnumerable<ExpandedState> ExpandedStates
        {
            get { return m_lstSaveStates; }
        }

        public virtual void SaveExpandedStates()
        {
            ResetExpandedStates();

            foreach (var variable in Variables)
            {
                var state = SaveExpandedStateHelper(variable, m_dictSaveStates);
                if (state == null)
                    continue;

                m_lstSaveStates.Add(state);
            }
        }

        public virtual void ResetExpandedStates()
        {
            m_lstSaveStates.Clear();
            m_dictSaveStates.Clear();
        }

        public virtual bool TryGetVariable(string name, out ISledLuaVarBaseType luaVar)
        {
            luaVar = null;
            
            foreach (var variable in Variables.Select(v => v.As<T>()))
            {
                if (TryGetVariableHelper(name, variable, out luaVar))
                    return true;
            }

            return false;
        }

        public void ValidationBeginning()
        {
            try
            {
                Beginning.Raise(this, EventArgs.Empty);
            }
            finally
            {
                m_validating = true;
            }
        }

        public void ValidationEnded()
        {
            if (!m_validating)
                return;

            try
            {
                Ending.Raise(this, EventArgs.Empty);
                Ended.Raise(this, EventArgs.Empty);
            }
            finally
            {
                m_validating = false;
            }
        }

        #region ExpandedState Class

        public class ExpandedState
        {
            public ExpandedState(T variable)
            {
                Variable = variable;

                var rootNode = variable.DomNode.GetRoot();
                Root = ReferenceEquals(variable.DomNode.Parent, rootNode);
                OwnerList = rootNode.As<SledLuaVarBaseListType<T>>();

                Context = SledLuaVarLookUpContextType.Normal;
                {
                    var luaVarWatchList = OwnerList.As<SledLuaVarWatchListType>();
                    if (luaVarWatchList != null)
                    {
                        Context = luaVarWatchList.IsCustomWatchedVariable(Variable)
                            ? SledLuaVarLookUpContextType.WatchCustom
                            : SledLuaVarLookUpContextType.WatchProject;
                    }
                }

                LookUp = SledLuaVarLookUpType.FromLuaVar(variable, Context);

                Children = new List<ExpandedState>();
            }

            public bool Root { get; private set; }

            public T Variable { get; private set; }

            public SledLuaVarLookUpType LookUp { get; private set; }

            public SledLuaVarLookUpContextType Context { get; private set; }

            public SledLuaVarBaseListType<T> OwnerList { get; private set; }

            public ICollection<ExpandedState> Children { get; private set; }

            public IEnumerable<ExpandedState> GetFlattenedHierarchy()
            {
                var flattened = new List<ExpandedState>();

                GetFlattenedHierarchyHelper(this, flattened);

                return flattened;
            }

            private static void GetFlattenedHierarchyHelper(ExpandedState state, ICollection<ExpandedState> lstStates)
            {
                lstStates.Add(state);

                foreach (var child in state.Children)
                    GetFlattenedHierarchyHelper(child, lstStates);
            }
        }

        #endregion

        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += DomNodeAttributeChanged;
            DomNode.ChildInserted += DomNodeChildInserted;
            DomNode.ChildRemoving += DomNodeChildRemoving;

            base.OnNodeSet();
        }

        private void DomNodeAttributeChanged(object sender, AttributeEventArgs e)
        {
            OnDomNodeAttributeChanged(sender, e);
        }

        private void DomNodeChildInserted(object sender, ChildEventArgs e)
        {
            OnDomNodeChildInserted(sender, e);
        }

        private void DomNodeChildRemoving(object sender, ChildEventArgs e)
        {
            OnDomNodeChildRemoving(sender, e);
        }

        protected virtual void OnDomNodeAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (!e.DomNode.Is<T>())
                return;

            ItemChanged.Raise(
                this,
                new ItemChangedEventArgs<object>(e.DomNode.As<T>()));
        }

        protected virtual void OnDomNodeChildInserted(object sender, ChildEventArgs e)
        {
            if (!e.Child.Is<T>())
                return;

            var child = e.Child.As<T>();
            if (child != null)
            {
                bool expanded;
                if (m_dictSaveStates.TryGetValue(child.UniqueName, out expanded))
                    child.Expanded = true;
            }

            // Respect variable filtering
            if ((child != null) && !child.Visible)
                return;

            ItemInserted.Raise(
                this,
                new ItemInsertedEventArgs<object>(
                    e.Index,
                    e.Child.As<T>(),
                    e.Parent.As<T>()));
        }

        protected virtual void OnDomNodeChildRemoving(object sender, ChildEventArgs e)
        {
            if (!e.Child.Is<T>())
                return;

            ItemRemoved.Raise(
                this,
                new ItemRemovedEventArgs<object>(
                    e.Index,
                    e.Child.As<T>(),
                    e.Parent.As<T>()));
        }

        protected static ExpandedState SaveExpandedStateHelper(T variable, IDictionary<string, bool> dictStates)
        {
            if (!variable.Expanded || !variable.Variables.Any())
                return null;

            if (string.IsNullOrEmpty(variable.UniqueName))
                return null;

            if (!dictStates.ContainsKey(variable.UniqueName))
                dictStates.Add(variable.UniqueName, true);

            var state = new ExpandedState(variable);

            foreach (var child in variable.Variables.Select(v => v.As<T>()))
            {
                var newState = SaveExpandedStateHelper(child, dictStates);
                if (newState == null)
                    continue;

                state.Children.Add(newState);
            }

            return state;
        }

        protected static bool TryGetVariableHelper(string name, T variable, out ISledLuaVarBaseType luaVar)
        {
            luaVar = null;

            if (string.Compare(variable.Name, name, StringComparison.Ordinal) == 0)
            {
                luaVar = variable;
                return true;
            }

            // Check if any children
            if (!variable.Variables.Any())
                return false;

            // Enumerate through children
            foreach (var var in variable.Variables.Select(v => v.As<T>()))
            {
                if (TryGetVariableHelper(name, var, out luaVar))
                    return true;
            }

            return false;
        }

        private static bool TryGetLuaVar(IDataObject dataObject, out ISledLuaVarBaseType luaVar)
        {
            luaVar = null;

            if (dataObject == null)
                return false;

            try
            {
                if (dataObject.GetDataPresent(typeof(SledLuaVarGlobalType)))
                    luaVar = (SledLuaVarGlobalType)dataObject.GetData(typeof(SledLuaVarGlobalType));
                else if (dataObject.GetDataPresent(typeof(SledLuaVarLocalType)))
                    luaVar = (SledLuaVarLocalType)dataObject.GetData(typeof(SledLuaVarLocalType));
                else if (dataObject.GetDataPresent(typeof(SledLuaVarUpvalueType)))
                    luaVar = (SledLuaVarUpvalueType)dataObject.GetData(typeof(SledLuaVarUpvalueType));
                else if (dataObject.GetDataPresent(typeof(SledLuaVarEnvType)))
                    luaVar = (SledLuaVarEnvType)dataObject.GetData(typeof(SledLuaVarEnvType));
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "[Variable List] Exception with drag-and-drop: {0}",
                    ex.Message);
            }

            return luaVar != null;
        }

        private bool m_validating;

        protected abstract string Description { get; }
        protected abstract string[] TheColumnNames { get; }
        protected abstract AttributeInfo NameAttributeInfo { get; }
        protected abstract ChildInfo VariablesChildInfo { get; }

        private readonly List<ExpandedState> m_lstSaveStates =
            new List<ExpandedState>();

        private readonly Dictionary<string, bool> m_dictSaveStates =
            new Dictionary<string, bool>();

        private readonly SledServiceReference<ISledLuaWatchedVariableService> m_luaWatchedVariableService =
            new SledServiceReference<ISledLuaWatchedVariableService>();
    }
}