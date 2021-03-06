namespace XmlTreeViewWindowsFormsApplication
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.contextMenuStripXmlTreeView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.buttonWriteData = new System.Windows.Forms.Button();
            this.buttonReadData = new System.Windows.Forms.Button();
            this.labelCount = new System.Windows.Forms.Label();
            this.xmlTreeViewGeneric1 = new XmlTreeViewWindowsFormsApplication.XmlTreeViewGeneric();
            this.xmlTreeView2 = new XmlTreeViewWindowsFormsApplication.XmlTreeViewSimple();
            this.xmlTreeView1 = new XmlTreeViewWindowsFormsApplication.XmlTreeViewSimple();
            this.contextMenuStripXmlTreeView.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonLoad
            // 
            this.buttonLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonLoad.Location = new System.Drawing.Point(13, 432);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(75, 23);
            this.buttonLoad.TabIndex = 3;
            this.buttonLoad.Text = "Load";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(685, 432);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 7;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonReset.Location = new System.Drawing.Point(350, 432);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(75, 23);
            this.buttonReset.TabIndex = 6;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // contextMenuStripXmlTreeView
            // 
            this.contextMenuStripXmlTreeView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextMenuStripXmlTreeView.Name = "contextMenuStripXmlTreeView";
            this.contextMenuStripXmlTreeView.Size = new System.Drawing.Size(132, 48);
            // 
            // insertToolStripMenuItem
            // 
            this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
            this.insertToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            this.insertToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.insertToolStripMenuItem.Text = "Insert";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // buttonWriteData
            // 
            this.buttonWriteData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonWriteData.Location = new System.Drawing.Point(94, 432);
            this.buttonWriteData.Name = "buttonWriteData";
            this.buttonWriteData.Size = new System.Drawing.Size(75, 23);
            this.buttonWriteData.TabIndex = 4;
            this.buttonWriteData.Text = "Insert Data";
            this.buttonWriteData.UseVisualStyleBackColor = true;
            this.buttonWriteData.Click += new System.EventHandler(this.buttonWriteData_Click);
            // 
            // buttonReadData
            // 
            this.buttonReadData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonReadData.Location = new System.Drawing.Point(174, 432);
            this.buttonReadData.Name = "buttonReadData";
            this.buttonReadData.Size = new System.Drawing.Size(75, 23);
            this.buttonReadData.TabIndex = 5;
            this.buttonReadData.Text = "Read Data";
            this.buttonReadData.UseVisualStyleBackColor = true;
            this.buttonReadData.Click += new System.EventHandler(this.buttonReadData_Click);
            // 
            // labelCount
            // 
            this.labelCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelCount.Location = new System.Drawing.Point(255, 432);
            this.labelCount.Name = "labelCount";
            this.labelCount.Size = new System.Drawing.Size(89, 23);
            this.labelCount.TabIndex = 8;
            this.labelCount.Text = "0 changes";
            this.labelCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // xmlTreeViewGeneric1
            // 
            this.xmlTreeViewGeneric1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xmlTreeViewGeneric1.LabelEdit = true;
            this.xmlTreeViewGeneric1.Location = new System.Drawing.Point(506, 13);
            this.xmlTreeViewGeneric1.Name = "xmlTreeViewGeneric1";
            this.xmlTreeViewGeneric1.Size = new System.Drawing.Size(253, 413);
            this.xmlTreeViewGeneric1.TabIndex = 2;
            // 
            // xmlTreeView2
            // 
            this.xmlTreeView2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.xmlTreeView2.BackColor = System.Drawing.SystemColors.WindowText;
            this.xmlTreeView2.CommentColor = System.Drawing.SystemColors.Highlight;
            this.xmlTreeView2.ContextMenuStrip = this.contextMenuStripXmlTreeView;
            this.xmlTreeView2.ForeColor = System.Drawing.SystemColors.GrayText;
            this.xmlTreeView2.LabelEdit = false;
            this.xmlTreeView2.Location = new System.Drawing.Point(255, 13);
            this.xmlTreeView2.Name = "xmlTreeView2";
            this.xmlTreeView2.Size = new System.Drawing.Size(245, 413);
            this.xmlTreeView2.TabIndex = 1;
            // 
            // xmlTreeView1
            // 
            this.xmlTreeView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.xmlTreeView1.ContextMenuStrip = this.contextMenuStripXmlTreeView;
            this.xmlTreeView1.LabelEdit = false;
            this.xmlTreeView1.Location = new System.Drawing.Point(13, 13);
            this.xmlTreeView1.Name = "xmlTreeView1";
            this.xmlTreeView1.Size = new System.Drawing.Size(236, 413);
            this.xmlTreeView1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(772, 467);
            this.Controls.Add(this.labelCount);
            this.Controls.Add(this.xmlTreeViewGeneric1);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.buttonReadData);
            this.Controls.Add(this.buttonWriteData);
            this.Controls.Add(this.buttonLoad);
            this.Controls.Add(this.xmlTreeView2);
            this.Controls.Add(this.xmlTreeView1);
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "XmlTreeViews";
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.contextMenuStripXmlTreeView.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private XmlTreeViewSimple xmlTreeView1;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonReset;
        private XmlTreeViewSimple xmlTreeView2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripXmlTreeView;
        private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private XmlTreeViewGeneric xmlTreeViewGeneric1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button buttonWriteData;
        private System.Windows.Forms.Button buttonReadData;
        private System.Windows.Forms.Label labelCount;
    }
}

