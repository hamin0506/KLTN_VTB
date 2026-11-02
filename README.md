# Hệ thống quản lý và đánh giá chất lượng hồ sơ cán bộ

Ứng dụng phục vụ khóa luận tốt nghiệp, hỗ trợ quản lý hồ sơ cán bộ, kiểm tra việc đáp ứng yêu cầu tài liệu và trực quan hóa chất lượng dữ liệu. Báo cáo được tham chiếu từ Power BI Desktop và triển khai trực tiếp trên website bằng Apache ECharts.

> Đây là sản phẩm nghiên cứu phục vụ khóa luận, không phải hệ thống chính thức của VietinBank. Không công khai dữ liệu nhân sự hoặc tài liệu nội bộ trong repository.

## 1. Mục tiêu

- Tập trung thông tin tổ chức, cán bộ, văn bằng và tài liệu hồ sơ.
- Theo dõi tài liệu hiện hành, lịch sử và các yêu cầu hồ sơ theo nghiệp vụ.
- Hỗ trợ nhận biết hồ sơ còn thiếu, tài liệu có vấn đề và hợp đồng cần xử lý.
- Cung cấp Dashboard tương tác và biểu mẫu in để phục vụ tra cứu, rà soát.

## 2. Chức năng chính

Các chức năng dưới đây có Controller, Service hoặc Razor View tương ứng trong mã nguồn; điều này không thay thế kiểm thử trên database triển khai.

| Phân hệ | Chức năng đã có trong mã nguồn |
|---|---|
| Quản lý tổ chức | Danh sách, tìm kiếm, thêm, sửa, chi tiết và xóa đơn vị/khối, phòng ban theo quyền và ràng buộc dữ liệu. |
| Quản lý cán bộ | Tra cứu, lọc, phân trang, quản lý thông tin cá nhân/công tác; hiển thị chỉ báo chất lượng hồ sơ. |
| Trình độ chuyên môn | Thêm, sửa, xóa văn bằng; hỗ trợ nhiều văn bằng cùng trình độ; sắp xếp ngày tốt nghiệp giảm dần, ngày trống cuối cùng. |
| Hồ sơ cán bộ | Tài liệu hiện hành phía trên, tài liệu lịch sử trong khu vực thu gọn riêng; phân nhóm hồ sơ gốc, hợp đồng và quyết định/văn bản khác. |
| Loại tài liệu và quy tắc | Quản lý danh mục loại tài liệu và quy tắc yêu cầu hồ sơ, kiểm tra ràng buộc trước khi lưu; thứ tự loại tài liệu dùng quy tắc sắp xếp chung. |
| Tài liệu đính kèm | Thêm phiên bản, cập nhật thông tin; upload, thay thế, xóa tệp; xem và tải PDF/JPG/JPEG/PNG theo quyền. Mỗi tài liệu một tệp, tối đa 10 MB. |
| Dashboard Tổng quan | KPI, cơ cấu chất lượng, tài liệu theo loại/trạng thái, nhân viên theo tổ chức; bảng cán bộ cần bổ sung hoặc xử lý hồ sơ. |
| Dashboard Chất lượng hồ sơ | KPI yêu cầu/đáp ứng/thiếu, biểu đồ hồ sơ thiếu và tỷ lệ đáp ứng; bảng tài liệu sắp hết hạn và có vấn đề. |
| Dashboard Hợp đồng lao động | KPI hợp đồng, trạng thái hợp đồng theo cán bộ, biểu đồ tổ chức và bảng cần xử lý. |
| Phân tích tương tác | Bộ lọc, tìm kiếm, sắp xếp, phân trang; drill-down đơn vị/phòng ban và drill-through chi tiết trạng thái tài liệu/hợp đồng. |
| In biểu mẫu | Lý lịch trích ngang và danh mục hồ sơ; chọn nội dung in, bố cục A4, in/lưu PDF qua trình duyệt. |

