using System.Windows.Input;
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
    }
}