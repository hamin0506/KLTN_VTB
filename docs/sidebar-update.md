# Cập nhật sidebar — 16/10/2026

- Tổng quan dùng trang Secure Embed Power BI và không truy vấn các KPI C# cũ. Thống kê hiển thị trong báo cáo sau khi cấu hình PowerBI:EmbedUrl; hiện URL chưa có.
- Giữ nhóm Quản lý tổ chức (Đơn vị, Phòng ban).
- Danh sách cán bộ là link trực tiếp, bỏ mục Hồ sơ cán bộ độc lập. Liên kết hồ sơ từng cán bộ trong bảng vẫn giữ.
- Danh sách tài liệu là link trực tiếp. Quản lý danh mục loại tài liệu bằng nút bên trong trang; vẫn có thêm/sửa/xóa theo quyền.
- Chất lượng dữ liệu mở danh sách requirement_rule; có thêm/sửa/xóa/chi tiết. Xóa yêu cầu có xác nhận và quyền Admin, chống CSRF. Không thực hiện xóa thử trên database thật.
- Bỏ menu Power BI riêng vì đã đưa vào Tổng quan. Các route báo cáo chất lượng cũ được giữ để không làm hỏng liên kết cũ, nhưng không còn xuất hiện trong sidebar.
- Giữ nguyên các mức độ BAT_BUOC/TUY_CHON/CO_DIEU_KIEN và validation của bảng; không tự chuyển dữ liệu thành BAT_BUOC.

Build kiểm tra ở .scaffold/final-review thành công, 0 lỗi và 0 cảnh báo. Chạy D:\KLTN\Web\Chay-Website.cmd để build bản hiện tại và mở tại http://localhost:5082. Dừng tiến trình cũ trước khi chạy. Không tự khởi động thêm server trong phiên này.
