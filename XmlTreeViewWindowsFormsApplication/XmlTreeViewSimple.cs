using System;
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
using static XmlTreeViewWindowsFormsApplication.XmlTreeViewWin32;

namespace XmlTreeViewWindowsFormsApplication
{
    [ToolboxItem(true)]
    //[ToolboxItemFilter("System.Windows.Forms", ToolboxItemFilterType.Custom)]
    [ToolboxBitmap(typeof(TreeView))]
    public partial class XmlTreeViewSimple : XmlTreeViewBase<XmlTreeViewSimple.XmlTreeNode>
    {
        public class XmlTreeNode : XmlTreeNodeBase
        {
            protected string ConstPrefix;
            protected string EditableValue;

            public new XmlTreeViewSimple TreeView => (XmlTreeViewSimple)base.TreeView; // null if not inserted (yet) into a TreeView
            protected EditBox EditBox => TreeView._editBox;

            public bool IsEditable => EditableValue != null;
            public new bool IsEditing => TreeView.IsEditing;

            public XmlTreeNode(XmlNode xmlNode) : base(xmlNode)
            {
            }

            public void UpdateText(XmlTreeViewSimple treeView)
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
                    this.ForeColor = new Color(); // use default
                }
                else if (XmlNode.NodeType == XmlNodeType.Comment)
                {
                    ConstPrefix = "# ";
                    EditableValue = XmlNode.Value.Trim();
                    this.ForeColor = treeView.CommentColor;
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

        public class EditBox : TextBox
        {
            public readonly XmlTreeViewSimple Host;
            public XmlTreeNode XmlTreeNode;

            public bool IsEditing => XmlTreeNode != null && Visible;

            public EditBox(XmlTreeViewSimple host)
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

        protected override void OnContextMenuAction(XmlTreeNode contextMenuNode, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Name)
            {
                case "deleteToolStripMenuItem":
                    RemoveXmlNode(contextMenuNode.XmlNode);
                    break;

                case "insertToolStripMenuItem":
                    using (var form = new SimpleXmlTreeViewInsertDialog())
                    {
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            InsertNewXmlNode(contextMenuNode.XmlNode, form.Result);
                        }
                    }
                    break;
            }
        }

        protected void RemoveXmlNode(XmlNode xmlNode)
        {
            int nrNodes = 1 + EnumerateAllNodes(_displayedNodes[xmlNode].Nodes).Count();
            var dialogResult = MessageBox.Show($"Remove {nrNodes} item(s) without undo?", "Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.OK)
            {
                xmlNode.ParentNode.RemoveChild(xmlNode);
            }
            else
            {
                this.SelectedNode = _displayedNodes[xmlNode];
            }
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
                _displayedNodes[xmlNode].ExpandAll();
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
            this.SelectedNode = _displayedNodes[newNode];
        }

        protected EditBox _editBox;
        protected bool IsEditing => _editBox != null && _editBox.IsEditing;

        public XmlTreeViewSimple()
        {
            InitializeComponent();
            //base.LabelEdit = true;
            _editBox = new EditBox(this);
        }

#region Designer
        [Browsable(true)]
        [Category("Behavior")]
        [Description("Indicates whether the user can edit the XML.")]
        [DefaultValue(true)]
        public new bool LabelEdit { get; set; }

        private Color _commentColor = Color.DarkGreen;

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Color of XML comments.")]
        [DefaultValue(typeof(Color), "DarkGreen")]
        public Color CommentColor
        {
            get { return _commentColor; }
            set { if (_commentColor != value) { _commentColor = value; UpdateTree(); } }
        }
#endregion

        protected void OnXmlDocumentNodeModified(XmlNode xmlNode, XmlNode xmlParentNode)
        {
            if (IsEditing)
            {
                var xmlEdit = _editBox.XmlTreeNode.XmlNode;
                if (GetXmlDocument(xmlEdit) != _xmlDocument ||
                    xmlNode == xmlEdit ||
                    xmlNode.NodeType == XmlNodeType.Text && xmlParentNode == xmlEdit)
                {
                    EndEdit(false); // someone else modfied XML we are editing? => cancel edit
                }
            }
        }

        protected override void OnNodeRemoved(XmlNode xmlNode, XmlNode xmlOldParentNode)
        {
            OnXmlDocumentNodeModified(xmlNode, xmlOldParentNode);

            XmlTreeNode treeNode;
            if (_displayedNodes.TryGetValue(xmlNode, out treeNode))
            {
                treeNode.Remove();
                _displayedNodes.Remove(xmlNode);
            }

            // some element types can influence how their parent is displayed => update parent text
            UpdateParentText(xmlNode, xmlOldParentNode);
        }

        protected override void OnNodeUpdated(XmlNode xmlNode)
        {
            OnXmlDocumentNodeModified(xmlNode, xmlNode.ParentNode);

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
                treeNode.UpdateText(this);
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
                    parentTreeNode.UpdateText(this);
            }
        }

        protected override void Clear()
        {
            this.EndEdit();

            base.Clear();
        }

        protected override void OnScrolling()
        {
            this.EndEdit();

            base.OnScrolling();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.EndEdit();

            base.OnSizeChanged(e);
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
