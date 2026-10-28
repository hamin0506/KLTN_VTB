# Hình thức lưu trữ: đúng 5 lựa chọn — 23/10/2026

Yêu cầu mới thay thế cơ chế form phụ thuộc nhóm/requirement_rule ở các lượt trước.

## Triển khai
Nguồn duy nhất Models/DocumentStorageForms.cs trả đúng thứ tự: -- (value rỗng), Bản gốc, Bản công chứng, Bản photo, Bản copy. Bản chính không còn được chọn. Mọi form Thêm/Sửa dùng cùng nguồn, luôn hiện select không required/disabled, không đọc rule để quyết định danh sách hoặc chặn null.

Normalize chỉ chuyển C# null, chuỗi rỗng/whitespace hoặc chuỗi NULL sau Trim không phân biệt hoa thường thành null; các giá trị khác giữ nguyên để validate. Chuỗi -- không được lưu: nhãn -- có option value rỗng. Binder riêng cập nhật đúng ModelState field trước validation; không Clear ModelState. Validation vẫn chặn mã ngoài bốn giá trị hoặc null.

Khi sửa, entity được EF theo dõi được chuẩn hóa trước binding. Vì giữ nguyên OriginalValue, EF đánh dấu StorageForm thay đổi từ literal NULL thành null khi lưu; không làm mất dấu thay đổi do chuẩn hóa lúc materialize. Nếu field vắng trong request, binder không ghi đè giá trị đã theo dõi. Ngày/ID/quyền/current/timestamp/file giữ cơ chế hiện có; không thêm UPDATE hàng loạt.

SQL Views và requirement_rule không đổi. Tài liệu thiếu hình thức theo rule có thể bị đánh giá chưa đáp ứng sau khi lưu; form không dùng kết quả đánh giá để cản lưu.

Giá trị cũ khác danh sách, ví dụ Bản chính: hiển thị cần kiểm tra ngoài dropdown, không sinh lựa chọn thứ 6. Với bản ghi sửa, server yêu cầu checkbox xác nhận thay thế cùng lựa chọn hợp lệ trước khi ghi; không tự đổi Bản chính thành Bản copy hoặc null. Literal NULL/whitespace không cần checkbox vì người dùng đã yêu cầu chuẩn hóa chúng khi lưu từng bản ghi.

## Hiển thị
- Form Thêm/Sửa: literal NULL/null/whitespace -> chọn --.
- Chi tiết tài liệu, danh sách tài liệu/hồ sơ cán bộ: helper chung trả --; giá trị khác danh sách có chú thích cần kiểm tra.
- Bảng drill-through: thêm thuộc tính DTO StorageDisplay dùng cùng helper; JS chỉ dùng thuộc tính hiển thị. Không sửa truy vấn, sort, filter, DISTINCT hoặc KPI. RequiredStorage của rule giữ nguyên.

## File sửa/tạo
- Models/DocumentStorageForms.cs
- Models/StorageFormModelBinder.cs (mới)
- Models/WorkspaceModels.cs
- Models/DocumentDrillModels.cs
- Data/Records.cs
- Controllers/CatalogController.cs
- Controllers/ProfilesController.cs
- Services/CatalogService.cs
- Views/Shared/Record.cshtml
- Views/Profiles/_DocumentGroups.cshtml
- wwwroot/js/document-drill.js
- tests/Verification/Program.cs

Đã bỏ các file hết hiệu lực: Models/DocumentStoragePolicy.cs, wwwroot/js/document-storage.js, tests/storage-policy-client.cjs. Các test rule-form cũ được thay bằng test đặc tả mới, không còn yêu cầu required_storage_form phải khớp để lưu.

## Schema và CHECK
Đã thử truy vấn metadata chỉ đọc tới ACER\SQLEXPRESS/VTB2, thất bại Error Locating Server/Instance và login timeout. Không xác nhận được CHECK constraints thực tế trên server, chưa xác nhận Bản copy được database live chấp nhận.

Theo Database/schema.sql đang có: storage_form nvarchar(50) NULL; không có CHECK giới hạn danh mục storage_form. Do đó schema repository hỗ trợ yêu cầu. Không sửa schema, constraint, View hoặc requirement_rule. Script scripts/inspect-storage-form-readonly.sql có thể dùng trong SSMS để kiểm tra cấu trúc live; chưa có tên constraint cản trở nào được xác nhận.

## Kết quả kiểm thử
- dotnet build: thành công, 0 warning, 0 error (docs/build-storage-five.txt).
- Verification: 267 PASS, 0 FAIL (docs/test-storage-five.txt).
- node --check wwwroot/js/document-drill.js: thành công.
- GET render Thêm/Sửa/đổi loại/dữ liệu cũ: đúng 5 option, không required/disabled, không nạp JS rule cũ.
- POST qua TestServer: cả bốn giá trị mới được chấp nhận; rỗng/NULL/whitespace bind thành null; literal --, Bản chính và mã giả bị từ chối. Sai ngày vẫn trả lỗi, không bị bỏ qua bởi normalization.
- EF ChangeTracker không kết nối database: original literal NULL -> null được đánh dấu modified; tham chiếu tệp không bị sửa.
- Hồi quy quyền/CRUD render/Dashboard query translation pass. Không thay đổi logic KPI.

CHƯA kiểm thử INSERT/UPDATE thật, dữ liệu sau commit hoặc CHECK live; các HTTP POST thử là endpoint validation test-only, không phải bằng chứng đã SaveChanges thành công trên VTB2. Không tạo/sửa dữ liệu nhân sự thật để thử. Chưa kiểm tra trực quan bằng browser.
