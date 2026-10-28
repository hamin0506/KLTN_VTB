# Tối ưu hai Dashboard — 26/10/2026

## Bố cục
- Trước: hai biểu đồ xếp chồng bên trái, bảng dài bên phải; bộ lọc chia nhiều hàng; biểu đồ thanh tối thiểu 320px, donut 390px.
- Sau: Chất lượng hồ sơ đặt biểu đồ thiếu theo loại cạnh tỷ lệ đáp ứng theo phòng ban (50/50). Hợp đồng đặt cơ cấu trạng thái cạnh số nhân viên cần xử lý theo phòng ban (40/60). Tất cả bảng giữ nguyên và đặt dưới biểu đồ.
- Bộ lọc cùng một hàng khi vùng nội dung >=950px; xuống 3/2/1 cột theo không gian thực tế. KPI 3 hoặc 4 cột trên desktop, thu nhỏ padding, tối thiểu 100px.
- Thanh 12px; chiều cao nội dung theo số danh mục và số dòng nhãn, tối thiểu 240px; viewport tối đa 320px cuộn nội bộ. Donut 280px. Không thêm dataZoom.
- Bảng cuộn nội bộ tối đa 420px, tiêu đề sticky, vẫn giữ tìm kiếm/sắp xếp/phân trang và tất cả dòng.
- Container query dưới 800px chuyển biểu đồ thành một cột, phụ thuộc chiều rộng còn lại sau sidebar. ResizeObserver hiện có tiếp tục resize từng ECharts khi container thay đổi; không thêm listener.

## Phạm vi
Chỉ sửa Views/Dashboard/ProblemDocuments.cshtml, Contract.cshtml, _ReportFilters.cshtml, wwwroot/css/reports.css, wwwroot/js/reports.js và kỳ vọng chiều cao trong tests/Verification/report-chart-layout.cjs. Không sửa Services, truy vấn, database, màu, KPI, series, sự kiện drill-through, hay logic bộ lọc.

## Kiểm tra
- dotnet build: thành công, 0 cảnh báo, 0 lỗi; docs/build-compact-reports.txt.
- Node --check reports.js: thành công.
- report-chart-layout.cjs: giữ đủ 35 danh mục dài và giá trị, nguyên từ, tooltip confined, không dataZoom; biểu đồ ít dữ liệu 240px.
- report-navigation.cjs: giữ trạng thái drill-through, nhiều bộ lọc, phân trang/tìm kiếm/sắp xếp và đường quay lại.
- Chưa kiểm tra trực quan trình duyệt/SQL thực tế: không có listener localhost:5082 khi kiểm tra. Các độ rộng 1920/1440/1366/1024/768/390 và hai trạng thái sidebar chưa được xác nhận bằng ảnh chụp/trình duyệt; CSS được thiết kế theo chiều rộng container, không coi rà soát CSS là kiểm thử trực quan đạt.
