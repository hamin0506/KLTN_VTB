# Rà soát mất tài liệu lịch sử — 28/10/2026

## Dữ liệu thật: chưa xác minh
Đã thử SELECT tổng số/current/history tại ACER\SQLEXPRESS, database VTB2, Windows Authentication. SQLCMD báo không tìm thấy server/instance và login timeout. Không nhận được bất kỳ số đếm database nào. Chưa thể kết luận lịch sử còn hay bị xóa. Không thực hiện INSERT/UPDATE/DELETE, sửa schema/View/rule, đổi is_current, file_path hoặc khôi phục dữ liệu.
Người dùng yêu cầu kiểm tra toàn bộ cán bộ; scripts/audit-document-history-readonly.sql cung cấp SELECT nhóm theo từng employee_id (kể cả không có tài liệu), số không phân loại, số thiếu danh mục và danh sách ID/ngày/trạng thái. Chưa chạy thành công script này với VTB2.

## Phát hiện trong code
- Controller lấy mọi document theo employee_id, không lọc is_current=1.
- _DocumentGroups hiện có markup lịch sử vô điều kiện, không có hidden; details không có open nên mặc định đóng. JavaScript sidebar chỉ tác động details.nav-group, không tác động accordion hồ sơ.
- Sorting cũ INNER JOIN document_type: có thể loại document nếu không tìm thấy danh mục tương ứng. Loại có danh mục nhưng không có requirement_rule vẫn được giữ ở mã cũ. Do đó không thể khẳng định thiếu rule là nguyên nhân.
- Không tái hiện được việc toàn bộ khu vực biến mất trong bản Razor hiện tại; chưa có bằng chứng lỗi JOIN gây tình trạng của tất cả cán bộ trong database thật. Có thể cần đối chiếu bản website đang chạy với bản source/build mới, nhưng đây chưa phải nguyên nhân đã xác nhận.

## Sửa
DocumentTypeOrdering dùng projection + MIN(requirement_id) tương quan theo document_type_id trên chính từng dòng source, không JOIN danh mục. Số dòng không đổi dù thiếu danh mục/rule hoặc nhiều rule cùng loại; fallback ID ổn định. Tiêu chí ngày cấp/ngày tạo/ID lịch sử giữ nguyên.
Khu vực lịch sử vẫn nằm cuối và thu gọn mặc định; thêm hướng dẫn mở và số tổng/hiện hành/lịch sử. Nhóm không xác định có nhãn Khác. Giữ nguyên bảng metadata, icon mắt và quyền thao tác. Các dropdown vẫn dùng thứ tự chuẩn.

## Kiểm thử
390 kiểm thử đạt, 0 thất bại (docs/test-history-preservation.txt). Kiểm tra EF SQL sinh ra không INNER JOIN với danh mục trong truy vấn document. Kiểm tra render không lặp/mất dòng, accordion luôn hiện cả khi rỗng, quyền sửa lịch sử, trường hợp hợp đồng thử việc/xác định thời hạn, không file, không rule, thiếu danh mục và nhiều rule.
Số đếm fixture (chỉ ở bộ nhớ TestServer):
- mixed: tổng 5 = hiện hành 1 + lịch sử 4.
- current: tổng 1 = hiện hành 1 + lịch sử 0.
- history: tổng 4 = hiện hành 0 + lịch sử 4.
- empty: tổng 0 = hiện hành 0 + lịch sử 0.
Không coi fixture là dữ liệu cán bộ thật. Không kiểm thử lưu/xóa trên VTB2. Không có listener localhost:5082 khi kiểm tra; chưa xác minh trực quan thao tác mở/đóng hoặc xem file trên website thật. Native details được giữ nguyên, không cần JS khởi tạo.
Build cuối: docs/build-history-preservation.txt.

## File sửa
Services/DocumentTypeOrdering.cs; Services/ProfilePresentation.cs; Views/Profiles/_DocumentGroups.cshtml; Views/Profiles/_DocumentSection.cshtml; tests/Verification/Program.cs; scripts/audit-document-history-readonly.sql (mới).
