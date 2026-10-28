# Bảng 7 cột và nút panel trong sidebar — 25/10/2026

- Views/Shared/Grid.cshtml: chỉ với employees, chọn đúng cột theo nhãn tương ứng trong Headers rồi lấy Cells cùng index. Thứ tự: Mã cán bộ, Họ và tên, Số điện thoại, Email, Giới tính, Phòng ban, Thao tác. Không đổi truy vấn, Entity, form hoặc dữ liệu.
- Giữ Hồ sơ, Chi tiết, Sửa, Xóa dưới dạng chữ, quyền và route cũ; Xóa vẫn đi qua trang xác nhận. Không nút vuông/icon.
- Views/Shared/_Layout.cshtml: chuyển nút desktop vào toolbar ở góc trên bên phải sidebar. Nút mobile bên ngoài chỉ hiển thị dưới 992px để mở drawer. SVG panel đặt trực tiếp trong nút để không phụ thuộc sprite cache.
- wwwroot/css/workspace-layout.css: phân bổ lại 7 cột, mã/số điện thoại một dòng, font giữ nguyên, giới tính căn giữa, email/phòng ban được xuống dòng mà không cắt nội dung. Table min-width 980px; sticky Thao tác 205px giữ đủ 4 liên kết chữ. Sidebar compact vẫn có nút panel, logo không đổi tỷ lệ.
- wwwroot/js/sidebar.js: giữ localStorage và submenu; bổ sung điều khiển mobile riêng, chuyển focus về nút mở mobile khi đóng; tooltip desktop Thu gọn thanh bên/Mở rộng thanh bên.
- tests/Verification/Program.cs, sidebar.cjs: kiểm tra bảng đúng 7 header/7 cell, dữ liệu mã/tên tách riêng, các trường bị ẩn không xuất hiện trong bảng, thao tác dạng chữ, nút panel nằm bên trong aside; trạng thái sidebar mô phỏng tại 1920/1440/1366/1024/768/390px và khi localStorage không dùng được.

Kết quả: 340 kiểm tra .NET đạt, JavaScript syntax/state tests đạt. Build cuối xem build-employee-seven-columns.txt. Chưa kiểm tra trực quan kích thước màn hình bằng trình duyệt hoặc thao tác CRUD trên SQL thật. Không sửa schema, Views, Dashboard, upload hoặc bản in.
