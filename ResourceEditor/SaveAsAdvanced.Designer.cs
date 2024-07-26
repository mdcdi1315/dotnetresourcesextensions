namespace ResourceEditor
{
    partial class SaveAsAdvanced
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
            this.SelectPathButton = new System.Windows.Forms.Button();
            this.L2 = new System.Windows.Forms.Label();
            this.GenStrClass = new System.Windows.Forms.CheckBox();
            this.StrClassVisibility = new System.Windows.Forms.ComboBox();
            this.L3 = new System.Windows.Forms.Label();
            this.DotNetClassNameBox = new System.Windows.Forms.TextBox();
            this.L4 = new System.Windows.Forms.Label();
            this.ManifestStreamNameBox = new System.Windows.Forms.TextBox();
            this.L5 = new System.Windows.Forms.Label();
            this.RClassSaveToButton = new System.Windows.Forms.Button();
            this.L6 = new System.Windows.Forms.Label();
            this.SaveDataButton = new System.Windows.Forms.Button();
            this.CButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // L1
            // 
            this.L1.AutoSize = true;
            this.L1.Location = new System.Drawing.Point(13, 13);
            this.L1.Name = "L1";
            this.L1.Size = new System.Drawing.Size(370, 26);
            this.L1.TabIndex = 0;
            this.L1.Text = "Save As Advanced Options. \r\nAllows you to save resource files with more settings " +
    "before creating the result.";
            // 
            // SelectPathButton
            // 
            this.SelectPathButton.Location = new System.Drawing.Point(16, 51);
            this.SelectPathButton.Name = "SelectPathButton";
            this.SelectPathButton.Size = new System.Drawing.Size(162, 23);
            this.SelectPathButton.TabIndex = 1;
            this.SelectPathButton.Text = "Set the result file save path...";
            this.SelectPathButton.UseVisualStyleBackColor = true;
            this.SelectPathButton.Click += new System.EventHandler(this.CM_SAVEOUTPATH);
            // 
            // L2
            // 
            this.L2.AutoSize = true;
            this.L2.Location = new System.Drawing.Point(204, 56);
            this.L2.Name = "L2";
            this.L2.Size = new System.Drawing.Size(169, 13);
            this.L2.TabIndex = 2;
            this.L2.Text = "You have not specified a path yet.";
            // 
            // GenStrClass
            // 
            this.GenStrClass.AutoSize = true;
            this.GenStrClass.Location = new System.Drawing.Point(12, 96);
            this.GenStrClass.Name = "GenStrClass";
            this.GenStrClass.Size = new System.Drawing.Size(227, 17);
            this.GenStrClass.TabIndex = 3;
            this.GenStrClass.Text = "Generate a strongly-typed resource class...";
            this.GenStrClass.UseVisualStyleBackColor = true;
            this.GenStrClass.CheckedChanged += new System.EventHandler(this.CH_CHANGED);
            // 
            // StrClassVisibility
            // 
            this.StrClassVisibility.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.StrClassVisibility.Enabled = false;
            this.StrClassVisibility.FormattingEnabled = true;
            this.StrClassVisibility.Items.AddRange(new object[] {
            "Public",
            "Internal"});
            this.StrClassVisibility.Location = new System.Drawing.Point(16, 139);
            this.StrClassVisibility.Name = "StrClassVisibility";
            this.StrClassVisibility.Size = new System.Drawing.Size(162, 21);
            this.StrClassVisibility.TabIndex = 4;
            // 
            // L3
            // 
            this.L3.AutoSize = true;
            this.L3.Location = new System.Drawing.Point(15, 116);
            this.L3.Name = "L3";
            this.L3.Size = new System.Drawing.Size(116, 13);
            this.L3.TabIndex = 5;
            this.L3.Text = "Specify class visibility...";
            // 
            // DotNetClassNameBox
            // 
            this.DotNetClassNameBox.Enabled = false;
            this.DotNetClassNameBox.Location = new System.Drawing.Point(12, 222);
            this.DotNetClassNameBox.Name = "DotNetClassNameBox";
            this.DotNetClassNameBox.Size = new System.Drawing.Size(297, 20);
            this.DotNetClassNameBox.TabIndex = 6;
            // 
            // L4
            // 
            this.L4.AutoSize = true;
            this.L4.Location = new System.Drawing.Point(15, 167);
            this.L4.Name = "L4";
            this.L4.Size = new System.Drawing.Size(275, 52);
            this.L4.TabIndex = 7;
            this.L4.Text = "Specify the Resource Class Name. \r\nThe Resource Class name is a .NET Name that co" +
    "nsists \r\nof namespaces seperated in dots and after the final \r\ndot the class nam" +
    "e is specified.";
            // 
            // ManifestStreamNameBox
            // 
            this.ManifestStreamNameBox.Enabled = false;
            this.ManifestStreamNameBox.Location = new System.Drawing.Point(12, 304);
            this.ManifestStreamNameBox.Name = "ManifestStreamNameBox";
            this.ManifestStreamNameBox.Size = new System.Drawing.Size(297, 20);
            this.ManifestStreamNameBox.TabIndex = 8;
            // 
            // L5
            // 
            this.L5.AutoSize = true;
            this.L5.Location = new System.Drawing.Point(12, 249);
            this.L5.Name = "L5";
            this.L5.Size = new System.Drawing.Size(271, 52);
            this.L5.TabIndex = 9;
            this.L5.Text = "Specify the manifest stream name. \r\nThe manifest stream name is a name inside the" +
    " resource\r\nindex of any .NET assembly. It can have any name. \r\nConsult your app " +
    "to see how you will supply this name.";
            // 
            // RClassSaveToButton
            // 
            this.RClassSaveToButton.Enabled = false;
            this.RClassSaveToButton.Location = new System.Drawing.Point(238, 139);
            this.RClassSaveToButton.Name = "RClassSaveToButton";
            this.RClassSaveToButton.Size = new System.Drawing.Size(145, 23);
            this.RClassSaveToButton.TabIndex = 10;
            this.RClassSaveToButton.Text = "Save Generated Class to...";
            this.RClassSaveToButton.UseVisualStyleBackColor = true;
            this.RClassSaveToButton.Click += new System.EventHandler(this.CM_STRCLASS_SAVEPATH);
            // 
            // L6
            // 
            this.L6.AutoSize = true;
            this.L6.Location = new System.Drawing.Point(214, 116);
            this.L6.Name = "L6";
            this.L6.Size = new System.Drawing.Size(169, 13);
            this.L6.TabIndex = 11;
            this.L6.Text = "You have not specified a path yet.";
            // 
            // SaveDataButton
            // 
            this.SaveDataButton.Location = new System.Drawing.Point(407, 176);
            this.SaveDataButton.Name = "SaveDataButton";
            this.SaveDataButton.Size = new System.Drawing.Size(75, 23);
            this.SaveDataButton.TabIndex = 12;
            this.SaveDataButton.Text = "&OK";
            this.SaveDataButton.UseVisualStyleBackColor = true;
            this.SaveDataButton.Click += new System.EventHandler(this.TrySaveData_Click);
            // 
            // CButton
            // 
            this.CButton.Location = new System.Drawing.Point(407, 206);
            this.CButton.Name = "CButton";
            this.CButton.Size = new System.Drawing.Size(75, 23);
            this.CButton.TabIndex = 13;
            this.CButton.Text = "&Cancel";
            this.CButton.UseVisualStyleBackColor = true;
            this.CButton.Click += new System.EventHandler(this.G_EXIT_FAIL);
            // 
            // SaveAsAdvanced
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(526, 347);
            this.Controls.Add(this.CButton);
            this.Controls.Add(this.SaveDataButton);
            this.Controls.Add(this.L6);
            this.Controls.Add(this.RClassSaveToButton);
            this.Controls.Add(this.L5);
            this.Controls.Add(this.ManifestStreamNameBox);
            this.Controls.Add(this.L4);
            this.Controls.Add(this.DotNetClassNameBox);
            this.Controls.Add(this.L3);
            this.Controls.Add(this.StrClassVisibility);
            this.Controls.Add(this.GenStrClass);
            this.Controls.Add(this.L2);
            this.Controls.Add(this.SelectPathButton);
            this.Controls.Add(this.L1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SaveAsAdvanced";
            this.ShowIcon = false;
            this.Text = "Save As... (Advanced Mode)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label L1;
        private System.Windows.Forms.Button SelectPathButton;
        private System.Windows.Forms.Label L2;
        private System.Windows.Forms.CheckBox GenStrClass;
        private System.Windows.Forms.ComboBox StrClassVisibility;
        private System.Windows.Forms.Label L3;
        private System.Windows.Forms.TextBox DotNetClassNameBox;
        private System.Windows.Forms.Label L4;
        private System.Windows.Forms.TextBox ManifestStreamNameBox;
        private System.Windows.Forms.Label L5;
        private System.Windows.Forms.Button RClassSaveToButton;
        private System.Windows.Forms.Label L6;
        private System.Windows.Forms.Button SaveDataButton;
        private System.Windows.Forms.Button CButton;
    }
}