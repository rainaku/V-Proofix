using System.Threading;
using System.Windows;
using System.Windows.Input;
using VProofix.Services;

namespace VProofix.Views
{
    public partial class PreviewWindow : Window
    {
        private string _text;
        private readonly ClipboardService _clipboardService;

        public PreviewWindow(string text, ClipboardService clipboardService)
        {
            InitializeComponent();
            _text = text;
            _clipboardService = clipboardService;
            txtPreview.Text = _text;

            // Optional: Move window near mouse cursor or caret
            // Point p = GetMousePosition();
            // this.Left = p.X; this.Top = p.Y;
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.Enter && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                // Ctrl+Enter to replace might be safer, but the user requested Enter
                // We will use Ctrl+Enter for multiline just in case, or just simple Enter if not focused on TextBox
                ReplaceAndClose();
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(txtPreview.Text);
            Close();
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            ReplaceAndClose();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ReplaceAndClose()
        {
            string modifiedText = txtPreview.Text;
            
            // ClipboardService operations are async to avoid blocking UI immediately
            await _clipboardService.ReplaceSelectedTextAsync(modifiedText);
            Close();
        }
    }
}
