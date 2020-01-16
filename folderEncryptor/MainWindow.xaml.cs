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
using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;


namespace folderEncryptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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


        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                tbFilePath.Text = openFileDialog.FileName.ToString();
            }
        }

        private void rbEncrypt_Checked(object sender, RoutedEventArgs e)
        {
            rbDecrypt.IsChecked = false;
        }

        private void rbDencrypt_Checked(object sender, RoutedEventArgs e)
        {
            rbEncrypt.IsChecked = false;
        }

        private void btnEncryptDecrypt_Click(object sender, RoutedEventArgs e)
        {
            if(!File.Exists(tbFilePath.Text))
            {
                MessageBox.Show(".پرونده یافت نشد");
                return;
            }

            if (String.IsNullOrEmpty(pwbPassword.Password))
            {
                MessageBox.Show(".رمز عبور خالی است. لطفاً یک رمز عبور انتخاب کنید");
                return;
            }
            if (String.IsNullOrEmpty(pwbInitializationVector.Password))
            {
                MessageBox.Show(".رمز عبور حامل اولیه خالی است. لطفاً یک رمز عبور منحصر به فرد انتخاب کنید");
                return;
            }

            AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider();
            aesProvider.BlockSize = 128;
            aesProvider.KeySize = 256;
            aesProvider.IV = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(pwbInitializationVector.Password)).Take(16).ToArray();
            aesProvider.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(pwbPassword.Password));
            aesProvider.Mode = CipherMode.CBC;
            aesProvider.Padding = PaddingMode.PKCS7;


            byte[] fileContent;
            try
            {
                fileContent = File.ReadAllBytes(tbFilePath.Text);
            } catch
            {
                MessageBox.Show(".پرونده ممکن است در حال استفاده باشد. برنامه ای را که از آن استفاده می کند ببندید و دوباره امتحان کنید");
                return;
            }

            byte[] resultantBytes;
            if (rbEncrypt.IsChecked == true)
            {
                ICryptoTransform transform = aesProvider.CreateEncryptor(aesProvider.Key, aesProvider.IV);
                resultantBytes = transform.TransformFinalBlock(fileContent, 0, fileContent.Length);
                
            } else if (rbDecrypt.IsChecked == true)
            {
                ICryptoTransform transform = aesProvider.CreateDecryptor(aesProvider.Key, aesProvider.IV);
                try
                {
                     resultantBytes = transform.TransformFinalBlock(fileContent, 0, fileContent.Length);
                } catch
                {
                    MessageBox.Show(".آیا مطمئن هستید که این پرونده درست است؟ اندازه پرونده مطابقت ندارد یا پرونده خراب شده است");
                    return;
                }
            }
            else
            {
                MessageBox.Show(".لطفاً گزینه ای را برای رمزگذاری یا رمزگشایی انتخاب کنید");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            string fileExtension = System.IO.Path.GetExtension(tbFilePath.Text);
            saveFileDialog.Filter = $"فایل ها (*{fileExtension}) | *{fileExtension}";

            if (saveFileDialog.ShowDialog() == true)
            {

                File.WriteAllBytes(saveFileDialog.FileName, resultantBytes);
            }
            else
            {
                MessageBox.Show(".پرونده ذخیره نشد");
                return;
            }
        }

    }
}
