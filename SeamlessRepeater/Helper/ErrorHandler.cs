using SeamlessRepeater.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SeamlessRepeater.Helper
{
    public static class ErrorHandler
    {
        private const Environment.SpecialFolder _file = Environment.SpecialFolder.LocalApplicationData;
        private const string _programFolder = "/SeamlessRepeater/";

        public static void Handle(string errorMessage, bool isError = true, Exception exception = null)
        {
            var dialog = new CustomDialog("Error", errorMessage, CustomDialogType.OK);
            dialog.ShowDialog();
            //MessageBox.Show(errorMessage, "Error");

            string fileName = "SeamLogs.txt";
            string folder = $"{Environment.GetFolderPath(_file)}{_programFolder}";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string path = $"{folder}{fileName}";

            string type = "[Info]";
            if (isError)
                type = "[Error]";

            string textLine = $"{DateTime.Now}: {type} {errorMessage}";
            if (exception != null)
                textLine += $"{exception}";

            if (!File.Exists(path))
            {
                using (var creator = File.CreateText(path))
                {
                }
            }

            using (var streamWriter = File.AppendText(path))
                streamWriter.WriteLine(textLine);
        }
    }
}
