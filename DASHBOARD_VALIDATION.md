# DASHBOARD_VALIDATION — Executive Overview

Ngày: 18/10/2026. Project hiện có: D:\KLTN\Web. Nguồn đối chiếu: PBIR trang 3321ff89b8aea65d9302, Measure.tmdl, relationships.tmdl và POWERBI_ANALYSIS.md.

## Kết quả kiểm chứng

- dotnet restore thành công; dotnet build .scaffold/final-review thành công: 0 lỗi, 0 cảnh báo.
- 130 kiểm thử TestServer/mapping/validation/SQL translation đạt, 0 thất bại. Log: docs/verification-stage-a.txt.
- Node kiểm tra cú pháp dashboard.js; kiểm tra trực quan bằng Edge headless đã thử nhưng trình duyệt bị đóng do lỗi quyền môi trường. Chưa xác minh responsive bằng browser thật.
- Truy vấn sqlcmd chỉ đọc tới ACER\SQLEXPRESS/VTB2 vẫn lỗi tìm instance/timeout trong phiên thực thi. Các API Data/Options/Missing trả JSON 503, không giả số liệu.
- Chưa có kết quả Power BI Desktop cùng thời điểm và bộ lọc. Tất cả số liệu: **Chưa xác minh**.
- Không sửa database, không chạy migration hoặc test ghi dữ liệu. Đã thêm drill-through chi tiết hồ sơ theo trạng thái. Problem documents, Contract và tích hợp hồ sơ cá nhân chưa triển khai trong lượt này.

## KPI và công thức gốc

| KPI | Giá trị website từ SQL | Power BI Desktop | Chênh lệch | Kiểm thử / kết luận |
|---|---|---|---|---|
| Tổng nhân viên | Chưa lấy được (SQL lỗi kết nối; không phải 0) | Chưa có | Chưa tính được | Công thức/translation kiểm tra; **Chưa xác minh** số liệu |
| Tỷ lệ nhân viên đủ hồ sơ | Chưa lấy được (SQL lỗi kết nối; không phải 0) | Chưa có | Chưa tính được | Công thức/translation kiểm tra; **Chưa xác minh** số liệu |
| Nhân viên thiếu hồ sơ | Chưa lấy được (SQL lỗi kết nối; không phải 0) | Chưa có | Chưa tính được | Công thức/translation kiểm tra; **Chưa xác minh** số liệu |
| Tài liệu sắp hết hạn | Chưa lấy được (SQL lỗi kết nối; không phải 0) | Chưa có | Chưa tính được | Công thức/translation kiểm tra; **Chưa xác minh** số liệu |
| Tài liệu đã hết hạn | Chưa lấy được (SQL lỗi kết nối; không phải 0) | Chưa có | Chưa tính được | Công thức/translation kiểm tra; **Chưa xác minh** số liệu |
| Tài liệu không hợp lệ | Chưa lấy được (SQL lỗi kết nối; không phải 0) | Chưa có | Chưa tính được | Công thức/translation kiểm tra; **Chưa xác minh** số liệu |
| Phụ thuộc: nhân viên có đủ hồ sơ | Chưa lấy được (SQL lỗi kết nối; không phải 0) | Chưa có | Chưa tính được | Công thức/translation kiểm tra; **Chưa xác minh** số liệu |
| Phụ thuộc: tổng tài liệu cho 2 biểu đồ | Chưa lấy được (SQL lỗi kết nối; không phải 0) | Chưa có | Chưa tính được | Công thức/translation kiểm tra; **Chưa xác minh** số liệu |

### Tổng nhân viên

```dax
CALCULATE(COUNTROWS(vw_employee_quality), vw_employee_quality[employment_status] IN {"DangLamViec", "TamHoanHDLD"})
```

EmployeeMeasureScope + COUNT_BIG; EmploymentStatus của View thuộc DangLamViec/TamHoanHDLD. DaxCount đổi kết quả đếm 0 thành null để giữ BLANK của COUNTROWS; không đổi thành DISTINCTCOUNT.

### Tỷ lệ nhân viên đủ hồ sơ

```dax
DIVIDE([Nhân viên có đủ hồ sơ], [Tổng nhân viên])
```

DaxDivide(CompleteEmployees,TotalEmployees); mẫu số 0/BLANK hoặc tử BLANK → null; hiển thị 1 chữ số thập phân phần trăm. DaxCount đổi kết quả đếm 0 thành null để giữ BLANK của COUNTROWS; không đổi thành DISTINCTCOUNT.

### Nhân viên thiếu hồ sơ

```dax
CALCULATE(COUNTROWS(vw_employee_quality), vw_employee_quality[employee_quality_status] = "THIEU_HO_SO", vw_employee_quality[employment_status] IN {"DangLamViec", "TamHoanHDLD"})
```

COUNT_BIG dòng vw_employee_quality có employee_quality_status=THIEU_HO_SO và trạng thái làm việc trong Measure. DaxCount đổi kết quả đếm 0 thành null để giữ BLANK của COUNTROWS; không đổi thành DISTINCTCOUNT.

### Tài liệu sắp hết hạn

```dax
CALCULATE(COUNTROWS(vw_document_quality), vw_document_quality[quality_status] = "Sap_Het_Han", vw_document_quality[is_kpi_scope]=1)
```

COUNT_BIG dòng vw_document_quality có quality_status=SAP_HET_HAN, is_kpi_scope=1. DaxCount đổi kết quả đếm 0 thành null để giữ BLANK của COUNTROWS; không đổi thành DISTINCTCOUNT.

### Tài liệu đã hết hạn

```dax
CALCULATE(COUNTROWS(vw_document_quality), vw_document_quality[quality_status] = "Da_Het_Han", vw_document_quality[is_kpi_scope] = 1)
```

COUNT_BIG dòng vw_document_quality có quality_status=DA_HET_HAN, is_kpi_scope=1. DaxCount đổi kết quả đếm 0 thành null để giữ BLANK của COUNTROWS; không đổi thành DISTINCTCOUNT.

### Tài liệu không hợp lệ

```dax
CALCULATE(COUNTROWS(vw_document_quality), vw_document_quality[quality_status] = "Khong_Hop_Le", vw_document_quality[is_kpi_scope] =1)
```

COUNT_BIG dòng vw_document_quality có quality_status=KHONG_HOP_LE, is_kpi_scope=1. DaxCount đổi kết quả đếm 0 thành null để giữ BLANK của COUNTROWS; không đổi thành DISTINCTCOUNT.

### Phụ thuộc: nhân viên có đủ hồ sơ

