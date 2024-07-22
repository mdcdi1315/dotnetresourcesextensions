using System;
using System.Windows.Forms;

namespace ResourceEditor
{
    public partial class ResourceEntryEditor : Form
    {
        private const System.String L3Constant = "Specify the resource value:";
        private System.String name;
        private System.Object value;
        private System.String filepath;
        private System.Boolean success;

        public ResourceEntryEditor(string name = null, object value = null)
        {
            InitializeComponent();
            this.name = name;
            this.value = value;
            if ((name != null && value == null) || (name == null && value != null)) { throw new ArgumentException("Not possible one of both values to not have a value."); }
            success = false;
        }

        public System.String ResourceName => name;

        public object ResourceValue => value;

        public System.Boolean Success => success;

        private KnownResourceType GetSelectedResourceType() => (KnownResourceType)Enum.Parse(typeof(KnownResourceType), ResTypes.SelectedItem.ToString());

        private void F_LOAD(object sender, EventArgs e)
        {
            ResName.Text = name;
            foreach (var item in Enum.GetNames(typeof(KnownResourceType)))
            {
                ResTypes.Items.Add(item);
            }
            if (value != null)
            {
                if (value is String)
                {
                    ResTypes.SelectedIndex = 0;
                    ResValue.Text = value.ToString();
                } else if (value is System.Byte[])
                {
                    ResTypes.SelectedIndex = 2;
                } else if (value is System.Drawing.Icon)
                {
                    ResTypes.SelectedIndex = 5;
                    ResValue.Text = "Icon loaded from memory";
                } else if (value is System.Drawing.Bitmap)
                {
                    ResTypes.SelectedIndex = 4;
                    ResValue.Text = "Bitmap loaded from memory";
                } else { ResTypes.SelectedIndex = 1; }
            } else { ResTypes.SelectedIndex = 0; }
        }

        private void SIC_RESTYPE(object sender, EventArgs e)
        {
            if (ResTypes.SelectedIndex == -1) { return; }
            ResValue.Text = System.String.Empty;
            if (GetSelectedResourceType() >= KnownResourceType.ByteArray) {
                L3.Text = "To specify the resource value for these resource types , \nyou need to specify a file path to read the file.";
                ResValue.Visible = false;
                L4.Visible = true;
                L4.Text = "You have not specified a file path yet.";
                OpenFileButton.Visible = true;
            } else {
                L3.Text = L3Constant;
                L4.Visible = false;
                ResValue.Visible = true;
                OpenFileButton.Visible = false;
            }
        }

        private void OPEN_FindFile(object sender, EventArgs e)
        {
            using OpenFileDialog OFD = new();
            System.String flt = "All Files|*.*";
            switch (GetSelectedResourceType())
            {
                case KnownResourceType.Bitmap:
                    flt = "All bitmap files|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.wmf;*.gif";
                    break;
                case KnownResourceType.Icon:
                    flt = "All icon files|*.ico";
                    break;
            }
            OFD.Filter = flt;
            OFD.Title = "Select a file to read for resource value";
            OFD.CheckFileExists = true;
            OFD.CheckPathExists = true;
            OFD.Multiselect = false;
            OFD.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (OFD.ShowDialog() == DialogResult.OK) 
            {
                filepath = OFD.FileName;
                // We are ensured that filepath is not null.
                System.String disp = filepath;
                if (disp.Length > 21) {
                    disp = disp.Substring(0, 21) + "...";
                } else {
                    disp += " .";
                }
                L4.Text = $"Path of the resource is {disp}";
            }
        }

        private void F_CLOSING(object sender, FormClosingEventArgs e)
        {
            if (success == false) return;
            name = ResName.Text;
            try { Helper.ValidateName(name); } catch (System.Exception ex)  { Helper.ShowErrorMessage($"Error: {ex.Message}"); e.Cancel = true; return; }
            L4.Visible = true;
            L4.Text = "Generating resource value...";
            try
            {
                Cursor = Cursors.WaitCursor;
                if (System.String.IsNullOrWhiteSpace(name)) 
                {
                    throw new ArgumentException("Neither the resource name or the resource value can be empty values.");
                }
                System.IO.FileStream FS = null;
                switch ((KnownResourceType)Enum.Parse(typeof(KnownResourceType) , ResTypes.SelectedItem.ToString()))
                {
                    case KnownResourceType.String:
                        value = ResValue.Text;
                        break;
                    case KnownResourceType.Numeric:
                        value = Helper.ParseAsNumericValue(ResValue.Text);
                        break;
                    case KnownResourceType.ByteArray:
                        FS = null;
                        try
                        {
                            FS = new(filepath, System.IO.FileMode.Open);
                            value = Helper.ReadBuffered(FS, FS.Length);
                        } finally { FS?.Dispose(); }
                        break;
                    case KnownResourceType.Bitmap:
                        value = new System.Drawing.Bitmap(filepath);
                        break;
                    case KnownResourceType.Icon:
                        value = new System.Drawing.Icon(filepath);
                        break;
                }
                success = true;
            } catch (System.Exception ex) 
            { 
                e.Cancel = true; 
                Helper.ShowErrorMessage($"Error: {ex.Message}");
            } finally { Cursor = Cursors.Default; }
        }

        private void CEXIT_CLICK(object sender, EventArgs e)
        {
            success = false;
            Close();
        }

        private void SIC_OK_CLICK(object sender, EventArgs e)
        {
            success = true;
            Close();
        }
    }

    public enum KnownResourceType : System.Byte
    {
        String,
        Numeric,
        ByteArray,
        File = ByteArray,
        Bitmap,
        Icon
    }

}
