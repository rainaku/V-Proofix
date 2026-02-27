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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            var windowHelper = new WindowInteropHelper(this);
            var handle = windowHelper.Handle;

            try
            {
                // Set corner preference for Win 11 handle - still good for the window frame itself if any
                int cornerPreference = 2; // DWMWCP_ROUND
                DwmSetWindowAttribute(handle, 33, ref cornerPreference, sizeof(int));

                // We are not using SetWindowRgn or AccentPolicy anymore
                // to allow WPF's DropShadowEffect to render naturally outside the border.
                // The window is already AllowsTransparency=True and Background=Transparent.
            }
            catch { /* Ignore if OS doesn't support */ }
        }

        public void SetStatus(string text)
        {
            if (text == "Fixed!" || text == "Đã sửa xong!" || text == "Đã hoàn thành!")
            {
                txtStatus.Text = "Đã hoàn thành!";
                var successAnim = (Storyboard)FindResource("SuccessAnimation");
                successAnim.Begin();
            }
            else if (text.StartsWith("Lỗi") || text.StartsWith("Chưa"))
            {
                txtStatus.Text = text;
                var errorAnim = (Storyboard)FindResource("ErrorAnimation");
                errorAnim.Begin();
            }
            else
            {
                txtStatus.Text = text;
            }
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