```dax
CALCULATE(COUNTROWS(vw_employee_quality), vw_employee_quality[employee_quality_status] = "DAY_DU", vw_employee_quality[employment_status] IN {"DangLamViec", "TamHoanHDLD"})
```

COUNT_BIG dòng vw_employee_quality có employee_quality_status=DAY_DU và trạng thái làm việc trong Measure. DaxCount đổi kết quả đếm 0 thành null để giữ BLANK của COUNTROWS; không đổi thành DISTINCTCOUNT.

### Phụ thuộc: tổng tài liệu cho 2 biểu đồ

```dax
CALCULATE(COUNTROWS(vw_document_quality), vw_document_quality[is_kpi_scope] =1)
```

COUNT_BIG dòng vw_document_quality có is_kpi_scope=1; GROUP BY nhãn trạng thái/code cho biểu đồ. Không dùng document lịch sử hoặc DISTINCT document_id. DaxCount đổi kết quả đếm 0 thành null để giữ BLANK của COUNTROWS; không đổi thành DISTINCTCOUNT.

## Filter context và relationship

Bộ lọc trạng thái nhân viên dùng employee.employment_status. Query EmployeeMeasureScope còn áp điều kiện employment_status trên vw_employee_quality như DAX; không thay thế bộ lọc trên bảng employee bằng điều kiện trong View. Bộ lọc division/department truyền tới employee rồi tới vw_employee_quality và vw_document_quality. Không dùng các quan hệ inactive của bảng requirement. Chỉ hai giá trị DangLamViec và TamHoanHDLD được cung cấp, đúng filterConfig slicer. Khi cả hai ô được chọn, UI gửi không có điều kiện trạng thái (tương ứng xóa lựa chọn slicer); chọn một ô gửi giá trị đó. Nếu bỏ chọn cả hai, UI trở lại không lọc.

Đơn vị dùng khóa định danh (division_name unique). Phòng ban trong PBIR là tên trong cây division/department; EmployeeScope mở rộng những phòng ban trùng tên trong cùng đơn vị để không vô tình tách một nhãn Power BI thành các nhóm khác nhau. Hai bộ lọc tổ chức giao nhau nếu cả hai được gửi. Không ép nhân viên phải có document chỉ vì relationship document–employee là hai chiều: chỉ truyền bộ lọc khi có context từ nhánh đó. Chưa cung cấp cross-highlight liên biểu đồ; tooltip, legend, zoom và slicer tương tác được lập trình. Sự ngang bằng runtime đường truyền lọc hai chiều vẫn cần Power BI Desktop để xác nhận.

Điều kiện CountRows nằm trong SELECT COUNT_BIG/conditional count; status SQL uppercase là giá trị thực do View trả về. Công thức gốc vẫn lưu đúng chữ hoa/thường. Nhãn SWITCH của trạng thái giữ BLANK cho mã không được ánh xạ. Không tự thêm KHONG_XAC_DINH thành trạng thái nghiệp vụ mới. UI ghi (Trống) cho BLANK, không dùng số 0.

Tìm kiếm, sort và page chỉ áp dụng API Missing, không đổi KPI/biểu đồ. Các query EF Core có tham số; sort được allowlist. Danh sách tối đa 100 dòng/trang. Không gửi toàn bộ hồ sơ nhân sự về client. Dữ liệu biểu đồ được GROUP BY phía SQL trước khi chuyển nhãn/series; không lấy toàn bộ document về C#.

## Tái tạo Visual

| Visual gốc | Nguồn / Measure / thứ tự | Website |
|---|---|---|
| 332d24f906b2cd8b11c9 clusteredColumnChart | division_name → department_name (active); series vw_employee_quality[Trạng thái]; Tổng nhân viên; division ASC, department ASC | Cột nhóm ECharts; nhãn gồm đơn vị/phòng ban, zoom trục để xem nhiều nhóm |
| a2e691af25e013a46800 donutChart | vw_document_quality[Trạng thái]; Tổng tài liệu; Measure DESC | ECharts donut; giữ nhóm BLANK, legend/tooltip |
| b9f1b9d800c5edd5a455 barChart | document_type.document_code; series vw_document_quality[Trạng thái]; Tổng tài liệu; Measure DESC | Bar ngang xếp chồng; tổng theo code giảm dần; code ASC chỉ làm tie-break ổn định |
| cd68080dde4e2ac0640e tableEx | vw_employee_quality, department_name; employee_id NOT BLANK; is_kpi_scope=1; employee_quality_status=THIEU_HO_SO; missing DESC | Đủ 8 cột, tìm kiếm riêng, sort allowlist, phân trang server; employee_id làm tie-break |

QueryRef lưu cũ không được dùng để đoán aggregate. Các cột của bảng thiếu hồ sơ lấy giá trị trực tiếp theo field Column gốc, không SUM theo alias Sum(...). SQL có thể nhân bản dòng do rule; số đếm View vẫn giữ nguyên. Schema filtered unique index không loại được trường hợp nhiều rule trong View; không tự sửa số liệu bằng DISTINCT.

Navigation có Executive Overview active và 5 nút disabled ghi Sắp có; không dựng trang dữ liệu giả. Dashboard không gọi Power BI Service và không render iframe. Apache ECharts 6.0.0 được lưu cục bộ với LICENSE/NOTICE; không cần CDN lúc chạy. Charts có bản bảng số liệu để đọc bằng bàn phím/trình đọc màn hình. CSS có breakpoint mobile, vùng bảng cuộn ngang, ResizeObserver cho charts. Những điều này mới xác nhận mã nguồn, chưa nghiệm thu trực quan.

## Kiểm thử đã chạy và chưa xác minh

