using System.Windows.Input;
using DemoWpf.ViewModels;

namespace DemoWpf.Views
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