Ba trang báo cáo nằm trong cùng mục **Báo cáo trực quan**: `/Dashboard`, `/Dashboard/ProblemDocuments`, `/Dashboard/Contract`. Website truy vấn SQL Server trực tiếp; không cần Power BI Service hoặc iframe. Route Power BI cũ chuyển hướng về Dashboard.

Chưa có chức năng xuất danh sách ra Excel/CSV trong mã nguồn hiện tại. Lưu PDF là chức năng in của trình duyệt, không phải dịch vụ tạo PDF phía server.

## 3. Công nghệ

| Thành phần | Công nghệ trong project |
|---|---|
| Web | ASP.NET Core MVC, .NET 10 (`net10.0`), C#, Razor Views |
| Dữ liệu | Entity Framework Core SQL Server và Design 10.0.13; SQL Server, database `VTB2` |
| Xác thực | Windows Authentication qua Negotiate 10.0.13; chính sách đọc và quản trị |
| Giao diện | Tabler UI 1.0.0 trên Bootstrap 5, CSS, JavaScript; jQuery và jQuery Validation |
| Biểu đồ | Apache ECharts, tài nguyên cục bộ trong `wwwroot/lib` |
| Báo cáo tham chiếu | Power BI Desktop/Project; công thức và định nghĩa báo cáo được chuyển sang SQL/C# và ECharts |
| Tệp và in | ASP.NET Core `IFormFile`/`FileStreamResult`, CSS in ấn và `window.print()`; không có thư viện xuất Excel/PDF chuyên dụng |

## 4. Kiến trúc và cơ sở dữ liệu

```text
Razor Views / JavaScript / ECharts
                ↓
         MVC Controllers
                ↓
       Services / EF Core
                ↓
       SQL Server: bảng + Views
```

Ứng dụng dùng database hiện có. Entity và Fluent API được quản lý trong `Data/`; các View đánh giá được ánh xạ dạng keyless. Đây không phải ứng dụng tự tạo database bằng migrations.

Các bảng chính: `division`, `department`, `employee`, `employee_qualification`, `document_type`, `document`, `requirement_rule`.

Các View phục vụ nghiệp vụ: `vw_document_quality`, `vw_employee_document_requirement`, `vw_employee_quality`, `vw_labor_contract_check`.

Database phải có cấu trúc, quy tắc và trigger quản lý tài liệu hiện hành tương thích với mapping. Không tự chạy `CREATE TABLE`, `ALTER TABLE`, migrations hoặc `EnsureCreated` trên database đang dùng. Script scaffold chỉ dùng để đối chiếu metadata và sinh bản ánh xạ riêng, không tự thay thế custom code.

## 5. Cấu trúc thư mục

Đường dẫn dưới đây tính từ thư mục chứa `Vtb.Hr.Web.csproj`:

```text
Controllers/          Điều phối request và API Dashboard
Data/                 Entity, DbContext và Fluent API
Models/               ViewModel, DTO và bộ lọc
Services/             Truy vấn, validation và xử lý tệp
Security/             Phân quyền Windows
Views/                Razor Views và mẫu in
wwwroot/              CSS, JavaScript, logo, thư viện giao diện
scripts/              Chạy ứng dụng, scaffold và kiểm thử
tests/Verification/  Chương trình kiểm thử C# và các script JavaScript
docs/                Tài liệu kỹ thuật, đối chiếu và giới hạn kiểm thử
Properties/           Launch profile phát triển
Program.cs
Vtb.Hr.Web.csproj
README.md
```

Tham khảo [mapping báo cáo](docs/REPORTS_SOURCE_MAP.md), [đối chiếu Dashboard](DASHBOARD_VALIDATION.md) và [bộ kiểm thử](tests/Verification/Verification.csproj). Tài liệu kỹ thuật được ghi ở nhiều thời điểm; cần đối chiếu với source và schema đang triển khai.

## 6. Yêu cầu môi trường

