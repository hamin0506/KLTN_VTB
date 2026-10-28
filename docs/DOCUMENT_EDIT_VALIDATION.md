# Chỉnh sửa thông tin tài liệu — 22/10/2026

Trạng thái: đã triển khai sửa Ngày cấp/Loại tài liệu, kiểm tra xung đột và dropdown StorageForm dùng chung theo danh sách người dùng xác nhận. Chưa kiểm chứng thao tác ghi trên SQL Server thực tế.

## Nguyên nhân và thay đổi
- Record.cshtml đã hiển thị IssueDate/DocumentTypeId dạng chỉ đọc; CatalogController.Save đã loại hai trường khỏi allowlist. Đã mở lại cả Razor và POST. EmployeeId, DocumentId, CreatedAt, IsCurrent vẫn không được bind từ form.
- Sửa bản hiện hành dùng entity đang được theo dõi và SaveChanges (UPDATE), không Add. Lịch sử vẫn chỉ xem. Tạo phiên bản mới dùng Create riêng như trước.
- CatalogService.Validate kiểm tra tài liệu hiện hành cùng cán bộ/loại, loại trừ chính ID đang sửa. Chạy trong transaction Serializable. Unique index là bảo vệ cuối cùng; lỗi unique khi lưu tài liệu được chuyển thành lỗi tiếng Việt tại DocumentTypeId.
- Không tự đổi is_current hoặc tài liệu khác. Trigger AFTER INSERT không chạy khi sửa metadata; sửa ngày cấp không tự xếp lại current. Đây là hành vi theo yêu cầu hiện tại, không phải tự tái chọn phiên bản.
- Giữ kiểm tra tồn tại loại tài liệu/cán bộ và ExpiryDate >= IssueDate. Date picker dùng ISO yyyy-MM-dd; thông báo lỗi parse ngày/ID được chuyển sang tiếng Việt.
- Form đọc lại ModelState.AttemptedValue khi lỗi để giữ lựa chọn đã gửi. Chỉ normalize trường văn bản thực sự có trong request; không trim StorageForm để tránh tự biến đổi giá trị.
- Không thay CreatedAt; UpdatedAt vẫn lấy SYSDATETIME theo cơ chế hiện có. FilePath vắng trong POST không bị gán lại; form vẫn hiện tham chiếu tệp cũ. Chức năng chỉ lưu đường dẫn, không quản lý upload.
- Sau lưu redirect Profiles.Details: truy vấn lại document_type, document, quality/requirement/contract Views nên nhóm và cảnh báo được tính lại từ DB. Dashboard tiếp tục truy vấn Views cũ. Không sửa SQL, Views, rule hoặc Dashboard.

## File thay đổi
- Controllers/CatalogController.cs
- Services/CatalogService.cs
- Views/Shared/Record.cshtml
- tests/Verification/Program.cs
- scripts/inspect-storage-form-readonly.sql (mới, chỉ SELECT)
- docs/build-document-edit.txt, docs/test-document-edit.txt và báo cáo này

## Kiểm thử
Build thành công: 0 lỗi, 0 cảnh báo. Verification: 241 PASS, 0 FAIL.
Đã kiểm tra render form, date input + giá trị đã lưu, dropdown loại + option chọn đúng, không có input EmployeeId/IsCurrent/CreatedAt, giữ tham chiếu tệp, validation ngày hết hạn, phân nhóm theo loại mới và các kiểm tra phân quyền/hồi quy hiện có.
Chưa kiểm thử UPDATE thực tế (sửa riêng/tổng hợp ba trường), xung đột unique thực tế, hoặc chất lượng sau ghi SQL. Không dùng dữ liệu nhân sự thật để thử. Chưa kiểm tra trực quan bằng browser.

## StorageForm: danh sách đã xác nhận
Schema trong repository: nvarchar(50) NULL; không có CHECK cố định cho storage_form. Đây không phải enum và không thể suy ra một danh sách mã hợp lệ chỉ từ schema. SQL Server ACER\SQLEXPRESS không truy cập được (Error Locating Server/Instance / login timeout).
Người dùng xác nhận document có: Bản chính, Bản công chứng, Bản gốc, Bản photo; requirement dùng bản gốc/bản công chứng. Đây là danh mục nghiệp vụ do người dùng xác nhận, không phải danh sách CHECK đã đọc trực tiếp từ server.

Models/DocumentStorageForms.cs là nguồn dùng chung; nhãn hiển thị và giá trị lưu giống hệt nhau. Không thêm mã ánh xạ mới. Chưa xác định gửi chuỗi rỗng được MVC chuyển thành NULL theo schema. Form chọn đúng giá trị cũ; giá trị cũ không được hỗ trợ vẫn hiển thị rõ và có cảnh báo, không tự đổi sang giá trị khác. Server Document.Validate kiểm tra ordinal trong đúng danh sách hoặc NULL; không tự trim/case-fold giá trị. Không sửa requirement hoặc SQL Views.

Đã bổ sung vào các file thay đổi: Models/DocumentStorageForms.cs (mới), Data/Records.cs; service cung cấp options cho mọi form document, Razor chung dùng select, không có textbox tự nhập StorageForm. tests/Verification có kiểm thử từng giá trị hợp lệ/NULL, giá trị sai và giả lập HTTP POST qua MVC model validation trong endpoint test-only không ghi dữ liệu. Endpoint test không nằm trong assembly website và không thay quyền/CSRF của CRUD thực tế.

## Ma trận kiểm tra yêu cầu
| Nội dung | Kết quả |
|---|---|
| Ngày cấp editable, loại select, giá trị hiện tại | PASS render Razor qua TestServer |
| Sửa riêng / đồng thời các metadata rồi ghi DB | Chưa kiểm thử ghi SQL; mã UPDATE cùng ID đã rà soát |
| ExpiryDate < IssueDate | PASS validation model |
| Chuyển sang loại có current khác | Đã thêm kiểm tra trong transaction + bắt lỗi unique; chưa thử cạnh tranh trên SQL |
| StorageForm ngoài danh sách | PASS model + POST giả lập MVC trả 400 |
| Thêm/Sửa dùng cùng dropdown, chọn đúng giá trị | PASS render cả hai chế độ |
| Giá trị storage cũ ngoài danh sách | PASS cảnh báo, giữ giá trị hiển thị |
| Chuyển nhóm sau đổi loại | PASS kiểm tra ProfilePresentation với fixture; chưa thử sau SQL UPDATE |
| Cảnh báo mới / KPI sau thay StorageForm | Redirect đọc lại SQL Views đã rà soát; chưa xác minh giá trị thực tế |
| Tham chiếu tệp, trường kỹ thuật | PASS render; allowlist/normalization đã rà soát; chưa xác minh ghi SQL |
| CRUD / phân quyền / CSRF hồi quy | Bộ verification PASS; không thực hiện ghi dữ liệu thật |

Lưu ý cơ chế hiện hành: sửa thông tin không chạy trigger AFTER INSERT, không thay is_current của bản nào và không thăng bản lịch sử của nhóm cũ khi chuyển loại. Giữ nguyên việc bản lịch sử chỉ xem. Không tự tạo chức năng sửa lịch sử hoặc sửa schema.
