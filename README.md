# VTB People — hướng dẫn bản cập nhật

Ứng dụng hiện có tại D:\KLTN\Web, ASP.NET Core MVC .NET 10, EF Core SQL Server, Razor và Tabler/Bootstrap 5. Không tạo lại project hoặc sửa database.

## Chạy bản mới

1. Dừng cửa sổ đang chạy bản website cũ bằng Ctrl+C để giải phóng cổng 5082.
2. Mở PowerShell bằng tài khoản Windows ACER\admin có quyền truy cập VTB2 và chạy:

```powershell
powershell -ExecutionPolicy Bypass -File D:\KLTN\Web\scripts\run-local.ps1
```

3. Mở **http://localhost:5082**. Địa chỉ cố định qua các lần chạy. Giữ cửa sổ chạy ứng dụng mở; Ctrl+C sẽ dừng website. Hiện chưa cài dịch vụ tự khởi động hoặc IIS.

Nếu trình duyệt hỏi đăng nhập, sử dụng tài khoản Windows được cấp quyền. Không nhập mật khẩu vào mã nguồn hoặc gửi qua chat. 401 nghĩa là chưa xác thực; 403 nghĩa là tài khoản chưa được cấp quyền. Không bỏ cơ chế xác thực để xử lý lỗi này.

Bản build mới nằm trong .scaffold/final-review nhằm tránh khóa DLL của tiến trình cũ. Không dùng các file DLL cũ trong bin hoặc bộ publish IIS trước lần cập nhật này.

## Cấu hình

- Kết nối Development: ACER\SQLEXPRESS, database VTB2, Windows Authentication; cấu hình trong appsettings.Development.json. Không có mật khẩu SQL.
- Access:Administrators trong appsettings.json đã đặt ACER\admin; Readers đang rỗng. Người chỉ xem không được mở form ghi hoặc gửi thao tác ghi. Tài khoản chạy tiến trình website cần quyền SQL phù hợp; danh sách quyền website không tự cấp quyền SQL.
- PowerBI:EmbedUrl chưa có. Cần URL Secure Embed dạng https://app.powerbi.com/reportEmbed?reportId=... và quyền Power BI phù hợp. File D:\KLTN\PowerBI\Dashboard_KLTN.pbix không thể nhúng trực tiếp. Không dùng Publish to web.
- Data Protection dùng .keys và Windows DPAPI. Thay tài khoản chạy hoặc triển khai IIS cần kiểm tra quyền thư mục và cấu hình xác thực riêng.

## Các phân hệ

Menu nhóm gồm Tổng quan, Tổ chức (đơn vị/phòng ban), Cán bộ (danh sách/hồ sơ tổng hợp), Tài liệu (tài liệu/loại tài liệu), Chất lượng (dashboard/bốn View/quy tắc), Power BI. Các trang nghiệp vụ dùng truy vấn database; khi kết nối lỗi trả trang 503, không thay bằng số giả.

CRUD bảng division, department, employee, document, document_type; requirement_rule hỗ trợ thêm/sửa/chi tiết. Hồ sơ cán bộ là trang tổng hợp, không có bảng mới. Tài liệu hỗ trợ thông tin đường dẫn, chưa có upload/download file. Mỗi cán bộ/loại chỉ được có một tài liệu hiện hành theo filtered unique index.

Mapping sáu bảng và bốn View được đối chiếu schema.sql; View dùng keyless/ToView. Đây là mapping dựa trên script, chưa xác nhận reverse-engineer từ database sống. Không gọi migration/EnsureCreated hoặc thực thi schema.sql. scripts/scaffold.ps1 vẫn là công cụ đối chiếu metadata khi kết nối được.

## Build và kiểm thử

```powershell
powershell -ExecutionPolicy Bypass -File D:\KLTN\Web\scripts\verify.ps1
```

Script restore/build và chạy 70 kiểm thử cách ly (không ghi database). Tài khoản giả phục vụ kiểm thử chỉ nằm trong tests/Verification, bị loại khỏi project website.

Đối chiếu dữ liệu bằng truy vấn chỉ đọc trên máy có sqlcmd và truy cập SQL:

```powershell
powershell -ExecutionPolicy Bypass -File D:\KLTN\Web\scripts\verify-readonly.ps1
```

Đọc docs/verification-final.md để phân biệt các kiểm thử đã chạy với phần chưa xác minh. Chưa chạy INSERT/UPDATE/DELETE kiểm thử trên VTB2. Chỉ kiểm thử ghi sau khi chủ dữ liệu phê duyệt phạm vi hoặc database thử riêng.

## Các giới hạn cần biết

SQL Views có thể nhân bản dòng khi một loại tài liệu có nhiều quy tắc, và không áp dụng thống nhất trạng thái/ngày hiệu lực quy tắc. Xem docs/SQL-VIEW-NOTES.md trước khi thay đổi nghiệp vụ. Website hiển thị cảnh báo và giữ kết quả SQL; không tự sửa View. Schema chưa có rowversion/audit, nên chưa có kiểm soát toàn diện việc hai người sửa cùng một bản ghi.

Bộ cài IIS cũ chưa cập nhật theo cơ chế xác thực mới; chưa triển khai. Website hiện chỉ được cấu hình localhost.

## Cập nhật cách chạy
Đã build lại bin Debug/Release. scripts/run-local.ps1 hiện restore và build Debug từ source trước mỗi lần chạy, không còn chạy bản .scaffold/final-review. Có thể mở D:\KLTN\Web\Chay-Website.cmd. Nếu build lỗi, script dừng thay vì mở bản cũ.

## Executive Overview (18/10/2026)
Dashboard hiện dùng SQL Server và ECharts cục bộ, không iframe. Có 6 KPI, 3 biểu đồ, slicer và bảng thiếu hồ sơ phân trang. Xem DASHBOARD_VALIDATION.md để biết phạm vi kiểm thử và các số liệu chưa đối chiếu. Chạy Chay-Website.cmd hoặc Rebuild trong Visual Studio sau khi dừng tiến trình cũ.
