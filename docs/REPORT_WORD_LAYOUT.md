# Căn chữ Problem documents và Contract

- reports.js: wrapReportLabel chỉ xuống dòng ở khoảng trắng; giữ nguyên từ tiếng Việt/mã liền. Tính chiều cao theo số dòng nhãn thực tế; line-height18px, khoảng cách nhãn/trục12px, chừa14px giữa nhóm nhãn. Từ quá dài được giữ nguyên với vùng cuộn CSS. Bar tối đa18px. Legend hợp đồng có khoảng cách16px giữa các mục.
- reports.css: bỏ overflow-wrap:anywhere cho lựa chọn bộ lọc; chuẩn hóa nhãn KPI, tiêu đề, bảng, trạng thái và nút drill. Padding ô bảng10–12px, line-height1.5. Các quy tắc trình bày chính được giới hạn vào hai trang problem/contract.
- tests/Verification/report-chart-layout.cjs: kiểm tra nhãn35hạng mục, từ tiếng Việt không bị tách và mã liền giữ nguyên.
- Node syntax, report-chart-layout và report-navigation đạt. Không sửa SQL, KPI, route, bộ lọc hoặc công thức.
- Build xem build-report-word-layout.txt. Chưa kiểm tra trực quan bằng trình duyệt với dữ liệu SQL thật.
