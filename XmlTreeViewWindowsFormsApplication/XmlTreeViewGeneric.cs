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
    public partial class XmlTreeViewGeneric : XmlTreeViewBase<XmlTreeViewGeneric.XmlTreeNode>
    {
        public class XmlTreeNode : XmlTreeNodeBase
        {
            public XmlTreeNode(XmlNode xmlNode) : base(xmlNode)
            {
            }

            public void UpdateText(XmlTreeViewGeneric xmlTreeViewGeneric)
            {
                string type = Regex.Replace(XmlNode.NodeType.ToString(), @"^Xml", "", RegexOptions.IgnoreCase);
                string name = XmlNode.LocalName.StartsWith("#") ? "" : " " + XmlNode.LocalName;
                string value = XmlNode.Value == null ? "" : ": \"" + XmlNode.Value + "\"";
                this.Text = $"{type}{name}{value}";
                if (XmlNode.Attributes != null && XmlNode.Attributes.Count > 0)
                    this.Text += " [" + string.Join(" ", XmlNode.Attributes.Cast<XmlAttribute>().Select(a => $"{a.Name}={a.Value}")) + "]";
            }
        }

        public XmlTreeViewGeneric()
        {
            InitializeComponent();
        }

        protected override void RemoveNode(XmlNode xmlNode, XmlNode xmlOldParentNode)
        {
            XmlTreeNode treeNode;
            if (_displayedNodes.TryGetValue(xmlNode, out treeNode))
            {
                treeNode.Remove();
                _displayedNodes.Remove(xmlNode);
            }
        }

        protected override void UpdateNode(XmlNode xmlNode)
        {
            XmlTreeNode treeNode;
            if (!_displayedNodes.TryGetValue(xmlNode, out treeNode) && xmlNode != _rootXmlNode)
            {
                treeNode = new XmlTreeNode(xmlNode);
                _displayedNodes[xmlNode] = treeNode;
            }

            if (treeNode != null)
            {
                UpdateLinks(treeNode);
                treeNode.UpdateText(this);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);


            switch (e.KeyCode)
            {
                case Keys.Enter:
                    BeginEdit(this.SelectedNode);
                    e.Handled = true;
                    break;

                case Keys.Delete:
                    RemoveXmlNode(((XmlTreeNode)this.SelectedNode).XmlNode);
                    e.Handled = true;
                    break;
            }
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            var me = (MouseEventArgs)e;

            if (me.Button == MouseButtons.Left &&
                this.SelectedNode != null &&
                this.SelectedNode.Nodes.Count == 0) // double-click on non-leaf node expands/collapses
            {
                BeginEdit(this.SelectedNode);
            }
        }

        private void BeginEdit(TreeNode node)
        {
            if (node == null)
                return;

            node.Text = ((XmlTreeNode)node).XmlNode.Value ?? "";
            node.BeginEdit();
        }

        protected override void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
        {
            base.OnBeforeLabelEdit(e);

            //var xmlNode = ((XmlTreeNode)e.Node).XmlNode;
            //if (xmlNode.NodeType == XmlNodeType.Element) // can't change elment name
            //{
            //    e.CancelEdit = true;
            //    ((XmlTreeNode)e.Node).UpdateText(this);
            //}
        }

        protected override void OnAfterLabelEdit(NodeLabelEditEventArgs e)
        {
            base.OnAfterLabelEdit(e);

            if (e.CancelEdit || e.Label == null)
            {
                e.CancelEdit = true;
                ((XmlTreeNode)e.Node).UpdateText(this);
                return;
            }

            try
            {
                e.CancelEdit = true; // real update of node is done via XML
                var xmlNode = ((XmlTreeNode)e.Node).XmlNode;
                xmlNode.Value = e.Label;
            }
            catch (Exception exception)
            {
                ((XmlTreeNode)e.Node).UpdateText(this);
                MessageBox.Show(exception.ToString(), "Edit", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnContextMenuAction(XmlTreeNode contextMenuNode, ToolStripItemClickedEventArgs e)
        {
            throw new NotImplementedException();
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
    }
}
