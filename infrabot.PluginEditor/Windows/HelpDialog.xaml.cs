using Infrabot.PluginEditor.Utils;
using System.Windows;
using System.Xml;

namespace Infrabot.PluginEditor.Windows
{
    /// <summary>
    /// Interaction logic for HelpDialog.xaml
    /// </summary>
    public partial class HelpDialog : Window
    {
        public HelpDialog(string helpitem)
        {
            InitializeComponent();
            LoadXml(helpitem);
        }

        private void LoadXml(string helpitem)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(CommonUtils.ReadDocumentationFile());

            if (doc is null)
                return;

            XmlNodeList nodeList = doc.SelectNodes("/items/item[name='" + helpitem + "']");

            HelpHeading.Text = nodeList[0]["heading"].InnerText;
            HelpUsed.Text = nodeList[0]["used"].InnerText;
            HelpDescription.Text = nodeList[0]["description"].InnerText;
        }
    }
}
