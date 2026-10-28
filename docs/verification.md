# Kết quả kiểm tra — 15/10/2026

- Build cuối: thành công, 0 warning, 0 error (.NET SDK 10.0.402, EF Core 10.0.13).
- Đã mở Dashboard và form Create bằng trình duyệt localhost.
- Form trống hiển thị đủ hai lỗi bắt buộc bằng tiếng Việt.
- GET Setup, Create, 5 trang phân hệ, CSS/JS Tabler: HTTP 200.
- POST Create thiếu antiforgery token: HTTP 400.
- GET Divisions khi SQL lỗi: điều hướng Setup, HTTP 200.
- Scaffold trực tiếp: chưa thành công, SQL error 26 với ACER\SQLEXPRESS.
- Chưa kiểm thử CRUD trên database thật; không chạy schema, migration hoặc thay đổi dữ liệu.
- Server kiểm tra đã được dừng sau khi hoàn tất. Chạy lại theo README.md.
## Cập nhật phòng ban

- Đã thêm entity/mapping department, FK tới division (NO ACTION), form chọn đơn vị, CRUD và menu Phòng ban.
- Mã phòng ban unique; tên không áp đặt unique. Lỗi FK khi xóa có cán bộ được chuyển thành thông báo tiếng Việt.
- Build kiểm tra ở .scaffold/department-check: 0 warning, 0 error.
- Build vào bin/Debug/net10.0 chưa cập nhật được vì DLL đang được website chạy giữ khóa. Cần dừng server rồi build lại.
- Chưa thực hiện thêm/sửa/xóa dữ liệu thật để kiểm thử chức năng phòng ban.
- Người dùng xác nhận kết nối VTB2 hoạt động trong phiên website của mình.
