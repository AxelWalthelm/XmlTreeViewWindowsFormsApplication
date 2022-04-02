using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace XmlTreeViewWindowsFormsApplication
{
    public partial class Form1 : Form
    {
        public string FilePath = @"C:\tmp\test.xml";
        public XmlDocument XmlDocument;
        public XmlAutoSave AutoSave;

        public Form1()
        {
            InitializeComponent();
            AutoSave = new XmlAutoSave(this.components) { Interval = 3000 };
            AutoSave.OnAutoSave += OnAutoSave;

            XmlDocument = new XmlDocument();
#if false
            LoadDummyXmlDocument();
#else
            try
            {
                XmlDocument.Load(FilePath);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());

                LoadDummyXmlDocument();
            }
#endif
            AutoSave.XmlDocument = XmlDocument;

            this.xmlTreeView1.Root = XmlDocument.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.NodeType == XmlNodeType.Element);
            this.xmlTreeView1.ExpandAll();
            this.xmlTreeView1.WriteConsole();

            this.xmlTreeView2.Root = XmlDocument;
            this.xmlTreeView2.ExpandAll();
            this.xmlTreeView2.WriteConsole();

            this.xmlTreeViewGeneric1.Root = XmlDocument;
            this.xmlTreeViewGeneric1.ExpandAll();
            this.xmlTreeViewGeneric1.WriteConsole();
        }

        private void LoadDummyXmlDocument()
        {
            XmlDocument.LoadXml(@"<?xml version=""1.0""?>
<!-- outer comment -->
<rooty>
    <!-- inner comment -->
    <param1>value1</param1>
    <param2></param2>
    <param3/>
    <!-- random words -->
    <words random=""true"">
        <word>falcon<!--commie--></word>
        <word>sky</word>
        <word>bottom</word>
        <word>cup</word>
        <word>book</word>
        <word>rock</word>
        <word>sand</word>
        <word>river</word>
    </words>
    <a><b><c><d><e><f><g><h><i><j><k><l><m><n>ooo</n></m></l></k></j></i></h></g></f></e></d></c></b></a>
    <more>no more</more>
</rooty>
");
        }

        public int AutoSaveCount = 0;

        private void OnAutoSave(object sender, EventArgs e)
        {
            Debug.Assert(sender == XmlDocument);

            if (!XmlDocument.ChildNodes.Cast<XmlNode>().Any(n => n.NodeType == XmlNodeType.Element))
            {
                this.Text = $"XmlTreeViews - autosave ignored empty document";
                return;
            }

            this.Text = "XmlTreeViews - autosaving...";
            try
            {
                XmlDocument.Save(FilePath);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
            this.Text = $"XmlTreeViews - autosaved {++AutoSaveCount} times";

            if (this.xmlTreeView2.CommentColor != SystemColors.Highlight)
            {
                this.xmlTreeView2.CommentColor = SystemColors.Highlight;
                this.xmlTreeView2.ForeColor = SystemColors.GrayText;
                this.xmlTreeView2.BackColor = SystemColors.WindowText;
                this.xmlTreeView2.Font = this.Font;
            }
            else
            {
                this.xmlTreeView2.CommentColor = Color.Orange;
                this.xmlTreeView2.ForeColor = this.xmlTreeView1.ForeColor;
                this.xmlTreeView2.BackColor = this.xmlTreeView1.BackColor;
                this.xmlTreeView2.Font = this.Font; // new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            this.xmlTreeView1.BeginUpdate();
            this.xmlTreeView2.BeginUpdate();
            this.xmlTreeViewGeneric1.BeginUpdate();
            AutoSave.XmlDocument = null;
            if ((Control.ModifierKeys & Keys.Control) != 0)
                LoadDummyXmlDocument();
            else
                this.xmlTreeView1.XmlDocument?.Load(FilePath);
            AutoSave.XmlDocument = XmlDocument;
            this.xmlTreeView1.EndUpdate();
            this.xmlTreeView2.EndUpdate();
            this.xmlTreeViewGeneric1.EndUpdate();
            this.xmlTreeView1.ExpandAll();
            this.xmlTreeView2.ExpandAll();
            this.xmlTreeViewGeneric1.ExpandAll();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            XmlDocument.Save(FilePath);
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) != 0)
            {
                if (timer1.Enabled)
                {
                    timer1.Stop();
                    this.Text = $"XmlTreeViews - destruction timer stopped";
                }
                else
                {
                    timer1.Start();
                    this.Text = $"XmlTreeViews - destruction timer started";
                }

                return;
            }

            var root = this.xmlTreeView1.Root;
            this.xmlTreeView1.Root = null;
            this.xmlTreeView1.Root = root ?? XmlDocument.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.NodeType == XmlNodeType.Element);
            this.xmlTreeView1.ExpandAll();

            root = this.xmlTreeView2.Root;
            this.xmlTreeView2.Root = null;
            this.xmlTreeView2.Root = root ?? XmlDocument;
            this.xmlTreeView2.ExpandAll();

            root = this.xmlTreeViewGeneric1.Root;
            this.xmlTreeViewGeneric1.Root = null;
            this.xmlTreeViewGeneric1.Root = root ?? XmlDocument;
            this.xmlTreeViewGeneric1.ExpandAll();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            Rectangle b1 = this.xmlTreeView1.Bounds;
            Rectangle b2 = this.xmlTreeView2.Bounds;
            Rectangle b3 = this.xmlTreeViewGeneric1.Bounds;

            int gap = b1.X;
            int w = (b3.Right - b1.Left - 2 * gap) / 3;
            this.xmlTreeView1.Bounds = new Rectangle(b1.X, b1.Y, w, b1.Height);
            this.xmlTreeView2.Bounds = new Rectangle(b1.X + w + gap, b2.Y, w, b2.Height);
            this.xmlTreeViewGeneric1.Bounds = new Rectangle(b3.Right - w, b3.Y, w, b3.Height);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var node = this.xmlTreeViewGeneric1.AllNodes.LastOrDefault();
            if (node != null && node.XmlNode != this.xmlTreeViewGeneric1.Root)
            {
                this.Text = $"XmlTreeViews - destruction timer removing {node.Text}";
                node.XmlNode.ParentNode.RemoveChild(node.XmlNode);
            }
            else
            {
                this.Text = $"XmlTreeViews - destruction timer finished";
                this.timer1.Stop();
            }
        }

        [XmlType(TypeName = "faulty")]
        public class TestData
        {
            public string Message;
            public string Null;
            public int ErrorCode;
            public Point Loki { get; set; }
        }

        private void buttonWriteData_Click(object sender, EventArgs e)
        {
            var fault = new TestData
            {
                Message = "Exception occurred",
                ErrorCode = 1010,
                Loki = new Point(47, 11),
            };

            var name = nameof(fault);
            var node = ((XmlTreeViewSimple.XmlTreeNode)this.xmlTreeView1.SelectedNode).XmlNode;
            if (node.NodeType != XmlNodeType.Element || node.LocalName != name)
            {
                var n = XmlDocument.CreateElement(name);
                node.AppendChild(n);
                node = n;
            }
            SerializeObject(node, fault);
            Console.WriteLine("Write Data: " + node.OuterXml);
        }

        private void buttonReadData_Click(object sender, EventArgs e)
        {
            var node = ((XmlTreeViewSimple.XmlTreeNode)this.xmlTreeView1.SelectedNode).XmlNode;
            var o = DeseralizeObject(node, typeof(TestData));
            Console.WriteLine("Read Data: " + o.ToString());
        }

        private static object DeseralizeObject(XmlNode xmlNode, Type type)
        {
            var overrides = new XmlAttributeOverrides();
            overrides.Add(type, new XmlAttributes { XmlType = new XmlTypeAttribute(xmlNode.LocalName) });
            XmlSerializer serializer = new XmlSerializer(type, overrides);
            using (XmlReader reader = new XmlNodeReader(xmlNode))
            {
                return serializer.Deserialize(reader);
            }
        }

        private static void SerializeObject(XmlNode xmlNode, object o)
        {
            var document = new XmlDocument();
            var navigator = document.CreateNavigator();

            using (XmlWriter writer = navigator.AppendChild())
            {
                var serializer = new XmlSerializer(o.GetType());
                var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                serializer.Serialize(writer, o, namespaces);
            }

            UpdateChildrenFromOtherDocument(xmlNode, document.FirstChild);
        }

        // Make oldNode have children equal to newNode, but modify only what has changed to avoid flickering,
        // assuming order of nodes stay the same and that nodes can only be deleted and inserted
        // (moving a node is both: delete and insert).
        // Document of newNode differs from the document of oldNode, so we need to copy them by XmlDocument.ImportNode().
        // When a node is different and needs be copied, all off its children are copied too.
        // TODO/TBD: could we change nodes instead of replacing them?
        private static void UpdateChildrenFromOtherDocument(XmlNode oldNode, XmlNode newNode)
        {
#if true // quick and simple
            while (oldNode.ChildNodes.Count > 0)
                oldNode.RemoveChild(oldNode.ChildNodes[0]);
            for (int i = 0; i < newNode.ChildNodes.Count; i++)
                oldNode.AppendChild(oldNode.OwnerDocument.ImportNode(newNode.ChildNodes[i], true));
#else
            var oldDocument = oldNode.OwnerDocument;

            for (int index = 0; index < newNode.ChildNodes.Count; index++)
            {
                if (index >= oldNode.ChildNodes.Count)
                {
                    var importNode = oldDocument.ImportNode(newNode.ChildNodes[index], true);
                    oldNode.AppendChild(importNode);
                    continue;
                }

                // TODO/TBD: could we change nodes to make them equal?

                if (IsEqual(oldNode.ChildNodes[index], newNode.ChildNodes[index]))
                {
                    UpdateChildrenFromOtherDocument(newNode.ChildNodes[index], oldNode.ChildNodes[index]);
                    continue;
                }

                int oldIndex = IndexOf(oldNode.ChildNodes, newNode.ChildNodes[index], index + 1);
                int newIndex = IndexOf(newNode.ChildNodes, oldNode.ChildNodes[index], index + 1);
                if (newIndex >= 0 && (oldIndex < 0 || oldIndex > newIndex))
                {
                    // insert from newIndex
                    var importNode = oldDocument.ImportNode(newNode.ChildNodes[newIndex], true);
                    oldNode.InsertBefore(importNode, oldNode.ChildNodes[index]);
                }
                else
                {
                    // delete
                    oldNode.RemoveChild(oldNode.ChildNodes[index]);
                }
            }

            while (oldNode.ChildNodes.Count > newNode.ChildNodes.Count)
            {
                oldNode.RemoveChild(oldNode.ChildNodes[oldNode.ChildNodes.Count - 1]);
            }
#endif
        }

        private static int IndexOf(XmlNodeList list, XmlNode item, int startIndex = 0)
        {
            for (int i = startIndex; i < list.Count; i++)
            {
                if (IsEqual(list[i], item))
                    return i;
            }

            return -1;
        }

        private static bool IsEqual(XmlNode xmlNode1, XmlNode xmlNode2)
        {
            if (xmlNode1 == null || xmlNode2 == null)
                return xmlNode1 == null && xmlNode2 == null;

            if (xmlNode1.NodeType != xmlNode2.NodeType)
                return false;

            if (xmlNode1.NodeType == XmlNodeType.Element)
            {
                if (xmlNode1.Attributes.Count != xmlNode2.Attributes.Count)
                    return false;

                for (int i = 0; i < xmlNode1.Attributes.Count; i++)
                {
                    if (!IsEqual(xmlNode1.Attributes[i], xmlNode2.Attributes[i]))
                        return false;
                }
            }

            return
                string.Compare(xmlNode1.Name, xmlNode2.Name, StringComparison.InvariantCulture) == 0 &&
                string.Compare(xmlNode1.LocalName, xmlNode2.LocalName, StringComparison.InvariantCulture) == 0 &&
                string.Compare(xmlNode1.Prefix, xmlNode2.Prefix, StringComparison.InvariantCulture) == 0 &&
                string.Compare(xmlNode1.Value, xmlNode2.Value, StringComparison.InvariantCulture) == 0;
        }
    }
}
