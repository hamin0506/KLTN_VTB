# Sidebar và bảng cán bộ — 25/10/2026

## Thay đổi
- Views/Shared/_Layout.cshtml: dùng sidebar hiện có, thêm nút header, nút đóng mobile, backdrop, nhãn/tooltip; riêng Danh sách cán bộ dạng fluid. Route/menu hiện có giữ nguyên.
- Views/Shared/Grid.cshtml: chỉ bảng Kind employees có style riêng và cột Thao tác sticky; 4 thao tác được biểu diễn bằng icon có title/aria-label, giữ nguyên links/quyền.
- wwwroot/css/workspace-layout.css (mới): desktop 260/72px bằng biến CSS chung; mobile drawer dưới 992px; bảng min-width 1180px, padding nhỏ, hover, cột phải có nền đặc/z-index thấp. Không ẩn cột dữ liệu.
- wwwroot/js/sidebar.js (mới): nhớ vtb.sidebar.compact trong localStorage, fallback mở rộng nếu storage bị chặn; flyout submenu bằng details, Escape/nhấn ngoài đóng; mobile có backdrop, inert khi đóng và giữ focus trong drawer. Phát resize để biểu đồ hiện có cập nhật kích thước.
- wwwroot/icons.svg: thêm các icon panel/edit/trash/settings theo kiểu SVG hiện có.
- tests/Verification/Program.cs và sidebar.cjs: kiểm tra render, links và trạng thái sidebar.

## Nguyên nhân bảng rộng
Container cũ bị giới hạn max-width, padding ô lớn, 11 cột dữ liệu bao gồm cả điện thoại/email/quê quán cộng 4 nút chữ. Không có cột Đơn vị hoặc Ngày sinh trong dữ liệu bảng hiện tại; giữ nguyên danh sách cột thực tế. Các trường dài được xuống dòng đầy đủ, không ellipsis/cắt nội dung. Tại desktop không đủ 1180px vùng bảng (ví dụ sidebar mở ở laptop nhỏ), vẫn cần cuộn dữ liệu; cột thao tác sticky luôn nằm mép phải vùng bảng. Đây là lựa chọn giữ đủ dữ liệu và độ đọc được, không ẩn thanh cuộn hoặc cột.

## Kiểm chứng
- Build thành công 0 lỗi/0 cảnh báo: build-sidebar.txt.
- 337 kiểm tra .NET đạt: test-sidebar.txt; gồm render employee table/classes và toàn bộ thao tác; không ghi dữ liệu SQL.
- node --check sidebar.js thành công. sidebar.cjs kiểm tra trạng thái tại 1920, 1440, 1366, 1024, 768, 390px và khi storage bị chặn. Đây là DOM/media-query mô phỏng, không phải kiểm tra hình ảnh hay đo bố cục trình duyệt.
- Mở localhost:5082 bằng công cụ trình duyệt bị timeout. Chưa xác nhận trực quan sáu kích thước, hover/tooltip thực tế, sticky khi cuộn hoặc tìm kiếm/phân trang với SQL thật. Logic truy vấn/bộ lọc/phân trang không sửa.
- Không đổi schema, Views, CRUD, upload hoặc DashboardService.
