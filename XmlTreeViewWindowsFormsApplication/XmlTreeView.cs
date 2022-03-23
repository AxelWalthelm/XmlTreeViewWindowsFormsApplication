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

namespace XmlTreeViewWindowsFormsApplication
{
    public partial class XmlTreeView : TreeView
    {
        protected class XmlTreeNode : TreeNode
        {
            public XmlNode XmlNode;
            public string ConstPrefix;
            public string EditableValue;

            public XmlTreeNode(XmlNode xmlNode, string constPrefix, string editableValue = null) : base(constPrefix + editableValue)
            {
                XmlNode = xmlNode;
                ConstPrefix = constPrefix;
                EditableValue = editableValue;
            }
        }

        protected class EditBox : TextBox
        {
            public readonly XmlTreeView Host;
            public XmlTreeNode XmlTreeNode;

            public EditBox(XmlTreeView host)
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

        protected EditBox _editTextBox;

        protected XmlNode _root;

        protected readonly Dictionary<XmlNode, XmlTreeNode> _editableXmlNodes = new Dictionary<XmlNode, XmlTreeNode>();

        public XmlTreeView()
        {
            InitializeComponent();
            //base.LabelEdit = true;
            _editTextBox = new EditBox(this);
            //this.components.Add(_editTextBox);
        }

#if true // activate double buffering to reduce flickering
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
#endif

