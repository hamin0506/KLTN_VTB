# Sửa luồng StorageForm NULL — 22/10/2026

## Chẩn đoán và mức độ xác minh
Đã rà soát schema repository, entity, EditRecord.Value, Razor, JS, TryUpdateModelAsync/ModelState, validation rule và luồng SaveChanges.
- Repository định nghĩa storage_form nvarchar(50) NULL, không có CHECK enum cho trường này. Document.StorageForm là string?, không có Required; IsAllowed(null) đúng. Quy tắc required riêng vẫn áp dụng qua DocumentStoragePolicy.
- Nhánh Razor tạo option “NULL (không được hỗ trợ)” chỉ chạy khi giá trị là chuỗi khác rỗng ngoài danh sách. C# null thật không vào nhánh đó. Không có phép chuyển C# null thành chuỗi NULL trong mã form đã đọc.
- Trước sửa, thông báo chung không phân biệt dữ liệu chữ NULL với SQL NULL; option sai bị disabled nên không gửi về POST, có thể khiến entity đang theo dõi tiếp tục giữ giá trị sai khi chỉ sửa metadata.
- Đã thử SELECT chỉ đọc tới ACER\SQLEXPRESS/VTB2 nhưng lỗi tìm instance/login timeout. Vì vậy chưa xác nhận HDXDTH1 thực tế lưu SQL NULL hay literal NULL, chưa xác minh schema/trigger/View live. Không khẳng định nguyên nhân dữ liệu thực tế chỉ dựa vào ảnh/mô tả giao diện.
- scripts/diagnose-storage-null-readonly.sql trả từng document_id HDXDTH1 với SQL_NULL/LITERAL_NULL/EMPTY_TEXT/OTHER_VALUE và DATALENGTH; đồng thời đọc nullable, CHECK và required_storage_form. Không UPDATE, không trả tên cán bộ.

## Thay đổi
1. Option rỗng có value="", selected rõ ràng khi giá trị trống, nhãn Để trống / Không áp dụng. Không dùng chữ NULL hay Không áp dụng làm giá trị lưu.
2. Literal NULL vẫn là dữ liệu sai, không được quy đổi tự động. Thông báo riêng giải thích “chuỗi chữ NULL, không phải SQL NULL”. Option sai không còn disabled để POST không âm thầm bỏ field; phải chủ động chọn giá trị hợp lệ hoặc dùng nút Xóa giá trị này.
3. Nút Xóa chỉ đổi select thành value="" sau thao tác người dùng, phát change và giữ nguyên quy tắc required. Không tự xóa khi đổi loại. MVC chuẩn hóa chuỗi rỗng/khoảng trắng được POST thành C# null như trước; không chuẩn hóa literal NULL.
4. Loại bỏ ModelState.Clear() toàn bộ khỏi CatalogController.Save. TryUpdateModelAsync đã thực hiện binding và validation; nếu có lỗi vẫn trả form với ModelState. Không bỏ lỗi ngày/loại/rule hoặc sửa IsCurrent/CreatedAt.
5. Trang chi tiết và danh sách trong hồ sơ hiển thị Không áp dụng cho null khi policy optional; trường hợp khác hiển thị Chưa khai báo. Literal NULL được chú thích là chuỗi chữ. Profiles.Details đọc lại policy cùng dữ liệu/Views sau redirect.
6. Giữ nguyên bốn giá trị danh mục được xác nhận và validation theo rule; required NULL vẫn bị chặn. Giá trị lưu trữ hợp lệ không bị đổi khi người dùng chỉ sửa ngày/loại/tên/hạn. Không sửa SQL schema, Views, requirement_rule hoặc dữ liệu nhân sự thật.

## File thay đổi
- Models/DocumentStorageForms.cs: nhận diện literal NULL, thông báo và hiển thị thống nhất.
- Data/Records.cs: dùng thông báo validation riêng, không cho literal NULL hợp lệ.
- Controllers/CatalogController.cs: bỏ ModelState.Clear toàn bộ.
- Views/Shared/Record.cshtml: option rỗng/selected, phân biệt literal, thao tác xóa chủ động, hiển thị chi tiết.
- wwwroot/js/document-storage.js: xử lý nút xóa và cập nhật cảnh báo, bảo toàn giá trị khi đổi loại.
- Models/WorkspaceModels.cs, Controllers/ProfilesController.cs, Views/Profiles/_DocumentGroups.cshtml: policy và nhãn null khi đọc lại hồ sơ.
- tests/Verification/Program.cs, tests/storage-policy-client.cjs: các kiểm thử mới.
- scripts/diagnose-storage-null-readonly.sql: chẩn đoán SQL chỉ đọc.

## Kiểm thử thực tế trong môi trường này
Build: 0 lỗi, 0 cảnh báo (build-storage-null-fix.txt).
Verification: 269 PASS, 0 FAIL (test-storage-null-fix.txt).
JS DOM stub: PASS; không phải kiểm tra trình duyệt.

| Trường hợp | Kết quả |
|---|---|
| GET Thêm/Sửa với C# null | PASS, chọn option rỗng, không hiện cảnh báo unsupported |
| Literal NULL khác null thật | PASS, validation chặn và thông báo riêng |
| Chọn xóa rõ ràng | PASS JS; POST rỗng bind C# null |
| Giá trị hợp lệ khi đổi rule/loại | PASS JS giữ value; model tiếp tục chấp nhận |
| Rule required nhận null/sai/đúng giá trị | PASS kiểm thử policy; không bỏ kiểm tra required |
| POST rỗng kèm ngày hết hạn sai | PASS trả 400 và lỗi ExpiryDate, không xóa lỗi khác |
| Sửa ngày/loại và lưu thật HDXDTH1 | CHƯA KIỂM THỬ SQL UPDATE |
| Đọc lại chất lượng/Dashboard sau lưu SQL | CHƯA XÁC MINH live; luồng redirect/query giữ nguyên |
| Unique current với cập nhật cạnh tranh | CHƯA KIỂM THỬ live; validation/unique mapping giữ nguyên |

Các HTTP POST kiểm thử chạy trong TestServer với endpoint validation riêng chỉ có trong assembly test; không thực hiện SQL INSERT/UPDATE. Không tuyên bố đã lưu thành công vào VTB2. Cần kết quả audit chỉ đọc để chốt nguyên nhân dữ liệu HDXDTH1. Nếu phát hiện literal NULL, người dùng có thể chủ động xóa trên từng bản ghi bằng form; không có thao tác cập nhật hàng loạt.
