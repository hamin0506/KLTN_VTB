# Mẫu in độc lập và tệp đính kèm — 25/10/2026

## Thực hiện
- Trang danh mục có mã cán bộ, họ tên, đơn vị, phòng ban, chức danh, ngày lập theo múi giờ Việt Nam, Người lập biểu và Người kiểm tra. Không tự điền người ký.
- Dropdown nội dung in thay đổi cả phần xem trước và phần in: toàn bộ, lý lịch hoặc danh mục. CSS ẩn phần không chọn, bỏ ngắt trang đầu khi chỉ in danh mục; không giới hạn một trang cho mỗi phần. Trình duyệt Ctrl+P cũng dùng lựa chọn hiện tại.
- Giữ bảng Hồ sơ gốc 7 cột, hợp đồng/quyết định 3 cột và nguyên cách đếm; không sửa KPI hay SQL Views.

## Lưu trữ và quyền
- `App_Data/DocumentFiles` dưới ContentRoot của ứng dụng, ngoài wwwroot. Chạy từ project: D:\KLTN\Web\App_Data\DocumentFiles. Chạy bản publish: nằm dưới thư mục publish, cần cấp quyền đọc/ghi cho tài khoản chạy ứng dụng và sao lưu cùng database. Không xóa thư mục khi triển khai bản mới.
- Tệp không được đóng gói vào bản build/publish hoặc Git; thư mục tạo khi upload lần đầu.
- `document.file_path` chỉ lưu tên GUID + phần mở rộng, không lưu tên/đường dẫn từ máy người dùng. Giao diện nhận diện bằng mã tài liệu, input file hiển thị tên tệp vừa chọn.
- Loại bỏ FilePath khỏi danh sách field binding để chống gửi đường dẫn tùy ý. Không upload thì giữ nguyên giá trị hiện có, thêm không upload có NULL.
- Một tệp mỗi bản ghi; PDF/JPG/JPEG/PNG tối đa 10 MiB. Kiểm tra extension và chữ ký nội dung, thêm giới hạn đọc stream; không tin Content-Type. Đây không phải bộ giải mã/kiểm tra toàn vẹn đầy đủ hay quét mã độc.
- Admin hiện tại mới được upload/thay thế qua Create/Edit với chống CSRF. Quy tắc chỉ sửa bản hiện hành không thay đổi; thêm phiên bản vẫn INSERT như trước.
- `DocumentFiles/Open/{id}` kiểm tra chính sách đọc (Windows account được cấu hình trong Readers/Administrators), lấy tham chiếu từ DB, chỉ chấp nhận tên GUID đúng mẫu, kiểm tra tồn tại, trả PDF/ảnh với MIME cố định, no-store/nosniff; `?download=true` tải xuống an toàn. Không có URL đường dẫn vật lý. Mô hình quyền hiện tại cho phép người đọc đã được cấp quyền xem mọi hồ sơ, không có phân quyền từng cán bộ.
- Bản ghi không có tệp: Chưa có tệp. Tham chiếu cũ/không hợp lệ hoặc mất tệp: Không tìm thấy tệp đính kèm; không mở đường dẫn tùy ý cũ. Có thể tải lại tệp qua form sửa để chuyển sang lưu trữ được bảo vệ.

## Tính nhất quán
- Kiểm tra tệp trước ghi; sau validation nghiệp vụ lưu tệp mới tên duy nhất rồi lưu SQL trong transaction hiện có. Thất bại trước commit xóa tệp vừa tạo; tệp cũ không bị xóa.
- Commit thành công mới kiểm tra lại tham chiếu và dọn tệp cũ nếu không còn bản ghi dùng. Xóa bản ghi cũng kiểm tra tham chiếu trước dọn tệp.
- Nếu commit mất phản hồi, không xóa mù: truy vấn SQL kiểm tra tham chiếu. Nếu SQL hoặc filesystem không sẵn sàng, giữ tệp và ghi cảnh báo không chứa nội dung/đường dẫn. Sự cố tiến trình/mất điện hoặc cleanup không thực hiện được có thể còn tệp không tham chiếu; chưa có tác vụ dọn tự động định kỳ. Không có transaction phân tán giữa SQL và filesystem.

## Schema và giới hạn kiểm chứng
- Đã thử sqlcmd đọc metadata, không kết nối được ACER\SQLEXPRESS (instance lookup/login timeout). Chưa xác nhận schema trực tiếp.
- Snapshot Database/schema.sql: dbo.document.file_path nvarchar(500) NULL; entity StringLength(500); không thấy CHECK riêng file_path. Đủ chứa mã tham chiếu mới; không thay schema/migration.
- Không ghi thử hồ sơ thật. Các kiểm thử file sử dụng dữ liệu kỹ thuật tạm, không ghi DB. Chưa kiểm chứng end-to-end upload/commit/thay thế/rollback bằng SQL thật, cũng chưa kiểm tra PDF/ảnh trực quan trong trình duyệt hoặc Print Preview A4.
- Kết quả cuối ở build-attachments.txt và test-attachments.txt. Bộ kiểm thử route hiện tại chấp nhận 503 khi SQL không truy cập được; PASS không đồng nghĩa dữ liệu SQL đã được đối chiếu.

## File source
- Controllers/CatalogController.cs; Controllers/DocumentFilesController.cs (mới)
- Services/DocumentFileStore.cs (mới); Services/CatalogService.cs
- Views/Shared/Record.cshtml; Views/Shared/_DocumentAttachment.cshtml (mới)
- Views/Profiles/_DocumentGroups.cshtml; Views/Profiles/Print.cshtml
- wwwroot/css/profile-print.css; wwwroot/js/profile-print.js
- Program.cs; Data/Records.cs; Vtb.Hr.Web.csproj; .gitignore
- tests/Verification/Program.cs
