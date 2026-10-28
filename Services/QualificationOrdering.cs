using Vtb.Hr.Web.Data;
namespace Vtb.Hr.Web.Services;
public static class QualificationOrdering
{
 public static IOrderedQueryable<Qualification> ByGraduation(this IQueryable<Qualification> rows)=>rows.OrderByDescending(x=>x.GraduationDate.HasValue).ThenByDescending(x=>x.GraduationDate).ThenByDescending(x=>x.QualificationId);
}
