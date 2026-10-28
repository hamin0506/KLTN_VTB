namespace Vtb.Hr.Web.Services;
public static class Labels
{
 public static readonly Dictionary<string,string> All=new(){["Nam"]="Nam",["Nu"]="Nữ",["DangLamViec"]="Đang làm việc",["TamHoanHDLD"]="Tạm hoãn HĐLĐ",["ChamDutHDLD"]="Chấm dứt HĐLĐ",["NghiHuu"]="Nghỉ hưu",["HOP_LE"]="Hợp lệ",["SAP_HET_HAN"]="Sắp hết hạn",["DA_HET_HAN"]="Đã hết hạn",["KHONG_HOP_LE"]="Không hợp lệ",["KHONG_XAC_DINH"]="Không xác định",["DAY_DU"]="Đầy đủ",["THIEU_HO_SO"]="Thiếu hồ sơ",["DA_CO"]="Đã có",["THIEU_HDXDTH1"]="Thiếu HĐXĐTH1",["CAN_KY_KET_TIEP"]="Cần ký kết tiếp",["KHONG_CANH_BAO"]="Không cảnh báo",["BAT_BUOC"]="Bắt buộc",["TUY_CHON"]="Tùy chọn",["CO_DIEU_KIEN"]="Có điều kiện"};
 public static string Text(string? v)=>v==null?"—":All.GetValueOrDefault(v,v);
 public static string Color(string? v)=>v is "HOP_LE" or "DAY_DU" or "DA_CO"?"green":v is "SAP_HET_HAN" or "CAN_KY_KET_TIEP" or "THIEU_HO_SO"?"yellow":v is "DA_HET_HAN" or "KHONG_HOP_LE" or "THIEU_HDXDTH1"?"red":"secondary";
}
