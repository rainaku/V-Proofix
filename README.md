# V-Proofix: Công cụ Sửa Lỗi Chính Tả & Ngữ Pháp Tích Hợp AI

<p align="center">
  <img src="Resources/logo.png" width="128" alt="V-Proofix Logo" />
</p>

V-Proofix là một ứng dụng desktop chuyên nghiệp dành cho hệ điều hành Windows, được thiết kế để tối ưu hóa quy trình hiệu đính văn bản. Thông qua việc tích hợp các mô hình ngôn ngữ lớn (LLM) tiên tiến từ Google Gemini, ứng dụng cung cấp khả năng sửa lỗi ngữ pháp, chính tả và tinh chỉnh văn phong một cách tức thời ngay trên các trình soạn thảo văn bản phổ biến.

---

## Tính Năng Chính

*   **Hiệu Đính AI Đa Ngôn Ngữ:** Sử dụng sức mạnh của Google Gemini (mặc định Gemini 2.0 Flash) để phân tích ngữ cảnh và đưa ra các đề xuất sửa đổi chính xác.
*   **Tích Hợp Hệ Thống Sâu:** Hoạt động thông qua phím tắt toàn hệ thống, cho phép xử lý văn bản trực tiếp trong bất kỳ ứng dụng nào (Word, Browser, IDE, Slack, v.v.) mà không cần sao chép thủ công.
*   **Giao Diện Liquid Glass:** Thiết kế theo phong cách hiện đại với hiệu ứng Glassmorphism, mang lại trải nghiệm thẩm mỹ cao và không làm gián đoạn không gian làm việc.
*   **Cơ Chế Kiểm Soát Preview:** Cung cấp cửa sổ so sánh (Diff) trực quan giữa văn bản gốc và văn bản đã sửa trước khi quyết định áp dụng.
*   **Bảo Mật Dữ Liệu:** API Key được mã hóa bằng DPAPI (Windows Data Protection API), đảm bảo thông tin cá nhân chỉ được lưu trữ và truy cập cục bộ.
*   **Quản Lý Lịch Sử:** Lưu trữ cục bộ các phiên hiệu đính để người dùng có thể đối chiếu và tra cứu lại khi cần thiết.

---

## Cấu Hình Phím Tắt Hệ Thống

Ứng dụng hỗ trợ hai chế độ hoạt động chính thông qua các tổ hợp phím có thể tùy chỉnh:

| Chế độ | Phím tắt mặc định | Mô tả |
| :--- | :--- | :--- |
| **Fix Now** | `Ctrl + Alt + F` | Tự động sửa lỗi và ghi đè trực tiếp vào vùng văn bản đang chọn. |
| **Preview Fix** | `Ctrl + Alt + P` | Hiển thị cửa sổ so sánh song song để người dùng kiểm tra trước khi xác nhận thay đổi. |

---

## Yêu Cầu Hệ Thống & Cài Đặt

### Yêu Cầu
*   Hệ điều hành: Windows 10/11 (64-bit).
*   Môi trường: .NET 8.0 Runtime.
*   Kết nối: Yêu cầu kết nối Internet để gọi API từ Google Gemini.

### Hướng Dẫn Cài Đặt
1.  Tải bản phát hành mới nhất từ mục [Releases](https://github.com/rainaku/V-Proofix/releases).
2.  Giải nén và thực thi `VProofix.exe`.
3.  Truy cập **Settings** từ khay hệ thống (System Tray).
4.  Cấu hình **Gemini API Key** (có thể khởi tạo miễn phí tại [Google AI Studio](https://aistudio.google.com/)).

---

## Kiến Trúc Kỹ Thuật

V-Proofix được xây dựng trên nền tảng công nghệ hiện đại, đảm bảo sự ổn định và hiệu suất:

*   **Ngôn ngữ:** C# / XAML (WPF).
*   **Framework:** .NET 8.0.
*   **AI Integration:** Google Gemini API qua giao thức REST.
*   **Automation:** Sử dụng Windows UI Automation để tương tác với các ứng dụng bên thứ ba.
*   **Security:** System.Security.Cryptography.ProtectedData để bảo mật thông tin nhạy cảm.

---

## Giấy Phép (License)

Dự án được phân phối dưới giấy phép **MIT License**. Mọi chi tiết vui lòng tham khảo file `LICENSE` trong thư mục gốc.

---

## Liên Hệ & Đóng Góp

Chúng tôi luôn hoan nghênh các đóng góp từ cộng đồng nhằm cải thiện chất lượng sản phẩm.
*   Báo lỗi hoặc yêu cầu tính năng: [Issues](https://github.com/rainaku/V-Proofix/issues).
*   Tác giả: **rainaku**
