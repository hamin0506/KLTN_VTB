using Microsoft.Data.SqlClient;
namespace Vtb.Hr.Web.Services;
public sealed class DashboardListReadException(string stage,Exception inner):Exception("Dashboard employee list read failed at "+stage,inner)
{
 public string Stage{get;}=stage;
 public SqlException? SqlError {get{for(Exception? e=InnerException;e!=null;e=e.InnerException)if(e is SqlException sql)return sql;return null;}}
 public static (int Status,string Message) Describe(int? number)=>number switch{
  2 or 53 or 26 or 40 or 233 or 10060 or 10061 or 11001 or 18456 or 4060=>(503,"Không thể kết nối cơ sở dữ liệu hoặc tài khoản chưa có quyền truy cập."),
  -2=>(503,"Truy xuất danh sách cán bộ quá thời gian chờ. Vui lòng thử lại."),
  _=>(500,"Không thể truy xuất dữ liệu danh sách cán bộ. Vui lòng cung cấp mã lỗi cho người quản trị.")
 };
}
