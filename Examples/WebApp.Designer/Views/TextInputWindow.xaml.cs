using MahApps.Metro.Controls;

namespace WebApp.Designer.Views
{
    public partial class TextInputWindow : MetroWindow
    {
        public string Text
        {
            get => _text.Text;
            set => _text.Text = value;
        }

        public TextInputWindow()
        {
            InitializeComponent();
        }

        private void ButtonOkClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancelClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
