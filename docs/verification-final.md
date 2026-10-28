# Báo cáo rà soát và kiểm thử website KLTN

Ngày: 15/10/2026. Source: D:\KLTN\Web. Database mục tiêu: VTB2 tại ACER\SQLEXPRESS. Báo cáo này thay thế các báo cáo kiểm thử giai đoạn đầu.

## Kết quả chính

Đã tái cấu trúc project hiện có, bổ sung các phân hệ theo yêu cầu và cấu hình Windows Authentication với quản trị viên ACER\admin. dotnet restore và dotnet build thành công; lần build cuối 0 warning, 0 error. Bộ TestServer kiểm thử cách ly: **70 passed, 0 failed** (log verification-latest.txt).

Chưa thể kết luận toàn bộ website hoàn chỉnh: môi trường thực thi không truy cập được SQL Server thật (named instance error 26; Shared Memory access denied/error 5). Windows xác thực thật chưa thành công trong phiên thử (401). Chưa đối chiếu KPI với dữ liệu VTB2, chưa kiểm thử CRUD ghi và chưa kiểm tra trực quan mọi kích thước màn hình. Không dùng các kết quả kiểm thử cách ly để khẳng định các phần này đã hoạt động trên máy người dùng.

## Bảng trạng thái

| Phân hệ | Chức năng | Trạng thái | Kết quả kiểm thử | Ghi chú |
|---|---|---|---|---|
| Project | Restore, build | Hoàn thành và đã kiểm thử | 0 lỗi, 0 cảnh báo | Output .scaffold/final-review |
| EF Core | Mapping 6 bảng, 4 View | Hoàn thành và đã kiểm thử đối chiếu script | Tên cột, PK, FK, filtered index, keyless được kiểm tra | Chưa xác minh metadata DB sống |
| Bảo mật | Chính sách quản trị/chỉ xem, chống CSRF | Hoàn thành và đã kiểm thử cách ly | Anonymous 401; ngoài whitelist 403; reader bị chặn ghi và form ghi, cả route chữ thường; admin POST thiếu CSRF 400 | Fake identity chỉ ở test harness |
| Bảo mật | Windows Authentication trên trình duyệt thật | Bị chặn bởi cấu hình hoặc phụ thuộc | Yêu cầu thật trong phiên trả 401 | Cần kiểm tra bằng ACER\admin trên máy người dùng |
| Tổ chức | Division CRUD, tìm kiếm/phân trang | Đã triển khai nhưng chưa kiểm thử CRUD thật | Render form và chính sách quyền được kiểm tra | Giữ validation và xử lý lỗi UNIQUE/FK |
| Tổ chức | Department CRUD, dropdown/lọc đơn vị | Đã triển khai nhưng chưa kiểm thử CRUD thật | Render form, route và xử lý DB lỗi được kiểm tra | Cần đối chiếu dữ liệu thật |
| Cán bộ | CRUD employee, tìm kiếm/lọc | Đã triển khai nhưng chưa kiểm thử CRUD thật | Validation mã/ngày/trạng thái, form và route đã kiểm tra | Không ghi dữ liệu thử |
| Hồ sơ cán bộ | Tổng hợp cá nhân, tổ chức, tài liệu, thiếu/cảnh báo | Đã triển khai nhưng chưa kiểm thử dữ liệu thật | Razor render mẫu kiểm thử thành công | Không tạo bảng hồ sơ |
| Tài liệu | CRUD document, lọc, trạng thái, hiện hành | Đã triển khai nhưng chưa kiểm thử CRUD thật | Validation và mapping unique được kiểm tra | Cần kiểm tra tranh chấp/current unique trên DB thử được duyệt |
| Loại tài liệu | CRUD document_type, nhóm/mô tả | Đã triển khai nhưng chưa kiểm thử CRUD thật | Form/metadata đã kiểm tra | Chặn đổi các mã có ý nghĩa trong SQL |
| Quy tắc | Thêm/sửa/chi tiết requirement_rule | Đã triển khai nhưng chưa kiểm thử ghi thật | Validation ngày/mức độ/ngưỡng đã kiểm tra | UI giới hạn một quy tắc/loại; chưa sửa SQL; không có xóa quy tắc |
| Tổng quan | 7 KPI, biểu đồ trạng thái và cảnh báo | Đã triển khai nhưng chưa kiểm thử số liệu thật | Razor và dịch LINQ sang SQL được kiểm tra | KPI không thay bằng số giả khi SQL lỗi |
| Chất lượng | Bốn danh sách từ 4 SQL Views | Đã triển khai nhưng chưa kiểm thử dữ liệu thật | Route, SQL translation, render và 503 được kiểm tra | View chỉ đọc; chưa đối chiếu số liệu/lọc với VTB2 |
| Power BI | Kiểm tra Secure Embed URL và trang hướng dẫn | Hoàn thành và đã kiểm thử cách ly | Chấp nhận reportEmbed hợp lệ, từ chối Publish to web và URL sai | EmbedUrl hiện rỗng |
| Power BI | Hiển thị báo cáo thật | Bị chặn bởi cấu hình hoặc phụ thuộc | Chưa có URL và phiên đăng nhập Power BI để thử | PBIX không nhúng trực tiếp |
| UI | Nhóm menu, active, breadcrumb, form, bảng, badge, empty/loading | Đã triển khai; kiểm thử render một phần | Razor render không lỗi; CSS/JS trả 200 trong lần chạy Kestrel | Chưa kiểm tra trực quan đầy đủ desktop/mobile |
| Lỗi kết nối | Trang 503 có hướng dẫn | Hoàn thành và đã kiểm thử | Các route đọc dữ liệu trả 503 khi SQL không truy cập được | Không lộ lỗi SQL thô cho người dùng |
| Triển khai | Địa chỉ localhost cố định | Đã cấu hình nhưng chưa kiểm thử bản cuối tại cổng 5082 | Script chạy dùng localhost:5082; bản app đã chạy kiểm thử riêng cổng 5093 | Phải dừng tiến trình cũ trước |
| Triển khai | IIS/tự khởi động | Chưa hoàn thành | Chưa cài đặt | Bộ IIS cũ không dùng cho bản mới |
| Tệp tài liệu | Upload/download | Chưa triển khai | Không thuộc phần CRUD metadata đã làm | Chỉ lưu thông tin đường dẫn |

