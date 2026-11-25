/****** Object:  Database [VTB2]    Script Date: 11/11/2026 1:58:39 AM ******/
CREATE DATABASE [VTB2]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'VTB2', FILENAME = N'C:\sqlserver\MSSQL16.SQLEXPRESS\MSSQL\DATA\VTB2.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'VTB2_log', FILENAME = N'C:\sqlserver\MSSQL16.SQLEXPRESS\MSSQL\DATA\VTB2_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [VTB2] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [VTB2].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [VTB2] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [VTB2] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [VTB2] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [VTB2] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [VTB2] SET ARITHABORT OFF 
GO
ALTER DATABASE [VTB2] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [VTB2] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [VTB2] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [VTB2] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [VTB2] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [VTB2] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [VTB2] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [VTB2] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [VTB2] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [VTB2] SET  ENABLE_BROKER 
GO
ALTER DATABASE [VTB2] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [VTB2] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [VTB2] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [VTB2] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [VTB2] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [VTB2] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [VTB2] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [VTB2] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [VTB2] SET  MULTI_USER 
GO
ALTER DATABASE [VTB2] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [VTB2] SET DB_CHAINING OFF 
GO
ALTER DATABASE [VTB2] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [VTB2] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [VTB2] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [VTB2] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [VTB2] SET QUERY_STORE = ON
GO
ALTER DATABASE [VTB2] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [VTB2]
GO
/****** Object:  Table [dbo].[document_type]    Script Date: 11/11/2026 1:58:39 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[document_type](
	[document_type_id] [int] IDENTITY(1,1) NOT NULL,
	[document_code] [varchar](50) NOT NULL,
	[document_name] [nvarchar](200) NOT NULL,
	[document_group] [nvarchar](100) NOT NULL,
	[description] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[document_type_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[document_name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[document_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[employee]    Script Date: 11/11/2026 1:58:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[employee](
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
	[phone_number] [varchar](10) NULL,
	[email] [varchar](254) NULL,
	[hometown] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[employee_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[document]    Script Date: 11/11/2026 1:58:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[document](
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
PRIMARY KEY CLUSTERED 
(
	[document_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[requirement_rule]    Script Date: 11/11/2026 1:58:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[requirement_rule](
	[requirement_id] [int] IDENTITY(1,1) NOT NULL,
	[document_type_id] [int] NOT NULL,
	[requirement_level] [varchar](30) NOT NULL,
	[required_storage_form] [nvarchar](50) NULL,
	[warning_threshold_days] [int] NULL,
	[effective_from] [date] NOT NULL,
	[effective_to] [date] NULL,
	[is_active] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[requirement_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_document_quality]    Script Date: 11/11/2026 1:58:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   VIEW [dbo].[vw_document_quality]
AS
SELECT
	d.document_id,
    d.employee_id,
    e.full_name,
    dt.document_code,
    dt.document_name,
    dt.document_group,
    d.issue_date,
    d.expiry_date,
    CASE WHEN d.expiry_date IS NOT NULL THEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), d.expiry_date) ELSE NULL END AS expiry_days,
    d.storage_form,
    rr.required_storage_form,
    rr.requirement_level,
    rr.warning_threshold_days,
    d.is_current,
    CASE 
		WHEN rr.required_storage_form IS NOT NULL AND (d.storage_form IS NULL OR d.storage_form <> rr.required_storage_form) 
			THEN N'Hình thức lưu trữ không đúng quy định'
		WHEN d.issue_date > CAST(GETDATE() AS DATE) THEN N'Ngày cấp lớn hơn ngày hiện tại'
        ELSE NULL
    END AS invalid_reason,
    -- Kết quả Data Quality
    CASE
        -- Rule 4: dữ liệu không hợp lệ
        WHEN d.issue_date > CAST(GETDATE() AS DATE) THEN 'KHONG_HOP_LE'
        WHEN rr.required_storage_form IS NOT NULL AND (d.storage_form IS NULL OR d.storage_form <> rr.required_storage_form)
            THEN 'KHONG_HOP_LE'
        -- Rule 3: đã hết hạn
        WHEN d.expiry_date IS NOT NULL AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), d.expiry_date) < 0 THEN 'DA_HET_HAN'
        -- Rule 2: sắp hết hạn
        WHEN d.expiry_date IS NOT NULL AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), d.expiry_date) BETWEEN 0 AND ISNULL(rr.warning_threshold_days, 0)
            THEN 'SAP_HET_HAN'
        -- Rule 1: không có hạn
        WHEN d.expiry_date IS NULL THEN 'HOP_LE'
        -- Rule 1: còn hạn xa
        WHEN d.expiry_date IS NOT NULL AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), d.expiry_date) > ISNULL(rr.warning_threshold_days, 0) THEN 'HOP_LE'
        ELSE 'KHONG_XAC_DINH'
    END AS quality_status, 
	CASE WHEN e.employment_status IN ('DangLamViec', 'TamHoanHDLD') THEN 1 ELSE 0 END AS working_status
FROM document d
JOIN employee e ON d.employee_id = e.employee_id
JOIN document_type dt ON d.document_type_id = dt.document_type_id
LEFT JOIN requirement_rule rr ON d.document_type_id = rr.document_type_id
    AND rr.is_active=1 AND rr.effective_from<=CAST(GETDATE() AS date)
    AND (rr.effective_to IS NULL OR rr.effective_to>=CAST(GETDATE() AS date))
WHERE d.is_current = 1;
GO
/****** Object:  Table [dbo].[department]    Script Date: 11/11/2026 1:58:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[department](
	[department_id] [int] IDENTITY(1,1) NOT NULL,
	[division_id] [int] NOT NULL,
	[department_code] [varchar](20) NOT NULL,
	[department_name] [nvarchar](200) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[department_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[department_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_labor_contract_check]    Script Date: 11/11/2026 1:58:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   VIEW [dbo].[vw_labor_contract_check]
AS
WITH contract_status AS
(
    SELECT
        e.employee_id,
        MAX(e.full_name) AS full_name,
		e.department_id,
		MAX(d.department_name) AS department_name,
        MAX(e.employment_status) AS employment_status,
		-- 3 cột HD thử việc: Còn hiệu lực | Hết hiệu lực | Đã tồn tại
		MAX(CASE WHEN dq.document_code = 'HDTHUVIEC' AND dq.quality_status IN ('HOP_LE','SAP_HET_HAN') THEN 1 ELSE 0 END) AS valid_hdthuviec,
		MAX(CASE WHEN dq.document_code = 'HDTHUVIEC' AND dq.quality_status = 'DA_HET_HAN' THEN 1 ELSE 0 END) AS hdthuviec_expired,
		MAX(CASE WHEN dq.document_code = 'HDTHUVIEC' THEN 1 ELSE 0 END) AS exist_hdthuviec,
        --3 cột HDXDTH1: Còn hiệu lực | Hết hiệu lực | Đã tồn tại
        MAX(CASE WHEN dq.document_code = 'HDXDTH1' AND (dq.quality_status = 'HOP_LE' OR dq.quality_status = 'SAP_HET_HAN') THEN 1 ELSE 0 END) AS valid_hdxdth1,
        MAX(CASE WHEN dq.document_code = 'HDXDTH1' AND dq.quality_status = 'DA_HET_HAN' THEN 1 ELSE 0 END) AS hdxdth1_expired,
        MAX(CASE WHEN dq.document_code = 'HDXDTH1' THEN 1 ELSE 0 END) AS exist_hdxdth1,
        --3 cột HDXDTH2: Còn hiệu lực | Hết hiệu lực | Đã tồn tại
        MAX(CASE WHEN dq.document_code = 'HDXDTH2' AND (dq.quality_status = 'HOP_LE' OR dq.quality_status = 'SAP_HET_HAN') THEN 1 ELSE 0 END) AS valid_hdxdth2,
        MAX(CASE WHEN dq.document_code = 'HDXDTH2' AND dq.quality_status = 'DA_HET_HAN' THEN 1 ELSE 0 END) AS hdxdth2_expired,
        MAX(CASE WHEN dq.document_code = 'HDXDTH2' THEN 1 ELSE 0 END) AS exist_hdxdth2,        
        --1 cột HDKXDTH đã tồn tại hay chưa
        MAX(CASE WHEN dq.document_code = 'HDKXDTH' THEN 1 ELSE 0 END) AS exist_hdkxdth
    FROM employee e
        LEFT JOIN vw_document_quality dq ON e.employee_id = dq.employee_id
		JOIN department d on e.department_id = d.department_id
    GROUP BY e.employee_id, e.department_id
)
SELECT
    employee_id,
    full_name,
	department_id,
	department_name,
    employment_status,
	valid_hdthuviec,
	hdthuviec_expired,
	exist_hdthuviec,
    valid_hdxdth1,
    hdxdth1_expired,
    exist_hdxdth1,
    valid_hdxdth2,
    hdxdth2_expired,
    exist_hdxdth2,
    exist_hdkxdth,
    CASE
		-- Dang làm việc, HĐ thử việc còn hạn hợp lệ
		WHEN employment_status IN ('DangLamViec','TamHoanHDLD') AND valid_hdthuviec = 1 THEN 'HOP_LE'
		-- Đang làm việc, HD thử việc đã hết hạn nhưng chưa có HDXDTH1
		WHEN employment_status IN ('DangLamViec', 'TamHoanHDLD') AND hdthuviec_expired = 1 AND exist_hdxdth1 = 0 THEN 'THIEU_HDXDTH1'
        /* Rule 7:  Có HDKXDTH hoặc HDKXDTH2 nhưng không có HDXDTH1 */ 
        WHEN employment_status IN ('DangLamViec', 'TamHoanHDLD') AND (exist_hdkxdth = 1 or valid_hdxdth2 = 1) AND exist_hdxdth1 = 0 THEN 'THIEU_HDXDTH1'
        /* Rule 6: Đã tồn tại HDXDTH1 */
		WHEN employment_status IN ('DangLamViec', 'TamHoanHDLD') AND hdxdth1_expired = 1 AND (valid_hdxdth2 = 1 OR exist_hdkxdth = 1) THEN 'HOP_LE'
		--HDXDTH1 còn hiệu lực
		WHEN employment_status IN ('DangLamViec', 'TamHoanHDLD') AND valid_hdxdth1 = 1 THEN 'HOP_LE'
		-- HDXDTH1 hết hạn, không có hợp đồng tiếp theo hợp lệ
		WHEN employment_status IN ('DangLamViec', 'TamHoanHDLD') AND hdxdth1_expired = 1 AND valid_hdxdth2 = 0 AND exist_hdkxdth = 0 THEN 'CAN_KY_KET_TIEP'
		--HDXDTH2 hết hạn, không có hợp đồng tiếp theo
		WHEN employment_status IN ('DangLamViec', 'TamHoanHDLD') AND hdxdth2_expired = 1 AND exist_hdkxdth = 0 THEN 'CAN_KY_KET_TIEP'
        /* Các trường hợp còn lại không phát sinh cảnh báo */
        ELSE 'KHONG_CANH_BAO'
    END AS labor_contract_status
