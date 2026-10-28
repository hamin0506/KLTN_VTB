# Sửa nguyên nhân tràn chữ bảng cán bộ — 25/10/2026

## Nguyên nhân đã tìm thấy trong mã nguồn
CatalogService.List dùng chung options với dropdown để tạo text cho mọi field. EmployeeId vì thế biến thành "mã — họ tên"; DepartmentId biến thành "đơn vị / phòng ban". Ô mã CSS chỉ 100px và nowrap khiến chuỗi ghép tràn sang ô họ tên. Các kiểm thử trước chỉ dùng cell tĩnh nên chưa bắt được lỗi ánh xạ này.
CSS cột trước đây trộn width trên th (px và %) với min-width ở ô thao tác, còn phòng ban không có ngân sách chiều rộng tối thiểu rõ ràng. Toolbar thu gọn cũng có rule căn giữa ở trạng thái compact thay vì sát phải.

## Sửa
- Services/CatalogService.cs: EmployeeListValue dùng EmployeeId/FullName nguyên bản; lấy DepartmentName theo ID phòng ban của các dòng trong trang. Chỉ thay dữ liệu trình bày bảng, không đổi dropdown form hoặc bộ lọc. Truy vấn bổ sung đọc tên phòng ban theo ID, không ghi dữ liệu.
- Views/Shared/Grid.cshtml: 7 col trong colgroup đồng bộ với 7 th/td; giữ Hồ sơ, Chi tiết, Sửa, Xóa bằng chữ.
- wwwroot/css/workspace-layout.css: width cột chỉ đặt tại colgroup (110/180/125/250/85/linh hoạt/230px), table min-width 1160px để phòng ban có ít nhất 180px. Không ẩn/truncate chữ, không dùng absolute/margin âm cho ô. Chỉ Thao tác sticky có nền đặc và z-index thấp; màn hình hẹp cuộn trong table-responsive.
- Sidebar container là flex column, padding ngang container bằng 0; toolbar một hàng riêng padding phải 10px ở cả trạng thái mở/thu. Logo vẫn căn giữa hàng riêng.
- tests/Verification/Program.cs: thêm hồi quy cho mã nguyên bản, tên nguyên bản, phòng ban không kèm đơn vị.

## Kiểm chứng
- Build thành công: 0 lỗi, 0 cảnh báo (build-employee-overlap.txt).
- 343 kiểm tra .NET đạt (test-employee-overlap.txt); không dùng hồ sơ thật để ghi thử.
- JavaScript syntax và state tests đạt tại các độ rộng mô phỏng 1920, 1440, 1366, 1024, 768, 390px; localStorage bị chặn vẫn hoạt động.
- Không có listener tại localhost:5082 khi kiểm tra. Chưa xác nhận trực quan sidebar mở/thu hoặc bảng cuộn tại các kích thước yêu cầu. Không tuyên bố lỗi hiển thị đã được xác minh hoàn toàn trong trình duyệt, cũng chưa đối chiếu dữ liệu SQL thực tế.
- Không sửa schema, Views, Model, CRUD, Dashboard, upload hoặc mẫu in.
