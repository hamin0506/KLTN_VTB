# Đồng bộ thứ tự loại tài liệu

## Nguồn chuẩn
CatalogController dùng CatalogService.List cho kind=rules; truy vấn trước thay đổi là OrderBy(RequirementId). Không có danh sách tùy chỉnh trong View. schema.sql có requirement_id và document_type_id, không có sort_order. Cấu hình Development trỏ ACER\SQLEXPRESS / VTB2 / Windows Authentication. Thử đọc metadata trực tiếp không thành công (instance lookup/login timeout), chưa xác minh dữ liệu SQL thực tế.

## Quy tắc dùng chung
Services/DocumentTypeOrdering.cs lấy MIN(requirement_id) cho mỗi document_type_id trên toàn bộ rule, kể cả rule tắt/hết hiệu lực, không bị ảnh hưởng bởi bộ lọc màn hình. Một loại chỉ có một vị trí dù có nhiều rule. Loại không có rule không xuất hiện trên trang rule cũ: giữ ở cuối theo document_type_id tăng dần để ổn định, không loại bỏ. Không thêm cột sort_order.
Trang rule giữ thứ tự requirement_id như cũ (không xóa dòng rule trùng). Danh sách loại dùng thứ tự lần xuất hiện đầu tiên đã loại trùng. Truy vấn được ORDER BY trước Skip/Take, không chỉ sắp xếp riêng từng trang.

Tài liệu: thứ tự loại, is_current giảm dần, issue_date giảm dần, created_at giảm dần, document_id giảm dần. Nhóm hồ sơ vẫn giữ như trước; mỗi nhóm giữ thứ tự tương đối của danh sách đã sắp. Không thay đổi trạng thái hiện hành/lịch sử.

## Màn hình cập nhật
- Hồ sơ cán bộ: toàn bộ tài liệu hiện hành/lịch sử theo ba nhóm.
- Danh sách tài liệu và danh sách loại tài liệu.
- Form Thêm/Sửa tài liệu (kể cả từ hồ sơ cán bộ); form rule; bộ lọc danh mục/chất lượng dùng CatalogService.Options.
- Bộ lọc loại hồ sơ của Dashboard: tên tiếng Việt kèm mã để tìm kiếm; value vẫn là ID. reports.js giữ thứ tự option do backend cung cấp.
- Danh mục hồ sơ trong bản in dùng cùng danh sách ID chuẩn.
- Trang Requirement Rule dùng InPageOrder, tương đương ORDER BY cũ.
Không thay đổi thứ tự xếp hạng biểu đồ, bảng phân tích Dashboard có sắp xếp chủ động, KPI, validation hay lưu dữ liệu.

## File mã nguồn
- Services/DocumentTypeOrdering.cs (mới)
- Services/CatalogService.cs
- Controllers/ProfilesController.cs
- Models/WorkspaceModels.cs
- Services/DashboardService.Reports.cs
- Views/Profiles/Print.cshtml
- tests/Verification/Program.cs

## Kiểm thử
Xem build-document-order.txt và test-document-order.txt. Fixture chỉ chạy trong bộ nhớ; kiểm tra trùng rule, loại chưa có rule, lọc, hiện hành/lịch sử và các tiêu chí phá hòa. ToQueryString xác minh EF dịch truy vấn có thứ tự/phân trang sang SQL mà không kết nối hoặc ghi dữ liệu thật.
Chưa xác nhận giao diện dropdown, tìm kiếm và thứ tự với dữ liệu SQL thực tế trên trình duyệt do chưa kết nối được instance. Không tuyên bố đã kiểm thử CRUD thật.
