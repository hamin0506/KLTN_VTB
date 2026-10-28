# Đồng bộ hai cột View đã xóa — 20/10/2026

Người dùng xác nhận:
- dbo.vw_employee_quality đã xóa is_kpi_scope.
- dbo.vw_employee_document_requirement đã xóa has_document.
- is_kpi_scope ở dbo.vw_document_quality vẫn còn.

## Sửa website
- Data/QualityViews.cs: bỏ đúng hai property tương ứng, để EF không SELECT các cột đã xóa khi tải toàn bộ View entity. Giữ DocumentQuality.IsKpiScope.
- Services/DashboardService.cs: MissingQuery dùng tập employee_id có employment_status IN ('DangLamViec','TamHoanHDLD'), thay e.is_kpi_scope=1. Đây chính xác là CASE định nghĩa cũ trong database/schema.sql; không bỏ giới hạn KPI, không thêm DISTINCT vào tập kết quả và không thay số lượng dòng của View.
- Services/QualityService.cs: KpiOnly dùng cùng hai trạng thái trên employee, thay tham chiếu cột View đã xóa.
- has_document không được dùng bởi Service hoặc Razor hiện tại; chỉ cần bỏ mapping, không thay công thức requirement_status.
- Tests/Verification/Program.cs: xác nhận SQL dịch cho bảng vẫn có cả hai trạng thái; full entity SELECT không chứa hai cột đã xóa; View tài liệu vẫn dùng is_kpi_scope; giữ kiểm tra phân trang/tham số.

Không thực thi ALTER VIEW, không sửa DB, không chạy schema.sql, không thay Power BI hoặc dữ liệu cán bộ. schema.sql trên đĩa vẫn là bản15/10 nên không được coi là snapshot cấu trúc hiện tại. Cần xuất lại schema từ SQL Server khi người dùng muốn cập nhật tài liệu cơ sở dữ liệu.

## Kiểm tra
Build: docs/build-view-schema.txt.
TestServer và SQL translation: docs/verification-view-schema.txt.
Các kiểm tra này không chứng minh website đã đọc được SQL Server thật; môi trường tác vụ trước đó không truy cập được instance ACER\SQLEXPRESS. Cấu trúc mới dựa trên xác nhận của người dùng.

## Áp dụng
Dừng phiên debug cũ, Build project D:\KLTN\Web\Vtb.Hr.Web.csproj trong Visual Studio rồi F5. Tải lại /Dashboard, kiểm tra bảng cuối; thử lọc, tìm kiếm và chuyển trang. Nếu còn SqlException, lấy lỗi gốc từ Output, vì dòng thông báo ở UI đang là thông báo chung.

Kết quả cuối: dotnet build thành công,0warnings/0errors;201kiểm tra đạt,0thất bại. Output build riêng .scaffold/view-schema-final. Chưa xác minh đọc dữ liệu trên SQL Server thật.