| Kiểm tra | Kết quả | Giới hạn |
|---|---|---|
| Restore/build | Thành công, 0 lỗi/cảnh báo | Output kiểm tra .scaffold/final-review; không tự dừng app người dùng |
| /Dashboard | HTTP 200 trong TestServer, render Razor, không iframe | Auth test cách ly, chưa login Windows thật |
| API anonymous/ngoài whitelist | 401/403 | Đã kiểm thử Data, Options, Missing |
| API reader | JSON 503 do SQL chưa kết nối | Không giả báo API có số liệu thành công |
| Tham số sai/negative id/page-size/sort | HTTP 400 | Không truy vấn SQL cho request sai |
| CountRows/Divide/SWITCH | Unit checks đạt | Không thay kiểm thử dữ liệu thật |
| EF query SQL | Translate scope/filter/COUNT_BIG/paging thành công | SQL in docs/dashboard-query-translation.txt; chưa thực thi được trên server |
| Chart/group/order | Đối chiếu PBIR và mã nguồn | Chưa đối chiếu kết quả SQL/DAX và hình ảnh dữ liệu thật |
| Bộ lọc tổ chức/trạng thái | Validation + SQL translation đạt | Chưa đo giá trị khi đổi slicer trên database thật |
| Bảng thiếu hồ sơ | Route/translation/server paging được kiểm tra | Không có dữ liệu thật để kiểm tra phân trang UI |
| Responsive | CSS/ResizeObserver và vùng cuộn đã thêm | Edge headless không chạy được do quyền; chưa xác minh hình ảnh |
| CRUD cũ | Bộ regression route/render/validation/phân quyền vẫn đạt | Chưa thực hiện ghi dữ liệu thật; không tuyên bố CRUD end-to-end thành công |

Các trường hợp cần so sánh tiếp: không bộ lọc, từng trạng thái, nhiều đơn vị/phòng ban, phòng ban trùng tên trong cùng đơn vị, scope không có dòng, có nhân viên nhưng không ai đầy đủ, nhiều rule/loại và status BLANK. So sánh cùng snapshot: Power BI Import phản ánh lần refresh, website đọc SQL hiện tại có GETDATE(). Nhiều query trong request dùng read committed, không phải snapshot transaction; dữ liệu thay đổi giữa query có thể gây chênh lệch tạm thời.

## Hướng dẫn đối chiếu sau khi SQL truy cập được

1. Dừng website cũ rồi chạy Chay-Website.cmd hoặc Rebuild + profile http trong Visual Studio; mở /Dashboard bằng Windows account có quyền.
2. Kiểm tra /Dashboard/Data, /Dashboard/Options và /Dashboard/Missing (GET). Không công khai API; chính sách quyền hiện có vẫn áp dụng.
3. Chạy scripts/validate-executive-overview.sql trong SSMS tại VTB2; file chỉ SELECT, hỗ trợ bộ lọc đơn vị/phòng ban/trạng thái một lựa chọn. Multi-select đối chiếu qua API với cùng bộ lọc; không tự tuyên bố script đơn lựa chọn bao phủ multi-select.
4. Refresh Power BI Desktop và đặt cùng slicer. Ghi số liệu thực vào bảng KPI trên; giữ NULL/BLANK riêng với 0. Kiểm tra table điều kiện và thứ tự như Visual gốc.
5. Kiểm tra giao diện tại 1440px và 390px: không tràn ngang toàn trang, bảng có vùng cuộn riêng, bộ lọc cập nhật, lỗi SQL không giữ số liệu cũ, không tiết lộ lỗi SQL thô.


## File tạo/chỉnh sửa

- Models/OverviewModels.cs: filter validation, KPI/series/table DTO.
- Services/DashboardService.cs: truy vấn read-only, Measures phụ thuộc, nhóm biểu đồ và paging.
- Controllers/DashboardController.cs: Index/Data/Options/Missing, JSON lỗi 503, quyền truy cập.
- Views/Dashboard/Index.cshtml: navigation báo cáo, slicer, 6 KPI, 3 charts, bảng.
- wwwroot/js/dashboard.js và wwwroot/css/dashboard.css: tải API, tương tác, cancel request cũ, bố cục responsive.
- wwwroot/lib/echarts/: ECharts 6.0.0 + LICENSE + NOTICE.
- Program.cs: đăng ký DashboardService.
- wwwroot/js/site.js: không disable submit của form đã preventDefault để AJAX tìm kiếm dùng lại được.
- tests/Verification/Program.cs: thêm kiểm thử dashboard; tài khoản giả chỉ thuộc test harness bị loại khỏi website.
- scripts/validate-executive-overview.sql: truy vấn đối chiếu chỉ đọc.
- docs/verification-stage-a.txt, docs/dashboard-query-translation.txt và docs/dashboard-api-*.json: bằng chứng kiểm thử.

Không sửa entity/schema, controller CRUD hoặc file Power BI nguồn. Views/PowerBI cũ còn trong source nhưng không được Dashboard sử dụng; route PowerBI chuyển hướng tới Dashboard. Không có server mới được để chạy từ phiên này để tránh khóa DLL của Visual Studio.


## Tham chiếu

