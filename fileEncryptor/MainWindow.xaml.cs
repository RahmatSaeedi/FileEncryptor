using fileEncryptor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace fileEncryptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new CryptAFileViewModel();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }
        private void Minimize(object sende, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        private void WindowDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void btnCryptAFile_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new CryptAFileViewModel();
        }

        private void btnArchive_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new ArchieveViewModel();
        }
    }
}
