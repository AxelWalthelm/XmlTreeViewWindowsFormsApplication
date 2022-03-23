using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

        public Form1()
        {
            InitializeComponent();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"<?xml version=""1.0""?>
<!-- outer comment -->
<rooty>
    <!-- inner comment -->
    <param1>value1</param1>
    <param2/>
    <!-- random words -->
    <words random=""true"">
        <word>falcon</word>
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
            this.xmlTreeView1.Root = (XmlNode)doc;
            this.xmlTreeView1.ExpandAll();
            this.xmlTreeView1.LabelEdit = true;
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            xmlTreeView1.XmlDocument.Load(FilePath);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            xmlTreeView1.XmlDocument.Save(FilePath);
        }
    }
}
