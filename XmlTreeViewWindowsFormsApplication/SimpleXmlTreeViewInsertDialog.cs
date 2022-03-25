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
    public partial class SimpleXmlTreeViewInsertDialog : Form
    {
        public SimpleXmlTreeViewInsertDialog()
        {
            InitializeComponent();
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
