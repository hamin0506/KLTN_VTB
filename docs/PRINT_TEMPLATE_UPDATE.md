# Cập nhật mẫu in lý lịch và danh mục hồ sơ — 25/10/2026

## Phạm vi và file
- Views/Profiles/Print.cshtml: chia lý lịch và danh mục bằng ngắt trang; phần IV chỉ có bảng Chỉ tiêu/Kết quả; không ảnh cán bộ, không card, không đoạn ghi chú dài.
- Views/Profiles/_PrintBrand.cshtml: dùng logo Vietinbank_logo.png hiện có, cùng vùng hiển thị SVG như sidebar.
- wwwroot/css/profile-print.css: A4, bảng hành chính, tiêu đề lặp khi bảng sang trang, tránh ngắt giữa dòng, khu vực chữ ký, ẩn nút khi in.
- tests/Verification/Program.cs: bổ sung fixture danh mục có/không có tài liệu, NULL, Bản copy và kiểm tra Razor render.

## Ánh xạ
- Phần I–III của lý lịch giữ dữ liệu và thứ tự văn bằng hiện tại.
- Phần IV: tổng yêu cầu/đã có/thiếu, tình trạng và cảnh báo lấy từ EmployeeQuality; số bản ghi chất lượng giữ ProfilePresentation.CurrentCount (mã tài liệu hiện hành riêng biệt theo trạng thái). Không đổi SQL hoặc logic KPI.
- Danh mục lấy tất cả DocumentTypes của ba nhóm, không bắt đầu từ Documents nên loại chưa có vẫn xuất hiện. Sắp thứ tự loại theo DocumentTypeId ổn định; STT bắt đầu lại ở mỗi nhóm.
- Số lượng là số bản ghi Documents của cán bộ thuộc loại tương ứng, gồm bản hiện hành và lịch sử như danh sách hồ sơ đang tải; không phải số loại tài liệu yêu cầu. Loại chưa có: 0, cột đánh dấu trống.
- Hồ sơ gốc: đánh dấu ✓ nếu có ít nhất một bản ghi khớp tên hình thức tương ứng (bỏ khoảng trắng hai đầu, không phân biệt hoa thường). Bản chính cũ được hiển thị đúng nếu tồn tại; không thêm lại lựa chọn này vào form lưu tài liệu. Bản copy, NULL, chuỗi NULL và giá trị không khớp không đánh dấu cột nào.
- Hợp đồng và quyết định: ghi chú lấy SpecificName có dữ liệu, không suy đoán số hợp đồng/số hiệu từ trường khác.
- Ngày in theo múi giờ Việt Nam; logo có sẵn, chữ ký người lập và cán bộ ở cuối tài liệu.
- Trang danh mục luôn bắt đầu trang mới. Nội dung dài được phép sang trang tiếp theo, không cố định chiều cao hoặc cắt dữ liệu.

## Giới hạn kiểm chứng
Không thay đổi database, SQL Views, CRUD, Dashboard hay logic lưu tài liệu. Không ghi dữ liệu thật để kiểm thử. Kiểm thử render sử dụng fixture trong bộ kiểm thử, không chèn database. Chưa xác nhận Print Preview/việc phân trang trực quan bằng trình duyệt và chưa đối chiếu bản in với dữ liệu SQL Server thật trong lượt này.

Kết quả build và bộ kiểm thử cuối cùng nằm trong build-print-template.txt và test-print-template.txt cùng thư mục.
