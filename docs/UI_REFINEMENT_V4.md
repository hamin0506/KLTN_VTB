# UI refinement V4

## Thay đổi
- Executive mặc định mode division (Theo khối), Razor và JavaScript thống nhất active/aria-pressed. Giữ chế độ all/detail và dữ liệu nhóm theo ID thật. Dropdown khối chỉ hiện khi detail; chuyển detail chưa có lựa chọn không tự chọn khối.
- overview-layout.js quan sát chiều cao cột trái và viewport bên phải. Trên desktop>=1200px, card phải bằng chiều cao cả cột trái; viewport flex dùng diện tích còn lại sau header. ECharts cao bằng max(chiều cao nội dung, viewport), do đó lấp đầy card hoặc cuộn nếu nhiều hạng mục. Legend đặt cuối biểu đồ, chừa khoảng trục dưới. Khi màn hình nhỏ, khôi phục chiều cao theo dữ liệu.
- Sidebar bỏ icon tạm. _Layout kiểm tra file images/vietinbank-logo.png bằng WebRootFileProvider; có file thì hiện logo và tên ứng dụng, chưa có thì chỉ hiện tên, không có ảnh lỗi hoặc logo giả. object-fit:contain giữ tỷ lệ, nền trắng giữ khả năng nhìn trên sidebar tối. Layout hiện tại chỉ thu gọn menu trên mobile, không có chế độ sidebar desktop thu thành thanh icon.

## File
Views/Dashboard/Index.cshtml; Views/Shared/_Layout.cshtml;
wwwroot/js/employee-hierarchy.js, dashboard.js, overview-layout.js;
wwwroot/css/dashboard.css, site.css;
wwwroot/images/README.md;
tests/Verification/employee-hierarchy.cjs, document-chart-layout.cjs, overview-layout.cjs.

## Logo còn chờ
Đặt logo chính thức tại D:\KLTN\Web\wwwroot\images\vietinbank-logo.png. Tải lại trang để hiện; asp-append-version chống cache ảnh cũ. Không có file logo chính thức trong project ở thời điểm sửa.

## Kiểm tra
Node tests mặc định division, chuyển all/detail, drill/back, giữ các series/hạng mục, nhãn nguyên từ và chiều cao desktop/mobile đạt. Build xem build-ui-v4.txt. Chưa kiểm tra trực quan căn đáy/resize/logo trong trình duyệt với dữ liệu SQL thật. Không sửa Service, SQL, KPI hay CRUD.
