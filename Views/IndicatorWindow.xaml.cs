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

        public IndicatorWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            try
            {
                // Enable Windows generic background blur for this layered window
                var windowHelper = new WindowInteropHelper(this);
                var accent = new AccentPolicy { AccentState = 3 }; // 3 = ACCENT_ENABLE_BLURBEHIND

                var accentStructSize = Marshal.SizeOf(accent);
                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttributeData
                {
                    Attribute = 19, // WCA_ACCENT_POLICY
                    SizeOfData = accentStructSize,
                    Data = accentPtr
                };

                SetWindowCompositionAttribute(windowHelper.Handle, ref data);
                Marshal.FreeHGlobal(accentPtr);
            }
            catch { /* Ignore if OS doesn't support */ }
        }

        public void SetStatus(string text)
        {
            if (text == "Fixed!" || text == "Đã sửa xong!")
            {
                txtStatus.Text = "Đã hoàn thành!";
                var successAnim = (Storyboard)FindResource("SuccessAnimation");
                successAnim.Begin();
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