- Windows và .NET **10 SDK**; kiểm tra bằng `dotnet --list-sdks`.
- Visual Studio hỗ trợ .NET 10 với workload **ASP.NET and web development**, hoặc dotnet CLI.
- SQL Server/SQL Server Express với database `VTB2` tương thích đã được cấp sẵn; SSMS hữu ích khi kiểm tra schema.
- Tài khoản chạy tiến trình website có quyền SQL phù hợp và quyền ghi thư mục lưu tệp/Data Protection.
- Edge hoặc Chrome hỗ trợ xác thực Windows; Node.js nếu chạy kiểm thử JavaScript, `sqlcmd` nếu chạy đối chiếu SQL.
- Kết nối NuGet khi khôi phục các gói chưa được lưu trong cache.

README không cung cấp database nhân sự để chạy thử. Người triển khai cần database kiểm thử được cho phép sử dụng; không dùng dữ liệu nhân sự thật để tạo tình huống thử nghiệm.

## 7. Cài đặt và chạy

### Cấu hình riêng cho máy triển khai

Sau khi clone repository, mở terminal tại thư mục chứa `Vtb.Hr.Web.csproj` (vào `web/` nếu repository chứa cả các thư mục khác).

[Program.cs](Program.cs) đọc đúng khóa **`ConnectionStrings:VTB2`**. Dùng User Secrets trong môi trường Development, thay các placeholder bằng giá trị của môi trường của bạn:

```powershell
dotnet user-secrets set "ConnectionStrings:VTB2" 'Server=<SERVER>\<INSTANCE>;Database=VTB2;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=15'
dotnet user-secrets set "Access:Administrators:0" '<DOMAIN_OR_MACHINE>\<USER>'
```

Project đã có `UserSecretsId`, không cần chạy `user-secrets init`. Nếu có người chỉ xem:

```powershell
dotnet user-secrets set "Access:Readers:0" '<DOMAIN_OR_MACHINE>\<READER>'
```

User Secrets giữ cấu hình ngoài repository, **không mã hóa bí mật**. `TrustServerCertificate=True` chỉ phù hợp môi trường phát triển được kiểm soát; triển khai thực tế cần chứng chỉ SQL Server và cấu hình tin cậy phù hợp.

Danh sách quyền ứng dụng so khớp tên tài khoản Windows đã xác thực. Quyền website không tự cấp quyền SQL: với cấu hình trên, kết nối database sử dụng danh tính Windows của tiến trình website. Cần kiểm tra toàn bộ danh sách `Access` đang có; cấu hình mảng của .NET có thể hợp nhất theo chỉ số, không mặc định thay thế toàn bộ mảng.

### Khôi phục và build

```powershell
dotnet restore Vtb.Hr.Web.csproj --configfile NuGet.Config
dotnet build Vtb.Hr.Web.csproj --no-restore
```

### Chạy bằng dotnet CLI

```powershell
dotnet run --project Vtb.Hr.Web.csproj --launch-profile http
```

