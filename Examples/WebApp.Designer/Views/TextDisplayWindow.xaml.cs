using MahApps.Metro.Controls;

namespace WebApp.Designer.Views
{
    public partial class TextDisplayWindow : MetroWindow
    {
        public string DisplayText
        {
            get => _text.Text;
            set => _text.Text = value;
        }

        public TextDisplayWindow()
            => InitializeComponent();

        public void AddDisplayText(string newText)
        {
            if (!string.IsNullOrWhiteSpace(newText))
            {
                _text.AppendText(newText);
                _scroll.ScrollToEnd();
            }
        }
    }
}
