# Phân tích schema VTB2

Đọc toàn bộ database/schema.sql. File có lệnh CREATE DATABASE, ALTER DATABASE, CREATE TABLE và VIEW; không được chạy để khởi tạo ứng dụng. 6 bảng, 4 view, 6 PK, 4 FK, 5 ràng buộc UNIQUE trong bảng, 1 filtered unique index, 5 DEFAULT và 10 CHECK. Không có trigger, stored procedure hoặc dữ liệu INSERT trong file.

## Bảng, khóa và quan hệ

| Bảng | PK | UNIQUE | FK |
|---|---|---|---|
| division | division_id int identity | division_code; division_name | Không |
| department | department_id int identity | department_code | division_id → division |
| employee | employee_id char(8), không identity | Không | department_id → department |
| document_type | document_type_id int identity | document_code; document_name | Không |
| document | document_id int identity | (employee_id, document_type_id) WHERE is_current=1 | employee_id → employee; document_type_id → document_type |
| requirement_rule | requirement_id int identity | Không | Không khai báo trong schema |

FK không có ON DELETE CASCADE / ON UPDATE CASCADE, mặc định NO ACTION. Xóa division đang được department tham chiếu sẽ bị SQL Server từ chối. CRUD xử lý lỗi 547 thành thông báo tiếng Việt. Các unique constraint được kiểm tra trước và xử lý lỗi 2601/2627 khi có tranh chấp đồng thời.

## Luồng các View

1. vw_document_quality: document hiện hành + employee + document_type, LEFT JOIN requirement_rule. Phân loại KHONG_HOP_LE, DA_HET_HAN, SAP_HET_HAN, HOP_LE và KHONG_XAC_DINH. Phạm vi KPI: DangLamViec/TamHoanHDLD. Dùng ngày hiện tại của SQL Server.
2. vw_labor_contract_check: tổng hợp tài liệu theo cán bộ, kiểm tra chuỗi HDTHUVIEC → HDXDTH1 → HDXDTH2 → HDKXDTH. Trả HOP_LE, THIEU_HDXDTH1, CAN_KY_KET_TIEP hoặc KHONG_CANH_BAO. Thứ tự CASE quyết định ưu tiên cảnh báo.
3. vw_employee_document_requirement: CROSS JOIN cán bộ và quy tắc is_active=1, lọc hồ sơ bắt buộc hoặc có điều kiện theo quản lý/trạng thái lao động. Loại HDXDTH1/HDXDTH2/HDKXDTH khỏi yêu cầu bắt buộc chung; LEFT JOIN tài liệu hiện hành để xác định DA_CO/THIEU_HO_SO.
4. vw_employee_quality: tổng hợp số hồ sơ bắt buộc/đã có/thiếu, STRING_AGG mã hồ sơ thiếu và kết quả hợp đồng; trả DAY_DU hoặc THIEU_HO_SO. Đây là mức độ đầy đủ, chưa phải tổng hợp toàn bộ tính hợp lệ/hết hạn của tài liệu.

## Các điểm cần quyết định ở giai đoạn sau (không sửa database)

- requirement_rule.document_type_id mang ý nghĩa quan hệ nhưng không có FK. Có thể có quy tắc mồ côi.
- Không có UNIQUE trên document_type_id của requirement_rule. Nhiều quy tắc có thể nhân bản dòng ở các View và ảnh hưởng mẫu số KPI.
- vw_document_quality không lọc is_active/effective_from/effective_to của quy tắc. vw_employee_document_requirement chỉ lọc is_active, không lọc ngày hiệu lực.
- updated_at chỉ có DEFAULT khi INSERT; không có trigger tự cập nhật khi UPDATE.
- gender NOT NULL dù CHECK cho phép IS NULL; điều kiện NOT NULL vẫn có hiệu lực. issue_date cũng NOT NULL dù CHECK ngày có nhánh IS NULL.
- employee_id không có CHECK định dạng ngoài char(8); dữ liệu tên và mã chưa có CHECK loại khoảng trắng/rỗng.
- exist_hdkxdth kiểm tra sự tồn tại, chưa kiểm tra chất lượng. Các trường hợp hợp đồng không khớp nhánh trả KHONG_CANH_BAO, không đồng nghĩa chắc chắn hợp lệ.
- invalid_reason ưu tiên lỗi lưu trữ trước ngày cấp; quality_status kiểm tra ngày cấp trước. Cùng là KHONG_HOP_LE nhưng lý do ưu tiên khác nhau.
- Chưa có bảng người dùng, vai trò hay audit log. Website giai đoạn đầu phục vụ localhost, chưa có xác thực đăng nhập.
- DB compatibility 160, AUTO_CLOSE ON, RECOVERY SIMPLE; đây là hiện trạng từ script, không điều chỉnh tự động.

