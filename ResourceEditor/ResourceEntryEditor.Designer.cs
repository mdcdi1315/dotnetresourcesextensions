namespace ResourceEditor
{
    partial class ResourceEntryEditor
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
            this.L1 = new System.Windows.Forms.Label();
            this.ResName = new System.Windows.Forms.TextBox();
            this.ResTypes = new System.Windows.Forms.ComboBox();
            this.L2 = new System.Windows.Forms.Label();
            this.L3 = new System.Windows.Forms.Label();
            this.ResValue = new System.Windows.Forms.TextBox();
            this.SaveButton = new System.Windows.Forms.Button();
            this.CButton = new System.Windows.Forms.Button();
            this.OpenFileButton = new System.Windows.Forms.Button();
            this.L4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // L1
            // 
            this.L1.AutoSize = true;
            this.L1.Location = new System.Drawing.Point(13, 13);
            this.L1.Name = "L1";
            this.L1.Size = new System.Drawing.Size(173, 13);
            this.L1.TabIndex = 0;
            this.L1.Text = "Specify the desired resource name:";
            // 
            // ResName
            // 
            this.ResName.Location = new System.Drawing.Point(16, 40);
            this.ResName.Name = "ResName";
            this.ResName.Size = new System.Drawing.Size(261, 20);
            this.ResName.TabIndex = 1;
            // 
            // ResTypes
            // 
            this.ResTypes.FormattingEnabled = true;
            this.ResTypes.Location = new System.Drawing.Point(16, 108);
            this.ResTypes.Name = "ResTypes";
            this.ResTypes.Size = new System.Drawing.Size(261, 21);
            this.ResTypes.TabIndex = 2;
            this.ResTypes.SelectedIndexChanged += new System.EventHandler(this.SIC_RESTYPE);
            // 
            // L2
            // 
            this.L2.AutoSize = true;
            this.L2.Location = new System.Drawing.Point(12, 77);
            this.L2.Name = "L2";
            this.L2.Size = new System.Drawing.Size(171, 13);
            this.L2.TabIndex = 3;
            this.L2.Text = "Specify the type of resource value:";
            // 
            // L3
            // 
            this.L3.AutoSize = true;
            this.L3.Location = new System.Drawing.Point(12, 145);
            this.L3.Name = "L3";
            this.L3.Size = new System.Drawing.Size(136, 13);
            this.L3.TabIndex = 4;
            this.L3.Text = "Specify the resource value:";
            // 
            // ResValue
            // 
            this.ResValue.Location = new System.Drawing.Point(16, 172);
            this.ResValue.Name = "ResValue";
            this.ResValue.Size = new System.Drawing.Size(261, 20);
            this.ResValue.TabIndex = 5;
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(275, 211);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 6;
            this.SaveButton.Text = "OK";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SIC_OK_CLICK);
            // 
            // CButton
            // 
            this.CButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CButton.Location = new System.Drawing.Point(275, 241);
            this.CButton.Name = "CButton";
            this.CButton.Size = new System.Drawing.Size(75, 23);
            this.CButton.TabIndex = 7;
            this.CButton.Text = "Cancel";
            this.CButton.UseVisualStyleBackColor = true;
            this.CButton.Click += new System.EventHandler(this.CEXIT_CLICK);
            // 
            // OpenFileButton
            // 
            this.OpenFileButton.Location = new System.Drawing.Point(12, 185);
            this.OpenFileButton.Name = "OpenFileButton";
            this.OpenFileButton.Size = new System.Drawing.Size(75, 23);
            this.OpenFileButton.TabIndex = 8;
            this.OpenFileButton.Text = "Open File...";
            this.OpenFileButton.UseVisualStyleBackColor = true;
            this.OpenFileButton.Visible = false;
            this.OpenFileButton.Click += new System.EventHandler(this.OPEN_FindFile);
            // 
            // L4
            // 
            this.L4.AutoSize = true;
            this.L4.Location = new System.Drawing.Point(35, 241);
            this.L4.Name = "L4";
            this.L4.Size = new System.Drawing.Size(0, 13);
            this.L4.TabIndex = 9;
            this.L4.Visible = false;
            // 
            // ResourceEntryEditor
            // 
            this.AcceptButton = this.SaveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CButton;
            this.ClientSize = new System.Drawing.Size(371, 283);
            this.Controls.Add(this.L4);
            this.Controls.Add(this.OpenFileButton);
            this.Controls.Add(this.CButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.ResValue);
            this.Controls.Add(this.L3);
            this.Controls.Add(this.L2);
            this.Controls.Add(this.ResTypes);
            this.Controls.Add(this.ResName);
            this.Controls.Add(this.L1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ResourceEntryEditor";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Resource Entry Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.F_CLOSING);
            this.Load += new System.EventHandler(this.F_LOAD);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label L1;
        private System.Windows.Forms.TextBox ResName;
        private System.Windows.Forms.ComboBox ResTypes;
        private System.Windows.Forms.Label L2;
        private System.Windows.Forms.Label L3;
        private System.Windows.Forms.TextBox ResValue;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button CButton;
        private System.Windows.Forms.Button OpenFileButton;
        private System.Windows.Forms.Label L4;
    }
}