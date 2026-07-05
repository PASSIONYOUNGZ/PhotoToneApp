using System.IO;
using System.Windows;

namespace PhotoToneApp.Controls
{
    public partial class RenameDialog : Window
    {
        public string NewFileName { get; private set; } = string.Empty;

        public RenameDialog(string currentFileNameWithoutExtension)
        {
            InitializeComponent();

            FileNameTextBox.Text = currentFileNameWithoutExtension;
            Loaded += RenameDialog_Loaded;
        }

        private void RenameDialog_Loaded(object sender, RoutedEventArgs e)
        {
            FileNameTextBox.Focus();
            FileNameTextBox.SelectAll();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string input = FileNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("文件名不能为空。", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!string.IsNullOrEmpty(Path.GetExtension(input)))
            {
                MessageBox.Show("请输入不含扩展名的文件名。", "PhotoToneApp", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            NewFileName = input;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