FROM contract_status;
GO
/****** Object:  View [dbo].[vw_employee_document_requirement]    Script Date: 11/11/2026 1:58:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   VIEW [dbo].[vw_employee_document_requirement]
AS
WITH active_rules AS
(
    SELECT
        rr.requirement_id,
        rr.document_type_id,
        rr.requirement_level,
        rr.required_storage_form,
        rr.warning_threshold_days
    FROM requirement_rule rr
    WHERE rr.is_active = 1
      AND rr.effective_from<=CAST(GETDATE() AS date)
      AND (rr.effective_to IS NULL OR rr.effective_to>=CAST(GETDATE() AS date))
),

required_documents AS
(
    SELECT
        e.employee_id,
        e.full_name,
        e.position_name,
		e.department_id,
		d.department_name,
        e.is_managerial,
        e.employment_status,
        ar.requirement_id,
        ar.document_type_id,
        dt.document_code,
        dt.document_name,
        dt.document_group,
        ar.requirement_level,
        ar.required_storage_form,
        ar.warning_threshold_days,
        CASE
            /* 1. Hồ sơ bắt buộc cho tất cả nhân viên */
            WHEN ar.requirement_level = 'BAT_BUOC' THEN N'Bắt buộc cho tất cả nhân viên'

            /* 2. Hồ sơ bắt buộc đối với nhân viên quản lý */
            WHEN ar.requirement_level = 'CO_DIEU_KIEN' AND e.is_managerial = 1 AND dt.document_code IN
                 (
                    'QDBONHIEM',
                    'HSBONHIEM',
                    'KEKHAITS'
                 )
                THEN N'Bắt buộc đối với nhân viên quản lý'

            /* 3. Quyết định tạm hoãn */
            WHEN ar.requirement_level = 'CO_DIEU_KIEN' AND e.employment_status = 'TamHoanHDLD' AND dt.document_code = 'QDTAMHOAN'
            THEN N'Bắt buộc đối với nhân viên tạm hoãn HĐLĐ'

            /* 4. Quyết định nghỉ hưu */
            WHEN ar.requirement_level = 'CO_DIEU_KIEN' AND e.employment_status = 'NghiHuu' AND dt.document_code = 'QDNGHIHUU'
            THEN N'Bắt buộc đối với nhân viên nghỉ hưu'
            ELSE NULL
        END AS requirement_reason
    FROM employee e
    CROSS JOIN active_rules ar
    JOIN document_type dt ON ar.document_type_id = dt.document_type_id
	JOIN department d on d.department_id = e.department_id
