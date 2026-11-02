# Lọc nhanh chất lượng trên Danh sách cán bộ

## Thay đổi
Bảng 7 cột: Mã cán bộ, Họ và tên, Số điện thoại, Email, Giới tính, Phòng ban, Thao tác. Chỉ bỏ EmploymentStatus khỏi cột hiển thị; form, chi tiết, dữ liệu và bộ lọc Trạng thái làm việc giữ nguyên. Giới tính dùng Labels.Text như chữ thường, null/rỗng là --, cả Nam/Nữ cùng kiểu ô và căn giữa.

Chú giải là liên kết GET nhỏ, có viền/nền nhẹ khi chọn, aria-current/aria-label/title và hỗ trợ bàn phím tự nhiên. Chỉ chọn success hoặc error. Nhấn lại màu đang chọn hoặc Tất cả bỏ lọc; links đặt page=1 và giữ các điều kiện khác. Form tìm kiếm giữ quality qua hidden input; Filter.Routes giữ quality trên phân trang. Xóa bộ lọc chỉ còn route kind, loại bỏ quality và các điều kiện khác. Không thêm JavaScript.

## Lọc trước phân trang
CatalogService.List<Employee> áp dụng Employees(f) (mã/tên, đơn vị, phòng ban) và employment_status trước. Khi quality được chọn, EmployeeQualityIndicators.Matching lấy danh sách ID trong toàn bộ phạm vi này, đánh giá theo lô 256 ID bằng Load/Evaluate đang dùng cho chấm màu. Mỗi lô 6 truy vấn, không truy vấn cho từng cán bộ. Không viết lại điều kiện chất lượng trong SQL khác.

Chỉ ID có cùng State được giữ; tập matched IDs được áp lại vào IQueryable employee trước CountAsync, OrderBy/Skip/Take. Page được clamp theo tổng đã lọc. Indicators đã đánh giá được tái dùng khi render, tránh tính lại khác kết quả. Unknown không thuộc xanh/đỏ nhưng vẫn xuất hiện khi không lọc chất lượng. Không tải tập đánh giá toàn bộ xuống trình duyệt.

Chi phí: chọn màu cần đánh giá toàn bộ phạm vi tìm kiếm trên server; số lô tăng theo số cán bộ. Chưa benchmark SQL thật. Đọc nhiều query không phải một snapshot transaction, nên nếu dữ liệu thay đổi đồng thời giữa các truy vấn thì lượt tải tiếp theo có thể khác; không thay đổi cơ chế giao dịch chung.

## File
Models/WorkspaceModels.cs
Services/CatalogService.cs
Services/EmployeeQualityIndicators.cs (chỉ thêm orchestration Matching, Evaluate không đổi)
Views/Shared/Grid.cshtml
Views/Shared/_Filters.cshtml
Views/Shared/_EmployeeQualityLegend.cshtml
wwwroot/css/workspace-layout.css
wwwroot/css/employee-quality.css
tests/Verification/Program.cs

## Kiểm chứng và giới hạn
Build log: build-quality-filter.txt. Test log: test-quality-filter.txt.
Kiểm thử route giữ chất lượng và trạng thái làm việc độc lập, toggle/reset page, giữ scope, whitelist. Render fixture Nam/Nữ/rỗng, 7 cột, bộ lọc làm việc còn nguyên, hidden quality, aria-current, link có bộ lọc và page=1. Regression Evaluate và hợp đồng lịch sử dùng lại kiểm thử cũ.
Chưa xác minh thao tác trên trình duyệt, responsive trực quan, tổng/phân trang trên SQL Server thật hoặc hiệu năng dữ liệu lớn. Không sửa dữ liệu nhân sự để tạo fixture. Không thay đổi View/schema/rule/KPI, chất lượng gốc, tài liệu lịch sử hay các CRUD.

Kết quả cuối: dotnet build 0 lỗi/0 cảnh báo; regression 434 passed, 0 failed. Các route có thể trả lỗi SQL có kiểm soát trong test host; đây không phải xác nhận số liệu SQL thực tế.
