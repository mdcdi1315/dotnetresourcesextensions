namespace ResourceEditor
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.TSM_actions = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenFileButton = new System.Windows.Forms.ToolStripMenuItem();
            this.CreateNewResFile = new System.Windows.Forms.ToolStripMenuItem();
            this.FEButton = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveOpenedButton = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveAsFileButton = new System.Windows.Forms.ToolStripMenuItem();
            this.AdvancedSaveButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitButton = new System.Windows.Forms.ToolStripMenuItem();
            this.TSM_resources = new System.Windows.Forms.ToolStripMenuItem();
            this.AddResButton = new System.Windows.Forms.ToolStripMenuItem();
            this.RemResource = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.ResourceView = new System.Windows.Forms.ListView();
            this.ResName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ResValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ResourceCMS = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.RCMS_1 = new System.Windows.Forms.ToolStripMenuItem();
            this.RCMS_2 = new System.Windows.Forms.ToolStripMenuItem();
            this.EXTRButton = new System.Windows.Forms.ToolStripMenuItem();
            this.RCMS_3 = new System.Windows.Forms.ToolStripMenuItem();
            this.MainMenu.SuspendLayout();
            this.ResourceCMS.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainMenu
            // 
            this.MainMenu.BackColor = System.Drawing.Color.Firebrick;
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSM_actions,
            this.TSM_resources,
            this.StatusTextBox});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(800, 27);
            this.MainMenu.TabIndex = 0;
            // 
            // TSM_actions
            // 
            this.TSM_actions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenFileButton,
            this.CreateNewResFile,
            this.FEButton,
            this.SaveOpenedButton,
            this.SaveAsFileButton,
            this.AdvancedSaveButton,
            this.ExitButton});
            this.TSM_actions.Name = "TSM_actions";
            this.TSM_actions.Size = new System.Drawing.Size(68, 23);
            this.TSM_actions.Text = "Actions...";
            // 
            // OpenFileButton
            // 
            this.OpenFileButton.Name = "OpenFileButton";
            this.OpenFileButton.Size = new System.Drawing.Size(236, 22);
            this.OpenFileButton.Text = "Open Existing File...";
            this.OpenFileButton.Click += new System.EventHandler(this.OPF_CLICK);
            // 
            // CreateNewResFile
            // 
            this.CreateNewResFile.Name = "CreateNewResFile";
            this.CreateNewResFile.Size = new System.Drawing.Size(236, 22);
            this.CreateNewResFile.Text = "Create new resource file...";
            this.CreateNewResFile.Click += new System.EventHandler(this.CREATE_CLICK);
            // 
            // FEButton
            // 
            this.FEButton.Name = "FEButton";
            this.FEButton.Size = new System.Drawing.Size(236, 22);
            this.FEButton.Text = "Finalize the current resources...";
            this.FEButton.Click += new System.EventHandler(this.FE_BUTTON_CLICK);
            // 
            // SaveOpenedButton
            // 
            this.SaveOpenedButton.Name = "SaveOpenedButton";
            this.SaveOpenedButton.Size = new System.Drawing.Size(236, 22);
            this.SaveOpenedButton.Text = "Save current opened file...";
            this.SaveOpenedButton.Click += new System.EventHandler(this.SAVEFILE_CLICK);
            // 
            // SaveAsFileButton
            // 
            this.SaveAsFileButton.Name = "SaveAsFileButton";
            this.SaveAsFileButton.Size = new System.Drawing.Size(236, 22);
            this.SaveAsFileButton.Text = "Save As...";
            this.SaveAsFileButton.Click += new System.EventHandler(this.SAVEFILE_CLICK);
            // 
            // AdvancedSaveButton
            // 
            this.AdvancedSaveButton.Name = "AdvancedSaveButton";
            this.AdvancedSaveButton.Size = new System.Drawing.Size(236, 22);
            this.AdvancedSaveButton.Text = "Save As... (Advanced)";
            this.AdvancedSaveButton.Click += new System.EventHandler(this.ADV_SAVE_CLICK);
            // 
            // ExitButton
            // 
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(236, 22);
            this.ExitButton.Text = "Exit...";
            this.ExitButton.Click += new System.EventHandler(this.EXIT_CLICK);
            // 
            // TSM_resources
            // 
            this.TSM_resources.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddResButton,
            this.RemResource});
            this.TSM_resources.Name = "TSM_resources";
            this.TSM_resources.Size = new System.Drawing.Size(72, 23);
            this.TSM_resources.Text = "Resources";
            // 
            // AddResButton
            // 
            this.AddResButton.Name = "AddResButton";
            this.AddResButton.Size = new System.Drawing.Size(177, 22);
            this.AddResButton.Text = "Add Resource...";
            this.AddResButton.Click += new System.EventHandler(this.ADDRES_CLICK);
            // 
            // RemResource
            // 
            this.RemResource.Name = "RemResource";
            this.RemResource.Size = new System.Drawing.Size(177, 22);
            this.RemResource.Text = "Remove Resource...";
            this.RemResource.Click += new System.EventHandler(this.ResourceActions_CLICK);
            // 
            // StatusTextBox
            // 
            this.StatusTextBox.BackColor = System.Drawing.Color.Firebrick;
            this.StatusTextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.StatusTextBox.Name = "StatusTextBox";
            this.StatusTextBox.ReadOnly = true;
            this.StatusTextBox.Size = new System.Drawing.Size(560, 23);
            this.StatusTextBox.Text = "Ready";
            // 
            // ResourceView
            // 
            this.ResourceView.BackColor = System.Drawing.SystemColors.MenuText;
            this.ResourceView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ResName,
            this.ResValue});
            this.ResourceView.ContextMenuStrip = this.ResourceCMS;
            this.ResourceView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResourceView.ForeColor = System.Drawing.SystemColors.Window;
            this.ResourceView.FullRowSelect = true;
            this.ResourceView.HideSelection = false;
            this.ResourceView.LabelEdit = true;
            this.ResourceView.Location = new System.Drawing.Point(0, 27);
            this.ResourceView.MultiSelect = false;
            this.ResourceView.Name = "ResourceView";
            this.ResourceView.Size = new System.Drawing.Size(800, 423);
            this.ResourceView.TabIndex = 1;
            this.ResourceView.UseCompatibleStateImageBehavior = false;
            this.ResourceView.View = System.Windows.Forms.View.Details;
            this.ResourceView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.LBE_EDIT);
            // 
            // ResName
            // 
            this.ResName.Text = "Resource Name";
            this.ResName.Width = 253;
            // 
            // ResValue
            // 
            this.ResValue.Text = "Resource Value";
            this.ResValue.Width = 480;
            // 
            // ResourceCMS
            // 
            this.ResourceCMS.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RCMS_1,
            this.RCMS_2,
            this.EXTRButton,
            this.RCMS_3});
            this.ResourceCMS.Name = "ResourceCMS";
            this.ResourceCMS.Size = new System.Drawing.Size(190, 92);
            this.ResourceCMS.Text = "Resource Actions";
            this.ResourceCMS.Opening += new System.ComponentModel.CancelEventHandler(this.G_CMS_OPENING);
            this.ResourceCMS.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.CMS_CLICKED);
            // 
            // RCMS_1
            // 
            this.RCMS_1.Name = "RCMS_1";
            this.RCMS_1.Size = new System.Drawing.Size(189, 22);
            this.RCMS_1.Text = "Edit Resource Name...";
            // 
            // RCMS_2
            // 
            this.RCMS_2.Name = "RCMS_2";
            this.RCMS_2.Size = new System.Drawing.Size(189, 22);
            this.RCMS_2.Text = "Edit Resource Value...";
            // 
            // EXTRButton
            // 
            this.EXTRButton.Name = "EXTRButton";
            this.EXTRButton.Size = new System.Drawing.Size(189, 22);
            this.EXTRButton.Text = "Extract Resource...";
            this.EXTRButton.Click += new System.EventHandler(this.ResourceActions_CLICK);
            // 
            // RCMS_3
            // 
            this.RCMS_3.Name = "RCMS_3";
            this.RCMS_3.Size = new System.Drawing.Size(189, 22);
            this.RCMS_3.Text = "Delete...";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ResourceView);
            this.Controls.Add(this.MainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.MainMenu;
            this.MinimumSize = new System.Drawing.Size(816, 489);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Resource Creator & Editor for DotNetResourcesExtensions";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.F_CLOSING);
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.ResourceCMS.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem TSM_actions;
        private System.Windows.Forms.ToolStripMenuItem OpenFileButton;
        private System.Windows.Forms.ToolStripMenuItem CreateNewResFile;
        private System.Windows.Forms.ToolStripMenuItem ExitButton;
        private System.Windows.Forms.ToolStripMenuItem TSM_resources;
        private System.Windows.Forms.ToolStripMenuItem AddResButton;
        private System.Windows.Forms.ToolStripMenuItem RemResource;
        private System.Windows.Forms.ListView ResourceView;
        private System.Windows.Forms.ToolStripMenuItem SaveOpenedButton;
        private System.Windows.Forms.ToolStripMenuItem SaveAsFileButton;
        private System.Windows.Forms.ColumnHeader ResName;
        private System.Windows.Forms.ColumnHeader ResValue;
        private System.Windows.Forms.ToolStripTextBox StatusTextBox;
        private System.Windows.Forms.ContextMenuStrip ResourceCMS;
        private System.Windows.Forms.ToolStripMenuItem RCMS_1;
        private System.Windows.Forms.ToolStripMenuItem RCMS_2;
        private System.Windows.Forms.ToolStripMenuItem RCMS_3;
        private System.Windows.Forms.ToolStripMenuItem FEButton;
        private System.Windows.Forms.ToolStripMenuItem EXTRButton;
        private System.Windows.Forms.ToolStripMenuItem AdvancedSaveButton;
    }
}

