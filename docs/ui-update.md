# Cập nhật giao diện và chức năng — 16/10/2026

Source được sửa trực tiếp trong D:\KLTN\Web, không tạo project mới.

- Đổi tên hiển thị thành Quản lý hồ sơ cán bộ; bỏ slogan, footer khóa luận, tiêu đề phụ và đoạn giới thiệu thừa.
- Báo cáo trực quan tại /Dashboard, dùng Secure Embed và PowerBI:EmbedUrl. Home/Index, / và /PowerBI chuyển hướng về đó. Khi chưa có URL, chỉ hiện thông báo ngắn. Chưa kiểm thử báo cáo Power BI thật vì URL chưa được cấu hình.
- Danh sách loại tài liệu trỏ /Catalog?kind=types, truy vấn dbo.document_type; cột STT theo trang, mã, tên, nhóm, mô tả, thao tác. Giữ validation, tìm mã/tên, lọc nhóm, phân trang, xử lý trùng và chặn xóa khi có tham chiếu.
- Route danh sách document tổng hợp chuyển về loại tài liệu. Các thao tác document trong hồ sơ cán bộ vẫn giữ; bổ sung liên kết sửa/xóa trong bảng tài liệu cán bộ và quay về hồ sơ sau lưu/xóa.
- Chevron SVG cùng bộ icon, xoay 90 độ trong 180ms. Giữ bàn phím native details/summary, đồng bộ aria-expanded qua sự kiện toggle; hỗ trợ reduced motion.
- Không sửa schema, SQL View, không thử INSERT/UPDATE/DELETE trên VTB2.

Build kiểm tra .scaffold/final-review: 0 lỗi, 0 cảnh báo sau restore lại thành công. Log kiểm thử: verification-ui.txt. Các kiểm thử chạy trong TestServer cách ly, không chứng minh CRUD thật hoặc đối chiếu dữ liệu SQL thành công. Chưa kiểm thử tương tác và kích thước màn hình bằng trình duyệt thật trong lần này.

Chạy trong Visual Studio: dừng website cũ trước, mở Vtb.Hr.Web.sln, Build/Rebuild rồi chạy profile http. Hoặc chạy Chay-Website.cmd; script tự build Debug. Địa chỉ localhost:5082 sẽ chuyển tới /Dashboard. Không tự chạy thêm server trong phiên chỉnh sửa này.

Kết quả kiểm thử cuối: 82 passed, 0 failed. Bao gồm redirect route cũ, tên hiển thị, markup chevron, phân quyền và render Razor.
