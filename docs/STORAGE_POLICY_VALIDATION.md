# Quy tắc hình thức lưu trữ theo loại tài liệu — 22/10/2026

## Kết quả triển khai
- Thay hướng dẫn áp dụng chung của lượt trước bằng chính sách đọc từ requirement_rule theo từng document_type_id, không hardcode ID hay tự miễn trừ toàn bộ nhóm.
- Ngày hiện tại lấy từ SQL Server; lọc is_active=1, effective_from <= ngày hiện tại và effective_to NULL hoặc >= ngày hiện tại, đúng với View trong schema.sql đang lưu ở repository.
- Một rule hiệu lực với required_storage_form NULL: không bắt buộc. Với nhóm Hợp đồng lao động, hiển thị Không áp dụng. Dropdown vẫn được giữ để người dùng xem/giữ giá trị cũ hoặc chủ động chọn Để trống / Xóa giá trị đã chọn. Không tự đổi/xóa giá trị khi đổi loại.
- Có required_storage_form: dropdown required, server yêu cầu đúng giá trị đó. Bốn giá trị hợp lệ dùng lại từ danh sách người dùng xác nhận: Bản chính, Bản công chứng, Bản gốc, Bản photo. Nhãn và giá trị lưu giống nhau; không tự tạo mã hoặc dùng chuỗi Không áp dụng để lưu.
- Nếu rule yêu cầu giá trị ngoài danh sách đã xác nhận, chặn và báo cần kiểm tra cấu hình; không mở rộng dropdown bằng dữ liệu tùy ý. So khớp chính xác chuỗi; trường hợp rule có cách viết khác cần đối chiếu, không tự đổi dữ liệu.
- Nhiều rule đang hiệu lực: chặn lưu, không tùy ý chọn một rule. Không có rule hiệu lực: báo rõ chưa có rule, cho phép NULL theo schema và LEFT JOIN của View trong repository; không khẳng định đây là rule miễn trừ đã được cấu hình.
- Form Thêm/Sửa lấy cùng policy. Đổi loại cập nhật required/hướng dẫn qua JS nhưng không đổi value hoặc disable field. POST đọc lại chính sách trong transaction Serializable trước SaveChanges, không tin metadata của client.
- Schema, SQL Views, requirement_rule và dữ liệu nhân sự thật không bị sửa. Giữ nguyên quyền, CSRF, unique-current check, timestamp, file reference và cơ chế phiên bản.

## Kiểm tra SQL và giới hạn
schema.sql: dbo.document.storage_form nvarchar(50) NULL; không có CHECK enum cho trường này. vw_document_quality chỉ đánh giá không đúng hình thức nếu required_storage_form IS NOT NULL và storage_form NULL hoặc khác giá trị yêu cầu. Do đó hợp đồng có rule NULL không bị đánh giá lỗi *vì storage_form NULL*; lỗi ngày cấp, hết hạn và các điều kiện khác vẫn giữ.

Đã thử SELECT metadata/rules/View bằng Windows Authentication vào ACER\SQLEXPRESS/VTB2; thất bại Error Locating Server/Instance và login timeout. Chưa thể liệt kê chính xác các loại hợp đồng đang được miễn trên database live, kiểm tra CHECK mới nếu có hoặc xác nhận View live đã giống file schema.sql. Không dùng seed cũ làm bằng chứng dữ liệu live.

Script scripts/inspect-storage-policy-readonly.sql chỉ SELECT: trả cấu trúc/constraints, định nghĩa View và danh sách mã/tên/nhóm loại tài liệu cùng chính sách OPTIONAL / REQUIRED / NO_EFFECTIVE_RULE. Có thể chạy trong SSMS để đối chiếu chính xác.

Nếu rule live của hợp đồng vẫn có required_storage_form khác NULL, web tiếp tục yêu cầu giá trị và thông báo rõ. Để miễn trừ loại đó, người dùng phải quyết định thay đổi metadata rule; chưa thực hiện. Nếu View live không lọc rule hiệu lực như repository hoặc có kiểm tra riêng chặn NULL, cần rà soát/điều chỉnh SQL sau khi được phép; chưa sửa hoặc bỏ qua cảnh báo trên web.

## Các file
- Models/DocumentStoragePolicy.cs: chính sách và validation dùng chung.
- Models/WorkspaceModels.cs: map chính sách theo ID cho EditRecord.
- Services/CatalogService.cs: đọc rule/loại từ SQL, kiểm tra lại lúc lưu.
- Controllers/CatalogController.cs: truyền policy vào form.
- Views/Shared/Record.cshtml: required ban đầu, thông báo theo rule, metadata cho từng option, lựa chọn xóa rõ ràng.
- wwwroot/js/document-storage.js: cập nhật trạng thái khi đổi loại, bảo toàn lựa chọn.
- tests/Verification/Program.cs và tests/storage-policy-client.cjs: hồi quy model/render/request và JS.
- scripts/inspect-storage-policy-readonly.sql: audit chỉ đọc.

## Kết quả kiểm thử
- dotnet build: thành công, 0 warning, 0 error (docs/build-storage-policy.txt).
- Verification: 255 PASS, 0 FAIL (docs/test-storage-policy.txt).
- Thêm/Sửa render đúng option hiện tại, trường required theo policy; model/POST giả lập blank -> NULL; optional contract giữ giá trị cũ; rule required từ chối blank/sai; rule chồng lấn/không nhận diện được bị chặn; quyền truy cập và các hồi quy cũ pass.
- JavaScript DOM stub: PASS chuyển optional -> required -> optional, không mất giá trị đang chọn, giữ lựa chọn xóa rõ ràng. Đây không phải kiểm thử trình duyệt.
- Chưa kiểm thử transaction ghi thực tế, thay đổi rule đồng thời, hoặc kết quả chất lượng sau UPDATE trên SQL Server live. Chưa kiểm tra trực quan bằng trình duyệt trong lượt này.
