using System.Reflection;
using System.Windows;
using Infrabot.PluginEditor.Utils;

namespace Infrabot.PluginEditor.Windows
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
            LoadDataInfoIntoTheForm();
        }

        public void LoadDataInfoIntoTheForm()
        {
            AboutTitle.Content = "Infrabot Plugin Editor " + Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            AboutInfo.Content = "Author: Akshin Mustafayev";
            LicenseText.Text = CommonUtils.ReadLicenseFile();
        }

        private void GetInspirationButton_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.OpenLinkInBrowser("https://www.youtube.com/watch?v=l0U7SxXHkPY");
        }

        private void CheckNewReleasesButton_Click(object sender, RoutedEventArgs e)
        {
            CommonUtils.OpenLinkInBrowser("https://github.com/infrabot-io/infrabot/releases");
        }

        private void GithubImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CommonUtils.OpenLinkInBrowser("https://github.com/infrabot-io/infrabot");
        }
    }
}
