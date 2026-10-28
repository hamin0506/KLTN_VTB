# Trạng thái mới nhất — hai Dashboard đã có mã nguồn thực tế

Ngày19/10/2026. Phần này thay thế trạng thái cũ “chưa triển khai” trong lịch sử phía dưới.

## Đã triển khai

- /Dashboard/ProblemDocuments: view/action/service/API/JS/CSS, 3KPI/2bar/2table, đủ bộ lọc và2textbox nguồn.
- /Dashboard/Contract: view/action/service/API/JS/CSS, 4KPI/donut/bar/2table, đủ bộ lọc và2textbox nguồn.
- /Dashboard/ContractDetails?State=CAN_KY_KET_TIEP: drill table5cột, state động, Back giữ EmployeeIds/DivisionIds/DepartmentIds và page/search/sort từng bảng Contract; không có tab độc lập.
- Ba tab _ReportTabs đều là liên kết thật. Executive Overview chỉ thay partial nav. Drill-through tài liệu giữ nguyên.
- API mới [Authorize], whitelist hiện tại, GET read-only; EF tham số; tables SQL search/sort/paging tối đa100; dropdown cán bộ tìm trên server tối đa50kết quả/lần + lựa chọn hiện có.
- COUNTROWS/COUNT/DISTINCTCOUNT và filter riêng từng visual phân biệt; requirements lọc qua document_id active, không kích hoạt quan hệ inactive. Xem DASHBOARD_VALIDATION.md và docs/REPORTS_SOURCE_MAP.md.
- Build thành công0lỗi/0cảnh báo;197/197 TestServer/SQLtranslation/validation/auth/regression đạt. Kiểm thử JS drill/back đạt; báo cáo render HTML trong docs/report-render-*.html. Không đưa dữ liệu giả vào ứng dụng.

## File chính

Models/ReportModels.cs; Services/DashboardService.Reports.cs; Controllers/DashboardController.cs;
Views/Dashboard/{ProblemDocuments,Contract,ContractDetails,_ReportTabs,_ReportFilters,_ReportTables}.cshtml;
Views/Dashboard/Index.cshtml(chỉ nav); wwwroot/js/reports.js; wwwroot/css/reports.css;
tests/Verification/Program.cs; tests/Verification/report-navigation.cjs; docs/REPORTS_SOURCE_MAP.md và log.

## Cần xác minh trên máy chạy thật

- SQL VTB2: sqlcmd báo error locating instance/timeout từ môi trường công cụ. Tất cả giá trị KPI/PowerBI/chênh lệch Chưa xác minh; không tuyên bố số liệu đã chính xác.
- Đã thử Kestrel cổng5097 và nhận Application started; HTTP từ sandbox vẫn bị socket policy/timeout; Edge headless bị lỗi pipe/permission. Chưa kiểm tra browser responsive hoặc đầy đủ dữ liệu end-to-end.
- Thao tác gửi Ctrl+C tới phiên thử bị kiểm duyệt tự động chặn vì sandbox approval không được phép. Phiên dùng .scaffold/final-review và cổng5097; không thay profile http5082. Không khẳng định đã dừng được phiên thử.
- Cần đối chiếu cùng snapshot Power BI và SQL các KPI, null/blank, selection giữa các slicer và quan hệ bothDirections.
- CRUD regression routes/auth/render đã qua; chưa thực hiện ghi database thật.

## Chạy bản mới