[Microsoft CALCULATE](https://learn.microsoft.com/dax/calculate-function-dax): filter mới thay filter trên cùng cột, không tự bỏ filter của cột ở bảng khác. [Microsoft DIVIDE](https://learn.microsoft.com/en-us/dax/divide-function-dax): alternate mặc định BLANK. [Apache ECharts](https://echarts.apache.org/handbook/en/concepts/dataset/): series và dataset.


## Giai đoạn A — cập nhật 18/10/2026

- Điều hướng chính chỉ còn 3 vị trí Executive Overview, Problem documents, Contract; hai mục sau đang disabled và chưa có chức năng. Chi tiết hồ sơ là drill-through, không phải tab chính.
- Màu không hợp lệ/đã hết hạn đỏ; thiếu hồ sơ cam/vàng; đầy đủ/hợp lệ xanh; BLANK xám. Biểu đồ tròn hiển thị phần trăm.
- Mỗi nhóm tài liệu trả thêm mã trạng thái nguồn. Click donut hoặc nút Xem chi tiết trong bảng dữ liệu biểu đồ mở `/Dashboard/DocumentDetails?state=...`; không suy luận mã từ nhãn hiển thị, không hardcode HOP_LE.
- Executive lưu bộ lọc và trạng thái bảng vào URL. Drill-through chuyển các bộ lọc tổ chức/trạng thái; tìm kiếm, sort, page của bảng tổng quan được giữ riêng trong overviewSearch/overviewSort/overviewDesc/overviewPage/overviewPageSize. Không áp dụng tìm kiếm nhân viên thiếu hồ sơ lên bảng chi tiết tài liệu. Back dùng route cố định, không nhận URL chuyển hướng bên ngoài.
- Nguồn: PBIR trang `2c4ccbaf33f0b059aa11`, table `1e737d69b967a57602f3`, nút Back `55fc32ada69f52f3a10f`.
- Bảng chi tiết 11 trường: employee_id, full_name, department.department_name, document_code, document_name, issue_date, expiry_date, expiry_days, required_storage_form, storage_form, invalid_reason. Sắp mặc định employee_id ASC; có search/sort/phân trang SQL, tối đa 100 dòng/trang.
- Filter trang là calculated column Trạng thái. Bốn mã đã biết + nhóm BLANK cho các giá trị còn lại. Giá trị Hợp lệ lưu trong PBIR được thay bằng lựa chọn runtime.
- Không thêm is_kpi_scope=1 vào drill-through: đây là điều kiện nội bộ Measure ở donut, không phải filter visual của bảng drill. Do đó không dùng việc số dòng drill khác KPI làm bằng chứng sai nếu chưa kiểm tra đúng ngữ cảnh.
- SELECT DISTINCT chỉ trên 11 trường hiển thị để mô phỏng bảng chỉ có Column projections; không dùng DISTINCT cho các KPI COUNTROWS.

### Kiểm thử giai đoạn A

- Restore thành công; build `.scaffold/final-review`: 0 lỗi, 0 cảnh báo.
- 130/130 kiểm thử đạt (`docs/verification-stage-a.txt`), gồm render cả 5 nhóm trạng thái; 401 anonymous, 403 tài khoản ngoài whitelist; 400 cho trạng thái/sort/page/slicer sai; LINQ dịch thành SQL có tham số, DISTINCT/OFFSET và không tự thêm is_kpi_scope vào drill.
- Kiểm thử Node `tests/Verification/document-drill-navigation.cjs` đạt: trạng thái động, nhiều phòng ban, trạng thái nhân viên, Back giữ search/sort/page/pageSize, tìm kiếm chi tiết độc lập, xử lý lỗi SQL. Đây là kiểm thử DOM mô phỏng tối thiểu, không phải kiểm thử trình duyệt thực hoặc dữ liệu nghiệp vụ giả.
- `dashboard.js` và `document-drill.js` qua kiểm tra cú pháp Node.
- Chưa xác minh: số liệu thật trên SQL, số liệu cùng snapshot Power BI Desktop, sai lệch từng KPI, quan hệ lọc hai chiều runtime, cross-highlight biểu đồ, responsive/ảnh chụp browser và CRUD end-to-end. SQL API trong harness trả lỗi có kiểm soát 503; không quy đổi thành 0.
- schema.sql SHA256 vẫn `ED6BAB17C753BB8A5EB2038C44B2FCF40A565C7B1B98E26E185E68798804D6FB`. Không thay đổi DB, Power BI hoặc chạy migration.
- Chưa chạy website nền để tránh khóa DLL của Visual Studio.

## Chỉnh giao diện Executive Overview — 18/10/2026

### Phạm vi mã nguồn

- Views/Dashboard/Index.cshtml: ba multi-select, bỏ hướng dẫn Ctrl/Cmd, tải script dropdown, class màu KPI.
- wwwroot/js/dashboard-multiselect.js (mới): dropdown native details/summary + checkbox, đồng bộ select gốc, Escape và click ngoài để đóng, hỗ trợ bàn phím.
- wwwroot/js/dashboard.js: nối dropdown vào FormData hiện có, cập nhật summary khi tải options/reset/khôi phục URL, bảng màu mới, bỏ dataZoom của employee-chart.
- wwwroot/css/dashboard.css: style dropdown responsive, sáu màu KPI, badge chi tiết; KPI vàng đặt trên nền đậm để tăng tương phản.
- Views/Dashboard/DocumentDetails.cshtml: thêm class trạng thái để badge không hợp lệ tím, hết hạn đỏ.
- tests/Verification/overview-multiselect.cjs; docs/verification-overview-dropdown.txt; docs/verification-overview-ui.txt.

Không thêm thư viện frontend, package hoặc framework. Không thay đổi C# service/controller/DTO, logic Measure, SQL hoặc database, CRUD.

### Hành vi lọc

Select ẩn vẫn là nguồn FormData: Statuses/DivisionIds/DepartmentIds giữ tên và giá trị cũ. Checkbox hiển thị không có name nên không thêm tham số trùng. Chọn nhiều giá trị gửi repeated query parameters. Hai trạng thái cùng chọn được bỏ khỏi query như trước; bỏ hết trạng thái tự trở về cả hai như hành vi cũ. Không chọn tổ chức nghĩa là tất cả.

Mỗi thay đổi tự refresh; khi đổi đơn vị reset lựa chọn phòng ban và tải lại Options theo đơn vị/trạng thái. Khi bỏ chọn đơn vị, tải lại danh sách tương ứng. Xóa bộ lọc trả về cả hai trạng thái và tất cả tổ chức. Luồng cập nhật KPI/chart/table và context Back giữ nguyên.

### Màu và biểu đồ

Tổng nhân viên #2563EB; đầy đủ/hợp lệ #059669; thiếu #EA580C; sắp hết hạn #F59E0B; hết hạn #DC2626; không hợp lệ #7C3AED. Series/legend/tooltip ECharts dùng cùng màu. Badge drill-through đồng bộ. BLANK vẫn xám.

Chỉ bỏ dataZoom ở Tổng nhân viên theo phòng ban và trạng thái hồ sơ; không cắt mảng category/data, không lọc bớt series. Nhãn xoay nhẹ, truncate; tooltip giữ tên gốc. Slider dọc của biểu đồ loại tài liệu giữ nguyên.

### Kiểm tra và hạn chế

- Build thành công, 0 error/0 warning (output .scaffold/final-review; không khởi chạy website nền).
- 130/130 kiểm thử regression đạt; node --check cho dashboard.js/dashboard-multiselect.js đạt.
- Test DOM độc lập đạt: cấu trúc dropdown, nhiều lựa chọn, binding select, summary, reset, cập nhật options và Escape trả focus. Không dùng dữ liệu cán bộ giả trong ứng dụng.
- Kiểm tra dependency đơn vị/phòng ban ở luồng mã: tiếp tục Options hiện có; chưa kiểm chứng lựa chọn thật từ SQL hoặc giá trị KPI thật sau lọc.
- CSS dùng các cột Bootstrap responsive và panel giới hạn chiều cao; Edge headless khởi chạy thất bại (pipe/permission) nên chưa có kiểm chứng viewport/screenshot thực tế. Không tuyên bố đã kiểm thử responsive bằng trình duyệt.
- Kiểm thử CRUD hồi quy không ghi database; chưa xác nhận CRUD end-to-end trên dữ liệu thật.

## UI Refinement V2 — 19/10/2026 (thay thế mô tả KPI nền đậm ở lần trước)

Nguyên nhân: selector #executive-overview .overview-tone-yellow do lần chỉnh CSS trước thêm background:#182433, border-radius:6px, padding:2px 8px, display:inline-block. Razor dùng div thường, không dùng badge; JavaScript chỉ cập nhật textContent. Không phải lỗi SQL, Tabler hoặc Bootstrap.

Đã xóa toàn bộ quy tắc overview-tone-* và kpi-card/kpi-label/kpi-value cũ (kể cả mobile override), thay bằng dashboard-kpi-grid/card/label/value và modifier --total/complete/missing/expiring/expired/invalid. Không chồng override che nền đen.

Cards nền trắng, padding20, viền nhẹ, radius12, shadow nhẹ; grid auto-rows1fr giữ chiều cao đồng nhất. Font tên14px, số34px/700, căn trái. Giá trị không có background/radius riêng, không phải badge. Số quá dài có thể xuống dòng để không cắt/tràn; cột không đổi kích thước theo số chữ số. Grid 1 cột dưới480, 2 từ480, 3 từ768, 6 từ1440px.

Màu theo thứ tự 6 KPI: #2563EB, #059669, #EA580C, #D97706, #DC2626, #7C3AED. Chỉ đổi KPI Card; không sửa chart, filter, drill-through, DashboardService, SQL hoặc CRUD.

File giao diện thay đổi: Views/Dashboard/Index.cshtml; wwwroot/css/dashboard.css.
File kiểm tra mới: tests/Verification/kpi-formatting.cjs; docs/verification-kpi-formatting.txt; docs/verification-kpi-v2.txt.

Build .scaffold/final-review: 0 error, 0 warning. 130/130 regression tests đạt. Test chạy vòng lặp render KPI thực tế với input cô lập kiểm tra cả 6 trường cho 0, 1, số dài, 12,5%, null/BLANK đạt; không đưa dữ liệu giả vào website và không thay số SQL. Trạng thái tải/lỗi vẫn dấu — như trước.

Problem documents và Contract còn là tab disabled, chưa có trang/card triển khai; không thể xác nhận giao diện hai trang chưa tồn tại. Shared/Dashboard.cshtml cũ dùng metric, không dùng class KPI bị thay; giữ nguyên.

Đã thử Edge headless nhưng browser đóng khi khởi chạy với lỗi pipe/permission. Chưa kiểm tra trực quan laptop/desktop/mobile hoặc bounding box số dài. Kiểm tra CSS và định dạng không thay thế kiểm tra layout thực. Chưa đối chiếu giá trị KPI với SQL/Power BI Desktop trong lượt UI này.

## Drill-down khối / phòng ban — 19/10/2026

### Mã nguồn và dữ liệu

- Tạo wwwroot/js/employee-hierarchy.js: điều khiển hai cấp và gom nhóm các dòng tổng hợp.
- Sửa wwwroot/js/dashboard.js: chuyển employeesByDepartment từ API hiện có vào EmployeeHierarchy, xóa cấu hình trục ghép tên cũ; xóa trạng thái drill khi tải báo cáo thất bại.
- Sửa Views/Dashboard/Index.cshtml: breadcrumb, nút Quay lại khối, select khối hỗ trợ bàn phím, nạp script mới trước dashboard.js.
- wwwroot/css/dashboard.css: đảm bảo nút Quay lại khối ẩn đúng ở cấp khối.
- tests/Verification/employee-hierarchy.cjs và log docs/verification-employee-hierarchy.txt; log regression docs/verification-hierarchy-regression.txt.

Tái sử dụng GET /Dashboard/Data có xác thực, read-only. EmployeeMeasureScope áp dụng slicer hiện tại; service trả số đếm đã GROUP BY division/department/status (không phải bản ghi nhân viên). Cấp khối cộng các số đếm phòng ban cùng parent và code DAY_DU/THIEU_HO_SO; cấp phòng ban lấy đúng các dòng thuộc parent đã bấm, vẫn nằm trong phạm vi API đã lọc. Không đổi COUNTROWS thành DISTINCTCOUNT, không suy ra thiếu = tổng - đủ. BLANK không đổi thành 0. Hai series luôn là Đầy đủ và Thiếu hồ sơ; giá trị trạng thái ngoài hai mã không được cộng vào hai series này.

Không sửa DashboardService, DTO, controller, SQL Views/database, KPI, CRUD, bộ lọc hoặc drill-through tài liệu. Không thêm endpoint vì endpoint hiện có đã cung cấp đủ dữ liệu tổng hợp cho hai cấp. Không tải thêm dữ liệu cán bộ vào trình duyệt. Không cố định 3 khối: hiển thị số khối có trong kết quả thực tế sau lọc.

### Tương tác và ngữ cảnh

- Mặc định cấp khối, rút tiền tố Khối chỉ ở formatter nhãn và breadcrumb; khóa chọn vẫn là tên gốc (division_name unique theo schema hiện có).
- Click cột (series) hoặc nhãn trục X (triggerEvent) mở phòng ban. Dropdown khối hỗ trợ thao tác bàn phím. Tên phòng ban không ghép tên khối.
- Back chỉ đặt division nội bộ của chart về null; không thay form lọc, URL filter, bảng hoặc KPI. Không gọi lại SQL khi đổi cấp vì dữ liệu tổng hợp cùng scope đã có.
- Khi lọc ngoài đổi: nhận dữ liệu API mới; giữ cấp phòng ban nếu khối còn trong dữ liệu, tự về cấp khối nếu khối không còn. Không hiển thị số liệu của phạm vi trước khi request hoàn tất vì vùng results đang aria-busy/pointer-events none. Lỗi API xóa chart và navigation cũ.
- Tooltip plain richText hiển thị tên gốc đầy đủ và hai số đếm. Legend hai series, grouped bars, không dataZoom; tất cả category được giữ, nhãn dài truncate/xoay nhẹ, ResizeObserver hiện có tiếp tục resize.
- Bảng dữ liệu biểu đồ dạng bảng sẵn có tiếp tục cung cấp chi tiết phòng ban trong toàn phạm vi bộ lọc, không phải kết quả lọc riêng theo drill nội bộ.

### Kiểm thử

Kiểm thử JavaScript cô lập đạt: gom tổng theo khối; lọc phòng ban đúng khối dù trùng tên; click cột/nhãn/select; Back; dữ liệu mới khi đổi filter; BLANK; tooltip; không slider; không sửa input. Fixtures chỉ trong test, không đưa dữ liệu giả vào ứng dụng hoặc database.

Chưa xác minh bằng SQL thật rằng có đúng 3 khối; chưa đối chiếu số với Power BI Desktop. Chưa kiểm tra trực quan responsive trong browser: các lần khởi chạy Edge headless ở môi trường này lỗi pipe/permission. CSS responsive/ResizeObserver giữ nguyên nhưng không thay thế kiểm tra layout thực tế.

Kết quả cuối lượt: dotnet build --no-restore --output .scaffold/final-review thành công, 0 lỗi/0 cảnh báo; 130/130 regression tests đạt; kiểm thử employee-hierarchy và kiểm tra cú pháp dashboard.js đạt. Không khởi chạy website nền hoặc thực hiện CRUD ghi database.

## Ba chế độ và bỏ dataZoom tài liệu — 19/10/2026

Phần A/B đã triển khai; phần C/D/E chưa triển khai. Nội dung này thay thế mô tả mặc định cấp khối và khóa tên ở các mục cũ.

### File đã thay đổi

- Models/OverviewModels.cs: ChartPoint thêm DivisionId/DepartmentId tùy chọn.
- Services/DashboardService.cs: GROUP BY biểu đồ nhân viên bổ sung khóa division_id/department_id; join vẫn trên PK/FK, COUNT_BIG/Measure scopes không đổi. Không join thêm rule/document làm nhân số nhân viên. Dữ liệu biểu đồ không gộp phòng ban chỉ vì trùng tên.
- Views/Dashboard/Index.cshtml: 3 nút chế độ, aria-pressed, breadcrumb, Back, vùng cuộn ngang CSS.
- wwwroot/js/employee-hierarchy.js: mặc định all; cấp division gom theo divisionId, all/detail theo departmentId. Label là tên; tooltip phòng ban bổ sung khối. Cột trùng tên vẫn có ID riêng. Bộ lọc ngoài giữ nguyên, khi khối đã chọn không còn thì xóa selection và về cấp khối nếu đang detail.
- wwwroot/js/dashboard.js: xóa dataZoom khỏi instance charts[2], grid.right20, left25, bottom25, chiều cao max(360, số loại*38+100). Giữ tất cả category và thứ tự query, stacked bars, màu và tooltip. Bổ sung series chưa có dữ liệu bằng null để luôn có 4 trạng thái chuẩn; nếu nguồn có nhóm BLANK vẫn giữ, không âm thầm bỏ dữ liệu ngoài 4 mã.
- wwwroot/css/dashboard.css: vùng cuộn ngang cho chart; không dùng CSS giấu dataZoom.
- tests/Verification/employee-hierarchy.cjs cập nhật 3 chế độ/ID; tests/Verification/document-chart-layout.cjs mới.

Các bộ lọc vẫn do API /Dashboard/Data áp dụng trước khi tổng hợp. Chuyển chế độ không sửa FormData/query/filter và không thay đổi KPI hoặc bảng. Không đổi logic EmployeeScope hiện có; việc mở rộng tên phòng ban trong slicer theo metadata cũ vẫn giữ, còn khóa cột/drill nay là ID.

### Kiểm chứng

- Build output .scaffold/final-review: 0 lỗi/0 cảnh báo.
- Regression130/130 đạt: docs/verification-three-modes.txt.
- Node test 3 chế độ đạt, có tình huống cả khối và phòng ban trùng tên nhưng khác ID, khối mất khỏi phạm vi lọc, Back, tooltip và BLANK.
- Chạy cấu hình biểu đồ tài liệu thực với fixture tổng hợp cô lập 30 loại: giữ đủ30, 4series, không dataZoom, cao1240px; không thay dữ liệu website. Log docs/verification-document-chart-layout.txt.
- Kiểm thử định dạng KPI và drill-through hồ sơ hiện có tiếp tục đạt.
- Chưa kiểm tra browser layout/responsive trong lượt này; những lần Edge headless trước lỗi pipe/permission. Chưa có kết quả SQL thật/snapshot Power BI Desktop; các KPI vẫn Chưa xác minh, không có số đối chiếu hoặc chênh lệch được tính.

### Rà soát nguồn cho bước tiếp theo (chưa phải chức năng đã triển khai)

Đã đọc lại Measure.tmdl, relationships.tmdl, page/query/filter metadata Problem documents, Contract và drill hợp đồng.
Problem documents có 12 visual: 3 card (Tổng yêu cầu hồ sơ; Tỷ lệ đáp ứng hồ sơ; Hồ sơ đang thiếu), 2 clusteredBar, 2 table, 3 slicer, 2 textbox. Không có KPI thứ tư trong metadata. Requirement view chỉ có quan hệ active qua document_id; trực tiếp employee/type/department inactive. Không được thay thế bằng filter employee_id trực tiếp. Bảng vấn đề loại HDXDTH1/HDXDTH2/HDTHUVIEC và HOP_LE; bảng sắp hết hạn chỉ filter calculated status, không tự thừa kế is_kpi_scope.
Contract có 4card, 2chart, 2table, 2slicer, 2textbox. COUNT(employee_id) ở card, DISTINCTCOUNT ở biểu đồ phòng ban, COUNTNONNULL ở donut là các công thức khác nhau. Drill hợp đồng có 5cột, full_name ASC, filter trạng thái runtime. Trang drill không tự thừa kế visual filter employment_status của donut.

# Hai Dashboard đã triển khai — 19/10/2026

Phần này cập nhật trạng thái mới nhất, thay thế các ghi chú cũ “chưa triển khai Problem documents/Contract”.

## Routes và thành phần thực tế

- /Dashboard: Executive Overview, chỉ đổi nav sang partial _ReportTabs, không viết lại chart/KPI.
- /Dashboard/ProblemDocuments: Razor riêng, 3 KPI, 2 clustered horizontal bar chart, bảng sắp hết hạn 6 cột, bảng vấn đề 9 cột, slicer cán bộ, đơn vị/phòng ban, nhóm/loại tài liệu và 2 nội dung textbox nguồn. 12 visual gốc có đại diện trong UI. Hierarchy slicers được tách thành dropdown để dễ sử dụng.
- /Dashboard/Contract: Razor riêng, 4 KPI, donut trạng thái nhân viên và bar theo phòng ban, bảng cần xử lý 5 cột, bảng hợp đồng hết hạn 8 cột, slicer cán bộ và đơn vị/phòng ban, 2 nội dung textbox nguồn. Đủ 12 visual.
- /Dashboard/ContractDetails?State=CAN_KY_KET_TIEP: drill table5 cột, full_name ASC; không có tab độc lập. Nút quay lại và các query context giữ filter/page/search/sort của Contract.
- APIs GET /Dashboard/ProblemData, /ContractData, /ReportOptions, /ReportTable: class DashboardController [Authorize], policy whitelist hiện có; NoStore; truy vấn tham số EF, không API ghi. Không nới quyền hoặc thêm tài khoản.
- Metadata mapping/hash của 26 visual: docs/REPORTS_SOURCE_MAP.md.

## KPI / DAX / triển khai

Tất cả giá trị Website từ SQL, Power BI Desktop cùng snapshot và chênh lệch hiện **Chưa xác minh**. Không dùng số liệu mẫu hoặc biến lỗi kết nối thành số0.

| KPI/Measure | DAX nguồn | C#/SQL tương đương | Website | Power BI | Chênh lệch |
|---|---|---|---|---|---|
| Tổng yêu cầu hồ sơ | CALCULATE(COUNTROWS(vw_employee_document_requirement), vw_employee_document_requirement[employment_status] in {"DangLamViec", "TamHoanHDLD"}) | ReportRequirements theo context; LongCount dòng active/paused, DaxCount(0) -> null | Chưa xác minh | Chưa xác minh | Chưa tính được |
| Hồ sơ đang thiếu | CALCULATE(COUNTROWS(vw_employee_document_requirement), vw_employee_document_requirement[employment_status] in {"DangLamViec", "TamHoanHDLD"}, vw_employee_document_requirement[requirement_status] = "Thieu_Ho_So") | Cùng scope + requirement_status=THIEU_HO_SO; COUNT_BIG; zero -> null | Chưa xác minh | Chưa xác minh | Chưa tính được |
| Tỷ lệ đáp ứng hồ sơ | DIVIDE([Tổng yêu cầu hồ sơ] - [Hồ sơ đang thiếu], [Tổng yêu cầu hồ sơ], 0) | RequirementRate: mẫu0 ->0; (total-missing)/total, thiếu BLANK tương đương0 khi trừ; 2chữ số phần trăm | Chưa xác minh | Chưa xác minh | Chưa tính được |
| Nhân viên có HDLD Hợp lệ | CALCULATE(COUNT(vw_labor_contract_check[employee_id]), vw_labor_contract_check[labor_contract_status] = "HOP_LE") | Đếm employee_id không null, statusHOP_LE; không DISTINCT | Chưa xác minh | Chưa xác minh | Chưa tính được |
| Nhân viên có HĐ cần ký tiếp | CALCULATE(COUNT(vw_labor_contract_check[employee_id]), vw_labor_contract_check[labor_contract_status] = "CAN_KY_KET_TIEP") | Đếm employee_id không null với mã tương ứng | Chưa xác minh | Chưa xác minh | Chưa tính được |
| Nhân viên thiếu HDXDTH1 | CALCULATE(COUNT(vw_labor_contract_check[employee_id]), vw_labor_contract_check[labor_contract_status] = "THIEU_HDXDTH1") | Đếm employee_id không null với mã tương ứng | Chưa xác minh | Chưa xác minh | Chưa tính được |
| HĐ sắp hết hạn | CALCULATE(COUNTROWS(vw_document_quality), vw_document_quality[is_kpi_scope] = 1, vw_document_quality[document_code] IN {"HDTHUVIEC", "HDXDTH1", "HDXDTH2"}, vw_document_quality[quality_status] = "SAP_HET_HAN") | ReportDocuments + đúng 3 mã/status/scope; COUNT_BIG không DISTINCT | Chưa xác minh | Chưa xác minh | Chưa tính được |
| Nhân viên cần xử lý HĐLĐ (bar) | CALCULATE(DISTINCTCOUNT(vw_labor_contract_check[employee_id]), vw_labor_contract_check[labor_contract_status] IN {"CAN_KY_KET_TIEP", "THIEU_HDXDTH1"}) | DISTINCT employee_id theo department.department_name, đúng 2 mã | Chưa xác minh | Chưa xác minh | Chưa tính được |
| Tổng nhân viên theo trạng thái hợp đồng (donut) | Visual Aggregation Function5: CountNonNull(employee_id), nhóm calculated Trạng thái | COUNT không null, gộp theo SWITCH; loại employment_status NULL/ChamDutHDLD/NghiHuu riêng visual này | Chưa xác minh | Chưa xác minh | Chưa tính được |

COUNTROWS/COUNT có zero row -> null qua DaxCount; DIVIDE của tỷ lệ hồ sơ dùng alternate0 theo nguồn. Không thay bằng tổng tài liệu hoặc tổng nhân viên. Donut giữ 4 trạng thái Hợp lệ, Thiếu HDXDTH1, Cần ký kết tiếp, Không cảnh báo riêng và nhóm BLANK; không mặc định gộp các trạng thái.

## Quan hệ và filter context

- Scope cán bộ/tổ chức đi qua employee/department/division. Loại tài liệu đi document_type->document và chiều document->employee (bothDirections) khi có lựa chọn, đồng thời document_type.code->vw_document_quality.code.
- Requirement view chỉ filter qua document_id->document khi có scope bên ngoài. Không lọc trực tiếp employee_id/type/department vì các relationships đó inactive. Không có filter thì giữ NULL document_id (các hồ sơ thiếu). Không tự sửa hiện tượng NULL document_id bị loại khi slicer truyền filter qua document, vì đó là mô hình nguồn.
- Visual department của tỷ lệ dùng chính department_name trong requirement view, không thay bằng department table. COUNTROWS toàn bộ các dòng scope; không GROUP BY kiểu DISTINCT để sửa duplicate.
- Type slicer chỉ cung cấp Hồ sơ gốc/Quyết định-Văn bản khác, mã không blank. Không có lựa chọn không tự hạn chế toàn bộ báo cáo vào hai nhóm này.
- Cards Contract không tự thêm employment_status active/paused: chỉ donut có filter loại nhân viên chấm dứt/nghỉ hưu. Textbox nguồn được giữ nhưng không dùng mô tả đó để thêm điều kiện vào Measures.
- Drill Contract lọc trạng thái được bấm + slicer context; không tự truyền filter employment_status riêng của donut. Các trạng thái ngoài SWITCH gom BLANK. Không hardcode saved state Hợp lệ.

## Bảng / sắp xếp / phân trang

| Bảng | Visual filter | Default sort | Cột |
|---|---|---|---|
| Hồ sơ sắp hết hạn | calculated status=Sắp hết hạn, không thêm KPI scope | expiry_date ASC | id/name/code/document/expiry/days |
| Tài liệu có vấn đề | NOT HDXDTH1/HDXDTH2/HDTHUVIEC; quality_status != HOP_LE | expiry_days ASC | id/name/document/group/issue/expiry/days/status/reason |
| Nhân viên cần xử lý | labor_contract_status không null/HOP_LE/KHONG_CANH_BAO | calculated Trạng thái ASC | id/name/department/employee status/contract status |
| Hợp đồng đã hết hạn | DA_HET_HAN; codeHDKXDTH/HDTHUVIEC/HDXDTH1/HDXDTH2 | expiry_days DESC | id/name/code/document/issue/expiry/days/status |
| Drill Contract | calculated Trạng thái runtime; không filter visual khác | full_name ASC | id/name/employee status/department/contract status |

Chỉ DISTINCT tuple các cột hiển thị ở table (Column projections PBIR), không DISTINCT KPI COUNTROWS. Search/sort/page phía SQL, maximum100/trang, allowlist riêng từng bảng, stable tie-break toàn bộ cột hiển thị. DateOnly JSON định dạng dd/MM/yyyy ở UI. Trạng thái badge màu chữ trên nền sáng. Tìm kiếm theo mã/họ tên/tên tài liệu/phòng ban tùy bảng. Dropdown cán bộ truy vấn tối đa50 kết quả/lần tìm và giữ lựa chọn hiện tại, không đẩy toàn bộ hồ sơ xuống browser.

## Kết quả kiểm thử và giới hạn

- Build thành công0 lỗi/0 cảnh báo.
- 197/197 kiểm thử TestServer/EF mapping/validation/SQL translation đạt (docs/verification-reports.txt): render cả hai trang và drill; 3 tab links; 401/403; 400 state/sort/page sai; 33 cấu hình sort của5 bảng đều dịch SQL có parameter/DISTINCT/OFFSET. CRUD regression không có request ghi dữ liệu thật.
- reports.js qua Node syntax check. Kiểm thử mã JavaScript drill/back thực tế đạt: state động, nhiều lựa chọn cán bộ, page/search/sort bảng Contract giữ khi Back, state không hợp lệ không mở route; docs/verification-report-navigation.txt.
- Đã khởi động Kestrel thực trên cổng kiểm tra5097, nhận Application started. HTTP từ sandbox tới localhost bị socket policy/timeout; docs/report-http-smoke.json. Không tuyên bố kiểm tra HTTP thực đã thành công.
- sqlcmd read-only ACER\SQLEXPRESS VTB2 timeout/error locating instance; chưa có số KPI SQL thật trong phiên này. Endpoint harness trả JSON503 khi SQL unavailable, không trả fake số.
- Edge headless tiếp tục lỗi pipe/permission; chưa kiểm tra responsive/ảnh chụp màn hình/interaction browser thực tế. Layout Razor/CSS theo bố cục nguồn (chart bên trái, bảng xử lý bên phải, bảng chi tiết cuối trang), responsive qua Bootstrap; chưa xác minh pixel/layout.
- Giá trị so sánh SQL/Power BI Desktop, full bidirectional runtime context và lựa chọn slicer trong Power BI vẫn Chưa xác minh. Danh sách dropdown là danh mục theo scope cơ bản; chưa đối chiếu đầy đủ hành vi làm mờ/ẩn lựa chọn chéo giữa mọi slicer của Power BI.
- Các tab/API/view đã có mã thực tế, nhưng chưa thể xác nhận end-to-end với SQL thật trong môi trường hiện tại.

## File tạo/cập nhật

Mới: Models/ReportModels.cs; Services/DashboardService.Reports.cs; Views/Dashboard/ProblemDocuments.cshtml; Contract.cshtml; ContractDetails.cshtml; _ReportTabs.cshtml; _ReportFilters.cshtml; _ReportTables.cshtml; wwwroot/js/reports.js; wwwroot/css/reports.css; tests/Verification/report-navigation.cjs; docs/REPORTS_SOURCE_MAP.md và log kiểm thử.
Sửa: Controllers/DashboardController.cs; Views/Dashboard/Index.cshtml (chỉ nav); tests/Verification/Program.cs; DASHBOARD_VALIDATION.md; DASHBOARD_IMPLEMENTATION_PROGRESS.md. Không cần sửa csproj/DI vì SDK tự nhận file và DashboardService đã đăng ký.

Xác nhận cuối lượt: build thành công0lỗi/0cảnh báo; chạy lại harness sau chỉnh bố cục đạt197/197. schema.sql SHA256 không đổi ED6BAB17C753BB8A5EB2038C44B2FCF40A565C7B1B98E26E185E68798804D6FB. Lệnh Ctrl+C dừng phiên Kestrel thử bị policy từ chối (sandbox_approval disabled); chưa xác nhận dừng phiên thử, cổng5097 tách biệt profile5082.

## Xử lý TaskCanceledException — 19/10/2026

Dashboard frontend gọi AbortController.abort khi đổi bộ lọc/tải lại/rời trang; MVC CancellationToken truyền xuống EF. Stack trace người dùng dừng tại OpenInternalAsync/Options phù hợp với thao tác mở SQL bị hủy, nhưng chưa đủ bằng chứng khẳng định đó là timeout hoặc lỗi kết nối SQL.

DashboardController.Read bổ sung catch OperationCanceledException CHỈ KHI HttpContext.RequestAborted.IsCancellationRequested: log Debug và trả status499 (quy ước client đóng/hủy request). Không bỏ CancellationToken, không bắt mọi lỗi để trả200/0, không che các cancellation khi request chưa bị hủy. SqlException vẫn xử lý503 như trước.

Build output cũ final-review bị khóa DLL MSB3027/MSB3021; build lại output mới .scaffold/cancellation-review thành công0lỗi/0cảnh báo. Harness thêm ReviewAssemblyPath override và --cancellation-only để kiểm tra đúng bản DLL mới mà không đọc SQL. Hai nhánh kiểm thử thực tế controller đạt2/2; docs/verification-cancellation-only.txt. Lần harness đầu dùng DLL cũ trong final-review không có catch đã kết thúc bằng exception; docs/verification-cancellation.txt là log cũ này, không phải kết quả của bản sửa. Không chạy lại toàn bộ regression SQL trong lượt sửa hủy request.

Không thay KPI/SQL/filter logic. Debugger bật break-on-thrown có thể vẫn dừng ngay tại nơi EF phát sinh exception, trước khi catch xử lý; kiểm tra bằng Continue hoặc chạy không debugger. Nếu lỗi xuất hiện ngay request đầu khi không đổi lọc/rời trang, cần log RequestAborted/SQL connection riêng, không coi bản sửa cancellation là sửa kết nối VTB2.