        [Browsable(false)]
        [Category("Appearance")]
        [Description("The XML tree or sub-tree to be displayed.")]
        [DefaultValue(null)]
        public XmlNode Root
        {
            get { return _root; }
            set { this.Init(value); }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Indicates whether the user can edit the XML.")]
        [DefaultValue(true)]
        public new bool LabelEdit { get; set; }

        private void Init(XmlNode root)
        {
            this.BeginUpdate();

            if (_root != null)
            {
                UnregisterEvents();
                this.Nodes.Clear();
                this._editableXmlNodes.Clear();
            }

            _root = root;
            if (_root != null)
            {
                PopulateTree(_root, this.Nodes);
                RegisterEvents();
            }

            this.EndUpdate();
        }

        private void RegisterEvents()
        {
            XmlDocument document = XmlDocument;
            document.NodeChanged += OnDocumentNodeModified;
            document.NodeInserted += OnDocumentNodeModified;
            document.NodeRemoved += OnDocumentNodeModified;
        }

        private void UnregisterEvents()
        {
            if (_root == null)
                return;

            XmlDocument document = XmlDocument;
            document.NodeChanged -= OnDocumentNodeModified;
            document.NodeInserted -= OnDocumentNodeModified;
            document.NodeRemoved -= OnDocumentNodeModified;
        }

        public XmlDocument XmlDocument
        {
            get
            {
                var node = _root;
                while (node != null && node.NodeType != XmlNodeType.Document)
                    node = node.ParentNode;

                return (XmlDocument)node;
            }
        }

        private void OnDocumentNodeModified(object sender, XmlNodeChangedEventArgs e)
        {
            if (_root == null)
                return;

            bool handled = false;
            if (e.Action == XmlNodeChangedAction.Insert || e.Action == XmlNodeChangedAction.Change)
            {
                XmlTreeNode node;
                if (_editableXmlNodes.TryGetValue(e.Node, out node) ||
                    _editableXmlNodes.TryGetValue(e.NewParent, out node))
                {
                    string newValue = e.NewValue;
                    node.EditableValue = newValue;
                    node.Text += newValue;
                    handled = true;
                }
            }
            else if (e.Action == XmlNodeChangedAction.Remove)
            {

            }

            if (!handled)
            {
                throw new InvalidOperationException("unhandled XML change");
            }
        }

        private void PopulateTree(XmlNode xmlNode, TreeNodeCollection nodes)
        {
            foreach (XmlNode xmlChild in xmlNode.ChildNodes)
            {
                if (xmlChild.NodeType == XmlNodeType.Element)
                {
                    if (xmlChild.ChildNodes.Count == 0 || xmlChild.ChildNodes.Count == 1 && xmlChild.ChildNodes[0].NodeType == XmlNodeType.Text)
                    {
                        string value = xmlChild.ChildNodes.Count == 0 ? "" : xmlChild.ChildNodes[0].Value ?? "";
                        var treeChild = new XmlTreeNode(xmlChild, xmlChild.Name + " = ", value);
                        nodes.Add(treeChild);
                        _editableXmlNodes[xmlChild] = treeChild;
                    }
                    else
                    {
                        var treeChild = new XmlTreeNode(xmlChild, xmlChild.Name);
                        nodes.Add(treeChild);
                        PopulateTree(xmlChild, treeChild.Nodes);
                    }
                }
                else if (xmlChild.NodeType == XmlNodeType.Comment)
                {
                    var treeChild = new XmlTreeNode(xmlChild, "# ", xmlChild.Value.Trim());
                    treeChild.ForeColor = Color.DarkGreen;
                    nodes.Add(treeChild);
                    _editableXmlNodes[xmlChild] = treeChild;
                }
            }
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


        private bool _doubleClickWithoutLayoutChanges;

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            // double-click may change the tree, e.g. collapse or expand;
            // this happens before this method is called, but after OnClick() is called for the first click of double-click,
            // so we detect layout changes between click and double-click to prevent working on out-dated event data.
            if (!_doubleClickWithoutLayoutChanges)
                return;

            var me = (MouseEventArgs)e;

            BeginEdit((XmlTreeNode)this.GetNodeAt(me.Location));
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

            if (node == null || node.EditableValue == null)
                return;

            EndEdit();

            EditBox edit = _editTextBox;
            _editTextBox = null; // following code of BeginEdit may cause indirect calls to EndEdit(), which shall be ignored

            edit.XmlTreeNode = node;
            edit.Text = node.EditableValue;
            if (!string.IsNullOrEmpty(node.ConstPrefix))
            {
                node.Text = node.ConstPrefix;
//                var prefixSize = MeasureText(node.ConstPrefix);
//                bounds.X += (int)prefixSize.Width;
            }
            Rectangle bounds = node.Bounds;
            if (!string.IsNullOrEmpty(node.ConstPrefix))
            {
                bounds.X += bounds.Width;
            }
            int borderX = GetSystemMetrics(SM_CXBORDER);
            int borderY = GetSystemMetrics(SM_CYBORDER);
            bounds.Inflate(borderX, borderY);
            bounds.X -= 4;
            bounds.Y -= 1;
            bounds.Width = this.ClientRectangle.Width - bounds.X;
            edit.Bounds = bounds;

            edit.Visible = true;
            edit.Focus();

            _editTextBox = edit;
        }

        private bool IsEditing => _editTextBox != null && _editTextBox.Visible && _editTextBox.XmlTreeNode != null;

        protected void EndEdit(bool acceptChanges = true)
        {
            if (!IsEditing)
                return;

            XmlTreeNode node = _editTextBox.XmlTreeNode;
            _editTextBox.XmlTreeNode = null;

            _editTextBox.Visible = false;
            this.Focus(); // ensure focus goes to tree view (most of the time this happens automatically)
            if (!acceptChanges || String.Equals(node.EditableValue, _editTextBox.Text, StringComparison.OrdinalIgnoreCase))
            {
                node.Text = node.ConstPrefix + node.EditableValue;
            }
            else
            {
#if false // fake it
                node.EditableValue = _editTextBox.Text;
                node.Text += node.EditableValue;
#else // modify XML
                XmlNode xmlNode = node.XmlNode;
                if (xmlNode.NodeType == XmlNodeType.Comment)
                {
                    Match match = Regex.Match(xmlNode.Value ?? "", @"^(\s*).*?(\s*)$");
                    Debug.Assert(match.Success);
                    Debug.Assert(match.Groups.Count == 3);
                    var value = match.Groups[1].Value + _editTextBox.Text + match.Groups[2].Value; // preverve spaces
                    xmlNode.Value = value;
                }
                else if (xmlNode.NodeType == XmlNodeType.Element)
                {
                    if (xmlNode.ChildNodes.Count == 1 && xmlNode.ChildNodes[0].NodeType == XmlNodeType.Text)
                    {
                        xmlNode.ChildNodes[0].Value = _editTextBox.Text;
                    }
                    else
                    {
                        Debug.Assert(xmlNode.ChildNodes.Count == 0);
                        xmlNode.InnerText = _editTextBox.Text;
                    }
                }
#endif
            }
        }

        protected void NextEdit(bool forward)
        {
            if (!IsEditing)
                return;

            var node = _editTextBox.XmlTreeNode;
            EndEdit();

            node = (XmlTreeNode)(forward ? node.NextNode : node.PrevNode ?? node.Parent);
            if (node != null)
            {
                this.SelectedNode = node;
                node.EnsureVisible();
                BeginEdit(node);
            }
        }

        // WM_VSCROLL message constants
        private const int WM_VSCROLL = 0x0115;
        private const int SB_THUMBTRACK = 5;
        private const int SB_ENDSCROLL = 8;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_VSCROLL)
            {
                this.EndEdit();
            }
        }
    }
}