## Phạm vi kiểm thử và các sửa lỗi

Kiểm thử tự động bao gồm mapping sáu bảng, bốn View; validation trường/ngày/mức độ; whitelist Windows; chính sách quyền kể cả URL chữ thường; CSRF; kiểm tra URL Power BI; render trang dùng dữ liệu kiểm thử cách ly; dịch các biểu thức LINQ quan trọng; xử lý mất kết nối. Đây không phải kiểm thử toàn bộ truy vấn với dữ liệu thực hay kiểm thử giao diện tương tác trong trình duyệt.

Đã sửa biểu thức GroupBy/Distinct không dịch được sang SQL bằng truy vấn tương quan; bổ sung jQuery trước validation scripts; dùng tên action chuẩn từ MVC để chặn đường dẫn viết thường; bảo vệ các mã loại tài liệu SQL sử dụng cố định. Build và 70 kiểm thử cuối đã chạy sau các sửa lỗi này.

## Những điểm nghiệp vụ cần quyết định

Xem SQL-VIEW-NOTES.md: requirement_rule thiếu FK/unique; các View áp dụng active/ngày hiệu lực không thống nhất; nhiều quy tắc có thể cho nhiều trạng thái trên cùng tài liệu. KPI đếm DISTINCT document_id trong từng trạng thái, các nhóm có thể chồng lấn. Tổng tài liệu gồm lịch sử nhưng chất lượng từ View chỉ xét tài liệu hiện hành. DAY_DU mô tả đầy đủ hồ sơ theo View, không bảo đảm mọi tài liệu còn hạn/hợp lệ.

Không sửa logic View để che các bất nhất. Không có audit/rowversion trong schema; chưa bảo đảm phát hiện mọi sửa đổi đồng thời từ lúc mở form. Các kiểm tra SQL/FK/unique và transaction không thay thế optimistic concurrency toàn diện.

## Kiểm tra tiếp trên máy người dùng

1. Dừng bản website cũ, chạy scripts/run-local.ps1; mở http://localhost:5082 bằng tài khoản Windows ACER\admin.
2. Kiểm tra Tổng quan, Tổ chức, Cán bộ, Tài liệu, bốn trang Chất lượng. So sánh dữ liệu và lọc với SSMS, kiểm tra layout desktop/mobile. Gửi lại thông báo lỗi nếu có; không gửi mật khẩu hoặc toàn bộ dữ liệu nhân sự.
3. Chạy scripts/verify-readonly.ps1 (chỉ SELECT/CTE) để đối chiếu KPI và phát hiện quy tắc trùng/xung đột. Nếu thiếu sqlcmd, mở scripts/verify-readonly.sql trong SSMS và thực thi tại VTB2.
4. Phê duyệt database thử/phạm vi dữ liệu trước kiểm thử thêm/sửa/xóa; khi đó kiểm tra PK/FK/UNIQUE/CHECK, lưu tài liệu hiện hành trùng, quy tắc và trường hợp đồng thời. Chưa thực hiện bước ghi này trong phiên.
5. Cung cấp Secure Embed URL sau khi xuất bản báo cáo và cấp quyền thích hợp để kiểm thử Power BI thật.

Không chạy schema.sql, migration, tạo bảng hoặc INSERT/UPDATE/DELETE kiểm thử trên VTB2. File schema giữ SHA256 ED6BAB17C753BB8A5EB2038C44B2FCF40A565C7B1B98E26E185E68798804D6FB. Không triển khai IIS trong lần này.
