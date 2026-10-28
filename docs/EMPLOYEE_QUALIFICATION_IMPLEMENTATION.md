# Cán bộ, văn bằng và lý lịch trích ngang — 24/10/2026

## Trạng thái và nguồn schema
Đã triển khai mã nguồn trong D:/KLTN/Web. **Chưa xác nhận đồng bộ với schema SQL Server live**: kết nối Windows tới ACER\SQLEXPRESS/VTB2 thất bại (Error Locating Server/Instance, login timeout). Không có bảng/cột nào được đọc thành công từ server trong lượt này.

appsettings.Development.json hiện dùng Server=ACER\SQLEXPRESS;Database=VTB2;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=5. Cấu hình nền có ConnectionStrings:VTB2 trống; cấu hình môi trường/User Secrets có thể ghi đè khi chạy. Không đổi connection string hoặc ghi mật khẩu vào báo cáo.

Database/schema.sql là snapshot cũ, chưa có ba trường liên hệ và bảng văn bằng. Đã tìm được DDL bổ sung thực tế trong file D:/KLTN/VTB2.sql (đoạn bắt đầu quanh dòng 3469). **Chỉ đọc**, không chạy ALTER/CREATE/INSERT trong file notebook này. Mapping mới dựa trên định nghĩa này, không dựa vào tên cột dự kiến trong yêu cầu.

| Đối tượng trong file DDL | Định nghĩa được dùng |
|---|---|
| employee.phone_number | varchar(10) NULL |
| employee.email | varchar(254) NULL |
| employee.hometown | nvarchar(255) NULL |
| qualification.qualification_id | int IDENTITY(1,1), PK |
| qualification.employee_id | char(8) NOT NULL, FK employee.employee_id, NO ACTION mặc định |
| qualification.qualification_level | nvarchar(50) NOT NULL |
| qualification.degree_name | nvarchar(200) NULL |
| qualification.major | nvarchar(200) NULL |
| qualification.institution | nvarchar(255) NULL |
| qualification.graduation_date | date NULL |
| qualification.notes | nvarchar(500) NULL |

Khác biệt quan trọng: DDL khai báo **dbo.qualification**, không phải dbo.employee_qualification. Chỉ tên index trong đoạn comment có employee_qualification; index đó không được tạo trong DDL đang đọc. Không thấy CHECK danh mục trình độ hoặc unique(employee_id,qualification_level) trong phần định nghĩa mới. Không tự tạo enum, is_highest, bảng danh mục, index hoặc constraint.

Script chỉ SELECT scripts/inspect-employee-qualification-readonly.sql trả cột/types/nullability/defaults/PK/index/FK/CHECK cho employee và cả hai tên bảng văn bằng, để xác minh trên SSMS trước khi xác nhận live. Nếu server dùng tên/cột khác file VTB2.sql thì cần cập nhật mapping theo kết quả này; không chạy notebook để ép server giống code.

## Entity và nghiệp vụ
- Employee có PhoneNumber, Email, Hometown nullable. Email có validation server bằng EmailAddress; maxlength bám DDL. Điện thoại dùng chuỗi, giữ số 0 đầu và giới hạn đúng varchar(10), không tự mở rộng cột.
- Qualification ánh xạ dbo.qualification, PK identity và quan hệ một-nhiều Employee.Qualifications. Không unique theo trình độ: một người có thể có nhiều bằng cùng cấp.
- QualificationLevel nhập text vì DDL không có CHECK định danh; bắt buộc, tối đa 50 ký tự. Các trường khác nullable theo DDL, date picker cho graduation_date.
- Dùng luồng Catalog Thêm/Sửa/Xóa hiện có. Chỉ admin được viết; chống CSRF; EmployeeId của văn bằng đã lưu không được bind thay đổi. Kiểm tra cán bộ tồn tại trước ghi. Xóa có GET xác nhận và POST chỉ xóa entity theo PK đã chọn. Không xóa văn bằng/tài liệu khác.
- QualificationOrdering.ByGraduation dùng OrderByDescending(GraduationDate.HasValue), ThenByDescending(GraduationDate), ThenByDescending(QualificationId). Dùng chung ở Catalog, trang hồ sơ và bản in (cùng loader).

## Hồ sơ và bản in
- Thêm Số điện thoại, Email, Quê quán vào thông tin cá nhân; null/trắng hiển thị --, text-break tránh cắt chuỗi dài.
- Mục Trình độ chuyên môn đặt trước hồ sơ thiếu/cảnh báo/danh sách tài liệu, có nút Thêm văn bằng và thao tác Sửa/Xóa theo quyền.
- Không tìm thấy trang In lý lịch trích ngang trước đó. Đã thêm /Profiles/Print/{employeeId}, liên kết từ hồ sơ. Trang in dùng chung dữ liệu đã truy vấn từ VTB2 với trang chi tiết.
- Bản in có 6 phần yêu cầu: cá nhân, công tác, tất cả văn bằng, tổng hợp hồ sơ, tài liệu phân nhóm, hồ sơ thiếu/cảnh báo/hợp đồng. Không suy luận bằng mới nhất là bằng cao nhất.
- Số loại tài liệu yêu cầu/có/thiếu lấy từ vw_employee_quality như hồ sơ hiện tại. Số tài liệu chất lượng đếm document_id hiện hành riêng biệt theo trạng thái trong vw_document_quality, tránh nhân do hàng trùng. Bản in giải thích nếu một document có nhiều trạng thái thì có thể nằm trong nhiều nhóm.
- Danh sách vẫn hiển thị bản lịch sử với nhãn Lịch sử — không đánh giá. Cảnh báo xử lý chỉ dùng ID hiện hành; không coi hợp đồng lịch sử hết hạn là lỗi. Hợp đồng đọc trạng thái từ View cũ.
- storage_form dùng helper 5 lựa chọn hiện hành: null/literal NULL/trắng -> --.
- CSS A4, @page 15mm, table headers lặp lại, không chiều cao cố định hoặc overflow hidden, cho phép nhiều trang; các controls ẩn khi in. Không sidebar/navbar do Layout=null. Có In / Lưu PDF và Quay lại hồ sơ. Có ghi rõ không phải mẫu chính thức VietinBank.

