# UI refinement V3 — 20/10/2026

## Phạm vi
Chỉ sửa Razor/CSS và cấu hình trình bày ECharts; không sửa Controller, DashboardService, KPI, truy vấn, cơ sở dữ liệu, Power BI, CRUD hoặc logic bộ lọc/drill. Dữ liệu thử trong tests không được đưa lên website.

## Đối chiếu Power BI
Đã đọc page.json và position của mọi visual trên ba trang, đối chiếu POWERBI_ANALYSIS.md. Tọa độ đầy đủ được lưu tại docs/ui-v3-source-layout.json (chỉ là bản trích xuất, không sửa PBIP).
- Executive: cột nhân viên tại x58/y298 (cao410), donut x58/y743.5 (cao445); tài liệu x1027.5/y297.5 (cao890). Vì vậy dùng hai cột gần 1:1: nhân viên và donut xếp bên trái, tài liệu bên phải; bảng bên dưới.
- Problem: hai chart x62/y312 và y772, rộng857.5/858, cao422.5/426; bảng sắp hết hạn bên phải x990. Web giữ hai chart bên trái, bảng bên phải, bảng vấn đề bên dưới. KPI giữ hàng riêng để đọc tốt trên laptop, không sao chép canvas 2000px.
- Contract: donut x98/y380 và bar x98/y768 rộng710; bảng vấn đề x900/y322 rộng996. Giữ lưới 5/7, donut và bar bên trái; bảng hết hạn bên dưới.

## Kích thước và khả năng đọc
- KPI min-height120px, padding16px, nhãn14px, giá trị34px (32px mobile); tự tăng nếu dữ liệu dài, không cắt số.
- Donut300px; cột nhân viên340px, tự tăng nếu nhãn nhiều dòng; ba chế độ và cuộn ngang CSS được giữ nguyên.
- Thanh ngang tối thiểu320px, chiều cao tính từ số hạng mục và độ dài nhãn lớn nhất. Nhãn xuống dòng mỗi18 ký tự, không dùng ellipsis, không bỏ hạng mục. Các chart nhiều loại tài liệu/phòng ban có thể cao hơn430px bên trong vùng cuộn.
- Vùng cuộn chart báo cáo phụ tối đa430px; tài liệu Executive tối đa760px trên desktop,430px mobile. Tiêu đề card nằm ngoài vùng cuộn.
- Bảng tối đa460px, header sticky; giữ tìm kiếm/sắp xếp/phân trang cũ.
- Gap16px; card radius12px, border nhẹ; heading16px. Tooltip richText confine, legend giữ scroll để không chồng các mục dài; không có dataZoom. ResizeObserver hiện có được giữ.
- >=1200px giữ bố cục hai cột;1024px và mobile xếp chart theo một cột. KPI Executive6 cột từ1440px,3 từ768px,2 từ480px; báo cáo phụ auto-fit,2 cột dưới768px,1 dưới480px. Đây là kiểm tra cấu hình CSS, chưa phải xác nhận bằng ảnh chụp.

## Kiểm thử và giới hạn
- dotnet build: xem docs/build-ui-v3.txt (output riêng .scaffold/ui-v3-review để tránh DLL website cũ bị khóa).
- Các test Node: document-chart-layout, employee-hierarchy, report-chart-layout, kpi-formatting, overview-multiselect, document-drill-navigation, report-navigation. Đạt; kiểm tra production JS với fixture cô lập, gồm35 nhãn dài,30 loại tài liệu, không mất series/hạng mục, drill/back và filter context.
- Test .NET: xem docs/verification-ui-v3.txt. TestServer kiểm tra route/auth/render và SQL translation; không tương đương kiểm tra số liệu SQL trực tiếp.
- Thử chạy Edge headless bằng Playwright thất bại khi khởi động (crashpad TransactNamedPipe0x6D; SiteList error-5). Chưa kiểm tra trực quan1920/1440/1366/1024/mobile; chưa xác nhận tránh chồng tooltip/legend trong trình duyệt với dữ liệu thực tế.
- Không xác nhận lại kết nối SQL/số KPI thực tế trong lần chỉnh giao diện này. Không ghi vào DB.

## File thay đổi
Views/Dashboard/Index.cshtml, ProblemDocuments.cshtml, Contract.cshtml;
wwwroot/css/dashboard.css, reports.css;
wwwroot/js/dashboard.js, employee-hierarchy.js, reports.js;
tests/Verification/document-chart-layout.cjs, report-chart-layout.cjs;
docs/ui-v3-source-layout.json và tài liệu/kết quả kiểm tra V3.

## Xem phiên bản cập nhật
Dừng debug website cũ trong Visual Studio, mở D:\KLTN\Web\Vtb.Hr.Web.csproj, Build rồi F5 theo profile http. Mở /Dashboard, /Dashboard/ProblemDocuments, /Dashboard/Contract trên địa chỉ launch profile; Ctrl+F5 trong trình duyệt nếu đang giữ tài nguyên cũ. Không cần thay đổi csproj vì SDK tự nhận Razor/CSS/JS hiện có.

Kết quả đã nhận: build cuối thành công0warning/0error; bộ kiểm tra .NET199passed/0failed;7 bộ kiểm tra JavaScript đạt. Kiểm tra .NET chạy trước điều chỉnh breakpoint cuối từ lg sang xl; build cuối đã bao gồm breakpoint này. CRUD trên SQL thật chưa được chạy lại.
