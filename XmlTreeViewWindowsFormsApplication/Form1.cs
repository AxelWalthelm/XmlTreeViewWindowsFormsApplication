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
            try
            {
                XmlDocument.Load(FilePath);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());

                LoadDummyXmlDocument();
            }
            AutoSave.XmlDocument = XmlDocument;

            this.xmlTreeView1.Root = XmlDocument.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.NodeType == XmlNodeType.Element);
            this.xmlTreeView1.ExpandAll();

            this.xmlTreeView2.Root = XmlDocument;
            this.xmlTreeView2.ExpandAll();
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
            this.Text = "SimpleXmlTreeView - autosaving...";
            try
            {
                XmlDocument.Save(FilePath);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
            this.Text = $"SimpleXmlTreeView - autosaved {++AutoSaveCount} times";
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {

            this.xmlTreeView1.BeginUpdate();
            this.xmlTreeView2.BeginUpdate();
            AutoSave.XmlDocument = null;
            if ((Control.ModifierKeys & Keys.Control) != 0)
                LoadDummyXmlDocument();
            else
                this.xmlTreeView1.XmlDocument?.Load(FilePath);
            AutoSave.XmlDocument = XmlDocument;
            this.xmlTreeView1.EndUpdate();
            this.xmlTreeView2.EndUpdate();
            this.xmlTreeView1.ExpandAll();
            this.xmlTreeView2.ExpandAll();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (this.xmlTreeView1.XmlDocument != null)
                this.xmlTreeView1.XmlDocument.Save(FilePath);
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            var root = this.xmlTreeView1.Root;
            this.xmlTreeView1.Root = null;
            this.xmlTreeView1.Root = root ?? XmlDocument;
            this.xmlTreeView1.ExpandAll();

            root = this.xmlTreeView2.Root;
            this.xmlTreeView2.Root = null;
            this.xmlTreeView2.Root = root ?? XmlDocument;
            this.xmlTreeView2.ExpandAll();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            Rectangle b1 = this.xmlTreeView1.Bounds;
            Rectangle b2 = this.xmlTreeView2.Bounds;

            int gap = b1.X;
            int w = (b2.Right - b1.Left - gap) / 2;
            this.xmlTreeView1.Bounds = new Rectangle(b1.X, b1.Y, w, b1.Height);
            this.xmlTreeView2.Bounds = new Rectangle(b2.Right - w, b2.Y, w, b2.Height);
        }
    }
}
