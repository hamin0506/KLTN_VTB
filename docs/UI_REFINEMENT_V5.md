# UI REFINEMENT V5 — Hồ sơ cán bộ

## Nguồn phân nhóm
Lần SELECT chỉ đọc từ môi trường tác vụ không kết nối được ACER\SQLEXPRESS (instance lookup/login timeout). Người dùng đã chạy GROUP BY document_group trong SSMS và xác nhận:
- Hồ sơ gốc:13loại tài liệu.
- Hợp đồng lao động:7loại tài liệu.
- Quyết định / Văn bản khác:14loại tài liệu.
Đây là số loại trong danh mục, không phải số tài liệu của một cán bộ.

Controller đọc document của cán bộ và danh mục document_type bằng EF AsNoTracking. Presentation ghép document_type_id vào dictionary từ DB, phân nhóm theo giá trị document_group chính xác. Chỉ đổi nhãn Quyết định / Văn bản khác thành Quyết định. Không hardcode ID hoặc suy đoán từ tên. Danh mục được tải lại mỗi lần xem hồ sơ; nhóm rỗng có0tài liệu. Nhóm mới, nhóm rỗng hoặc loại không tìm thấy được hiển thị riêng kèm cảnh báo, không bị bỏ hay gán sai.

## Cảnh báo
Lỗi cũ do Razor viết cố định “tên · InvalidReason · Hết hạn”, khi InvalidReason rỗng sinh hai dấu chấm. Hai partial mới dùng cấu trúc heading/badge/dl và điều kiện HasValue/IsNullOrWhiteSpace; không nối HTML bằng dấu phân cách.
Ngày hiển thị qua QualityService.Date (dd/MM/yyyy). ExpiryDays lấy từ View; số âm ghi đã quá hạn, không tính lại nghiệp vụ. DistinctBy trên document_id, quality_status, nguyên nhân đã trim, expiry_date, expiry_days loại trùng hiển thị; vẫn giữ nguyên nhân hoặc trạng thái khác nhau. Không áp dụng dedup vào SQL/KPI.
Màu CSS riêng profile: hợp lệ#059669, sắp hết hạn#F59E0B, hết hạn#DC2626, không hợp lệ#7C3AED, thiếu#EA580C, chưa xác định xám. Badge sắp hết hạn có nền tối để chữ vàng dễ đọc; đây là badge trạng thái, không phải giá trị KPI.

## CRUD và quyền
Giữ nguyên action/view thêm, chi tiết, sửa, xóa và link bổ sung hồ sơ. Các nút sửa/xóa/thêm vẫn kiểm tra quyền admin. Controller CRUD hiện có chuyển về Profiles/Details sau lưu/xóa, nên nhóm và số đếm được tính lại từ DB, không lưu nhóm độc lập. Tài liệu lịch sử hiển thị không đánh giá, không bịa trạng thái View. Không thay upload/đường dẫn tệp hiện có hoặc thêm chức năng upload mới.

## File
- Controllers/ProfilesController.cs
- Models/WorkspaceModels.cs
- Services/ProfilePresentation.cs
- Views/Profiles/Details.cshtml
- Views/Profiles/_DocumentWarnings.cshtml
- Views/Profiles/_DocumentGroups.cshtml
- wwwroot/css/profile.css
- tests/Verification/Program.cs

## Kiểm tra
Xem build-ui-v5.txt và verification-ui-v5-checked.txt. Các fixture chỉ tồn tại trong test executable, không đưa vào website hoặc ghi DB. Kiểm tra nhóm/đếm không mất tài liệu, đổi loại đổi nhóm, nhóm lạ, dedup giữ nhiều nguyên nhân, mapping trạng thái; TestServer render ngày, NULL, badge, link chi tiết và nút sửa theo quyền admin/reader.
Accordion dùng details/summary native, mở/đóng độc lập, có focus/chevron; bảng có cuộn ngang. Chưa kiểm tra trực quan thao tác Accordion/laptop với trình duyệt, chưa kiểm tra hồ sơ thật nhiều tài liệu hoặc thực hiện thêm/sửa/xóa trên SQL thật. Không ghi vào bản ghi nhân sự, không sửa schema/View/rules/Dashboard.

Kết quả cuối: build thành công, 0 cảnh báo / 0 lỗi; 212 kiểm tra đạt, 0 thất bại. Đã sửa đường dẫn partial tuyệt đối sau khi TestServer phát hiện render từ controller kiểm thử không tìm thấy partial theo tên ngắn. Kết quả xác nhận cuối ở verification-ui-v5-checked.txt; các log thử trước có thất bại đã được khắc phục.
