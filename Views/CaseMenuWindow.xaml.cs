using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using VProofix.Services;
using Key = System.Windows.Input.Key; // Explicit namespace alias to resolve ambiguity

namespace VProofix.Views
{
    public partial class CaseMenuWindow : Window
    {
        private Action<string> _onOptionSelected;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        public CaseMenuWindow(Action<string> onOptionSelected)
        {
            InitializeComponent();
            _onOptionSelected = onOptionSelected;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (GetCursorPos(out POINT p))
            {
                // Convert screen coordinates to WPF coordinates
                var source = PresentationSource.FromVisual(this);
                if (source != null && source.CompositionTarget != null)
                {
                    var matrix = source.CompositionTarget.TransformFromDevice;
                    var wpfPoint = matrix.Transform(new System.Windows.Point(p.X, p.Y));
                    this.Left = wpfPoint.X - (this.Width / 2);
                    this.Top = wpfPoint.Y + 15; // slightly below cursor
                }
            }
            this.Activate();
            this.Focus();

            // Set focus to the first item for keyboard navigation
            if (ButtonPanel.Children.Count > 1 && ButtonPanel.Children[1] is Button firstBtn)
            {
                firstBtn.Focus();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            CloseWindow();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseWindow();
            }
            else if (e.Key == Key.U)
            {
                SelectOption("UPPER");
            }
            else if (e.Key == Key.D || e.Key == Key.L)
            {
                SelectOption("LOWER");
            }
            else if (e.Key == Key.C)
            {
                SelectOption("CAPITAL");
            }
            else if (e.Key == Key.S)
            {
                SelectOption("SENTENCE");
            }
        }

        private void BtnUpper_Click(object sender, RoutedEventArgs e) => SelectOption("UPPER");
        private void BtnLower_Click(object sender, RoutedEventArgs e) => SelectOption("LOWER");
        private void BtnCapitalize_Click(object sender, RoutedEventArgs e) => SelectOption("CAPITAL");
        private void BtnSentence_Click(object sender, RoutedEventArgs e) => SelectOption("SENTENCE");

        private bool _isClosing = false;

        private void SelectOption(string option)
        {
            if (_isClosing) return;
            _onOptionSelected?.Invoke(option);
            CloseWindow();
        }

        private void CloseWindow()
        {
            if (_isClosing) return;
            _isClosing = true;
            this.Deactivated -= Window_Deactivated;
            this.Close();
        }
    }
}
