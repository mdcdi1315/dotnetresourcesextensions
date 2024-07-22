﻿using System;
using System.Windows.Forms;
using System.Collections.Generic;
using DotNetResourcesExtensions;

namespace ResourceEditor
{
    public partial class Main : Form
    {
        private System.Boolean fileisopened , fileisnotsaved , isreadonly;
        private List<System.Object> objects;
        private System.String openedfile;

        public Main()
        {
            InitializeComponent();
            objects = new();
            isreadonly = false;
        }

        private void OPF_CLICK(object sender, EventArgs e)
        {
            using OpenFileDialog OFD = new();
            OFD.Multiselect = false;
            OFD.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            OFD.CheckPathExists = true;
            OFD.CheckFileExists = true;
            OFD.AddExtension = true;
            OFD.DefaultExt = ".resj";
            OFD.Filter = "Custom JSON Resource files|*.resj|Custom XML Resource files|*.resxx|Custom ResX Resource files|*.rescx|ResX Resource files|*.resx|Custom MS-INI Resource files|*.resi|.NET binary .resources files (Read-Only)|*.resources|Custom Binary Resources Files (Read-Only)|*.resb";
            if (OFD.ShowDialog() == DialogResult.OK) {
                openedfile = OFD.FileName;
                ParseFileAndAddContentsToListView(OFD.FileName, OFD.OpenFile());
                return;
            } else {
                Helper.ShowErrorMessage("You must select a file from the list provided so as to open it.\n" +
                    "If you do not have such a file , you can create a new one using the Create New Resource File option.");
            }
        }

        private void CREATE_CLICK(object sender, EventArgs e)
        {
            using SaveFileDialog OFD = new();
            OFD.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            OFD.CheckPathExists = true;
            OFD.CheckFileExists = false;
            OFD.AddExtension = true;
            OFD.Title = "Create New Modifidable Resource File";
            OFD.DefaultExt = ".resj";
            OFD.Filter = "Custom JSON Resource files|*.resj|Custom XML Resource files|*.resxx|Custom ResX Resource files|*.rescx|ResX Resource files|*.resx|Custom MS-INI Resource files|*.resi";
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                openedfile = OFD.FileName;
                ResourceView.Items.Clear();
                objects.Clear();
                fileisopened = true;
                fileisnotsaved = true;
                StatusTextBox.Text = "You have opened a blank resource file. Add to it resource data and save it.";
                return;
            }
        }

        private void SaveToFileTask(System.String file , System.Boolean issaveas = false)
        {
            if (fileisnotsaved == false && issaveas == false) { StatusTextBox.Text = "There are no outstanding resources to be saved. File is already updated."; return; }
            System.Threading.Thread TD = new(() => {
                StatusTextBox.Text = $"Saving modified resources to file {file} ...";
                System.Resources.IResourceWriter writer = null;
                try
                {
                    if (System.IO.File.Exists(file)) { System.IO.File.Delete(file); }
                    switch (file.Substring(file.LastIndexOf('.')))
                    {
                        case ".rescx":
                            writer = new DotNetResourcesExtensions.Internal.ResX.ResXResourceWriter(file);
                            break;
                        case ".resj":
                            writer = new JSONResourcesWriter(file);
                            break;
                        case ".resxx":
                            writer = new XMLResourcesWriter(file);
                            break;
                        case ".resi":
                            writer = new MsIniResourcesWriter(file);
                            break;
                        case ".resx":
                            writer = new System.Resources.ResXResourceWriter(file);
                            break;
                    }
                    ResourceView.BeginUpdate(); // Do this so as to access the items faster.
                    Cursor = Cursors.WaitCursor;
                    System.String rn;
                    System.Object value;
                    for (System.Int32 I = 0; I < ResourceView.Items.Count; I++) 
                    {
                        ListViewItem item = ResourceView.Items[I];
                        rn = item.Text;
                        if (System.String.IsNullOrEmpty(rn)) { continue; }
                        value = objects[I];
                        switch (value)
                        {
                            case System.String dd:
                                writer.AddResource(rn, dd);
                                break;
                            case System.Byte[] data:
                                writer.AddResource(rn, data);
                                break;
                            default:
                                writer.AddResource(rn, value);
                                break;
                        }
                    }
                    ResourceView.EndUpdate();
                    StatusTextBox.Text = "Generating resources...";
                    writer.Generate();
                    StatusTextBox.Text = $"Resources were successfully generated to {file} .";
                    if (file == openedfile) { fileisnotsaved = false; }
                } catch (System.Exception e) {
                    ResourceView.EndUpdate();
                    StatusTextBox.Text = "Failed to write the specified resource file.";
                    Helper.ShowErrorMessage($"Failed to write resources to the resource file due to {e.GetType().Name}:\n {e}");
                } finally {
                    Cursor = Cursors.Default;
                    try
                    {
                        writer?.Dispose();
                    }
                    catch (System.Exception e)
                    {
                        Helper.ShowErrorMessage("CRITICAL: Writer was unexpectedly locked. Error is occuring from\n" +
                            $"a {e.GetType().FullName} . The Resource Editor will shut down as soon as you press \'OK\'");
                        System.Environment.FailFast("The DotNetResourcesExtensions Resource Editor application was shut down due to a locking exception while attempting to free resources from the resource writer.", e);
                    }
                }
            });
            TD.TrySetApartmentState(System.Threading.ApartmentState.STA);
            TD.Start();
        }