Profile `http` đặt môi trường Development và địa chỉ [http://localhost:5082](http://localhost:5082). Giữ tiến trình chạy khi sử dụng website; `Ctrl+C` để dừng. Dừng bản cũ trước khi build/chạy lại nếu cổng hoặc DLL bị khóa.

### Chạy bằng Visual Studio

Mở `Vtb.Hr.Web.csproj`, kiểm tra User Secrets, chọn launch profile **http**, rồi nhấn **F5** hoặc **Ctrl+F5**. Dùng tài khoản Windows được cấp quyền. HTTP localhost là cấu hình phát triển, không phải hướng dẫn triển khai Internet.

### Script có sẵn

[Chay-Website.cmd](Chay-Website.cmd) gọi [scripts/run-local.ps1](scripts/run-local.ps1). Script đặt Development, restore/build Debug rồi chạy DLL mới tại `http://localhost:5082`; build lỗi thì không khởi động website.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\run-local.ps1
```

Script yêu cầu sử dụng tài khoản Windows được cấp quyền trong cấu hình `Access`; thông báo khởi động không cấp quyền truy cập.

Lỗi **401**: chưa xác thực Windows; **403**: chưa có quyền ứng dụng. Lỗi truy cập database cần kiểm tra instance, database, quyền tài khoản chạy tiến trình và schema. Không tắt xác thực để khắc phục các lỗi này.

## 8. Kiểm thử

Bộ kiểm thử C# dùng TestServer và danh tính thử nghiệm, được loại khỏi project website:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify.ps1
```

Script restore/build vào thư mục kiểm tra và chạy `tests/Verification`; không phải cơ chế tạo dữ liệu trong VTB2. Một số kiểm tra tuyến có thể chỉ xác nhận ứng dụng xử lý lỗi SQL đúng cách, không chứng minh truy vấn dữ liệu thật đã thành công.

Ví dụ kiểm thử JavaScript riêng:

```powershell
node tests/Verification/sidebar.cjs
node tests/Verification/contract-report.cjs
node tests/Verification/print-donut.cjs
```

Script `scripts/verify-readonly.ps1` hiện cố định server của môi trường cũ, không nên dùng nguyên trạng trên máy khác. Có thể gọi trực tiếp file SQL chỉ đọc với server của bạn:

```powershell
sqlcmd -S '<SERVER>\<INSTANCE>' -E -d VTB2 -b -l 10 -i scripts/verify-readonly.sql
```

Rà soát truy vấn với schema hiện tại trước khi chạy; không đưa kết quả có thông tin cá nhân lên GitHub. Đối chiếu KPI với Power BI Desktop phải dùng cùng dữ liệu, thời điểm và phạm vi lọc. Build thành công không chứng minh KPI đúng hoặc toàn bộ giao diện đã được kiểm thử.

## 9. Bảo mật dữ liệu

- Xác thực Windows và phân quyền đọc/quản trị; thao tác ghi có kiểm tra quyền và chống giả mạo request.
- Tệp được lưu ngoài `wwwroot`, tại `App_Data/DocumentFiles`, truy cập qua Controller có phân quyền; đường dẫn tệp do server tạo. Kiểm tra định dạng/kích thước không thay thế quét mã độc.
- Khóa Data Protection ở `.keys/`, được bảo vệ bằng Windows DPAPI; cần cấu hình và quyền thư mục riêng khi triển khai máy chủ.
- Không commit bí mật, database backup, dữ liệu nhân sự, tệp đính kèm, khóa riêng, log hoặc ảnh chụp chứa dữ liệu thật.
- Rà soát cấu hình, script, `docs/`, `wwwroot/` và lịch sử Git trước khi công khai. `.gitignore` không loại bỏ file đã được Git theo dõi hoặc dữ liệu trong lịch sử commit.
- Cấu hình và một số script hiện vẫn chứa định danh máy/tài khoản của môi trường phát triển. Cần làm sạch trước khi công khai repository; README này không khẳng định repository đã an toàn để publish.

## 10. Giới hạn hiện tại

- Ba Dashboard và các luồng quản lý đã có mã nguồn; chưa xác nhận toàn bộ số liệu với Power BI Desktop và database thực tế trong lần rà soát tài liệu này.
- Lỗi tải bảng cán bộ cần xử lý ở Tổng quan đã có cơ chế chẩn đoán; chưa có xác nhận nguyên nhân và kết quả khắc phục trên môi trường thực tế. Không coi bảng đã được nghiệm thu chỉ vì route tồn tại.
- Schema/View/trigger có thể khác các snapshot cũ; cần đối chiếu database khi triển khai. Chưa có kiểm soát đồng thời bằng rowversion hoặc nhật ký kiểm toán toàn diện.
- Mẫu A4, trình duyệt, quyền Windows và thao tác ghi cần kiểm thử trên môi trường riêng. Chưa xác nhận triển khai IIS/production; cấu hình mặc định phục vụ localhost.
- Chưa có xuất Excel/CSV. Không tuyên bố toàn bộ báo cáo Power BI đã được chuyển đổi hoặc nghiệm thu chỉ từ ba tab đang có.