## Routes
- Hồ sơ: /Profiles/Details/{employeeId}
- In: /Profiles/Print/{employeeId}
- Thêm văn bằng: /Catalog/Create?kind=qualifications&employeeId={employeeId}
- Sửa: /Catalog/Edit/{qualificationId}?kind=qualifications
- Xác nhận xóa: /Catalog/Delete/{qualificationId}?kind=qualifications
- Danh sách: /Catalog?kind=qualifications&employeeId={employeeId}

## File sửa/tạo
- Data/Records.cs: ba trường liên hệ + navigation.
- Data/Qualification.cs (mới): entity văn bằng.
- Data/VtbContext.Records.cs: DbSet, PK identity, FK, snake_case mapping.
- Models/WorkspaceModels.cs: ProfileModel.Qualifications.
- Services/CatalogService.cs: field definitions, truy vấn danh sách, validate FK, kind/ID.
- Services/QualificationOrdering.cs (mới): thứ tự dùng chung.
- Services/ProfilePresentation.cs: text fallback, đếm tài liệu hiện hành theo trạng thái.
- Controllers/CatalogController.cs: dispatch CRUD văn bằng, khóa owner khi sửa, redirect về hồ sơ.
- Controllers/ProfilesController.cs: loader chung Details/Print + query văn bằng.
- Views/Shared/Record.cshtml: owner bất biến, quay lại hồ sơ, ngày dd/MM/yyyy khi chỉ đọc.
- Views/Profiles/Details.cshtml: thông tin liên hệ, link in, mục văn bằng.
- Views/Profiles/_Qualifications.cshtml (mới).
- Views/Profiles/Print.cshtml (mới).
- wwwroot/css/profile-print.css, wwwroot/js/profile-print.js (mới).
- tests/Verification/Program.cs: kiểm thử mới và đối chiếu model với phần DDL bổ sung.
- scripts/inspect-employee-qualification-readonly.sql (mới, chỉ SELECT).
- docs/build-qualifications.txt, docs/test-qualifications.txt, docs/print-profile-multiple.html, docs/print-profile-empty.html: log/HTML test, chỉ dùng dữ liệu fixture trong bộ test.

Không sửa .csproj vì SDK đã tự nhận các file C#/Razor mới. Không tạo project mới, không migrations/EnsureCreated, không chạy SQL DDL/DML, không sửa schema.sql, Views hoặc requirement_rule. DashboardService/KPI không thay đổi.

## Kết quả kiểm thử
Build: **0 lỗi, 0 cảnh báo**. Verification: **292 PASS, 0 FAIL**.

| Yêu cầu | Đã kiểm tra | Giới hạn |
|---|---|---|
| Ba trường liên hệ | Entity length/null, giữ số 0 đầu, render trong bản in | Chưa đọc/ghi live |
| Email sai / trường NULL | Model validation PASS | Chưa POST lưu live |
| Nhiều bằng đại học/thạc sĩ | Model không unique trình độ, fixture 2 bằng đại học + thạc sĩ đều xuất hiện | Chưa INSERT live |
| Sửa văn bằng | Form có trường đúng, owner không bind, route admin | Chưa UPDATE live |
| Xóa văn bằng | Render xác nhận POST/CSRF, reader bị chặn | Chưa DELETE live |
| Sắp xếp | Ngày giảm dần, NULL cuối, ID giảm dần khi cùng ngày; dịch thành SQL PASS | Chưa chạy query live |
| In nhiều bằng / không có bằng | TestServer render cả hai PASS, giữ đầy đủ bằng và liên hệ, không sidebar | Chưa xác minh preview/PDF và ngắt trang thực tế bằng trình duyệt |
| Bảo mật | Anonymous chặn, reader không CRUD, thiếu CSRF bị chặn | Không thay cơ chế Windows Authentication |
| Hồi quy web/Dashboard | Bộ verification/query translation cũ PASS | Không xác nhận số liệu live |

Không tuyên bố đã lưu thành công vào SQL Server. Toàn bộ fixture chỉ nằm trong process kiểm thử, không ghi dữ liệu nhân sự thật. Cần xác minh schema live, chạy trên bản sao an toàn các ca thêm nhiều bằng, sửa/xóa và kiểm tra print preview dài trước khi coi triển khai đã được kiểm chứng end-to-end.
