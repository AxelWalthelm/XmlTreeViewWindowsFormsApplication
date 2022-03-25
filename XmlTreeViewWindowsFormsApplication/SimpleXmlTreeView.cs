﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace XmlTreeViewWindowsFormsApplication
{
    [ToolboxItem(true)]
    //[ToolboxItemFilter("System.Windows.Forms", ToolboxItemFilterType.Custom)]
    [ToolboxBitmap(typeof(TreeView))]
    public partial class SimpleXmlTreeView : TreeView
    {
        protected class XmlTreeNode : TreeNode
        {
            public readonly XmlNode XmlNode;
            private string ConstPrefix;
            private string EditableValue;

            public new SimpleXmlTreeView TreeView => (SimpleXmlTreeView)base.TreeView;
            public EditBox EditBox => TreeView._editBox;

            public bool IsEditable => EditableValue != null;
            public new bool IsEditing => TreeView.IsEditing;

            public XmlTreeNode(XmlNode xmlNode)
            {
                XmlNode = xmlNode;
            }

            public void UpdateText()
            {
                if (XmlNode.NodeType == XmlNodeType.Element)
                {
                    bool isLeaf = !XmlNode.ChildNodes.Cast<XmlNode>().Any(n => n.NodeType == XmlNodeType.Element);
                    var textChildren = XmlNode.ChildNodes.Cast<XmlNode>().Where(n => n.NodeType == XmlNodeType.Text).ToList();
                    if (textChildren.Count <= 1 && isLeaf)
                    {
                        ConstPrefix = XmlNode.Name + " = ";
                        EditableValue = textChildren.FirstOrDefault()?.Value ?? "";
                    }
                    else
                    {
                        ConstPrefix = XmlNode.Name;
                        EditableValue = null;
                    }
                }
                else if (XmlNode.NodeType == XmlNodeType.Comment)
                {
                    ConstPrefix = "# ";
                    EditableValue = XmlNode.Value.Trim();
                    this.ForeColor = Color.DarkGreen;
                }

                SetText(true);
            }

            private void SetText(bool showValue)
            {
                this.Text = showValue ? ConstPrefix + EditableValue : ConstPrefix;
            }

            public new void BeginEdit()
            {
                Debug.Assert(EditBox != null);
                Debug.Assert(EditBox.XmlTreeNode == null);
                Debug.Assert(IsEditable);
                Debug.Assert(!IsEditing);

                SetText(false);
                Rectangle bounds = Bounds;
                bounds.X += bounds.Width;
                int borderX = GetSystemMetrics(SM_CXBORDER);
                int borderY = GetSystemMetrics(SM_CYBORDER);
                bounds.Inflate(borderX, borderY);
                bounds.X -= 4; // TODO: check these magic values
                bounds.Y -= 1;
                bounds.Width = TreeView.ClientRectangle.Width - bounds.X;
                EditBox.Bounds = bounds;
                EditBox.Text = EditableValue;
                EditBox.Visible = true;
                EditBox.Focus();

                Debug.Assert(!IsEditing);
                EditBox.XmlTreeNode = this;
                Debug.Assert(IsEditing);
            }

            public new void EndEdit(bool cancel)
            {
                Debug.Assert(EditBox != null);
                Debug.Assert(EditBox.XmlTreeNode != null);
                EditBox.Visible = false;
                EditBox.XmlTreeNode = null;
                TreeView.Focus(); // ensure focus goes to tree view (most of the time this happens automatically)

                if (cancel || string.Equals(EditableValue, EditBox.Text, StringComparison.OrdinalIgnoreCase))
                {
                    SetText(true);
                    return;
                }

#if false // fake it
                EditableValue = EditBox.Text;
                this.Text += EditableValue;
#else // modify XML
                if (XmlNode.NodeType == XmlNodeType.Comment)
                {
                    Match match = Regex.Match(XmlNode.Value ?? "", @"^(\s*).*?(\s*)$");
                    Debug.Assert(match.Success);
                    Debug.Assert(match.Groups.Count == 3);
                    var value = match.Groups[1].Value + EditBox.Text + match.Groups[2].Value; // preverve spaces
                    XmlNode.Value = value;
                }
                else if (XmlNode.NodeType == XmlNodeType.Element)
                {
                    if (XmlNode.ChildNodes.Count == 1 && XmlNode.ChildNodes[0].NodeType == XmlNodeType.Text)
                    {
                        XmlNode.ChildNodes[0].Value = EditBox.Text;
                    }
                    else
                    {
                        Debug.Assert(XmlNode.ChildNodes.Count == 0);
                        XmlNode.InnerText = EditBox.Text;
                    }
                }
#endif
            }
        }

        protected class EditBox : TextBox
        {
            public readonly SimpleXmlTreeView Host;
            public XmlTreeNode XmlTreeNode;

            public bool IsEditing => XmlTreeNode != null && Visible;

            public EditBox(SimpleXmlTreeView host)
            {
                Host = host;
                this.Visible = false;
                Host.Controls.Add(this);
            }

            protected override void OnLostFocus(EventArgs e)
            {
                base.OnLostFocus(e);

                Host.EndEdit();
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);

                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        Host.EndEdit(false);
                        e.Handled = true;
                        break;

                    case Keys.Enter:
                        Host.EndEdit();
                        e.Handled = true;
                        break;

                    case Keys.Up:
                    case Keys.Down:
                        Host.NextEdit(e.KeyCode == Keys.Down);
                        e.Handled = true;
                        break;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.KeyCode)
            {
                case Keys.Enter:
                    BeginEdit((XmlTreeNode)this.SelectedNode);
                    e.Handled = IsEditing;
                    break;
            }
        }

        protected override void OnContextMenuStripChanged(EventArgs e)
        {
            base.OnContextMenuStripChanged(e);

            if (this.ContextMenuStrip == null)
                return;

            this.ContextMenuStrip.Opening -= OnContextMenuStripOpening;
            this.ContextMenuStrip.Opening += OnContextMenuStripOpening;

            this.ContextMenuStrip.Closed -= OnContextMenuStripClosed;
            this.ContextMenuStrip.Closed += OnContextMenuStripClosed;

            this.ContextMenuStrip.ItemClicked -= OnContextMenuStripItemClicked;
            this.ContextMenuStrip.ItemClicked += OnContextMenuStripItemClicked;
        }

        private void OnContextMenuStripOpening(object sender, CancelEventArgs e)
        {
            var cms = sender as ContextMenuStrip;
            if (cms == null || cms.SourceControl != this)
                return;

            if (_contextMenuNode == null)
                e.Cancel = true;
        }

        private void OnContextMenuStripClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            var cms = sender as ContextMenuStrip;
            if (cms == null || cms.SourceControl != this)
                return;

            _contextMenuNode = null;
        }

        // Method is also called if context menu item is selected by keyboard (cursor keys and return key).
        // Method is also called if context menu item is activated by associated keyboard shortcut,
        // but member SourceControl may be null in some situations.
        private void OnContextMenuStripItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var cms = sender as ContextMenuStrip;
            if (cms == null || cms.SourceControl != this && cms.SourceControl != null)
                return;

            if (cms.SourceControl == null && !this.Focused)
            {
                return; // Multiple SimpleXmlTreeView may share the same menu - only the one with keyboard focus shall react
            }

            if (_contextMenuNode == null)
                _contextMenuNode = GetVisibleSelectedNode();

            if (_contextMenuNode == null)
                return;

            switch (e.ClickedItem.Name)
            {
                case "deleteToolStripMenuItem":
                    RemoveXmlNode(_contextMenuNode.XmlNode);
                    break;

                case "insertToolStripMenuItem":
                    XmlTreeNode contextMenuNode = _contextMenuNode;
                    using (var form = new SimpleXmlTreeViewInsertDialog())
                    {
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            InsertNewXmlNode(contextMenuNode.XmlNode, form.Result);
                        }
                    }
                    break;
            }

            _contextMenuNode = null;
        }

        protected void RemoveXmlNode(XmlNode xmlNode)
        {
            xmlNode.ParentNode.RemoveChild(xmlNode);
        }

        private void InsertNewXmlNode(XmlNode xmlNode, SimpleXmlTreeViewInsertDialog.Results result)
        {
            XmlNode newNode = null;
            if (result.Name == "" && result.Value == "" && result.Comment != "")
            {
                newNode = XmlDocument.CreateComment(result.Comment);
            }
            else
            {
                string name = XmlConvert.EncodeName(result.Name == "" ? "_" : result.Name);
                newNode = XmlDocument.CreateElement(name);
                if (result.Value != "")
                {
                    newNode.AppendChild(XmlDocument.CreateTextNode(result.Value));
                }
                if (result.Comment != "")
                {
                    newNode.AppendChild(XmlDocument.CreateComment(result.Comment));
                }
            }

            if (result.InsertLocation == SimpleXmlTreeViewInsertDialog.Results.InsertLocations.Inside &&
                xmlNode.NodeType != XmlNodeType.Comment) // can not insert in comments => insert after
            {
                xmlNode.PrependChild(newNode);
            }
            else if (result.InsertLocation == SimpleXmlTreeViewInsertDialog.Results.InsertLocations.Before)
            {
                xmlNode.ParentNode.InsertBefore(newNode, xmlNode);
            }
            else
            {
                xmlNode.ParentNode.InsertAfter(newNode, xmlNode);
            }

            _displayedNodes[newNode].ExpandAll();
        }

        protected XmlTreeNode GetVisibleSelectedNode()
        {
            var node = (XmlTreeNode)this.SelectedNode;
            if (node == null)
                return null;

            Rectangle b = node.Bounds;
            b.Intersect(this.ClientRectangle);
            return b.IsEmpty ? null : node;
        }

        protected EditBox _editBox;
        protected bool IsEditing => _editBox != null && _editBox.IsEditing;

        protected XmlNode _rootXmlNode;
        protected XmlDocument _xmlDocument;
        public XmlDocument XmlDocument => _xmlDocument;

        protected readonly Dictionary<XmlNode, XmlTreeNode> _displayedNodes = new Dictionary<XmlNode, XmlTreeNode>();

        public SimpleXmlTreeView()
        {
            InitializeComponent();
            //base.LabelEdit = true;
            _editBox = new EditBox(this);
        }

