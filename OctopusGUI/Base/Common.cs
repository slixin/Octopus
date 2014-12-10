using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OctopusGUI
{
    public class Common
    {
        public static bool SaveFileDialog(out string file)
        {
            file = null;
            return SaveFileDialog(out file, "xml", null);
        }

        public static bool SaveFileDialog(out string file, string extension, string defaultFileName)
        {
            bool result = false;

            file = null;

            try
            {
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.DefaultExt = extension;
                sfd.FileName = defaultFileName;
                sfd.Filter = string.Format("{0} File (*.{0})|*.{0}", extension);
                sfd.AddExtension = true;

                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    file = sfd.FileName;
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public static bool OpenFileDialog(out string[] files)
        {
            files = null;
            return OpenFileDialog(out files, false, "xml");
        }

        public static bool OpenFileDialog(out string[] files, bool isMultiSelect, string extension)
        {
            bool result = false;

            files = null;

            try
            {
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                ofd.Filter = string.Format("{0} File (*.{0})|*.{0}", extension);
                ofd.Multiselect = isMultiSelect;

                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    files = ofd.FileNames;
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public static bool OpenFolderDialog(out string folder)
        {
            bool result = false;
            folder = null;

            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = "C:\\";

            DialogResult diagResult = folderDialog.ShowDialog();
            if (diagResult == DialogResult.OK)
            {
                folder = folderDialog.SelectedPath;
                result = true;
            }

            return result;
        }
    }
}
