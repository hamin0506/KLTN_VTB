# Tổng quan — bố cục theo ảnh và xuống dòng theo từ

- Chỉ chỉnh Dashboard/Index, dashboard.css, dashboard.js, employee-hierarchy.js và hai test nhãn/biểu đồ.
- Sáu KPI trên một hàng từ1200px. Hai cột chart tỷ lệ95:105, gap24px; cột nhân viên và donut bên trái, tài liệu bên phải, bảng dưới cùng. Màn hình nhỏ vẫn tự xuống hàng.
- Bỏ chia chuỗi thành đoạn18 ký tự. Nhãn nhân viên ghép nguyên từ vào dòng khoảng10 ký tự, mã tài liệu/nhãn tài liệu khoảng18 ký tự; từ đơn dài giữ nguyên. Không bỏ dữ liệu hoặc thêm dataZoom.
- Cột tối đa10px, nhãn11px, line-height18px, cách trục12px. Tính độ rộng vùng cuộn dựa trên dòng nhãn dài nhất và chừa16px giữa hạng mục; giảm mức tối thiểu cũ115px/hạng mục. Chiều cao cột từ300px và tăng nếu nhãn nhiều dòng.
- CSS riêng Tổng quan dùng word-break/overflow-wrap normal, không bẻ từ trong tên KPI, bộ lọc, tiêu đề và bảng. Bảng tăng padding để chữ không dính nhau.
- Giữ toàn bộ filter, ba chế độ biểu đồ, drill, màu trạng thái, KPI và SQL hiện có.
- Node tests: employee-hierarchy (bổ sung kiểm tra nguyên từ tiếng Việt/mã liền), document-chart-layout, kpi-formatting, overview-multiselect, document-drill-navigation đạt.
- Build: docs/build-overview-word-layout.txt.
- Chưa kiểm tra trực quan phiên website đang chạy bằng trình duyệt; kết quả trên là kiểm tra mã/JS/build, không phải kiểm tra ảnh với dữ liệu SQL thật.