#region Activate double buffering to reduce flickering
        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        private const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        protected override void OnHandleCreated(EventArgs e)
        {
            SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }
#endregion

#region Detect scrolling and the node a context menu is opened on
        private const int WM_VSCROLL = 0x0115;
        private const int WM_CONTEXTMENU = 0x007B;
        protected XmlTreeNode _contextMenuNode;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_VSCROLL)
            {
                this.EndEdit();
            }
            else if (m.Msg == WM_CONTEXTMENU)
            {
                // lParam == -1 is a special value if context menu is opened by Shift-F10 or Menu key.
                if ((long)m.LParam == -1)
                {
                    this._contextMenuNode = GetVisibleSelectedNode();
                }
                else
                {
                    var p = this.PointToClient(new Point(
                        unchecked((short)(long)m.LParam),
                        unchecked((short)((long)m.LParam >> 16))));
                    this._contextMenuNode = (XmlTreeNode)this.GetNodeAt(p);
                }
            }

            base.WndProc(ref m);
        }
        #endregion

        #region Designer
        [Browsable(false)]
        [Category("Data")]
        [Description("The XML tree or sub-tree to be displayed.")]
        [DefaultValue(null)]
        public XmlNode Root
        {
            get { return _rootXmlNode; }
            set { this.Init(value); }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Indicates whether the user can edit the XML.")]
        [DefaultValue(true)]
        public new bool LabelEdit { get; set; }
#endregion

        private void Init(XmlNode root)
        {
            if (_rootXmlNode == root)
                return;

            this.BeginUpdate();

            Clear();

            var xmlDocument = GetXmlDocument(root);
            if (xmlDocument != null)
            {
                _rootXmlNode = root;
                _xmlDocument = xmlDocument;
                UpdateTree(_rootXmlNode);
                RegisterEvents();
            }

            this.EndUpdate();
        }

        private void Clear()
        {
            if (_rootXmlNode != null)
            {
                UnregisterEvents();
                this.Nodes.Clear();
                this._displayedNodes.Clear();
                _rootXmlNode = null;
                _xmlDocument = null;
            }
        }

        private void RegisterEvents()
        {
            _xmlDocument.NodeChanged += OnDocumentNodeModified;
            _xmlDocument.NodeInserted += OnDocumentNodeModified;
            _xmlDocument.NodeRemoved += OnDocumentNodeModified;
        }

        private void UnregisterEvents()
        {
            if (_rootXmlNode == null)
                return;

            _xmlDocument.NodeChanged -= OnDocumentNodeModified;
            _xmlDocument.NodeInserted -= OnDocumentNodeModified;
            _xmlDocument.NodeRemoved -= OnDocumentNodeModified;
        }

        protected static XmlDocument GetXmlDocument(XmlNode node)
        {
            while (node != null && node.NodeType != XmlNodeType.Document)
                node = node.ParentNode;

            return (XmlDocument)node;
        }

        private void OnDocumentNodeModified(object sender, XmlNodeChangedEventArgs e)
        {
            if (_rootXmlNode == null)
                return;

            if (GetXmlDocument(_rootXmlNode) != _xmlDocument)
            {
                Clear(); // XML document did became invalid (e.g. root removed) => clear all
                return;
            }

            if (_editBox.IsEditing && e.Node == _editBox.XmlTreeNode.XmlNode)
            {
                EndEdit(false); // someone else modfied XML? => cancel edit
            }

            if (e.Action == XmlNodeChangedAction.Insert || e.Action == XmlNodeChangedAction.Change)
            {
                UpdateNode(e.Node);
            }
            else if (e.Action == XmlNodeChangedAction.Remove)
            {
                RemoveNode(e.Node, e.OldParent);
            }
        }

        private void RemoveNode(XmlNode xmlNode, XmlNode xmlOldParentNode)
        {
            XmlTreeNode treeNode;
            if (_displayedNodes.TryGetValue(xmlNode, out treeNode))
            {
                treeNode.Remove();
                _displayedNodes.Remove(xmlNode);
            }

            // some element types can influence how their parent is displayed => update parent text
            UpdateParentText(xmlNode, xmlOldParentNode);
        }

        public void WriteConsole()
        {
            Console.WriteLine($"BEGIN {this.Name}");

            WriteConsole(this.Nodes, "");

            Console.WriteLine($"END {this.Name}");
        }

        private void WriteConsole(TreeNodeCollection nodes, string indent)
        {
            indent += "  ";
            foreach (XmlTreeNode node in nodes)
            {
                Console.WriteLine($"{indent}{node.Text}");

                WriteConsole(node.Nodes, indent);
            }
        }

        private void UpdateTree(XmlNode xmlNode)
        {
            foreach (XmlNode xmlChildNode in xmlNode.ChildNodes)
            {
                UpdateTree(xmlChildNode);
            }

            UpdateNode(xmlNode);
        }

        private void UpdateNode(XmlNode xmlNode)
        {
            XmlTreeNode treeNode;
            if (!_displayedNodes.TryGetValue(xmlNode, out treeNode) && xmlNode != _rootXmlNode)
            {
                if (xmlNode.NodeType == XmlNodeType.Element || xmlNode.NodeType == XmlNodeType.Comment)
                {
                    treeNode = new XmlTreeNode(xmlNode);
                    _displayedNodes[xmlNode] = treeNode;
                }
            }

            if (treeNode != null)
            {
                UpdateLinks(treeNode);
                treeNode.UpdateText();
            }

            // some element types can influence how their parent is displayed => update parent text
            UpdateParentText(xmlNode);
        }

        private void UpdateParentText(XmlNode xmlNode, XmlNode xmlParentNode = null)
        {
            if (xmlNode.NodeType == XmlNodeType.Text || xmlNode.NodeType == XmlNodeType.Element)
            {
                if (xmlParentNode == null)
                    xmlParentNode = xmlNode.ParentNode;

                XmlTreeNode parentTreeNode;
                if (xmlParentNode != null && _displayedNodes.TryGetValue(xmlParentNode, out parentTreeNode))
                    parentTreeNode.UpdateText();
            }
        }

        private XmlTreeNode UpdateLinks(XmlTreeNode treeNode)
        {
            XmlNode xmlNode = treeNode.XmlNode;

            // try to connect parent
            TreeNodeCollection nodesOfParent = null;
            if (xmlNode.ParentNode == _rootXmlNode)
            {
                nodesOfParent = this.Nodes;
            }
            else if (xmlNode.ParentNode != null)
            {
                XmlTreeNode parentTreeNode;
                if (_displayedNodes.TryGetValue(xmlNode.ParentNode, out parentTreeNode))
                {
                    nodesOfParent = parentTreeNode.Nodes;
                }
            }

            UpdateChildren(xmlNode.ParentNode, nodesOfParent);

            // try to connect children
            UpdateChildren(xmlNode, treeNode.Nodes);

            return treeNode;
        }

        private void UpdateChildren(XmlNode xmlNode, TreeNodeCollection treeChildren)
        {
            if (xmlNode == null || treeChildren == null)
                return;

            var newChildren = new List<TreeNode>();
            foreach (XmlNode xmlChild in xmlNode.ChildNodes.Cast<XmlNode>())
            {
                XmlTreeNode treeChild;
                if (_displayedNodes.TryGetValue(xmlChild, out treeChild))
                {
                    newChildren.Add(treeChild);
                }
            }

#if false
            // simple, but causes flickering
            treeChildren.Clear();
            treeChildren.AddRange(newChildren.ToArray());
#else
            // modify only what has changed to avoid flickering
            for (int index = 0; index < newChildren.Count; index++)
            {
                if (index >= treeChildren.Count)
                {
                    treeChildren.AddRange(newChildren.Skip(index).ToArray());
                    break;
                }

                var newChild = newChildren[index];
                var treeChild = treeChildren[index];
                if (newChild == treeChild)
                {
                    continue;
                }

                // removed nodes are already removed from treeChildren automatically;
                // child nodes are unique (no child appears twice in the list of children);
                // newChildren and treeChildren are the same for i < index;
                // => treeChild must be in newChildren at higher position than index
                Debug.Assert(newChildren.IndexOf(treeChild) > index);

                // a move consists of a remove and an add;
                // removed nodes are already removed from treeChildren automatically;
                // => we only have to consider add, newChild can not be in treeChildren
                Debug.Assert(treeChildren.IndexOf(newChild) < 0);
                treeChildren.Insert(index, newChild);
            }
#endif
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            EndEdit();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            _doubleClickWithoutLayoutChanges = false;

            base.OnLayout(levent);

            EndEdit();
        }

        protected override void OnClick(EventArgs e)
        {
            _doubleClickWithoutLayoutChanges = true;

            base.OnClick(e);
        }


        protected bool _doubleClickWithoutLayoutChanges;

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            // double-click may change the tree, e.g. collapse or expand;
            // this happens before this method is called, but after OnClick() is called for the first click of double-click,
            // so we detect layout changes between click and double-click to prevent working on out-dated event data.
            if (!_doubleClickWithoutLayoutChanges)
                return;

            var me = (MouseEventArgs)e;

            if (me.Button == MouseButtons.Left)
            {
                BeginEdit((XmlTreeNode)this.GetNodeAt(me.Location));
            }
        }

