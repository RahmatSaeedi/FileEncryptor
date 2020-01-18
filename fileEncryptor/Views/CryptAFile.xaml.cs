using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace fileEncryptor.Views
{
    /// <summary>
    /// Interaction logic for CryptAFile.xaml
    /// </summary>
    public partial class CryptAFile : UserControl
    {
        public CryptAFile()
        {
            InitializeComponent();
        }


        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                tbFilePath.Text = openFileDialog.FileName.ToString();
                if (tbStatusUpdate != null)
                {
                    tbStatusUpdate.Text = string.Empty;
                }
            }
        }

        private void rbEncrypt_Checked(object sender, RoutedEventArgs e)
        {
            if (tbStatusUpdate != null)
            {
                tbStatusUpdate.Text = string.Empty;
            }
        }

        private void rbDencrypt_Checked(object sender, RoutedEventArgs e)
        {
            if(tbStatusUpdate != null)
            {
                tbStatusUpdate.Text = string.Empty;
            }
        }

        private void btnEncryptDecrypt_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(tbFilePath.Text))
            {
                MessageBox.Show("پرونده یافت نشد", "پیام", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                return;
            }

            if (String.IsNullOrEmpty(pwbPassword.Password))
            {
                MessageBox.Show("رمز عبور خالی است. لطفاً یک رمز عبور انتخاب کنید", "پیام", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                return;
            }
            if (String.IsNullOrEmpty(pwbInitializationVector.Password))
            {
                MessageBox.Show("رمز عبور حامل اولیه خالی است. لطفاً یک رمز عبور منحصر به فرد انتخاب کنید", "پیام", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                return;
            }



            byte[] fileContent;
            try
            {
                fileContent = File.ReadAllBytes(tbFilePath.Text);
            }
            catch
            {
                MessageBox.Show("پرونده ممکن است در حال استفاده باشد. برنامه ای را که از آن استفاده می کند ببندید و دوباره امتحان کنید", "پیام", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                return;
            }

            if (rbEncrypt.IsChecked == true)
            {
                System.Threading.Thread encryptThread = new System.Threading.Thread(() => {
                    aesEncrypt();
                });
                encryptThread.Start();
            }
            else if (rbDecrypt.IsChecked == true)
            {
                System.Threading.Thread  decryptThread= new System.Threading.Thread(() => {
                    aesDecrypt();
                });
                decryptThread.Start();
            }
            else
            {
                MessageBox.Show("لطفاً گزینه ای را برای رمزگذاری یا رمزگشایی انتخاب کنید", "پیام", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                return;
            }
        }

        void aesEncrypt()
        {
            this.Dispatcher.Invoke(() =>
            {
                byte[] fileContent;
                try
                {
                    tbStatusUpdate.Text = "در حال خواندن پرونده هستیم.";
                    fileContent = File.ReadAllBytes(tbFilePath.Text);
                    tbStatusUpdate.Text = "پرونده خوانده شد.";
                }
                catch
                {
                    MessageBox.Show("پرونده ممکن است در حال استفاده باشد. برنامه ای را که از آن استفاده می کند ببندید و دوباره امتحان کنید", "پیام", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                    return;
                }

                AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider();
                aesProvider.BlockSize = 128;
                aesProvider.KeySize = 256;



                aesProvider.IV = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(pwbInitializationVector.Password)).Take(16).ToArray();
                aesProvider.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(pwbPassword.Password));
                aesProvider.Mode = CipherMode.CBC;
                aesProvider.Padding = PaddingMode.PKCS7;

                byte[] resultantBytes;
                ICryptoTransform transform = aesProvider.CreateEncryptor(aesProvider.Key, aesProvider.IV);
                try
                {
                    
                    tbStatusUpdate.Text = "پرونده در حال رمزگذاری شدن است.";
                    resultantBytes = transform.TransformFinalBlock(fileContent, 0, fileContent.Length);
                    tbStatusUpdate.Text = "پرونده رمزگذاری شد.";
                }
                catch (Exception e)
                {
                    MessageBox.Show(" آیا مطمئن هستید که این پرونده درست است؟" + e.Message, "پیام", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                    return;
                }
                

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                string fileExtension = System.IO.Path.GetExtension(tbFilePath.Text);
                saveFileDialog.Filter = $"فایل ها (*{fileExtension}) | *{fileExtension}";

                if (saveFileDialog.ShowDialog() == true)
                {
                    tbStatusUpdate.Text = "پرونده در حال ذخیره شدن است.";
                    File.WriteAllBytes(saveFileDialog.FileName, resultantBytes);
                    tbStatusUpdate.Text = "پرونده ذخیره شد. تمام ";
                }
                else
                {
                    MessageBox.Show("پرونده ذخیره نشد", "پیام", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                    tbStatusUpdate.Text = "پرونده ذخیره نشد!";
                    return;
                }
            });
        }
        void aesDecrypt()
        {
            this.Dispatcher.Invoke(() =>
            {

                byte[] fileContent;
                try
                {
                    tbStatusUpdate.Text = "پرونده در حال خوانده شدن است.";
                    fileContent = File.ReadAllBytes(tbFilePath.Text);
                    tbStatusUpdate.Text = "پرونده خوانده شد.";
                }
                catch (Exception e)
                {
                    MessageBox.Show("پرونده ممکن است در حال استفاده باشد. برنامه ای را که از آن استفاده می کند ببندید و دوباره امتحان کنید" + e.Message, "پیام", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                    return;
                }

                AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider();
                aesProvider.BlockSize = 128;
                aesProvider.KeySize = 256;

                aesProvider.IV = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(pwbInitializationVector.Password)).Take(16).ToArray();
                aesProvider.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(pwbPassword.Password));
                aesProvider.Mode = CipherMode.CBC;
                aesProvider.Padding = PaddingMode.PKCS7;

                byte[] resultantBytes;
                ICryptoTransform transform = aesProvider.CreateDecryptor(aesProvider.Key, aesProvider.IV);
                try
                {
                    tbStatusUpdate.Text = "پرونده در حال رمزگشایی شدن است.";
                    resultantBytes = transform.TransformFinalBlock(fileContent, 0, fileContent.Length);
                    tbStatusUpdate.Text = "پرونده رمزگشایی شد.";
                }
                catch (Exception e)
                {
                    MessageBox.Show("آیا مطمئن هستید که این پرونده درست است؟ اندازه پرونده مطابقت ندارد یا پرونده خراب شده است" + e.Message, "پیام", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                    tbStatusUpdate.Text = "آیا مطمئن هستید که این پرونده درست است؟ اندازه پرونده مطابقت ندارد یا پرونده خراب شده است.";
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                string fileExtension = System.IO.Path.GetExtension(tbFilePath.Text);
                saveFileDialog.Filter = $"فایل ها (*{fileExtension}) | *{fileExtension}";

                if (saveFileDialog.ShowDialog() == true)
                {
                    tbStatusUpdate.Text = "پرونده در حال ذخیره شدن است.";
                    File.WriteAllBytes(saveFileDialog.FileName, resultantBytes);
                    tbStatusUpdate.Text = "پرونده ذخیره شد. تمام ";
                }
                else
                {
                    MessageBox.Show("پرونده ذخیره نشد", "پیام", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                    tbStatusUpdate.Text = "پرونده ذخیره نشد!";
                    return;
                }
            });
        }

    }
}
