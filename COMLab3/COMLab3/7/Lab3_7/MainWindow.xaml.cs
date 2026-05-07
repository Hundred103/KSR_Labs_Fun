using System;
using System.IO;
using System.Reflection;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace Lab3_7
{
    public class InputDialog : Window
    {
        private System.Windows.Controls.TextBox txtInput;
        public string Answer { get; private set; }

        public InputDialog(string question, string defaultAnswer = "")
        {
            Title = question;
            Width = 400;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var stackPanel = new System.Windows.Controls.StackPanel { Margin = new Thickness(10) };
            stackPanel.Children.Add(new System.Windows.Controls.TextBlock { Text = question, Margin = new Thickness(0, 0, 0, 10) });

            txtInput = new System.Windows.Controls.TextBox { Text = defaultAnswer, Margin = new Thickness(0, 0, 0, 10) };
            stackPanel.Children.Add(txtInput);

            var btnOk = new System.Windows.Controls.Button { Content = "OK", Width = 80, HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
            btnOk.Click += (s, e) => { Answer = txtInput.Text; DialogResult = true; };
            btnOk.IsDefault = true;
            stackPanel.Children.Add(btnOk);

            Content = stackPanel;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private void BtnWww_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Podaj adres WWW", "https://www.google.com");
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Answer))
            {
                wbWww.Navigate(dialog.Answer);
            }
        }

        private void BtnPdf_Click(object sender, RoutedEventArgs e)
        {
            string defaultWav = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Wybierz plik PDF",
                Filter = "PDF (*.pdf)|*.wav|Wszystkie pliki (*.*)|*.*",
                FileName = File.Exists(defaultWav) ? defaultWav : string.Empty,
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string wavPath = openFileDialog.FileName;
                pdfHost.Navigate(openFileDialog.FileName);
            }
        }

        private void BtnWav_Click(object sender, RoutedEventArgs e)
        {
            string defaultWav = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ring.wav");

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Wybierz plik WAV",
                Filter = "WAV (*.wav)|*.wav|Wszystkie pliki (*.*)|*.*",
                FileName = File.Exists(defaultWav) ? defaultWav : string.Empty,
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string wavPath = openFileDialog.FileName;
                meAudio.Source = new Uri(wavPath, UriKind.Absolute);
                meAudio.Play();
            }
        }
    }
}