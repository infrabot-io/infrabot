using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace infrabot.PluginEditor.Utils
{
    public class CommonUtils
    {
        public static void OpenLinkInBrowser(string url)
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.FileName = url;
                proc.Start();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public static string ReadDocumentationFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = assembly.GetManifestResourceNames()
                    .Single(str => str.EndsWith(@"infrabot.PluginEditor.Documentation.HelpDocumentation.xml"));

            Stream stream = assembly.GetManifestResourceStream(resourcePath);
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static string ReadLicenseFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = assembly.GetManifestResourceNames()
                    .Single(str => str.EndsWith(@"infrabot.PluginEditor.LICENSE.txt"));

            Stream stream = assembly.GetManifestResourceStream(resourcePath);
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
