using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace VProofix.Views
{
    public partial class SummaryWindow : Window
    {
        private bool _isClosing = false;

        public SummaryWindow(string summaryText)
        {
            InitializeComponent();
            txtSummary.Markdown = summaryText;
            
            this.Loaded += (s, e) => 
            {
                txtSummary.Focus();
            };
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseWindowWrapper();
                e.Handled = true;
            }
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (e.Delta / 3.0));
                e.Handled = true;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // Close the summary when the user clicks elsewhere
            CloseWindowWrapper();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindowWrapper();
        }

        private void CloseWindowWrapper()
        {
            if (_isClosing) return;
            _isClosing = true;

            try
            {
                // Optional: Fade out animation
                var sb = new System.Windows.Media.Animation.Storyboard();
                var da = new System.Windows.Media.Animation.DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(150)));
                System.Windows.Media.Animation.Storyboard.SetTargetProperty(da, new PropertyPath("Opacity"));
                sb.Children.Add(da);

                sb.Completed += (s, args) => { this.Close(); };
                this.BeginStoryboard(sb);
            }
            catch
            {
                this.Close();
            }
        }
    }
}