WHERE
(
    -- 1. Nhân viên đang làm việc hoặc tạm hoãn HĐLĐ
    (
        e.employment_status IN ('DangLamViec', 'TamHoanHDLD')
        AND
        (
            -- Hồ sơ bắt buộc chung
            (
                ar.requirement_level = 'BAT_BUOC'
            )

            OR

            -- Hồ sơ của nhân viên quản lý
            (
                ar.requirement_level = 'CO_DIEU_KIEN'
                AND e.is_managerial = 1
                AND dt.document_code IN (
                    'QDBONHIEM',
                    'HSBONHIEM',
                    'KEKHAITS'
                )
            )

            OR

            -- Hồ sơ của nhân viên tạm hoãn HĐLĐ
            (
                ar.requirement_level = 'CO_DIEU_KIEN'
                AND e.employment_status = 'TamHoanHDLD'
                AND dt.document_code = 'QDTAMHOAN'
            )
        )
    )

    OR

    -- 2. Nhân viên nghỉ hưu: chỉ yêu cầu quyết định nghỉ hưu
    (
        e.employment_status = 'NghiHuu'
        AND ar.requirement_level = 'CO_DIEU_KIEN'
        AND dt.document_code = 'QDNGHIHUU'
    )
)

-- 3. Loại toàn bộ hợp đồng lao động khỏi danh sách hồ sơ bắt buộc chung
AND dt.document_code NOT IN (
    'HDTHUVIEC',
    'HDXDTH1',
    'HDXDTH2',
    'HDKXDTH'
))

