# Sửa cảnh báo hợp đồng đã được thay thế — 25/10/2026

## Nguyên nhân trong mã nguồn
ProfilesController.Load đọc vw_document_quality theo employee_id và đưa mọi row không HOP_LE vào Model.Warnings. _DocumentWarnings gọi ProfilePresentation.ProfileWarnings.
Bộ lọc cũ có hai sai lệch:
1. Chỉ bỏ row khi document.IsCurrent=false. Trigger snapshot phân vùng theo employee_id + document_type_id, nên HDTHUVIEC và HDXDTH1 đã hết hạn có thể đều IsCurrent=true dù cán bộ đã chuyển sang HDXDTH2/HDKXDTH.
2. Tự dựng thêm điều kiện HOP_LE từ quality rows hiện hành và kiểm tra xung đột, có thể không nhất quán với tình trạng hợp đồng cấp cán bộ đang hiển thị.
Vì vậy sửa trước không xử lý được tình huống hợp đồng cũ khác loại còn là current trong nhóm loại của nó. Không cần đổi ngày hết hạn hoặc cờ is_current.

## Nguồn thẩm quyền
Contract = db.LaborContractChecks.AsNoTracking().SingleOrDefaultAsync(x=>x.EmployeeId==id), chính là nguồn phần Hợp đồng trên hồ sơ. Chỉ xem là hợp lệ khi Contract.EmployeeId khớp và LaborContractStatus=HOP_LE; không tính lại hợp lệ từ ngày hết hạn/quality/is_current.
SingleOrDefault không chọn tùy tiện nếu View trả nhiều dòng: truy vấn sẽ báo lỗi thay vì lấy First/Any. Định nghĩa snapshot View group theo cán bộ và phòng ban; không có cache application cho dữ liệu này.

## Phân loại hợp đồng đã được thay thế
Chỉ áp dụng sau khi trạng thái tổng hợp đúng cán bộ là HOP_LE và dòng cảnh báo là DA_HET_HAN, document đúng cán bộ, document_type thuộc Hợp đồng lao động.
Dựa trên các cờ và chuỗi mã hợp đồng trong CASE của vw_labor_contract_check:
- HDTHUVIEC: exist_hdxdth1=1 và có valid_hdxdth1=1 hoặc valid_hdxdth2=1 hoặc exist_hdkxdth=1.
- HDXDTH1: valid_hdxdth2=1 hoặc exist_hdkxdth=1.
- HDXDTH2: exist_hdkxdth=1.
Các trường hợp chuyển tiếp khác loại không yêu cầu is_current=0, vì cờ đó chỉ phân loại phiên bản trong cùng loại.
- Phiên bản cũ cùng loại HDTHUVIEC/HDXDTH1/HDXDTH2: is_current=false, valid flag tương ứng từ View=1, và có document khác cùng cán bộ/cùng loại đang current.
- Không tự suy luận đối với mã lạ hoặc HDKXDTH cũ khi thiếu cờ chứng minh phiên bản thay thế hợp lệ; giữ cảnh báo. Không đồng nhất KHONG_CANH_BAO với HOP_LE.

Mọi cảnh báo không hợp lệ, sắp hết hạn, tài liệu ngoài nhóm hợp đồng và hợp đồng hết hạn không đủ bằng chứng thay thế được giữ lại. Không sửa Warnings gốc, DocumentQualityRows, Documents, trạng thái SQL, Dashboard hoặc bản in.

## File thay đổi
- Services/ProfilePresentation.cs: bỏ định nghĩa hợp lệ độc lập; phân loại successor theo View.
- Controllers/ProfilesController.cs: thêm AsNoTracking vào truy vấn kết quả hợp đồng duy nhất theo cán bộ.
- Views/Profiles/_DocumentWarnings.cshtml: trạng thái rỗng “Không có tài liệu cần cảnh báo.”
- tests/Verification/Program.cs: thay các test đã đóng đinh điều kiện sai; bổ sung thử việc/HDXDTH1 còn current nhưng đã được thay thế, hợp đồng hợp lệ cấp cán bộ, cảnh báo khác giữ nguyên, cùng loại cần bằng chứng thay thế, thiếu/khác cán bộ/View không hợp lệ không ẩn.

## Kiểm chứng
Build: 0 lỗi, 0 cảnh báo (build-contract-successor.txt). Bộ kiểm thử: 359 đạt, 0 thất bại (test-contract-successor.txt). Không sửa dữ liệu thật để tạo tình huống.
Đã kiểm tra source luồng tải lại mỗi request, không cache và không phụ thuộc sửa tài liệu. Các kiểm thử hàm thay đổi trạng thái View giữa các lần gọi cho thấy cảnh báo được tính lại.
Chưa đối chiếu hồ sơ cụ thể trên SQL Server thật hoặc kiểm tra trực quan bằng trình duyệt. Các định nghĩa View/trigger được đối chiếu từ database/schema.sql và VTB2.sql vì môi trường chưa kết nối được ACER\\SQLEXPRESS trong các lượt kiểm tra gần nhất. Nếu View thực tế đã đổi tên/cờ hoặc chuỗi nghiệp vụ khác snapshot, cần đối chiếu lại; không tuyên bố đã xác minh dữ liệu sản xuất.
