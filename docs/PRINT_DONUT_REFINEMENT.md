# Mẫu danh mục A4 và tổng giữa donut

## Phạm vi và file
- wwwroot/css/profile-print.css: CSS chỉ áp dụng .document-catalog, named page document-catalog.
- wwwroot/js/donut-total.js: helper dùng chung, tính tổng các nhóm biểu đồ và graphic text không nhận sự kiện.
- wwwroot/js/dashboard.js: tổng Tổng tài liệu, cập nhật khi nhận dữ liệu/resize, xóa tổng khi lỗi tải.
- wwwroot/js/contract-report.js: tổng Tổng nhân viên, theo center/radius thực của donut; cập nhật cùng render/filter/resize.
- Views/Dashboard/Index.cshtml và Contract.cshtml: nạp helper; tiêu đề hợp đồng rút gọn (kể cả aria-label).
- tests/Verification/contract-report.cjs; tests/Verification/print-donut.cjs.
- Không sửa backend, KPI, SQL, CRUD, lịch sử hoặc markup/dữ liệu của mẫu lý lịch.

## Danh mục A4
A4 dọc, named page margin 9mm; mẫu lý lịch vẫn giữ margin 12mm 15mm cũ. Nội dung danh mục Times New Roman 9.5pt; bảng 9pt/line-height 1.12, padding 1px 3px, viền .5pt. Tiêu đề 14pt, logo rộng 36mm với tỷ lệ SVG giữ nguyên; chữ ký tối thiểu 19mm. Header và chữ ký tránh ngắt giữa khối, từng hàng bảng tránh cắt; dữ liệu dài vẫn được sang trang, không overflow hidden/scale/cắt dòng. Giữ ba lựa chọn in cũ, danh mục có trang riêng khi in toàn bộ, bỏ break-before khi in riêng. Không thay đổi cột/dòng, cách đếm tài liệu hay dấu hình thức lưu trữ.

Đã thử xuất PDF bằng Chrome headless với HTML render từ Razor trong test host và fixture kích thước 13+7+14 dòng; Chrome không tạo PDF, tiến trình GPU exit -1073741790 và lỗi quyền/mã hóa trong môi trường chạy. Vì vậy CHƯA xác nhận số trang bằng Print Preview/PDF. Fixture nằm trong .scaffold/print-donut-preview, chỉ dùng thử bố cục, không ghi vào database. Cần kiểm tra bản in thật với printer margin hỗ trợ 9mm, tắt header/footer trình duyệt. Trường hợp nhiều dòng/tên dài được giữ đủ và có thể sang trang thứ hai.

## Nguồn số tổng
Tổng quan: sum(value) của documentDistribution do Overview trả về. Dữ liệu hiện tại đi qua DocumentMeasureScope, EmployeeScope và IsKpiScope, GROUP BY DocumentCode/QualityStatus rồi nhóm theo DocumentState. Mỗi dòng nguồn được phân loại một lần; tổng chính là mẫu số các lát cắt ECharts, bao gồm nhóm trống nếu có. Không COUNT(*) bảng document, không đổi COUNTROWS thành DISTINCTCOUNT. Nếu View thực tế bị nhân bản bản ghi so với snapshot, đây vẫn là số đếm đang dùng của biểu đồ; chưa đối chiếu số tài liệu duy nhất trên server.

Hợp đồng: sum(value) của second do ContractData trả về. Nguồn ReportContracts là vw_labor_contract_check; theo VTB2.sql đã đọc, contract_status GROUP BY employee_id, department_id (employee có một department), CASE xuất một labor_contract_status/cán bộ. ContractData giữ filter hiện có, loại ChamDutHDLD/NghiHuu như biểu đồ cũ, nhóm mỗi cán bộ một trạng thái. Vì vậy các nhóm loại trừ nhau, không đếm số bản ghi hợp đồng hoặc lịch sử. Không tự thêm/bớt trạng thái, bao gồm Không cảnh báo nếu nguồn trả về.

Graphic dùng chính center 50%/43% (Tổng quan), 50%/45% (Hợp đồng), cỡ chữ theo bán kính trong và độ dài số, silent=true. Không có lớp HTML chặn chuột. Tổng rỗng là 0. Tổng phản ánh tất cả nhóm thuộc phạm vi lọc, không thay đổi chỉ vì người dùng ẩn một mục legend. Màu, phần trăm, tooltip, state/code và click handlers drill-through giữ nguyên.

## Kiểm thử
- dotnet build thành công: 0 warning, 0 error (docs/build-print-donut.txt).
- Regression .NET: 399 passed, 17 failed (docs/test-print-donut.txt). Cả 17 liên quan route truy cập dữ liệu trả 500: EF InvalidOperationException bọc SqlException “instance requires encryption but this machine does not support it”. Không thay cấu hình encryption/validation để che lỗi và không coi những kiểm thử này là đạt.
- Node print-donut: đạt tổng nhóm, dữ liệu rỗng, tọa độ/cỡ chữ/silent, CSS phạm vi danh mục và tiêu đề.
- Node contract-report: đạt 3 chế độ, tổng động, màu/code, tab, chiều cao, resize từ 390 đến 1920 (mock DOM).
- Node report-navigation: đạt drill/back thực tế với trạng thái động và bảo toàn filter/page/search/sort.
- JS syntax: dashboard.js, contract-report.js, donut-total.js đạt.
- Chưa kiểm chứng thao tác chuột/tooltip/drill trực quan trên website thật, số trang in, hoặc tổng khi áp dụng bộ lọc trên SQL thật. Không tuyên bố số liệu đã đối chiếu Power BI.
