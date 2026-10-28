# Dashboard Hợp đồng lao động — bố cục và cấp tổ chức

## Trước/sau
Trước: donut/thanh chưa đồng đều, viewport thanh max-height 320px cuộn dọc; hai bảng nối tiếp. Sau: bộ lọc, 4 KPI gọn, hai card cân bằng chiều ngang/chiều cao, card Danh sách hợp đồng cần theo dõi có hai tab. Mặc định Nhân viên cần xử lý, tab thứ hai Hợp đồng đã hết hạn. Chỉ một panel chiếm chiều cao, giữ đối tượng bảng/search/page/sort riêng, không đổi bộ lọc chung.

## Chiều cao và typography
Tính chiều cao thanh theo số danh mục và dòng nhãn, tối thiểu 200px, thanh 12px. Desktop: chiều cao chung = max(chiều cao tự nhiên + header/controls/chi tiết) của hai card; mỗi vùng ECharts nhận chiều cao còn lại sau trừ phần điều khiển. Donut căn giữa với bán kính pixel từ min(chiều rộng, chiều cao), không kéo giãn hình tròn. Khi nhiều phòng ban, cả hai card tăng chiều cao, không Top N/dataZoom/cuộn dọc. Một cột trên mobile dùng chiều cao tự nhiên riêng.
Font lấy từ getComputedStyle của website; nhãn/legend 12px, tooltip 13px. Dùng lại ResizeObserver đang có và kiểm tra chiều rộng để tránh render lặp. Mở phần nút drill-through thu gọn cũng tính lại chiều cao.

## Ba chế độ
Theo đơn vị mặc định; Tất cả phòng ban; Chi tiết đơn vị với dropdown và nút quay lại. Click thanh đơn vị mở chi tiết. Danh mục trục dùng ID, tên phòng ban không ghép đơn vị nhưng tooltip có tên đơn vị. Không gộp các phòng ban trùng tên. Giữ chế độ khi lọc còn hợp lệ, quay về đơn vị nếu đơn vị đang chọn không còn trong phạm vi.

## Nghiệp vụ
ContractOrganizationQuery lấy ReportContracts(f), giữ hai trạng thái CAN_KY_KET_TIEP/THIEU_HDXDTH1, join employee/department/division theo khóa và đếm COUNT(DISTINCT employee_id) từng phòng ban. Cấp đơn vị cộng các phòng ban: mỗi employee có duy nhất một department_id qua PK employee nên không thể xuất hiện ở hai phòng ban trong kết quả. Không join hợp đồng/tài liệu để đếm nhiều lần.
Không thay đổi KPI, truy vấn donut, quy tắc trạng thái, expiredContracts/contractIssues, điều kiện hiện hành/lịch sử. Donut vẫn dùng mã trạng thái và callback drill-through cũ, palette cũ. Legend giữ nguyên, các nút điều hướng trạng thái vẫn truy cập được trong mục thu gọn.

## File thay đổi
- Models/ReportModels.cs
- Services/DashboardService.Reports.cs
- Views/Dashboard/Contract.cshtml
- wwwroot/css/reports.css
- wwwroot/js/reports.js
- wwwroot/js/contract-report.js (mới)
- tests/Verification/Program.cs
- tests/Verification/contract-report.cjs (mới)

## Kết quả
Build 0 lỗi/0 cảnh báo (build-contract-layout.txt). 392 kiểm thử C# đạt (test-contract-layout.txt), gồm SQL translation COUNT DISTINCT, ID tổ chức và bộ lọc có tham số. Node syntax check đạt; contract-report.cjs, problem-report.cjs và report-navigation.cjs đạt. Kiểm tra module thực với mock DOM: 3 chế độ, trùng tên khác ID, 35 danh mục không bị cắt, tooltip, font, chiều cao, dữ liệu/màu/mã donut, tab, resize.
Không có listener localhost:5082 lúc kiểm tra. Chưa xác nhận trực quan ở 1366/1440/1920 hoặc mobile/sidebar thật, chưa đo chiều dài trang. Không coi mock DOM là kiểm chứng nhãn không chồng trên trình duyệt. Chưa đối chiếu KPI/đếm nhân viên/tìm kiếm/phân trang với SQL thật; không tạo hay sửa dữ liệu nhân sự để kiểm thử.
