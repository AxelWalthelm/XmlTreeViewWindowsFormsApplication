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
    public abstract partial class XmlTreeViewBase<TREE_NODE> : TreeView where TREE_NODE : XmlTreeViewBase<TREE_NODE>.XmlTreeNodeBase
    {
        public abstract class XmlTreeNodeBase : TreeNode
        {
            public readonly XmlNode XmlNode;

            public XmlTreeNodeBase(XmlNode xmlNode)
            {
                XmlNode = xmlNode;
            }
        }

        protected XmlNode _rootXmlNode;
        protected XmlDocument _xmlDocument;
        public XmlDocument XmlDocument => _xmlDocument;

        protected readonly Dictionary<XmlNode, TREE_NODE> _displayedNodes = new Dictionary<XmlNode, TREE_NODE>();

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
#endregion

        public XmlTreeViewBase()
        {
            InitializeComponent();
        }

#region Activate double buffering to reduce flickering
        protected override void OnHandleCreated(EventArgs e)
        {
            SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }
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
                UpdateTree();
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

        protected void UpdateTree()
        {
            if (_xmlDocument != null && _rootXmlNode != null)
            {
                UpdateTree(_rootXmlNode);
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

        private void OnDocumentNodeModified(object sender, XmlNodeChangedEventArgs e)
        {
            if (_rootXmlNode == null || _xmlDocument == null || _xmlDocument != sender)
                return;

            if (GetXmlDocument(_rootXmlNode) != _xmlDocument)
            {
                Clear(); // XML document did became invalid (e.g. root removed) => clear all
                return;
            }

            if (e.Action == XmlNodeChangedAction.Remove)
            {
                RemoveNode(e.Node, e.OldParent);
            }
            else
            {
                Debug.Assert(e.Action == XmlNodeChangedAction.Insert || e.Action == XmlNodeChangedAction.Change);
                UpdateNode(e.Node);
            }
        }

        protected abstract void UpdateNode(XmlNode xmlNode);
        protected abstract void RemoveNode(XmlNode node, XmlNode oldParent);

        // Assumes that the XML tree structure is basically displayed as is, but some leaves or sub-trees may be omitted.
        // More advanced views may need a different implementation of UpdateLinks().
        protected void UpdateLinks(TREE_NODE treeNode)
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
                TREE_NODE parentTreeNode;
                if (_displayedNodes.TryGetValue(xmlNode.ParentNode, out parentTreeNode))
                {
                    nodesOfParent = parentTreeNode.Nodes;
                }
            }

            UpdateChildren(xmlNode.ParentNode, nodesOfParent);

            // try to connect children
            UpdateChildren(xmlNode, treeNode.Nodes);
        }

        private void UpdateChildren(XmlNode xmlNode, TreeNodeCollection treeChildren)
        {
            if (xmlNode == null || treeChildren == null)
                return;

            var newChildren = new List<TreeNode>();
            foreach (XmlNode xmlChild in xmlNode.ChildNodes.Cast<XmlNode>())
            {
                TREE_NODE treeChild;
                if (_displayedNodes.TryGetValue(xmlChild, out treeChild))
                {
                    newChildren.Add(treeChild);
                }
            }

            UpdateChildren(newChildren, treeChildren);
        }

        protected static void UpdateChildren(List<TreeNode> newChildren, TreeNodeCollection treeChildren)
        {
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

        protected IEnumerable<TREE_NODE> EnumerateAllNodes(TreeNodeCollection nodes)
        {
            foreach (TREE_NODE node in nodes)
            {
                yield return node;

                EnumerateAllNodes(node.Nodes);
            }
        }

        protected IEnumerable<TREE_NODE> AllNodes => EnumerateAllNodes(this.Nodes);

        protected TREE_NODE GetVisibleSelectedNode()
        {
            var node = this.SelectedNode;
            if (node == null)
                return null;

            Rectangle b = node.Bounds;
            b.Intersect(this.ClientRectangle);
            return b.IsEmpty ? null : (TREE_NODE) node;
        }

        protected static XmlDocument GetXmlDocument(XmlNode node)
        {
            while (node != null && node.NodeType != XmlNodeType.Document)
                node = node.ParentNode;

            return (XmlDocument)node;
        }

#region Detect scrolling and the node a context menu is opened on
        private const int WM_VSCROLL = 0x0115;
        private const int WM_CONTEXTMENU = 0x007B;
        private TreeNode _contextMenuNode;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_VSCROLL)
            {
                OnScrolling();
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
                    this._contextMenuNode = this.GetNodeAt(p);
                }
            }

            base.WndProc(ref m);
        }

        protected virtual void OnScrolling() { }
#endregion

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

            OnContextMenuAction((TREE_NODE) _contextMenuNode, e);

            _contextMenuNode = null;
        }

        protected virtual void OnContextMenuAction(TREE_NODE treeNode, ToolStripItemClickedEventArgs e) { }

        public void WriteConsole()
        {
            Console.WriteLine($"BEGIN {this.Name}");

            WriteConsole(this.Nodes, "");

            Console.WriteLine($"END {this.Name}");
        }

        private void WriteConsole(TreeNodeCollection nodes, string indent)
        {
            indent += "  ";
            foreach (TreeNode node in nodes)
            {
                Console.WriteLine($"{indent}{node.Text}");

                WriteConsole(node.Nodes, indent);
            }
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);

            InvalidateBorder();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            InvalidateBorder();
        }

        protected void InvalidateBorder()
        {
            RedrawWindow(this.Handle, IntPtr.Zero, IntPtr.Zero, RDW_FRAME | RDW_INVALIDATE);
        }
    }

    static class XmlTreeViewWin32
    {
        public const int SM_CXEDGE = 45;
        public const int SM_CYEDGE = 46;
        public const int SM_CXBORDER = 5;
        public const int SM_CYBORDER = 6;
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        public const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        public const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;
        public const int TVS_EX_DOUBLEBUFFER = 0x0004;
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        public const uint RDW_INVALIDATE = 0x0001;
        public const uint RDW_INTERNALPAINT = 0x0002;
        public const uint RDW_ERASE = 0x0004;
        public const uint RDW_VALIDATE = 0x0008;
        public const uint RDW_NOINTERNALPAINT = 0x0010;
        public const uint RDW_NOERASE = 0x0020;
        public const uint RDW_NOCHILDREN = 0x0040;
        public const uint RDW_ALLCHILDREN = 0x0080;
        public const uint RDW_UPDATENOW = 0x0100;
        public const uint RDW_ERASENOW = 0x0200;
        public const uint RDW_FRAME = 0x0400;
        public const uint RDW_NOFRAME = 0x0800;
        [DllImport("user32.dll")]
        public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);
    }

}
