# Tách tài liệu hiện hành và lịch sử — 28/10/2026

- Hồ sơ hiện hành xuất hiện trước; chỉ lấy IsCurrent=true.
- Tài liệu lịch sử ở cuối trong details mặc định không có thuộc tính open, tiêu đề đếm IsCurrent=false. Khi mở, sử dụng cùng partial phân nhóm và bảng thao tác.
- _DocumentSection.cshtml dùng lại ProfilePresentation.Groups rồi lọc theo IsCurrent, giữ nguyên thứ tự nguồn từ DocumentTypeOrdering: Requirement Rule, ngày cấp, ngày tạo, ID giảm dần cho lịch sử cùng loại.
- Giữ nhóm ngoài danh mục nếu có để không mất tài liệu. Giữ nguyên kiểm tra quyền, xem file, sửa, xóa và chất lượng.
- Dropdown/bộ lọc và bản in không sửa trong lượt này.
- File: Views/Profiles/_DocumentGroups.cshtml, Views/Profiles/_DocumentSection.cshtml (mới), Models/WorkspaceModels.cs, tests/Verification/Program.cs.
- Build: docs/build-history-section.txt; kiểm thử: docs/test-history-section.txt. Kiểm tra HTML Razor bằng TestServer với fixture riêng, không sửa dữ liệu SQL.
- Chưa kiểm tra trực quan trình duyệt: không có listener localhost:5082 khi kiểm tra. Không coi kiểm tra HTML là xác nhận bố cục thực tế trên mọi kích thước màn hình.
