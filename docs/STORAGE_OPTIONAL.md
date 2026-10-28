# Hình thức lưu trữ không bắt buộc — 22/10/2026

- Nhóm Hợp đồng lao động và Quyết định / Văn bản khác có thể để trống storage_form khi Thêm/Sửa.
- Schema repository đã là nvarchar(50) NULL; model nullable và DocumentStorageForms.IsAllowed(null) đã hỗ trợ. Không thay schema/Views/requirement_rule hoặc thêm danh mục.
- Razor chung đổi lựa chọn trống thành “Để trống (Chưa xác định / Không yêu cầu)” và thêm hướng dẫn cho hai nhóm, liên kết bằng aria-describedby. Select không required; không ẩn/disable để người dùng có thể chủ động xóa giá trị cũ hoặc khai báo thêm.
- MVC model binding chuyển lựa chọn trống thành NULL; không lưu chữ “Không yêu cầu” hoặc chuỗi rỗng vào database. Không tự xóa giá trị đã lưu khi đổi loại tài liệu.
- Giữ nguyên lựa chọn bốn giá trị đã xác nhận và validation chống giá trị ngoài danh sách. Không bổ sung yêu cầu bắt buộc cho các nhóm khác ngoài phạm vi.
- Thêm kiểm thử HTTP POST rỗng/khoảng trắng -> model NULL và render cả hai form không required, có hướng dẫn nhóm. Test-only endpoint không ghi SQL, không thuộc website production.
- Build log: build-storage-optional.txt. Verification log: test-storage-optional.txt.
- Chưa kiểm tra ghi thật trên VTB2. Nếu requirement_rule.required_storage_form của một loại vẫn có giá trị, SQL View hiện tại có thể tiếp tục cảnh báo khi storage_form=NULL. Không tự sửa hoặc bỏ qua quy tắc SQL đó trong lượt này.
