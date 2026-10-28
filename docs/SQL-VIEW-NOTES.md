# Các vấn đề SQL cần phê duyệt trước khi điều chỉnh

Không sửa schema hoặc View trong lần phát triển này.

1. requirement_rule chưa có FK document_type_id và chưa giới hạn số quy tắc/loại. Nhiều quy tắc khiến vw_document_quality nhân bản dòng, thậm chí cho nhiều trạng thái trên một tài liệu. Website đếm DISTINCT document_id trong từng nhóm trạng thái, hiển thị số dòng/số tài liệu và cảnh báo xung đột; không tự ưu tiên trạng thái thay SQL.
2. vw_document_quality không lọc is_active, effective_from hoặc effective_to. Tắt quy tắc không đồng nghĩa ngừng ảnh hưởng chất lượng tài liệu. vw_employee_document_requirement chỉ lọc is_active, bỏ qua ngày hiệu lực. Giao diện nhắc rõ trước khi lưu; không hỗ trợ tạo thêm phiên bản quy tắc cùng loại qua UI. Cần thống nhất mô hình một quy tắc/loại hay phiên bản lịch sử trước khi sửa SQL.
3. vw_employee_quality đánh giá tính đầy đủ và chuỗi hợp đồng; chưa tổng hợp tài liệu hết hạn/không hợp lệ. Do đó hiển thị riêng đầy đủ và chất lượng tài liệu, không gọi DAY_DU là hoàn toàn hợp lệ.
4. vw_labor_contract_check dùng sự tồn tại HDKXDTH; không kiểm tra trạng thái chất lượng của hợp đồng này. KHONG_CANH_BAO không đồng nghĩa HOP_LE.
5. invalid_reason và quality_status ưu tiên ngày cấp/lưu trữ theo thứ tự khác nhau. Không tự sửa kết quả View trong C#.
6. updated_at chỉ có DEFAULT. Website chủ động lấy SYSDATETIME() của SQL Server khi sửa document, không giả định có trigger.
7. Các mã hợp đồng và hồ sơ điều kiện được hard-code trong SQL. UI chặn đổi các mã nghiệp vụ đã nhận diện hoặc loại tài liệu đang có quy tắc để tránh vô hiệu hóa logic View. Thay đổi đặc biệt cần đánh giá nghiệp vụ riêng.
8. Không có rowversion/audit/user tables. Không thêm bảng. Chỉnh sửa thông thường chưa có phát hiện toàn diện việc người khác sửa từ lúc mở form đến khi gửi; unique/FK/rule checks và giao dịch xử lý một phần tranh chấp đồng thời.

KPI mặc định tính toàn bộ cán bộ trong phạm vi tổ chức được chọn. Khi đánh dấu “phạm vi KPI”, scope lấy is_kpi_scope=1 từ View. Tổng tài liệu bao gồm lịch sử; KPI tình trạng chỉ lấy tài liệu hiện hành có mặt trong View. Các nhóm có thể chồng lấn nếu View cho nhiều trạng thái. Truy vấn đọc tuần tự, không phải một snapshot giao dịch.