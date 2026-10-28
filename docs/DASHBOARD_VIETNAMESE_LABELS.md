# Việt hóa báo cáo và nhãn tài liệu — 25/10/2026

## Thực hiện
- Views/Dashboard/Index.cshtml: giữ tiêu đề Báo cáo trực quan, bỏ phụ đề tiếng Anh; breadcrumb biểu đồ Tổng quan.
- Views/Dashboard/_ReportTabs.cshtml, ProblemDocuments.cshtml, Contract.cshtml, ContractDetails.cshtml: tab/title/nút quay lại bằng tiếng Việt; nhãn thiếu hợp đồng viết rõ.
- Models/OverviewModels.cs: ChartPoint bổ sung Label tùy chọn; thông báo bộ lọc Tổng quan bằng tiếng Việt.
- Services/DashboardService.cs: đọc DocumentName từ document_type theo các mã có trong kết quả chart và bổ sung Label. Category vẫn là mã, giữ nguyên GroupBy, COUNTROWS, tổng và khóa sắp xếp. Không join làm nhân bản row; không nhóm theo tên nên tên trùng vẫn là hai loại riêng.
- Services/DashboardService.Reports.cs: nhãn trạng thái thiếu hợp đồng viết đầy đủ, lựa chọn loại tài liệu dùng tên có sẵn trong DB. Không đổi điều kiện lọc hoặc công thức.
- wwwroot/js/dashboard.js: trục Y và tooltip dùng Label; tên dài wrap theo từ; chiều cao tính theo tên hiển thị. Bảng dữ liệu truy cập cũng dùng tên. Series không có điểm ở một loại có giá trị hiển thị 0, không sửa KPI BLANK hoặc dữ liệu API. Nếu không có loại nào vẫn hiển thị Không có dữ liệu.
- wwwroot/js/employee-hierarchy.js: breadcrumb tiếng Việt, không đổi drill-down.
- wwwroot/js/reports.js: đồng bộ màu theo nhãn trạng thái mới, series số lượng/tỷ lệ tiếng Việt.
- tests/Verification/Program.cs; document-chart-labels.cjs: kiểm thử render và frontend.

Không hardcode tên tài liệu. Nếu mã không tìm thấy tên trong danh mục, hiện Chưa có tên loại tài liệu, không suy đoán tên từ mã.

## Kiểm chứng
Build 0 lỗi/0 cảnh báo: build-dashboard-vietnamese.txt.
360 kiểm tra .NET đạt: test-dashboard-vietnamese.txt; các kiểm tra COUNTROWS/BLANK/DIVIDE và route hiện có vẫn đạt.
node --check cho dashboard.js, reports.js, employee-hierarchy.js đạt. Test frontend kiểm tra tên tiếng Việt, tooltip đủ bốn trạng thái/số 0, tên trùng không gộp mã, số liệu giữ nguyên.
Chưa đối chiếu nhãn trên SQL Server thật hoặc kiểm tra trực quan tên dài trong trình duyệt. Không đổi schema, Views hoặc dữ liệu thật. Các kiểm thử sử dụng fixture an toàn, không ghi database.
