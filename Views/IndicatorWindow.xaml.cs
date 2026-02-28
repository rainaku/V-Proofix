using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace VProofix.Views
{
    public partial class IndicatorWindow : Window
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public uint GradientColor;
            public int AnimationId;
        }

        [DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        public IndicatorWindow()
        {
            InitializeComponent();
        }

        [DllImport("dwmapi.dll")]
        internal static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            var windowHelper = new WindowInteropHelper(this);
            var handle = windowHelper.Handle;

            try
            {
                // Set corner preference for Win 11 handle
                int cornerPreference = 2; // DWMWCP_ROUND
                DwmSetWindowAttribute(handle, 33, ref cornerPreference, sizeof(int));

                // Add WS_EX_NOACTIVATE (0x08000000) and WS_EX_TOOLWINDOW (0x00000080)
                const int GWL_EXSTYLE = -20;
                const int WS_EX_NOACTIVATE = 0x08000000;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                int extStyle = GetWindowLong(handle, GWL_EXSTYLE);
                SetWindowLong(handle, GWL_EXSTYLE, extStyle | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);
            }
            catch { /* Ignore if OS doesn't support */ }
        }

        public void SetStatus(string text, string subText = "")
        {
            if (text == "Fixed!" || text == "Đã sửa xong!" || text == "Đã hoàn thành!")
            {
                txtStatus.Text = "Đã hoàn thành!";
                txtSubStatus.Text = subText;
                txtSubStatus.Visibility = string.IsNullOrEmpty(subText) ? Visibility.Collapsed : Visibility.Visible;
                var successAnim = (Storyboard)FindResource("SuccessAnimation");
                successAnim.Begin();
            }
            else if (text.StartsWith("Lỗi") || text.StartsWith("Chưa") || text.StartsWith("Văn bản"))
            {
                txtStatus.Text = text;
                txtSubStatus.Text = subText;
                txtSubStatus.Visibility = string.IsNullOrEmpty(subText) ? Visibility.Collapsed : Visibility.Visible;
                var errorAnim = (Storyboard)FindResource("ErrorAnimation");
                errorAnim.Begin();
            }
            else
            {
                txtStatus.Text = text;
                txtSubStatus.Text = subText;
                txtSubStatus.Visibility = string.IsNullOrEmpty(subText) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public void SetSubStatus(string text)
        {
            txtSubStatus.Text = text;
            txtSubStatus.Visibility = string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
        }

        public async Task CloseAnimatedAsync()
        {
            var closeAnim = (Storyboard)FindResource("WindowCloseAnimation");
            var tcsClose = new TaskCompletionSource<bool>();
            closeAnim.Completed += (s, e) =>
            {
                this.Close();
                tcsClose.TrySetResult(true);
            };
            closeAnim.Begin();

            await tcsClose.Task;
        }
    }
}
