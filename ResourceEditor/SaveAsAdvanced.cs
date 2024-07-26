
using System;
using System.Windows.Forms;
using ResourceEditor.Properties;

namespace ResourceEditor
{
    public partial class SaveAsAdvanced : Form
    {
        private System.Boolean succ;
        private System.String ressavepath;
        private StrClassGenerationOptions opts;

        public SaveAsAdvanced()
        {
            succ = false;
            ressavepath = String.Empty;
            opts = new StrClassGenerationOptions();
            InitializeComponent();
        }

        private static System.String GenerateSFD(System.String filter)
        {
            using SaveFileDialog SFD = new();
            SFD.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            SFD.CheckPathExists = true;
            SFD.CheckFileExists = false;
            SFD.AddExtension = true;
            SFD.Filter = filter;
            if (SFD.ShowDialog() == DialogResult.OK) { return SFD.FileName; } else { return System.String.Empty; }
        }

        private void CH_CHANGED(object sender, EventArgs e)
        {
            System.Boolean temp = GenStrClass.Checked;
            if (temp) {
                StrClassVisibility.SelectedIndex = 0;
            } else {
                L6.Text = Resources.PathToFileNotSpecified;
                opts.StrClassSavePath = System.String.Empty;
                DotNetClassNameBox.Text = string.Empty;
                StrClassVisibility.Text = string.Empty;
                ManifestStreamNameBox.Text = string.Empty;
            }
            DotNetClassNameBox.Enabled = StrClassVisibility.Enabled = ManifestStreamNameBox.Enabled = temp;
            RClassSaveToButton.Enabled = temp;
            opts.Generate = temp;
        }
    
        public StrClassGenerationOptions StrClassGenerationOptions => opts;

        public System.String SavePath => ressavepath;

        public System.Boolean Successfull => succ;

        private void CM_SAVEOUTPATH(object sender, EventArgs e)
        {
            ressavepath = GenerateSFD(Resources.FileExtsWithBinaryClasses);
            if (System.String.IsNullOrEmpty(ressavepath)) {
                L2.Text = Resources.PathToFileNotSpecified;
            } else {
                System.String temp = ressavepath;
                if (temp.Length > 23) {
                    temp = temp.Remove(22) + "...";
                }
                L2.Text = $"Selected file save path is at {temp}";
            }
        }

        private void CM_STRCLASS_SAVEPATH(object sender, EventArgs e)
        {
            opts.StrClassSavePath = GenerateSFD(Resources.FileExtsStrClass);
            if (System.String.IsNullOrEmpty(opts.StrClassSavePath)) {
                L6.Text = Resources.PathToFileNotSpecified;
            } else {
                System.String temp = opts.StrClassSavePath;
                if (temp.Length > 23) {
                    temp = temp.Remove(22) + "...";
                }
                L6.Text = $"Selected file save path is at {temp}";
            }
        }

        private void TrySaveData_Click(object sender, EventArgs e)
        {
            succ = false;
            if (System.String.IsNullOrEmpty(ressavepath)) {
                Helper.ShowErrorMessage(Resources.SavePath_RequiresNotEmpty);
                return;
            }
            ResourceClasses selected = Helper.FromFileName(ressavepath);
            switch (selected)
            {
                case ResourceClasses.DotNetResources:
                case ResourceClasses.CustomBinary:
                    succ = Helper.ShowQuestionMessage(Resources.FinalizeResFileMsg);
                    if (succ == false) { Close(); return; }
                    break;
            }
            if (opts.Generate) {
                opts.StrClassName = DotNetClassNameBox.Text;
                opts.ManifestStreamName = ManifestStreamNameBox.Text;
                if (System.String.IsNullOrEmpty(opts.StrClassSavePath)) {
                    Helper.ShowErrorMessage(Resources.StrClassSavePath_RequiresNotEmpty);
                    return;
                }
                if (System.String.IsNullOrEmpty(opts.StrClassName)) {
                    Helper.ShowErrorMessage(Resources.StrClassName_RequiresNotEmpty);
                    return;
                }
                if (System.String.IsNullOrEmpty(opts.ManifestStreamName)) {
                    Helper.ShowErrorMessage(Resources.StrClassManifestResourceName_RequiresNotEmpty);
                    return;
                }
                try {
                    opts.OutResType = selected.ToResourceType();
                } catch (ArgumentException) {
                    Helper.ShowErrorMessage(Resources.StrClassSupportedForBinaryClasses);
                    return;
                }
                opts.ClassVisibilty = Helper.ToVisibilityFromString(StrClassVisibility.SelectedItem.ToString());
            }
            succ = true;
            Close();
            return;
        }

        private void G_EXIT_FAIL(object sender, EventArgs e) {
            succ = false;
            Close();
            return;
        }
    }

    public sealed class StrClassGenerationOptions
    {
        public StrClassGenerationOptions() { }

        /// <summary>
        /// Specifies a value whether to generate a str class or not. When this field is <see langword="false"/> , the data on the other fields can have undefined/unexpected results when retrieved.
        /// </summary>
        public System.Boolean Generate;

        public System.String StrClassName , ManifestStreamName , StrClassSavePath;

        public DotNetResourcesExtensions.BuildTasks.ResourceClassVisibilty ClassVisibilty;

        public DotNetResourcesExtensions.BuildTasks.OutputResourceType OutResType;
    }
}
