using System.Linq.Expressions;
using Vtb.Hr.Web.Data;
namespace Vtb.Hr.Web.Services;

// Canonical order is the first occurrence in the unfiltered Requirement Rule page.
public static class DocumentTypeOrdering
{
 private sealed class Ranked<T>{public T Item{get;set;}=default!;public int TypeId{get;set;}public int? FirstRule{get;set;}}
 private static IOrderedQueryable<Ranked<T>> Rank<T>(IQueryable<T> source,Expression<Func<T,int>> key,IQueryable<DocumentType> types,IQueryable<RequirementRule> rules)
 {
  // Project each source row once: sorting must never depend on a matching catalog row.
  var item=key.Parameters[0];
  var projection=Expression.Lambda<Func<T,Ranked<T>>>(Expression.MemberInit(Expression.New(typeof(Ranked<T>)),
   Expression.Bind(typeof(Ranked<T>).GetProperty(nameof(Ranked<T>.Item))!,item),
   Expression.Bind(typeof(Ranked<T>).GetProperty(nameof(Ranked<T>.TypeId))!,key.Body)),item);
  return source.Select(projection).Select(x=>new Ranked<T>{Item=x.Item,TypeId=x.TypeId,
   FirstRule=rules.Where(r=>r.DocumentTypeId==x.TypeId).Min(r=>(int?)r.RequirementId)})
   .OrderBy(x=>x.FirstRule==null).ThenBy(x=>x.FirstRule).ThenBy(x=>x.TypeId);
 }
 public static IQueryable<DocumentType> InRequirementOrder(this IQueryable<DocumentType> source,VtbContext db)=>source.InRequirementOrder(db.RequirementRules);
 public static IQueryable<DocumentType> InRequirementOrder(this IQueryable<DocumentType> source,IQueryable<RequirementRule> rules)=>Rank(source,x=>x.DocumentTypeId,source,rules).Select(x=>x.Item);
 public static IQueryable<Document> InRequirementOrder(this IQueryable<Document> source,VtbContext db)=>source.InRequirementOrder(db.DocumentTypes,db.RequirementRules);
 public static IQueryable<Document> InRequirementOrder(this IQueryable<Document> source,IQueryable<DocumentType> types,IQueryable<RequirementRule> rules)=>Rank(source,x=>x.DocumentTypeId,types,rules)
  .ThenByDescending(x=>x.Item.IsCurrent).ThenByDescending(x=>x.Item.IssueDate)
  .ThenByDescending(x=>x.Item.CreatedAt).ThenByDescending(x=>x.Item.DocumentId).Select(x=>x.Item);
 public static IOrderedQueryable<RequirementRule> InPageOrder(this IQueryable<RequirementRule> source)=>source.OrderBy(x=>x.RequirementId);
}
