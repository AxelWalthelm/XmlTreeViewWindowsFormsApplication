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
            if (this.xmlTreeView1.XmlDocument != null)
                this.xmlTreeView1.XmlDocument.Save(FilePath);
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
    }
}
