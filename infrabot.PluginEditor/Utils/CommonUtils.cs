using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Infrabot.PluginEditor.Utils
{
    public static class CommonUtils
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
                    .Single(str => str.EndsWith(@"PluginEditor.Documentation.HelpDocumentation.xml"));

            Stream? stream = assembly.GetManifestResourceStream(resourcePath);
            StreamReader? reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static string ReadLicenseFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = assembly.GetManifestResourceNames()
                    .Single(str => str.EndsWith(@"PluginEditor.LICENSE.txt"));

            Stream? stream = assembly.GetManifestResourceStream(resourcePath);
            StreamReader? reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
