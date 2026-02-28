namespace VProofix.Helpers
{
    public static class L
    {
        public static string CurrentLanguage = "English";

        public static string Working => CurrentLanguage == "Vietnamese" ? "V-Proofix đang làm việc..." : "V-Proofix is working...";
        public static string Init => CurrentLanguage == "Vietnamese" ? "Khởi tạo..." : "Initializing...";
        public static string NoTextSelected => CurrentLanguage == "Vietnamese" ? "Chưa bôi đen đoạn văn bản cần sửa" : "No text selected for fixing";
        public static string TextTooLong => CurrentLanguage == "Vietnamese" ? "Văn bản quá dài" : "Text is too long";
        public static string MaxWords(int count) => CurrentLanguage == "Vietnamese" ? $"Tối đa 600 từ (hiện tại: {count})" : $"Maximum 600 words (current: {count})";
        public static string Completed => CurrentLanguage == "Vietnamese" ? "Đã hoàn thành!" : "Completed!";
        public static string Done => CurrentLanguage == "Vietnamese" ? "Đã xong" : "Done";
        public static string Timeout => CurrentLanguage == "Vietnamese" ? "Kết nối bị quá hạn (Timeout)" : "Connection timed out";
        public static string SysError => CurrentLanguage == "Vietnamese" ? "Lỗi hệ thống!" : "System error!";
        public static string ErrMissingKey => CurrentLanguage == "Vietnamese" ? "Lỗi: Chưa thiết lập API Key" : "Error: API Key is missing";
        public static string ErrInvalidKey => CurrentLanguage == "Vietnamese" ? "Lỗi: API Key không hợp lệ" : "Error: Invalid API Key";
        public static string ErrRateLimit => CurrentLanguage == "Vietnamese" ? "Lỗi: Đã hết lượt dùng (Rate Limit)" : "Error: Rate Limit Exceeded";
        public static string Fixed => CurrentLanguage == "Vietnamese" ? "Đã sửa xong!" : "Fixed!";
        
        public static string CallingApi(string model) => CurrentLanguage == "Vietnamese" ? $"Đang gọi API ({model})..." : $"Calling API ({model})...";
        public static string Analyzing(int chars) => CurrentLanguage == "Vietnamese" ? $"Đang phân tích {chars} ký tự..." : $"Analyzing {chars} characters...";
        public static string Fixing(int chars) => CurrentLanguage == "Vietnamese" ? $"Đang sửa {chars} ký tự..." : $"Fixing {chars} characters...";
        public static string FixingPercent(int chars, int pct) => CurrentLanguage == "Vietnamese" ? $"Đang sửa {chars} ký tự ({pct}%)..." : $"Fixing {chars} characters ({pct}%)...";

        // Settings Window
        public static string SettingsTitle => CurrentLanguage == "Vietnamese" ? "Cài đặt V-Proofix" : "V-Proofix Settings";
        public static string UILanguage => CurrentLanguage == "Vietnamese" ? "Ngôn ngữ giao diện:" : "UI Language:";
        public static string TargetLanguageUI => CurrentLanguage == "Vietnamese" ? "Ngôn ngữ fix:" : "Target Language:";
        public static string FixHotkey => CurrentLanguage == "Vietnamese" ? "Phím tắt Sửa:" : "Fix Hotkey:";
        public static string PreviewHotkey => CurrentLanguage == "Vietnamese" ? "Phím tắt Xem thử:" : "Preview Hotkey:";
        public static string PromptFormatUI => CurrentLanguage == "Vietnamese" ? "Định dạng mẫu Prompt:" : "Prompt Format:";
        public static string CheckPreview => CurrentLanguage == "Vietnamese" ? "Hiện cửa sổ Xem thử trước khi thay thế (Tùy chọn)" : "Show Preview window before replacing (Optional)";
        public static string CheckPrivacy => CurrentLanguage == "Vietnamese" ? "Chế độ riêng tư (Không tải lịch sử)" : "Privacy Mode (Do not save history)";
        public static string BtnExit => CurrentLanguage == "Vietnamese" ? "Thoát App" : "Exit App";
        public static string BtnCancel => CurrentLanguage == "Vietnamese" ? "Hủy" : "Cancel";
        public static string BtnSave => CurrentLanguage == "Vietnamese" ? "Lưu" : "Save";
    }
}
