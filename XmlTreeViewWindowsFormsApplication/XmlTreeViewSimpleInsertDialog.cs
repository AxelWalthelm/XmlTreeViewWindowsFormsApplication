using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XmlTreeViewWindowsFormsApplication
{
    public partial class XmlTreeViewSimpleInsertDialog : Form
    {
        protected static string InsertDefault = "radioButtonInsertAfter";
        private IEnumerable<RadioButton> InsertButtons => this.groupBoxInsertWhere.Controls.OfType<RadioButton>();

        public XmlTreeViewSimpleInsertDialog()
        {
            InitializeComponent();

            foreach (RadioButton button in InsertButtons)
                button.Checked = button.Name == InsertDefault;
        }

        private void SimpleXmlTreeViewInsertDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            InsertDefault = InsertButtons.Single(b => b.Checked).Name;
        }

        public class Results
        {
            public string Name;
            public string Value;
            public string Comment;

            public enum InsertLocations { After, Before, Inside }
            public InsertLocations InsertLocation;
        }

        public Results Result => new Results
        {
            Name = this.textBoxName.Text,
            Value = this.textBoxValue.Text,
            Comment = this.textBoxComment.Text,
            InsertLocation = this.InsertLocation
        };

        Results.InsertLocations InsertLocation =>
            this.radioButtonInsertBefore.Checked ? Results.InsertLocations.Before :
            this.radioButtonInsertInside.Checked ? Results.InsertLocations.Inside :
            Results.InsertLocations.After;
    }
}