## DEFAULT

- document.is_current=1; created_at=sysdatetime(); updated_at=sysdatetime().
- employee.is_managerial=0; requirement_rule.is_active=1.

## CHECK

- document: is_current ∈ {0,1}; expiry_date NULL hoặc >= issue_date.
- document_type.document_group ∈ {Hồ sơ gốc, Hợp đồng lao động, Quyết định / Văn bản khác}.
- employee: gender ∈ {Nam,Nu}; leave_date NULL hoặc >= join_date; is_managerial ∈ {0,1}; employment_status ∈ {DangLamViec,TamHoanHDLD,ChamDutHDLD,NghiHuu}.
- requirement_rule: effective_to NULL hoặc >= effective_from; requirement_level ∈ {BAT_BUOC,TUY_CHON,CO_DIEU_KIEN}; warning_threshold_days NULL hoặc >=0.

## Chi tiết cột (trích tự động từ toàn bộ schema)

### document_type

```sql
[document_type_id] [int] IDENTITY(1,1) NOT NULL,
	[document_code] [varchar](50) NOT NULL,
	[document_name] [nvarchar](200) NOT NULL,
	[document_group] [nvarchar](100) NOT NULL,
	[description] [nvarchar](200) NULL,
```


### employee

```sql
[employee_id] [char](8) NOT NULL,
	[full_name] [nvarchar](50) NOT NULL,
	[date_of_birth] [date] NULL,
	[gender] [varchar](10) NOT NULL,
	[department_id] [int] NOT NULL,
	[position_name] [nvarchar](200) NULL,
	[is_managerial] [bit] NOT NULL,
	[employment_status] [varchar](30) NOT NULL,
	[join_date] [date] NOT NULL,
	[leave_date] [date] NULL,
```


### document

```sql
[document_id] [int] IDENTITY(1,1) NOT NULL,
	[employee_id] [char](8) NOT NULL,
	[document_type_id] [int] NOT NULL,
	[specific_name] [nvarchar](300) NULL,
	[issue_date] [date] NOT NULL,
	[expiry_date] [date] NULL,
	[storage_form] [nvarchar](50) NULL,
	[file_path] [nvarchar](500) NULL,
	[is_current] [bit] NOT NULL,
	[created_at] [datetime2](0) NOT NULL,
	[updated_at] [datetime2](0) NOT NULL,
```


### requirement_rule

```sql
[requirement_id] [int] IDENTITY(1,1) NOT NULL,
	[document_type_id] [int] NOT NULL,
	[requirement_level] [varchar](30) NOT NULL,
	[required_storage_form] [nvarchar](50) NULL,
	[warning_threshold_days] [int] NULL,
	[effective_from] [date] NOT NULL,
	[effective_to] [date] NULL,
	[is_active] [bit] NOT NULL,
```


### department

```sql
[department_id] [int] IDENTITY(1,1) NOT NULL,
	[division_id] [int] NOT NULL,
	[department_code] [varchar](20) NOT NULL,
	[department_name] [nvarchar](200) NOT NULL,
```


### division

```sql
[division_id] [int] IDENTITY(1,1) NOT NULL,
	[division_code] [varchar](20) NOT NULL,
	[division_name] [nvarchar](200) NOT NULL,
```

