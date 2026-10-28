namespace Vtb.Hr.Web.Data;
public partial class Department
{
    public int DepartmentId { get; set; }
    public int DivisionId { get; set; }
    public string DepartmentCode { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public Division Division { get; set; } = null!;
}