# Rà soát trước thay đổi
| Phân hệ | Trạng thái | Bằng chứng / quyết định |
|---|---|---|
| Division | Đã có mã nguồn nhưng chưa kiểm thử CRUD thật | Có validation, unique, FK, paging; giữ và thêm bảo vệ quyền |
| Department | Đã có mã nguồn nhưng chưa kiểm thử CRUD thật | Có dropdown division, thiếu lọc đơn vị; bổ sung |
| Tổng quan | Chưa hoàn thành | Chỉ đếm division, thiếu KPI nghiệp vụ |
| Employee / hồ sơ | Chưa triển khai | Home.Module trả placeholder |
| Document / document_type | Chưa triển khai | Chưa mapping hoặc controller |
| 4 SQL Views | Chưa triển khai | Chưa mapping keyless / query service |
| requirement_rule | Chưa triển khai | Chưa có màn hình hoặc kiểm tra quy tắc |
| Power BI | Chưa hoàn thành | Chỉ có hướng dẫn .pbix, chưa Secure Embed |
| Xác thực/phân quyền | Có lỗi cần sửa | Tất cả controller truy cập ẩn danh được |
| EF Core | Chưa hoàn thành | Chỉ 2 bảng, mapping thủ công dựa schema |
| UI | Chưa hoàn thành | Sidebar phẳng, placeholder, thiếu breadcrumb/bộ lọc |
| Service nghiệp vụ | Chưa triển khai | Logic truy vấn trong controller |

Đã đọc lại toàn bộ schema: 6 bảng, 4 view; tài liệu database-analysis.md mô tả đầy đủ. Không có bảng hồ sơ riêng. Không có bảng tài khoản. requirement_rule thiếu FK/unique; view document_quality không lọc active/effective dates; view requirement chỉ lọc active. Không sửa database. Không ghi dữ liệu thật để kiểm thử.