SELECT
    rd.employee_id,
    rd.full_name,
    rd.position_name,
	rd.department_id,
	rd.department_name,
    rd.is_managerial,
    rd.employment_status,
    rd.requirement_id,
    rd.document_type_id,
    rd.document_code,
    rd.document_name,
    rd.document_group,
    rd.requirement_level,
    rd.required_storage_form,
    rd.warning_threshold_days,
    rd.requirement_reason,
    d.document_id,
    d.issue_date,
    d.expiry_date,
    d.storage_form,
    d.file_path,
CASE
        WHEN d.document_id IS NOT NULL THEN 'DA_CO' ELSE 'THIEU_HO_SO'
    END AS requirement_status
FROM required_documents rd
LEFT JOIN document d ON d.employee_id = rd.employee_id
    AND d.document_type_id = rd.document_type_id
    AND d.is_current = 1;
GO
/****** Object:  View [dbo].[vw_employee_quality]    Script Date: 11/11/2026 1:58:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   VIEW [dbo].[vw_employee_quality]
AS
WITH requirement_summary AS
(
    SELECT
        r.employee_id,

        COUNT(*) AS total_required_documents,

        SUM(CASE
            WHEN r.requirement_status = 'DA_CO' THEN 1
            ELSE 0
        END) AS total_documents_present,

        SUM(CASE
            WHEN r.requirement_status = 'THIEU_HO_SO' THEN 1
            ELSE 0
        END) AS total_documents_missing,

        SUM(CASE
            WHEN dq.quality_status = 'DA_HET_HAN' THEN 1
            ELSE 0
        END) AS total_documents_expired,

        SUM(CASE
            WHEN dq.quality_status = 'KHONG_HOP_LE' THEN 1
            ELSE 0
        END) AS total_documents_invalid

    FROM vw_employee_document_requirement r
    LEFT JOIN vw_document_quality dq
        ON r.document_id = dq.document_id
    GROUP BY r.employee_id
),

missing_documents AS
(
    SELECT
        employee_id,
        STRING_AGG(document_code, ', ') AS missing_document_codes
    FROM vw_employee_document_requirement
    WHERE requirement_status = 'THIEU_HO_SO'
    GROUP BY employee_id
),

contract_summary AS
(
    SELECT
        employee_id,
        labor_contract_status
    FROM vw_labor_contract_check
)

SELECT
    e.employee_id,
    e.full_name,
    e.department_id,
    e.position_name,
    e.is_managerial,
    e.employment_status,
    e.join_date,
    e.leave_date,

    -- Tổng hợp hồ sơ
    ISNULL(rs.total_required_documents, 0) AS total_required_documents,
    ISNULL(rs.total_documents_present, 0) AS total_documents_present,
    ISNULL(rs.total_documents_missing, 0) AS total_documents_missing,

    -- Danh sách hồ sơ thiếu
    ISNULL(md.missing_document_codes, '') AS missing_document_codes,

    -- Trạng thái hợp đồng
    cs.labor_contract_status,

    -- Nguyên nhân cảnh báo
    CASE
        WHEN e.employment_status = 'ChamDutHDLD'
            THEN N'Không áp dụng đánh giá hồ sơ bắt buộc'

        ELSE COALESCE(
            NULLIF(
                CONCAT_WS(N'; ',

                    CASE
                        WHEN ISNULL(rs.total_documents_missing, 0) > 0
                        THEN N'Thiếu tài liệu'
                    END,

                    CASE
                        WHEN ISNULL(rs.total_documents_expired, 0) > 0
                        THEN N'Tài liệu bắt buộc đã hết hạn'
                    END,

                    CASE
                        WHEN ISNULL(rs.total_documents_invalid, 0) > 0
                        THEN N'Tài liệu bắt buộc không hợp lệ'
                    END,

                    CASE
                        WHEN e.employment_status IN ('DangLamViec', 'TamHoanHDLD')
                             AND cs.labor_contract_status = 'THIEU_HDXDTH1'
                        THEN N'Thiếu hợp đồng HDXDTH1'
                    END,

                    CASE
                        WHEN e.employment_status IN ('DangLamViec', 'TamHoanHDLD')
                             AND cs.labor_contract_status = 'CAN_KY_KET_TIEP'
                        THEN N'Cần ký kết hợp đồng tiếp theo'
                    END
                ),
                ''
            ),
            N'Không có cảnh báo'
        )
    END AS quality_issue,

    -- Chỉ còn 2 trạng thái chất lượng
    CASE
        WHEN ISNULL(rs.total_documents_missing, 0) > 0
          OR ISNULL(rs.total_documents_expired, 0) > 0
          OR ISNULL(rs.total_documents_invalid, 0) > 0
          OR (
                e.employment_status IN ('DangLamViec', 'TamHoanHDLD')
                AND cs.labor_contract_status IN (
                    'THIEU_HDXDTH1',
                    'CAN_KY_KET_TIEP'
                )
             )
            THEN 'THIEU_HO_SO'

        ELSE 'DAY_DU'
    END AS employee_quality_status

FROM employee e
LEFT JOIN requirement_summary rs
    ON e.employee_id = rs.employee_id
LEFT JOIN missing_documents md
    ON e.employee_id = md.employee_id
LEFT JOIN contract_summary cs
    ON e.employee_id = cs.employee_id;

GO
/****** Object:  Table [dbo].[division]    Script Date: 11/11/2026 1:58:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[division](
	[division_id] [int] IDENTITY(1,1) NOT NULL,
	[division_code] [varchar](20) NOT NULL,
	[division_name] [nvarchar](200) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[division_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[division_name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[division_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[qualification]    Script Date: 11/11/2026 1:58:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[qualification](
	[qualification_id] [int] IDENTITY(1,1) NOT NULL,
	[employee_id] [char](8) NOT NULL,
	[qualification_level] [nvarchar](50) NOT NULL,
	[degree_name] [nvarchar](200) NULL,
	[major] [nvarchar](200) NULL,
	[institution] [nvarchar](255) NULL,
	[graduation_date] [date] NULL,
	[notes] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[qualification_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_document_current]    Script Date: 11/11/2026 1:58:40 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_document_current] ON [dbo].[document]
(
	[employee_id] ASC,
	[document_type_id] ASC
)
WHERE ([is_current]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[document] ADD  CONSTRAINT [DF_document_is_current]  DEFAULT ((0)) FOR [is_current]
GO
ALTER TABLE [dbo].[document] ADD  CONSTRAINT [DF_document_created_at]  DEFAULT (sysdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[document] ADD  CONSTRAINT [DF_document_updated_at]  DEFAULT (sysdatetime()) FOR [updated_at]
GO
ALTER TABLE [dbo].[employee] ADD  CONSTRAINT [DF_employee_is_managerial]  DEFAULT ((0)) FOR [is_managerial]
GO
ALTER TABLE [dbo].[requirement_rule] ADD  CONSTRAINT [DF_requirement_rule_is_active]  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [dbo].[department]  WITH CHECK ADD FOREIGN KEY([division_id])
REFERENCES [dbo].[division] ([division_id])
GO
ALTER TABLE [dbo].[document]  WITH CHECK ADD FOREIGN KEY([document_type_id])
REFERENCES [dbo].[document_type] ([document_type_id])
GO
ALTER TABLE [dbo].[document]  WITH CHECK ADD FOREIGN KEY([employee_id])
REFERENCES [dbo].[employee] ([employee_id])
GO
ALTER TABLE [dbo].[employee]  WITH CHECK ADD FOREIGN KEY([department_id])
REFERENCES [dbo].[department] ([department_id])
GO
ALTER TABLE [dbo].[qualification]  WITH CHECK ADD FOREIGN KEY([employee_id])
REFERENCES [dbo].[employee] ([employee_id])
GO
ALTER TABLE [dbo].[document]  WITH CHECK ADD  CONSTRAINT [CK_document_current] CHECK  (([is_current]=(1) OR [is_current]=(0)))
GO
ALTER TABLE [dbo].[document] CHECK CONSTRAINT [CK_document_current]
GO
ALTER TABLE [dbo].[document]  WITH CHECK ADD  CONSTRAINT [CK_document_date] CHECK  (([expiry_date] IS NULL OR [issue_date] IS NULL OR [expiry_date]>=[issue_date]))
GO
ALTER TABLE [dbo].[document] CHECK CONSTRAINT [CK_document_date]
GO
ALTER TABLE [dbo].[document_type]  WITH CHECK ADD  CONSTRAINT [CK_document_type_group] CHECK  (([document_group]=N'Quyết định / Văn bản khác' OR [document_group]=N'Hợp đồng lao động' OR [document_group]=N'Hồ sơ gốc'))
GO
ALTER TABLE [dbo].[document_type] CHECK CONSTRAINT [CK_document_type_group]
GO
ALTER TABLE [dbo].[employee]  WITH CHECK ADD  CONSTRAINT [CK_employee_gender] CHECK  (([gender] IS NULL OR ([gender]='Nu' OR [gender]='Nam')))
GO
ALTER TABLE [dbo].[employee] CHECK CONSTRAINT [CK_employee_gender]
GO
ALTER TABLE [dbo].[employee]  WITH CHECK ADD  CONSTRAINT [CK_employee_leave_date] CHECK  (([leave_date] IS NULL OR [leave_date]>=[join_date]))
GO
ALTER TABLE [dbo].[employee] CHECK CONSTRAINT [CK_employee_leave_date]
GO
ALTER TABLE [dbo].[employee]  WITH CHECK ADD  CONSTRAINT [CK_employee_managerial] CHECK  (([is_managerial]=(1) OR [is_managerial]=(0)))
GO
ALTER TABLE [dbo].[employee] CHECK CONSTRAINT [CK_employee_managerial]
GO
ALTER TABLE [dbo].[employee]  WITH CHECK ADD  CONSTRAINT [CK_employee_status] CHECK  (([employment_status]='NghiHuu' OR [employment_status]='ChamDutHDLD' OR [employment_status]='TamHoanHDLD' OR [employment_status]='DangLamViec'))
GO
ALTER TABLE [dbo].[employee] CHECK CONSTRAINT [CK_employee_status]
GO
ALTER TABLE [dbo].[requirement_rule]  WITH CHECK ADD  CONSTRAINT [CK_requirement_rule_effective_date] CHECK  (([effective_to] IS NULL OR [effective_to]>=[effective_from]))
GO
ALTER TABLE [dbo].[requirement_rule] CHECK CONSTRAINT [CK_requirement_rule_effective_date]
GO
ALTER TABLE [dbo].[requirement_rule]  WITH CHECK ADD  CONSTRAINT [CK_requirement_rule_level] CHECK  (([requirement_level]='CO_DIEU_KIEN' OR [requirement_level]='TUY_CHON' OR [requirement_level]='BAT_BUOC'))
GO
ALTER TABLE [dbo].[requirement_rule] CHECK CONSTRAINT [CK_requirement_rule_level]
GO
ALTER TABLE [dbo].[requirement_rule]  WITH CHECK ADD  CONSTRAINT [CK_requirement_rule_threshold] CHECK  (([warning_threshold_days] IS NULL OR [warning_threshold_days]>=(0)))
GO
ALTER TABLE [dbo].[requirement_rule] CHECK CONSTRAINT [CK_requirement_rule_threshold]
GO
USE [master]
GO
ALTER DATABASE [VTB2] SET  READ_WRITE 
GO