#if false
        protected SizeF MeasureText(string text)
        {
#if true
            using (Graphics g = Graphics.FromHwnd(this.Handle))
            {
                return g.MeasureString(text, Font);
            }
#else
            TextFormatFlags treeFlags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoClipping | TextFormatFlags.NoPadding;
            return TextRenderer.MeasureText(node.ConstPrefix, this.Font, bounds.Size, treeFlags);
#endif
        }
#endif



        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int SM_CXEDGE = 45;
        private const int SM_CYEDGE = 46;

        private const int SM_CXBORDER = 5;
        private const int SM_CYBORDER = 6;

        protected void BeginEdit(XmlTreeNode node)
        {
            if (base.LabelEdit)
            {
                node.BeginEdit();
                return;
            }

            if (node == null || !node.IsEditable)
                return;

            EndEdit();

            node.BeginEdit(); // warning: this may cause indirect calls to EndEdit(), which shall be ignored
        }

        protected void EndEdit(bool acceptChanges = true)
        {
            if (!IsEditing)
                return;

            XmlTreeNode node = _editBox.XmlTreeNode;
            node.EndEdit(!acceptChanges);
        }

        protected void NextEdit(bool forward)
        {
            if (!IsEditing)
                return;

            var node = _editBox.XmlTreeNode;
            EndEdit();

            node = (XmlTreeNode)(forward ? node.NextVisibleNode : node.PrevVisibleNode);
            if (node != null)
            {
                this.SelectedNode = node;
                node.EnsureVisible();
                BeginEdit(node);
            }
        }
    }
}