        private void EXIT_CLICK(object sender, EventArgs e) => Close();

        private ListViewItem.ListViewSubItem GetValueText(System.Object value , List<System.Drawing.Image> images)
        {
            ListViewItem.ListViewSubItem item = new();
            switch (value)
            {
                case System.String di:
                    item.Text = di;
                    break;
                case System.Int64:
                case System.UInt64:
                case System.Single:
                case System.UInt16:
                case System.Int16:
                case System.UInt32:
                case System.Int32:
                case System.Double:
                    item.Text = $"Numeric value: {value} , Type: {value.GetType().FullName}";
                    break;
                case System.Drawing.Bitmap bitmap:
                    item.Text = $"Bitmap Image , Size: Width={bitmap.Width} Height={bitmap.Height}";
                    images.Add(bitmap);
                    break;
                case System.Drawing.Icon icon:
                    item.Text = $"Win32 Icon , Size: Width={icon.Width} Height={icon.Height}";
                    images.Add(icon.ToBitmap());
                    break;
                case System.DateTime dt:
                    item.Text = $"Date and Time point - {dt:F}";
                    break;
                case System.TimeSpan ts:
                    item.Text = $"Time point - {ts.Days}:{ts.Hours}:{ts.Minutes}:{ts.Seconds}.{ts.Milliseconds}";
                    break;
                case System.Guid guid:
                    item.Text = $"GUID identifier - {guid}";
                    break;
                case System.Boolean tf:
                    item.Text = $"Boolean - {tf}";
                    break;
                case System.Uri uri:
                    item.Text = $"URI - {uri}";
                    break;
                case System.Drawing.Point pt:
                    item.Text = $"Point - X={pt.X} Y={pt.Y}";
                    break;
                case System.Drawing.PointF ptf:
                    item.Text = $"Point as Floating Numbers - X={ptf.X} Y={ptf.Y}";
                    break;
                case System.Drawing.Size se:
                    item.Text = $"Size - Width={se.Width} Height={se.Height}";
                    break;
                case System.Drawing.SizeF sef:
                    item.Text = $"Size as Floating Numbers - Width={sef.Width} Height={sef.Height}";
                    break;
                case System.Drawing.Rectangle rect:
                    item.Text = $"Rectangle - Width={rect.Width} Height={rect.Height} X={rect.X} Y={rect.Y}";
                    break;
                case System.Drawing.RectangleF rect:
                    item.Text = $"Rectangle as Floating Numbers - Width={rect.Width} Height={rect.Height} X={rect.X} Y={rect.Y}";
                    break;
                case System.Drawing.Color color:
                    item.Text = $"Color - A={color.A} R={color.R} G={color.G} B={color.B}";
                    break;
                case System.Byte[]:
                    item.Text = "Byte arrays cannot be previewed on the resource viewer.";
                    item.ForeColor = System.Drawing.Color.Yellow;
                    break;
                default:
                    item.Text = $"Unknown type or the app does not support generation for type {value.GetType().AssemblyQualifiedName} .";
                    item.ForeColor = System.Drawing.Color.DarkRed;
                    return item;
            }
            return item;
        }

