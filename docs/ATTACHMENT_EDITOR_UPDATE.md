# Cải thiện form đính kèm — 25/10/2026

## File
- Views/Shared/Record.cshtml: bỏ khung xanh; thêm tên tệp, xem trước, bỏ lựa chọn, xóa tệp có xác nhận và giữ lại tệp.
- wwwroot/js/document-attachment.js: dùng chung Thêm/Sửa, kiểm tra extension/kích thước/chữ ký, Blob URL xem trước PDF/ảnh tại chỗ; revoke khi đóng, đổi/bỏ tệp hoặc rời trang. Không upload để xem trước.
- Controllers/CatalogController.cs: xử lý RemoveAttachment trên POST Edit được bảo vệ Admin và CSRF; FilePath=null chỉ khi xác nhận và không có file mới; kiểm tra tham chiếu có bị thay đổi đồng thời. Tệp mới được ưu tiên nếu POST cả hai. Không ModelState.Clear.
- tests/Verification/attachment-editor.cjs: kiểm thử hành vi JavaScript bằng DOM mô phỏng.

## Quy tắc
- Bỏ tệp đã chọn không gọi server, không đổi file cũ. Chọn file mới hủy yêu cầu xóa đang chờ; bỏ file mới sau đó vẫn giữ file cũ.
- Xóa tệp yêu cầu confirm, sau đó chỉ đánh dấu chờ lưu. Có nút Giữ lại tệp; rời form không thay đổi server. Lưu thành công mới dọn tệp không còn tham chiếu qua cơ chế hiện có.
- Xóa chỉ tác động file_path của document đang sửa. Các field do DB quản lý và quy tắc bản hiện hành không đổi. Các thay đổi metadata do người dùng nhập vẫn qua validation hiện tại.
- Nếu form trả lỗi validation, không khôi phục file input hoặc ý định xóa tự động; người dùng chọn lại/xác nhận lại trước khi gửi.
- Schema snapshot file_path nvarchar(500) NULL hỗ trợ thao tác. Chưa xác nhận SQL thực tế; nếu có constraint khác, transaction thất bại và tệp cũ được giữ. Không đổi schema/Views/rules.

## Kiểm chứng
- Build 0 lỗi, 0 cảnh báo (build-attachment-editor.txt).
- Bộ kiểm thử .NET: 324 pass, 0 fail (test-attachment-editor.txt).
- node --check thành công; test JS đạt: PDF/PNG preview, revoke URL, clear không xóa file cũ, confirm/hủy, thay thế ưu tiên, extension/content/size invalid. Không gọi server.
- Chưa kiểm tra trực quan trình duyệt, preview PDF bằng trình xem tích hợp, hoặc lưu/thay thế/xóa/rollback trên SQL thật; không thao tác dữ liệu nhân sự thật để kiểm thử. Bộ kiểm thử route có thể chấp nhận 503 khi SQL không truy cập được.
