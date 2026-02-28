# V-Proofix Changelogs

## Phiên bản 1.1.0

### Tính năng mới (New Features)
- Bổ sung hệ thống khóa bàn phím và chuột hoàn toàn bằng Global API Hook (InputBlocker) nhằm ngăn người dùng chuyển tab (Alt+Tab) hoặc vô tình click chuột làm mất focus đoạn text trong lúc App chờ AI trả lời định dạng lại.
- Tự động thay thế tên API thực tế của Model AI chuyển thành tên hiển thị chuyên nghiệp (`gemma-3-27b-it` -> `Gemma 3 27B`, `gemini-2.5-flash` -> `Gemini 2.5 Flash`) để tăng tính thẩm mỹ.
- Cảnh báo: Tự động chặn và từ chối gọi API nếu đoạn văn bản bôi đen vượt quá giới hạn **600 từ**, tiết kiệm tài nguyên rate limit API vô ích.

### Cải thiện UI/UX (UI Improvements)
- Điều chỉnh kích cỡ Hộp thoại trạng thái UI to và sắc nét hơn. Thêm hiệu ứng phát sáng (Glow Xanh biển nhạt) quanh Icon Vector và dòng chữ Text.
- Cải thiện luồng tiến trình UI: Thay vì bắt user đợi thời gian mù (chỉ gọi API chay), giờ đây ứng dụng chỉ "Đang gọi API" tròn 1 giây đầu tiên, tất cả thời gian ping mạng còn lại đều được ẩn đi và giả lập thành "Đang xử lý XYZ ký tự..." tạo cảm giác xử lý local tốc độ rất cao.
- Điều chỉnh và nâng cấp lại toàn bộ nội dung Status và Warning cho hợp lý, tinh gọn hơn.

### Hệ thống & Tối ưu (Under the Hood)
- Đổi Default Model thành Phiên bản trí tuệ thông minh mới nhất: Google `Gemma 3 27B` (Mô hình mở thế hệ thứ 3 của Google thay vì `gemini-2.5-flash`).
- Nâng cấp độ ưu tiên fallback Model. Nếu Gemma hết lượt, app có thể thử Gemini 2.5 Flash, rồi Gemini 2.0 Flash...
- Làm trống hoàn toàn code Logic tính RPM, RPD thủ công (do Google không trả về số liệu này). Tối giản hóa thời gian khởi tạo dịch vụ.
