using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Navigation;
using YoutubeExplode.DemoWpf.ViewModels;

namespace YoutubeExplode.DemoWpf.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void tbVideoId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            ((MainViewModel) DataContext).GetVideoInfoCommand.Execute(null);
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
            e.Handled = true;
        }
    }
}