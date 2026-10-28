# Tài liệu lịch sử, cảnh báo hợp đồng và trạng thái cán bộ — 25/10/2026

## File đã chỉnh sửa
- Controllers/CatalogController.cs: bỏ chặn GET Edit khi IsCurrent=false và chặn POST kiểm tra stillCurrent. Vẫn dùng Find/UPDATE cùng bản ghi, Admin/CSRF, allowlist field; EmployeeId không bind khi sửa, CreatedAt/IsCurrent vẫn do EF mapping bỏ qua khi ghi. Upload/thay thế/xóa file và validation giữ nguyên.
- Views/Profiles/_DocumentGroups.cshtml: bỏ link Thêm phiên bản, luôn có Sửa cho Admin kể cả lịch sử; giữ Thêm tài liệu ở đầu nhóm, Chi tiết/Xóa và icon file. Nếu View có trạng thái chất lượng cho tài liệu lịch sử thì hiển thị đúng trạng thái đó thay vì thay bằng dòng Lịch sử — không đánh giá; cột phiên bản vẫn độc lập.
- Services/ProfilePresentation.cs: thêm lựa chọn cảnh báo chỉ cho trang hồ sơ, hàm kiểm tra hợp đồng và nhãn/màu trạng thái việc làm.
- Views/Profiles/_DocumentWarnings.cshtml: dùng ProfileWarnings, không sửa source rows.
- Views/Profiles/Details.cshtml: trạng thái việc làm dùng badge.
- tests/Verification/Program.cs: thêm kiểm tra logic và quyền/link lịch sử.

## Điều kiện miễn cảnh báo hết hạn lịch sử
Phải đồng thời:
1. vw_labor_contract_check của cán bộ trả LaborContractStatus=HOP_LE.
2. Có Document của chính cán bộ, IsCurrent=true, DocumentType.DocumentGroup chính xác Hợp đồng lao động.
3. Mã HDTHUVIEC/HDXDTH1/HDXDTH2 có valid flag tương ứng=1; HDKXDTH có ExistHdkxdth=1. Các mã khác không được dùng làm bằng chứng thay thế.
4. Có quality row cho document hiện hành với HOP_LE hoặc SAP_HET_HAN (đúng tập giá trị valid của View); không có row xung đột trạng thái khác cho cùng document.

Khi đủ điều kiện chỉ loại khỏi danh sách cảnh báo dòng DA_HET_HAN mà Document tương ứng là lịch sử, thuộc chính cán bộ và đúng nhóm hợp đồng. Không ẩn KHONG_HOP_LE, SAP_HET_HAN, hết hạn của tài liệu khác hoặc của hợp đồng hiện hành. SAP_HET_HAN được View coi còn hiệu lực nhưng cảnh báo sắp hết hạn vẫn giữ. Không sửa Model.Warnings/Documents hoặc trạng thái SQL. Khi thiếu Contract View, không có HOP_LE, thiếu current/quality/flag hoặc có xung đột thì giữ mọi cảnh báo. KHONG_CANH_BAO không tự suy luận thành HOP_LE.

## Sửa lịch sử và ràng buộc
Hai điều kiện Controller là nguyên nhân lịch sử trước đây không sửa được; không phải yêu cầu bắt buộc của filtered unique index.
Filtered index UX_document_current trên EmployeeId+DocumentTypeId WHERE is_current=1 vẫn giữ. Chuyển loại bản hiện hành sang loại đã có bản hiện hành vẫn bị service validation chặn, SQL unique exception cũng được xử lý. Lịch sử không thuộc filtered index khi vẫn IsCurrent=false; không xóa/chuyển cờ thủ công để né ràng buộc.
Snapshot D:\KLTN\VTB2.sql định nghĩa trg_document_set_current AFTER INSERT, chọn theo issue_date DESC, created_at DESC, document_id DESC trong nhóm cán bộ/loại. UPDATE metadata không kích hoạt trigger này. Sửa ngày/loại ảnh hưởng thứ tự/nhóm cho lần INSERT sau. Nếu trigger thực tế đã hỗ trợ UPDATE thì có thể chọn lại hiện hành ngay; code không tự ngăn hay thay đổi quy tắc DB.
Ứng dụng chưa có nhật ký audit giữ giá trị metadata trước/sau. Lịch sử phiên bản tài liệu không thay thế audit chỉnh sửa. Không tạo bảng audit trong lượt này.

## Trạng thái
Mã theo CHECK/Labels hiện có: DangLamViec → Đang làm việc/xanh lá; TamHoanHDLD → Tạm hoãn HĐLĐ/vàng; ChamDutHDLD → Đã nghỉ việc/xám; NghiHuu → Nghỉ hưu/xám; trạng thái chưa biết → Trạng thái khác/trung tính. Không đổi giá trị lưu hoặc mapping Dashboard.

## Kiểm chứng và hạn chế
- Build thành công 0 lỗi/0 cảnh báo: build-history-contract.txt.
- 356 kiểm tra đạt: test-history-contract.txt. Bao gồm hợp đồng hiện hành đủ bằng chứng, lịch sử hợp lệ không đủ, trạng thái nghiệp vụ cần xử lý, nhóm sai, quality xung đột, thiếu View, giữ cảnh báo khác, giữ dữ liệu nguồn, link sửa lịch sử theo quyền và màu badge.
- sqlcmd truy vấn read-only metadata trigger/index ACER\SQLEXPRESS thất bại (instance lookup/login timeout). Dùng schema.sql và VTB2.sql để đối chiếu, chưa xác minh định nghĩa SQL trực tiếp.
- Chưa thử lưu hiện hành/lịch sử, xung đột index và tác động trigger trên SQL thật; chưa kiểm tra trực quan bằng trình duyệt. Không ghi dữ liệu nhân sự thật để kiểm thử.
- Không đổi schema/Views/requirement_rule, Dashboard/KPI hoặc dữ liệu/file hiện có.