        private void LBE_EDIT(object sender, LabelEditEventArgs e)
        {
            AssertFileChanges();
        }

        private void AssertFileChanges()
        {
            if (fileisopened)
            {
                fileisnotsaved = true;
                StatusTextBox.Text = "This resource file has been undergone changes and must be saved.";
            }
        }

        private void F_CLOSING(object sender, FormClosingEventArgs e)
        {
            if (fileisopened && fileisnotsaved) 
            {
                e.Cancel = Helper.ShowQuestionMessage($"Exit without saving {openedfile}?") == false;
            }
        }

        private void G_CMS_OPENING(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ResourceView.SelectedIndices.Count < 0) { e.Cancel = true; return; }
        }

        private void CMS_CLICKED(object sender, ToolStripItemClickedEventArgs e)
        {
            ResourceCMS.Hide();
            if (EnsureNotReadOnly()) { return; }
            ListViewItem item = ResourceView.Items[ResourceView.SelectedIndices[0]];
            if (e.ClickedItem == RCMS_1)
            {
                item.BeginEdit();
            } else if (e.ClickedItem == RCMS_2) 
            {
                ResourceEntryEditor ED = new(item.Text , objects[ResourceView.SelectedIndices[0]]);
                ED.ShowDialog();
                if (ED.Success)
                {
                    ResourceView.Items[ResourceView.SelectedIndices[0]].Text = ED.ResourceName;
                    objects[ResourceView.SelectedIndices[0]] = ED.ResourceValue;
                    ResourceView.Update();
                    AssertFileChanges();
                }
            } else if (e.ClickedItem == RCMS_3)
            {
                if (Helper.ShowQuestionMessage($"Are you sure that you want to delete the resource {item.Text} ?"))
                {
                    ResourceView.Items.RemoveAt(ResourceView.SelectedIndices[0]);
                    ResourceView.Update();
                    AssertFileChanges();
                }
            }
        }

        private void ResourceActions_CLICK(object sender, EventArgs e)
        {
            if (EnsureNotReadOnly()) { return; }
            if (sender == RemResource)
            {
                if (ResourceView.SelectedIndices.Count != 1)
                {
                    Helper.ShowErrorMessage("You must select a resource so as to be deleted.");
                    return;
                }
                if (Helper.ShowQuestionMessage($"Are you sure that you want to delete the resource " +
                        $"{ResourceView.Items[ResourceView.SelectedIndices[0]].Text} ?"))
                {
                    ResourceView.Items.RemoveAt(ResourceView.SelectedIndices[0]);
                    ResourceView.Update();
                    AssertFileChanges();
                }
            } else if (sender == EXTRButton) { ExtractResource(); }
        }

        [System.Diagnostics.DebuggerHidden]
        private System.Boolean EnsureNotReadOnly()
        {
            if (isreadonly)
            {
                Helper.ShowErrorMessage("You cannot add , rename , modify or copy finalized resources.");
            }
            return isreadonly;
        }

        private void SAVEFILE_CLICK(object sender, EventArgs e)
        {
            if (EnsureNotReadOnly()) { return; }
            if (fileisopened == false)
            {
                Helper.ShowErrorMessage("You must open a resource file so as to save it.");
                return;
            }
            if (sender == SaveAsFileButton)
            {
                using SaveFileDialog SFD = new();
                SFD.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                SFD.CheckPathExists = true;
                SFD.CheckFileExists = false;
                SFD.AddExtension = true;
                SFD.DefaultExt = ".resj";
                SFD.Filter = "Custom JSON Resource files|*.resj|Custom XML Resource files|*.resxx|Custom ResX Resource files|*.rescx|ResX Resource files|*.resx|Custom MS-INI Resource files|*.resi";
                if (SFD.ShowDialog() == DialogResult.OK)
                {
                    SaveToFileTask(SFD.FileName , true);
                }
            } else if (sender == SaveOpenedButton)
            {
                SaveToFileTask(openedfile);
            }
        }

        private void ADDRES_CLICK(object sender, EventArgs e)
        {
            if (EnsureNotReadOnly()) { return; }
            ResourceEntryEditor ED = new();
            ED.ShowDialog();
            if (ED.Success)
            {
                ResourceView.BeginUpdate();
                List<System.Drawing.Image> images = new();
                if (ResourceView.SmallImageList != null)
                {
                    foreach (System.Drawing.Image image in ResourceView.SmallImageList.Images)
                    {
                        images.Add(image);
                    }
                }
                System.Int32 d = images.Count;
                ListViewItem.ListViewSubItem LVI = GetValueText(ED.ResourceValue, images);
                if (d < images.Count)
                {
                    ResourceView.Items.Add(ED.ResourceName , images.Count - 1).SubItems.Add(LVI);
                } else
                {
                    ResourceView.Items.Add(ED.ResourceName).SubItems.Add(LVI);
                }
                objects.Add(ED.ResourceValue);
                ResourceView.SmallImageList = new() { ImageSize = new(50, 50) };
                ResourceView.SmallImageList.Images.AddRange(images.ToArray());
                ResourceView.EndUpdate();
                ResourceView.Update();
                AssertFileChanges();
            }
        }

        private void ExtractResource()
        {
            if (ResourceView.SelectedIndices.Count != 1) // Although that it does never reach here , we must be ensured of it.
            {
                Helper.ShowErrorMessage("You must select a resource so as to be exported.");
                return;
            }
            System.Object obj = objects[ResourceView.SelectedIndices[0]];
            System.String CreateSaveDialog(System.String filter)
            {
                using SaveFileDialog SFD = new();
                SFD.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                SFD.CheckPathExists = true;
                SFD.CheckFileExists = false;
                SFD.AddExtension = true;
                SFD.Filter = filter;
                if (SFD.ShowDialog() == DialogResult.OK)
                {
                    return SFD.FileName;
                } else { return null; }
            }
            System.String fn = null;
            switch (obj)
            {
                case System.String:
                    Helper.ShowErrorMessage("String resources cannot be exported.");
                    break;
                case System.Byte[] dg:
                    if ((fn = CreateSaveDialog("All Files|*.*")) != null) 
                    {
                        try
                        {
                            using (System.IO.FileStream FD = new(fn, System.IO.FileMode.Create))
                            {
                                FD.Write(dg, 0, dg.Length);
                            }
                        } catch (System.Exception ex)
                        {
                            Helper.ShowErrorMessage($"Saving to file failed: \n{ex}");
                            StatusTextBox.Text = $"Resource extraction failed due to an unexpected {ex.GetType().FullName} .";
                            return;
                        }
                    }
                    break;
                case System.Drawing.Bitmap BM:
                    if ((fn = CreateSaveDialog("PNG files|*.png|JPEG Files|*.jpeg|Bitmap Files|*.bmp")) != null)
                    {
                        System.Drawing.Imaging.ImageFormat fmt;
                        switch (fn.Substring(fn.LastIndexOf('.')))
                        {
                            case ".png":
                                fmt = System.Drawing.Imaging.ImageFormat.Png;
                                break;
                            case ".jpeg":
                                fmt = System.Drawing.Imaging.ImageFormat.Jpeg;
                                break;
                            case ".bmp":
                                fmt = System.Drawing.Imaging.ImageFormat.Bmp;
                                break;
                            default:
                                return;
                        }
                        try {
                            BM.Save(fn, fmt);
                        } catch (System.Exception ex) {
                            Helper.ShowErrorMessage($"Saving to file failed: \n{ex}");
                            StatusTextBox.Text = $"Resource extraction failed due to an unexpected {ex.GetType().FullName} .";
                            return;
                        }
                    }
                    break;
                case System.Drawing.Icon IC:
                    if ((fn = CreateSaveDialog("Win32 Icon Files|*.ico")) != null) {
                        try {
                            using (System.IO.FileStream FD = new(fn, System.IO.FileMode.Create)) { IC.Save(FD); }
                        } catch (System.Exception ex) {
                            Helper.ShowErrorMessage($"Saving to file failed: \n{ex}");
                            StatusTextBox.Text = $"Resource extraction failed due to an unexpected {ex.GetType().FullName} .";
                            return;
                        }
                    }
                    break;
                default:
                    Helper.ShowErrorMessage($"Requested to export an unknown type for the app. The type's assembly qualified name is: \n\'{obj.GetType().AssemblyQualifiedName}\'.");
                    StatusTextBox.Text = $"Resource extraction failed because the app does not have the information to extract a {obj.GetType().FullName} .";
                    return;
            }
            if (fn != null) { StatusTextBox.Text = $"Resource {ResourceView.Items[ResourceView.SelectedIndices[0]].Text} extracted to file {fn} ."; }
        }

        private void FE_BUTTON_CLICK(object sender, EventArgs e)
        {
            if (fileisopened == false) { StatusTextBox.Text = "You have to load an existing modifidable resource file to finalize it."; return; }
            if (EnsureNotReadOnly()) { return; }
            if (Helper.ShowQuestionMessage("Are you sure that you want to finalize the current resource file?\n" +
                "Finalizing a resource file means that the resources are written to a binary format\n" +
                "readable by production apps. After doing this and if you lose all the resource files\n" +
                "that have these contents you cannot modify or copy to other format the produced resource file. Continue?"))
            {
                using SaveFileDialog SFD = new();
                SFD.CheckPathExists = true;
                SFD.CheckFileExists = false;
                SFD.Filter = ".NET binary .resources files|*.resources|Custom Binary Resource file|*.resb";
                SFD.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                SFD.AddExtension = true;
                SFD.Title = "Finalize the current resources...";
                if (SFD.ShowDialog() == DialogResult.OK)
                {
                    System.Threading.Thread TD = new(() => {
                        System.Resources.IResourceWriter writer = null;
                        try
                        {
                            StatusTextBox.Text = $"Saving finalized resources to file {SFD.FileName} ...";
                            if (System.IO.File.Exists(SFD.FileName)) { System.IO.File.Delete(SFD.FileName); }
                            switch (SFD.FileName.Substring(SFD.FileName.LastIndexOf('.')))
                            {
                                case ".resources":
                                    writer = new DotNetResourcesExtensions.Internal.DotNetResources.PreserializedResourceWriter(SFD.FileName);
                                    break;
                                case ".resb":
                                    writer = new CustomBinaryResourceWriter(SFD.FileName);
                                    break;
                            }
                            ResourceView.BeginUpdate(); // Do this so as to access the items faster.
                            Cursor = Cursors.WaitCursor;
                            System.String rn;
                            System.Object value;
                            for (System.Int32 I = 0; I < ResourceView.Items.Count; I++)
                            {
                                ListViewItem item = ResourceView.Items[I];
                                rn = item.Text;
                                if (System.String.IsNullOrEmpty(rn)) { continue; }
                                value = objects[I];
                                switch (value)
                                {
                                    case System.String dd:
                                        writer.AddResource(rn, dd);
                                        break;
                                    case System.Byte[] data:
                                        writer.AddResource(rn, data);
                                        break;
                                    default:
                                        writer.AddResource(rn, value);
                                        break;
                                }
                            }
                            ResourceView.EndUpdate();
                            StatusTextBox.Text = "Generating resources...";
                            writer.Generate();
                            StatusTextBox.Text = $"Resources were successfully generated to {SFD.FileName} .";
                        } catch (System.Exception e)
                        {
                            ResourceView.EndUpdate();
                            StatusTextBox.Text = "Failed to write the specified resource file.";
                            Helper.ShowErrorMessage($"Failed to write resources to the resource file due to {e.GetType().Name}:\n {e}");
                        } finally { 
                            Cursor = Cursors.Default;
                            try
                            {
                                writer?.Dispose();
                            } catch (System.Exception e) 
                            {
                                Helper.ShowErrorMessage("CRITICAL: Writer was unexpectedly locked. Error is occuring from\n" +
                                    $"a {e.GetType().FullName} . The Resource Editor will shut down as soon as you press \'OK\'");
                                System.Environment.FailFast("The DotNetResourcesExtensions Resource Editor application was shut down due to a locking exception while attempting to free resources from the resource writer." , e);
                            }
                        }
                    });
                    TD.TrySetApartmentState(System.Threading.ApartmentState.STA);
                    TD.Start();
                }
            }
        }

        private void ParseFileAndAddContentsToListView(System.String file , System.IO.Stream str)
        {
            System.Threading.Thread TD = new(() => {
                IResourceLoader RL = null;
                System.String cd = System.Environment.CurrentDirectory;
                System.Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(file);
                List<System.Drawing.Image> images = new() { new System.Drawing.Bitmap(50, 50) };
                try
                {
                    StatusTextBox.Text = $"Opening file {file} ...";
                    switch (file.Substring(file.LastIndexOf('.')))
                    {
                        case ".rescx":
                            RL = new CustomResXResourcesLoader(str);
                            isreadonly = false;
                            break;
                        case ".resj":
                            RL = new JSONResourcesLoader(str);
                            isreadonly = false;
                            break;
                        case ".resxx":
                            RL = new XMLResourcesLoader(str);
                            isreadonly = false;
                            break;
                        case ".resi":
                            RL = new CustomMsIniResourcesLoader(str);
                            isreadonly = false;
                            break;
                        case ".resx":
                            RL = new ResXResourceLoader(str);
                            isreadonly = false;
                            break;
                        case ".resb":
                            RL = new CustomDataResourcesLoader(str);
                            isreadonly = true;
                            break;
                        case ".resources":
                            RL = new DotNetResourceLoader(str);
                            isreadonly = true;
                            break;
                    }
                    if (RL == null) { Helper.ShowErrorMessage("Reading the resource file failed unexpectedly."); StatusTextBox.Text = "Failed to read the specified resource file."; return; }
                    ResourceView.SmallImageList = null;
                    ResourceView.BeginUpdate();
                    ResourceView.Items.Clear();
                    System.Int32 d;
                    StatusTextBox.Text = "Reading resource entries...";
                    foreach (IResourceEntry entry in RL)
                    {
                        d = images.Count;
                        ListViewItem.ListViewSubItem lsi = GetValueText(entry.Value, images);
                        if (d < images.Count)
                        {
                            ResourceView.Items.Add(entry.Name, images.Count - 1).SubItems.Add(lsi);
                        }
                        else
                        {
                            ResourceView.Items.Add(entry.Name, 0).SubItems.Add(lsi);
                        }
                        objects.Add(entry.Value);
                    }
                    ResourceView.SmallImageList = new() { ImageSize = new(50, 50) };
                    ResourceView.SmallImageList.Images.AddRange(images.ToArray());
                    ResourceView.EndUpdate();
                    ResourceView.Update();
                    StatusTextBox.Text = "Ready";
                    fileisopened = true;
                } catch (System.Exception e) {
                    StatusTextBox.Text = "Failed to read the specified resource file.";
                    Helper.ShowErrorMessage($"Failed to read resources from the resource file due to {e.GetType().Name}: {e}");
                } finally {
                    System.Environment.CurrentDirectory = cd;
                    try { RL?.Dispose(); } catch (System.Exception e) {
                        Helper.ShowErrorMessage("CRITICAL: Writer was unexpectedly locked. Error is occuring from\n" +
                            $"a {e.GetType().FullName} . The Resource Editor will shut down as soon as you press \'OK\'");
                        System.Environment.FailFast("The DotNetResourcesExtensions Resource Editor application was shut down due to a locking exception while attempting to free resources from the resource writer.", e);
                    }
                    images.Clear();
                    images = null;
                }
            });
            TD.TrySetApartmentState(System.Threading.ApartmentState.STA);
            TD.Start();
        }
    }
}
