# Chất lượng hồ sơ: bố cục và drill-down

## Thay đổi
- Cuộn dọc cũ là HTML: viewport max-height 320px + overflow:auto, trong khi chart cao theo số danh mục. Không phải dataZoom.
- Riêng trang problem bỏ max-height/overflow của viewport. Chiều cao mới: max(150, số danh mục × max(24, số dòng nhãn lớn nhất ×16 +8) +54). Thanh 12px. Không Top N, không bỏ danh mục, không đổi màu/thứ tự biểu đồ hồ sơ thiếu.
- Nhãn ngắt theo từ, độ dài dòng dựa trên chiều rộng card (vùng nhãn tối đa 270px). Tooltip giữ tên đầy đủ và số/tỷ lệ tiếng Việt. ResizeObserver hiện có gọi tính lại khi chiều rộng đổi, không tạo thêm observer.
- Bộ lọc desktop: Cán bộ/Loại hồ sơ rộng hơn; giữ toàn bộ widget, tìm kiếm, thứ tự Requirement Rule và logic lựa chọn.
- KPI tối thiểu 84px, nhãn và số gần nhau, giữ màu và format.
- Hai biểu đồ cạnh nhau; dưới 800px chiều rộng nội dung chuyển một cột; không ép chiều cao bằng nhau.
- Hai bảng nằm trong card Danh sách hồ sơ cần theo dõi, mặc định Tài liệu có vấn đề. Tab dùng hidden thực sự, chỉ một bảng chiếm diện tích. Hỗ trợ bàn phím trái/phải/Home/End. Không tạo lại đối tượng Table nên search/page/sort mỗi bảng được giữ, bộ lọc chung không đổi. Hai bảng vẫn tải theo cơ chế hiện có.

## Tổng hợp tổ chức
ProblemOrganizationQuery bắt đầu từ chính ReportRequirements(f), giữ filter context (bao gồm quan hệ document_id hiện có). Join employee/department/division bằng ID, nhóm theo ID + tên. Mỗi phòng ban trả Total/Missing đếm theo đúng hai trạng thái nhân viên hiện tại. Không gửi hồ sơ nhân sự chi tiết để tính chart.
Cấp đơn vị: (SUM(Total)-SUM(Missing))/SUM(Total), mẫu số 0 trả 0 theo RequirementRate hiện có; không lấy trung bình tỷ lệ. Nhấn thanh chỉ đổi cấp hiển thị trong chart, không gán vào bộ lọc chung để tránh thay đổi filter context. Quay lại xem các đơn vị còn trong phạm vi. Khi lọc loại bỏ đơn vị đang drill, tự quay về cấp đơn vị. Nút Xóa bộ lọc cũng đặt lại cấp đơn vị.
KPI/biểu đồ thiếu giữ nguyên truy vấn; không sửa dữ liệu/schema/View/rule. Trang Hợp đồng và drill-through hợp đồng giữ nguyên.

## File
Models/ReportModels.cs; Services/DashboardService.Reports.cs; Views/Dashboard/ProblemDocuments.cshtml; wwwroot/css/reports.css; wwwroot/js/reports.js; wwwroot/js/problem-report.js (mới); tests/Verification/Program.cs; tests/Verification/problem-report.cjs (mới).

## Kiểm chứng và giới hạn
- Build: docs/build-problem-drilldown.txt.
- C# suite: docs/test-problem-drilldown.txt; có kiểm tra ToQueryString của truy vấn tổ chức với/không có các bộ lọc.
- Node kiểm tra cú pháp và module thực tế: giữ 35 danh mục/giá trị 0, tỷ lệ tổng hợp .99 thay vì trung bình .5, nhấn đơn vị/quay lại, đổi phạm vi, tab, tính lại kích thước ở các chiều rộng 1920/1440/1366/1024/768/390. Đây là mock DOM, không xác nhận responsive trực quan.
- Không có listener localhost:5082 khi kiểm tra. Chưa quan sát/chụp ảnh trước-sau, chưa đo chiều cao trang hoặc số lần cuộn; chưa xác nhận nhãn, tooltip và sidebar trên trình duyệt thực tế. Không tuyên bố phần trăm giảm chiều dài trang: danh mục nhiều sẽ làm chart cao hơn để hiển thị đủ dữ liệu.
- Chưa đối chiếu KPI hoặc thao tác lọc/phân trang với SQL thật. Không ghi dữ liệu nhân sự để thử nghiệm.