Mở D:\KLTN\Web\Vtb.Hr.Web.csproj trong Visual Studio, Rebuild, chạy profile http (http://localhost:5082). Mở /Dashboard/ProblemDocuments và /Dashboard/Contract. Chọn trạng thái trên donut Contract để mở /Dashboard/ContractDetails?State=<mã thực tế>. Không cần tạo project/migration hoặc sửa DB.

---

## Lịch sử (trạng thái cũ, không dùng để kết luận hiện tại)
# Trạng thái mới nhất — yêu cầu ba chế độ / Problem documents / Contract

Ngày 19/10/2026. Ưu tiên trạng thái trong phần này khi nội dung lịch sử bên dưới khác.

## Đã hoàn thành mã nguồn A–B của yêu cầu mới

- Mặc định tất cả phòng ban; nút Theo khối; nút Chi tiết khối và dropdown ID; Back về các khối; chuyển nhanh về tất cả.
- ID division/department được trả từ DashboardService, không dùng tên làm khóa cột/drill. Nhãn chỉ tên phòng ban, tooltip có khối. Cộng số theo cùng nguồn và scope, không thay Measures.
- CSS horizontal scroll khi nhiều cột, chart min-width theo số category, không dataZoom.
- Xóa hoàn toàn dataZoom của biểu đồ tài liệu, tăng chiều cao theo số loại, giữ stacked bars/all categories/4 trạng thái chuẩn, giữ BLANK nếu thực có.
- Build0 lỗi/0 cảnh báo; regression130/130; Node tests3modes/document chart/KPI formatting/document drill đều đạt. Chưa xác minh số SQL/Power BI thật hoặc browser responsive.

## Chưa hoàn thành C–E — bắt đầu tiếp từ Problem documents

Chưa tạo trang/API/service Problem documents hoặc Contract. Hai tab vẫn disabled; không có route Contract drill. Không được báo đã triển khai các KPI mới chỉ vì đã đọc nguồn.

1. Problem documents: trang 9c302b54668d117a421c, 3KPI/2bar/2table/3slicer/2textbox; tất cả query/filter đã đọc lại. Cần service/DTO, API read-only auth, Razor, JS và table pagination/search/sort. Giữ quan hệ requirement.document_id -> document active; không kích hoạt các quan hệ employee/type/department inactive. Type slicer giới hạn lựa chọn nhóm Hồ sơ gốc/Quyết định-Văn bản khác, không tự áp điều kiện lựa chọn lên page khi không chọn. Không áp is_kpi_scope lên hai bảng khi metadata không có. Đối chiếu measure COUNTROWS và DIVIDE(total-missing,total,0), 2chữ số phần trăm. Build/test trước bước2.
2. Contract: trang ef5809a1010218ccd3ab, 4KPI/2chart/2table/2slicer/2textbox. COUNT employee_id ở3card, COUNTROWS ởcard HĐ sắp hết hạn, DISTINCTCOUNT ởchart phòng ban, COUNTNONNULL ởdonut. Giữ filter riêng bảng và donut; không gộp trạng thái hoặc sao chép is_kpi_scope sang visual không có. Dùng nguyên trạng thái SWITCH trong TMDL.
3. Drill hợp đồng: trang04cce6d3fe0432219dcb/table cbc3b38ec88834cd0a76, 5cột employee_id/full_name/employee.Trạng thái nhân viên/department_name/Trạng thái, full_name ASC, không filter visual riêng. State runtime từ donut; Back và URL context. Không hardcode trạng thái saved Hợp lệ, không thêm tab riêng. Không tự kế thừa employment_status filter nội bộ donut vào page nếu chỉ là visual filter.
4. Hoàn thiện/kiểm thử các dropdown tìm kiếm, phân trang, auth và CRUD regression. Chưa sửa component dropdown hiện có ngoài yêu cầu chart.
5. Browser thực + SQL + Power BI snapshot vẫn chưa có bằng chứng; đánh dấu Chưa xác minh cho mọi KPI chưa đối chiếu.

File A–B: Models/OverviewModels.cs, Services/DashboardService.cs, Views/Dashboard/Index.cshtml, wwwroot/js/employee-hierarchy.js, wwwroot/js/dashboard.js, wwwroot/css/dashboard.css. Test và log tại tests/Verification, docs. Chi tiết trong phần cuối DASHBOARD_VALIDATION.md.

Nguồn vẫn D:\KLTN\powerbi\POWERBI_ANALYSIS.md và PBIR/TMDL gốc. Không sửa Power BI/database. Không chạy website nền để tránh khóa DLL; Rebuild profile http trong Visual Studio để xem mã mới.

---

## Lịch sử các lượt trước
# DASHBOARD IMPLEMENTATION PROGRESS

Cập nhật: 18/10/2026. Project: D:\KLTN\Web\Vtb.Hr.Web.csproj.

## Đã thực hiện trong lượt này — giai đoạn A

Hoàn tất phần mã nguồn Executive Overview và drill-through trạng thái tài liệu; kiểm chứng số liệu thực còn chờ SQL/Power BI Desktop. Không tuyên bố đã hoàn thành toàn bộ A–E.

- Đọc lại PBIR/semantic model, đối chiếu trường và ngữ cảnh lọc của trang chi tiết.
- Điều hướng 3 báo cáo chính; Problem documents và Contract mới là vị trí chờ, chưa bấm được.
- Click donut mở chi tiết theo mã trạng thái, có đường truy cập bằng nút trong bảng dữ liệu biểu đồ.
- Bảng chi tiết đúng 11 trường, tìm kiếm, sắp xếp và phân trang phía server; mở hồ sơ cán bộ qua mã nhân viên.
- Giữ bộ lọc và trạng thái bảng tổng quan khi quay lại; không dùng iframe/Power BI Service.
- Restore thành công, build 0 warning/0 error; 130 kiểm thử đạt. Node kiểm tra cú pháp và navigation đạt.

## File tạo mới

- Models/DocumentDrillModels.cs
- Services/DashboardService.Drill.cs
- Views/Dashboard/DocumentDetails.cshtml
- wwwroot/js/document-drill.js
- tests/Verification/document-drill-navigation.cjs
- docs/verification-stage-a.txt, docs/verification-drill-navigation.txt
- docs/dashboard-drill-query-{STATE}.txt (5 SQL translation, không chứa dữ liệu cán bộ)
- DASHBOARD_IMPLEMENTATION_PROGRESS.md

## File cập nhật

- Services/DashboardService.cs (partial, mã trạng thái trong dữ liệu biểu đồ)
- Models/OverviewModels.cs (ChartPoint.Code)
- Controllers/DashboardController.cs (2 GET route bảo vệ bằng xác thực)
- Views/Dashboard/Index.cshtml (điều hướng, màu, drill URL)
- wwwroot/js/dashboard.js (drill, context URL, nhãn phần trăm)
- Services/Labels.cs (thiếu hồ sơ màu vàng)
- tests/Verification/Program.cs
- DASHBOARD_VALIDATION.md
- Các log query/render/API được bộ kiểm thử cập nhật lại.

Project SDK tự nhận các file C#/Razor/JS mới; không cần thêm Compile Include vào csproj. Không tạo project mới hoặc đổi Program/DI vì DashboardService đã được đăng ký.

## Tiếp theo — B

Triển khai Problem documents theo trang PBIR 9c302b54668d117a421c. Đọc lại toàn bộ 12 visual trước khi viết mã. Có 3 card, 2 bar chart, 2 bảng, 3 slicer, 2 textbox.

Cần đặc biệt giữ đường lọc requirement view qua document_id -> document (active); các quan hệ trực tiếp employee/type/department là inactive. Không tự lọc trực tiếp requirement theo employee để khắc phục dòng NULL document_id. Tổng yêu cầu và thiếu dùng COUNTROWS; tỷ lệ DIVIDE(total-missing,total,0), giữ BLANK subtraction/alternate result. Slicer loại tài liệu có filter giới hạn lựa chọn; không tự áp lên toàn trang khi chưa chọn. Build và test xong B mới làm C.

## Tiếp theo — C

Contract theo ef5809a1010218ccd3ab. Giữ COUNT(employee_id), DISTINCTCOUNT ở chart phòng ban và COUNTNONNULL ở donut đúng từng visual. Filter trạng thái lao động và mã hợp đồng khác nhau giữa card/table/chart. Drill hợp đồng theo 04cce6d3fe0432219dcb: 5 cột, sort fullname ASC, trạng thái runtime, giữ context khi Back. Không tạo thêm tab drill. Build và test sau C.

## Tiếp theo — D

Tích hợp trang 379c1a9213063047d527 vào /Profiles/Details/{id}; không tạo tab hồ sơ mới. 12 card (7 thuộc tính employee, COUNTNONNULL document_id toàn bộ lịch sử và 4 số đếm chất lượng), 2 bảng. Measure tài liệu hợp lệ không có is_kpi_scope, ba Measure khác có. Bảng lịch sử không có điều kiện is_current được lưu trong PBIR; không thêm điều kiện theo suy đoán. Giữ các thao tác CRUD hiện có. Build và test sau D.

## Tiếp theo — E / xác minh còn thiếu

- Đọc được SQL thật từ môi trường chạy ứng dụng với Windows Authentication.
- So sánh từng KPI, bảng, chart, slicer trên cùng snapshot Power BI Desktop/SQL và cùng context; ghi chênh lệch. Hiện tất cả là Chưa xác minh.
- Kiểm tra tương tác cross-highlight/relationship hai chiều, responsive và keyboard trong browser thực. Kiểm thử Node hiện tại không thay thế browser layout.
- Kiểm tra CRUD end-to-end trên dữ liệu kiểm thử được phép; harness hiện tại không ghi DB thực.
- Nguồn chuẩn: D:\KLTN\powerbi\POWERBI_ANALYSIS.md và PBIR/TMDL; không chỉnh các nguồn này.

## Mở bản cập nhật

Dừng phiên website cũ nếu còn đang chạy; mở Vtb.Hr.Web.sln hoặc Vtb.Hr.Web.csproj trong Visual Studio, Rebuild rồi chạy profile http. Vào /Dashboard, bấm một phần biểu đồ chất lượng hoặc mở bảng dữ liệu biểu đồ và chọn Xem chi tiết. Lượt này build vào .scaffold/final-review để không khóa DLL Debug; cần Rebuild khi mở Visual Studio. Không có tiến trình website nền do lượt này khởi chạy.

## Cập nhật riêng theo yêu cầu UI Executive Overview

Đã thay 3 bộ lọc bằng dropdown nhiều lựa chọn không thêm thư viện; màu KPI/biểu đồ/badge đồng bộ, không hợp lệ tím và hết hạn đỏ; bỏ slider ngang employee-chart. Chi tiết file và kiểm thử tại phần cuối DASHBOARD_VALIDATION.md. Không triển khai thêm giai đoạn B–E trong yêu cầu chỉnh giao diện này. Cần Rebuild rồi chạy lại profile http để nhận Razor/JS mới; asset có asp-append-version.

## Cập nhật drill-down biểu đồ nhân viên — 19/10/2026

Đã hoàn thành mã nguồn hai cấp khối/phòng ban trong Executive Overview, sử dụng dữ liệu tổng hợp từ GET /Dashboard/Data hiện có. Có click cột/nhãn, chọn bằng bàn phím, breadcrumb và Back giữ filter ngoài. Script mới wwwroot/js/employee-hierarchy.js; sửa dashboard.js, Dashboard/Index.cshtml và dashboard.css. Build 0 lỗi/0 cảnh báo, regression130/130 và kiểm thử drill-down đạt. Còn kiểm chứng dữ liệu SQL/Power BI thật, số khối thực tế và browser responsive. Chi tiết tại phần cuối DASHBOARD_VALIDATION.md. Không triển khai thêm Problem documents/Contract trong lượt này.


