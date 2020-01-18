using System;
using System.Collections.Generic;
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
using System.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;


namespace fileEncryptor.Views
{
    /// <summary>
    /// Interaction logic for Archieve.xaml
    /// </summary>
    public partial class Archieve : UserControl
    {
        public Archieve()
        {
            InitializeComponent();
        }

        private void btBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                tbFilePath.Text = dialog.FileName;
                pbCompressingFiles.Value = 0;
            }
        }

        private void btBrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                tbFolderPath.Text = dialog.FileName;
                pbCompressingFiles.Value = 0;
            }
        }

        private void btCompressFile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbFilePath.Text))
            {
                MessageBox.Show("لطفاً یک پرونده را انتخاب کنید", "پیام", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                tbFilePath.Focus();
                return;
            }

            string path = tbFilePath.Text;
            Thread thread = new Thread(t =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = " زیپ | *zip";
                if (saveFileDialog.ShowDialog() == true)
                {
                    if (!System.IO.File.Exists(saveFileDialog.FileName + ".zip"))
                    {
                        using (Ionic.Zip.ZipFile zipFile = new Ionic.Zip.ZipFile())
                        {
                            zipFile.AddFile(path);
                            zipFile.SaveProgress += zipFile_SaveFileProgress;
                            zipFile.Save(saveFileDialog.FileName + ".zip");
                        }
                        MessageBox.Show("(-: تمام شده", "پیام", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                        return;
                    }
                    else
                    {
                        MessageBox.Show("پرونده ای با این نام وجود دارد. لطفاً نام دیگری را انتخاب کنید", "پیام", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                        return;
                    }
                }
            })
            { IsBackground = true };
            thread.Start();
        }

        private void btCompressFolder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbFolderPath.Text))
            {
                MessageBox.Show("لطفاً یک پوشه را انتخاب کنید", "پیام", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                tbFolderPath.Focus();
                return;
            }

            string path = tbFolderPath.Text;
            Thread thread = new Thread(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = " زیپ | *zip";
                if (saveFileDialog.ShowDialog() == true)
                {
                    if (!System.IO.File.Exists(saveFileDialog.FileName + ".zip"))
                    {
                        using (Ionic.Zip.ZipFile zipFile = new Ionic.Zip.ZipFile())
                        {
                            zipFile.AddDirectory(path);
                            zipFile.SaveProgress += zipFile_SaveFolderProgress;
                            zipFile.Save(saveFileDialog.FileName + ".zip");
                        }
                        MessageBox.Show("(-: تمام شده", "پیام", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                        return;
                    }
                    else
                    {
                        MessageBox.Show("پرونده ای با این نام وجود دارد. لطفاً نام دیگری را انتخاب کنید", "پیام", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                        return;
                    }
                }
            })
            { IsBackground = true };
            thread.Start();
        }
        private void zipFile_SaveFolderProgress(object sender, Ionic.Zip.SaveProgressEventArgs e)
        {
            this.Dispatcher.Invoke(() => { 
                if (e.EventType == Ionic.Zip.ZipProgressEventType.Saving_BeforeWriteEntry)
                {
                    pbCompressingFiles.Maximum = e.EntriesTotal;
                    pbCompressingFiles.Value = e.EntriesSaved + 1;
                }
            });
        }
        private void zipFile_SaveFileProgress(object sender, Ionic.Zip.SaveProgressEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (e.EventType == Ionic.Zip.ZipProgressEventType.Saving_EntryBytesRead)
                {
                    pbCompressingFiles.Maximum = 100;
                    pbCompressingFiles.Value = (int)(e.BytesTransferred * 100 / e.TotalBytesToTransfer);
                }
            });

        }
    }
}
