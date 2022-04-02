namespace XmlTreeViewWindowsFormsApplication
{
    partial class XmlTreeViewSimpleInsertDialog
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
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.labelName = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.radioButtonInsertBefore = new System.Windows.Forms.RadioButton();
            this.radioButtonInsertAfter = new System.Windows.Forms.RadioButton();
            this.radioButtonInsertInside = new System.Windows.Forms.RadioButton();
            this.textBoxValue = new System.Windows.Forms.TextBox();
            this.labelValue = new System.Windows.Forms.Label();
            this.textBoxComment = new System.Windows.Forms.TextBox();
            this.labelComment = new System.Windows.Forms.Label();
            this.groupBoxInsertWhere = new System.Windows.Forms.GroupBox();
            this.groupBoxInsertWhere.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(81, 12);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(295, 20);
            this.textBoxName.TabIndex = 1;
            // 
            // labelName
            // 
            this.labelName.Location = new System.Drawing.Point(12, 10);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(63, 23);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Name";
            this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(220, 92);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 10;
            this.buttonOk.Text = "Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(301, 92);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // radioButtonInsertBefore
            // 
            this.radioButtonInsertBefore.Location = new System.Drawing.Point(12, 21);
            this.radioButtonInsertBefore.Name = "radioButtonInsertBefore";
            this.radioButtonInsertBefore.Size = new System.Drawing.Size(74, 24);
            this.radioButtonInsertBefore.TabIndex = 7;
            this.radioButtonInsertBefore.Text = "before";
            this.radioButtonInsertBefore.UseVisualStyleBackColor = true;
            // 
            // radioButtonInsertAfter
            // 
            this.radioButtonInsertAfter.Location = new System.Drawing.Point(12, 45);
            this.radioButtonInsertAfter.Name = "radioButtonInsertAfter";
            this.radioButtonInsertAfter.Size = new System.Drawing.Size(74, 24);
            this.radioButtonInsertAfter.TabIndex = 8;
            this.radioButtonInsertAfter.Text = "after";
            this.radioButtonInsertAfter.UseVisualStyleBackColor = true;
            // 
            // radioButtonInsertInside
            // 
            this.radioButtonInsertInside.Location = new System.Drawing.Point(12, 69);
            this.radioButtonInsertInside.Name = "radioButtonInsertInside";
            this.radioButtonInsertInside.Size = new System.Drawing.Size(74, 24);
            this.radioButtonInsertInside.TabIndex = 9;
            this.radioButtonInsertInside.Text = "inside";
            this.radioButtonInsertInside.UseVisualStyleBackColor = true;
            // 
            // textBoxValue
            // 
            this.textBoxValue.Location = new System.Drawing.Point(81, 38);
            this.textBoxValue.Name = "textBoxValue";
            this.textBoxValue.Size = new System.Drawing.Size(295, 20);
            this.textBoxValue.TabIndex = 3;
            // 
            // labelValue
            // 
            this.labelValue.Location = new System.Drawing.Point(12, 36);
            this.labelValue.Name = "labelValue";
            this.labelValue.Size = new System.Drawing.Size(63, 23);
            this.labelValue.TabIndex = 2;
            this.labelValue.Text = "Value";
            this.labelValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxComment
            // 
            this.textBoxComment.Location = new System.Drawing.Point(81, 64);
            this.textBoxComment.Name = "textBoxComment";
            this.textBoxComment.Size = new System.Drawing.Size(295, 20);
            this.textBoxComment.TabIndex = 5;
            // 
            // labelComment
            // 
            this.labelComment.Location = new System.Drawing.Point(12, 62);
            this.labelComment.Name = "labelComment";
            this.labelComment.Size = new System.Drawing.Size(63, 23);
            this.labelComment.TabIndex = 4;
            this.labelComment.Text = "Comment";
            this.labelComment.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBoxInsertWhere
            // 
            this.groupBoxInsertWhere.Controls.Add(this.radioButtonInsertBefore);
            this.groupBoxInsertWhere.Controls.Add(this.radioButtonInsertInside);
            this.groupBoxInsertWhere.Controls.Add(this.radioButtonInsertAfter);
            this.groupBoxInsertWhere.Location = new System.Drawing.Point(382, 12);
            this.groupBoxInsertWhere.Name = "groupBoxInsertWhere";
            this.groupBoxInsertWhere.Size = new System.Drawing.Size(92, 103);
            this.groupBoxInsertWhere.TabIndex = 6;
            this.groupBoxInsertWhere.TabStop = false;
            this.groupBoxInsertWhere.Text = "Insert";
            // 
            // SimpleXmlTreeViewInsertDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 126);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.labelComment);
            this.Controls.Add(this.labelValue);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.textBoxComment);
            this.Controls.Add(this.textBoxValue);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.groupBoxInsertWhere);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SimpleXmlTreeViewInsertDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Insert New Items";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SimpleXmlTreeViewInsertDialog_FormClosing);
            this.groupBoxInsertWhere.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.RadioButton radioButtonInsertBefore;
        private System.Windows.Forms.RadioButton radioButtonInsertAfter;
        private System.Windows.Forms.RadioButton radioButtonInsertInside;
        private System.Windows.Forms.TextBox textBoxValue;
        private System.Windows.Forms.Label labelValue;
        private System.Windows.Forms.TextBox textBoxComment;
        private System.Windows.Forms.Label labelComment;
        private System.Windows.Forms.GroupBox groupBoxInsertWhere;
    }
}