# Hai biểu đồ Chất lượng hồ sơ: chiều cao, typography, ba chế độ

## File thay đổi
- Views/Dashboard/ProblemDocuments.cshtml: ba nút chế độ, bộ chọn đơn vị trong chế độ chi tiết.
- wwwroot/js/problem-report.js: chiều cao phối hợp, typography, ba chế độ, tooltip, khóa trục theo ID.
- wwwroot/css/reports.css: card giãn bằng nhau, tiêu đề và segmented control thống nhất.
- tests/Verification/problem-report.cjs: kiểm thử module thực tế với mock DOM.

## Chiều cao
Đo chiều cao cần thiết theo số danh mục/số dòng nhãn cho từng chart. Trên grid hai cột, chiều cao card chung bằng max(chiều cao chart tự nhiên + chiều cao header/bộ điều khiển + border) của hai bên. Vùng ECharts mỗi bên được đặt bằng chiều cao chung trừ phần header/controls của chính nó, nên vùng vẽ bên phải cũng tăng, không chỉ kéo dài nền card. Khi xuống một cột, mỗi chart dùng chiều cao tự nhiên. Dữ liệu nhiều làm cả hai card tăng chiều cao; giữ thanh 12px, không dataZoom, không cuộn dọc chart.
ResizeObserver hiện có được tái sử dụng; chỉ tính lại khi chiều rộng đổi hoặc dữ liệu/chế độ đổi, không thêm observer/listener resize khác. Không lấy chiều cao card đã giãn làm đầu vào để tránh tăng chiều cao lặp.

## Font
Lấy fontFamily qua getComputedStyle của secondary-report, cùng font website. Trục X/Y: 12px, weight 400, màu #52667b, khoảng cách 10px. Tooltip richText 13px cùng font; tiêu đề hai card 15px/600. Không đổi cam/xanh lá.

## Chế độ
- Theo đơn vị mặc định: cộng Total/Missing, giữ công thức tỷ lệ hiện có; click đơn vị mở Chi tiết đơn vị.
- Tất cả phòng ban: mỗi department_id là một thanh; tên trục chỉ là tên phòng ban. Tooltip có tên phòng ban, đơn vị và tỷ lệ. Không gộp tên trùng, không Top N.
- Chi tiết đơn vị: chọn một đơn vị trong phạm vi dữ liệu đã lọc; không chọn thì có hướng dẫn. Có nút quay lại. Bộ lọc chung không bị thay đổi bởi chuyển chế độ.
Giữ chế độ nếu còn hợp lệ; khi đơn vị đang xem bị loại khỏi phạm vi thì về Theo đơn vị. Xóa bộ lọc cũng đặt về mặc định.
Không sửa backend/DTO/SQL, dùng dữ liệu Organization đã có. Không sửa KPI, CRUD, Dashboard khác hay hai tab bảng.

## Kiểm thử và hạn chế
- Node syntax check đạt. problem-report.cjs đạt: cân bằng chiều cao có trừ controls, font chung, ba chế độ, tên trùng khác ID, tooltip, tỷ lệ cộng có trọng số, drill/quay lại, đổi bộ lọc, tab và resize.
- Build thành công 0 lỗi/0 cảnh báo: docs/build-problem-three-modes.txt. Lần đầu obj cache bị tiến trình khác giữ; build thành công bằng IntermediateOutputPath riêng .scaffold/problem-modes-obj, không dừng tiến trình của người dùng.
- C# regression: docs/test-problem-three-modes.txt.
- Không có listener 5082 lúc kiểm tra. Chưa xác nhận trực quan 1366/1440 hoặc tablet/mobile, chưa chụp ảnh hay đo mép card trên trình duyệt thật. Mô phỏng các độ rộng không thay thế kiểm thử bố cục thực tế.
- Không thử ghi dữ liệu nhân sự; chưa đối chiếu số liệu với SQL thật.
