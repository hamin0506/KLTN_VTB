using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.RegularExpressions;
namespace Vtb.Hr.Web.Data;
public partial class VtbContext
{
 public DbSet<Qualification> Qualifications=>Set<Qualification>();
 public DbSet<Employee> Employees=>Set<Employee>();public DbSet<Document> Documents=>Set<Document>();public DbSet<DocumentType> DocumentTypes=>Set<DocumentType>();public DbSet<RequirementRule> RequirementRules=>Set<RequirementRule>();
 public DbSet<DocumentQuality> DocumentQualities=>Set<DocumentQuality>();public DbSet<EmployeeQuality> EmployeeQualities=>Set<EmployeeQuality>();public DbSet<DocumentRequirement> DocumentRequirements=>Set<DocumentRequirement>();public DbSet<LaborContractCheck> LaborContractChecks=>Set<LaborContractCheck>();
 private static string Snake(string name)=>Regex.Replace(name,"([a-z0-9])([A-Z])","$1_$2").ToLowerInvariant();
 private void ConfigureRecords(ModelBuilder m)
 {
  m.Entity<Employee>().Property(x=>x.EmployeeId).ValueGeneratedNever();
  m.Entity<Qualification>().Property(x=>x.QualificationId).UseIdentityColumn();
  m.Entity<Qualification>().HasOne(x=>x.Employee).WithMany(x=>x.Qualifications).HasForeignKey(x=>x.EmployeeId).OnDelete(DeleteBehavior.NoAction);
  m.Entity<Employee>().HasOne<Department>().WithMany().HasForeignKey(x=>x.DepartmentId).OnDelete(DeleteBehavior.NoAction);
  m.Entity<Document>().HasOne<Employee>().WithMany().HasForeignKey(x=>x.EmployeeId).OnDelete(DeleteBehavior.NoAction);
  m.Entity<Document>().HasOne<DocumentType>().WithMany().HasForeignKey(x=>x.DocumentTypeId).OnDelete(DeleteBehavior.NoAction);
  // AFTER INSERT trigger owns current-version selection. SQL OUTPUT without INTO is incompatible with triggers.
  m.Entity<Document>().ToTable("document","dbo",t=>t.UseSqlOutputClause(false));
  var current=m.Entity<Document>().Property(x=>x.IsCurrent).HasDefaultValue(false).ValueGeneratedOnAddOrUpdate();
  current.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
  current.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
  m.Entity<Document>().HasIndex(x=>new{x.EmployeeId,x.DocumentTypeId}).IsUnique().HasFilter("[is_current] = 1").HasDatabaseName("UX_document_current");
  m.Entity<Document>().Property(x=>x.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("sysdatetime()");
  m.Entity<Document>().Property(x=>x.CreatedAt).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
  m.Entity<Document>().Property(x=>x.CreatedAt).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
  m.Entity<Document>().Property(x=>x.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("sysdatetime()");
  m.Entity<DocumentType>().HasIndex(x=>x.DocumentCode).IsUnique();m.Entity<DocumentType>().HasIndex(x=>x.DocumentName).IsUnique();
  m.Entity<RequirementRule>(); // No FK exists for this table in schema.sql.
  m.Entity<DocumentQuality>().HasNoKey().ToView("vw_document_quality","dbo");
  m.Entity<EmployeeQuality>().HasNoKey().ToView("vw_employee_quality","dbo");
  m.Entity<DocumentRequirement>().HasNoKey().ToView("vw_employee_document_requirement","dbo");
  m.Entity<LaborContractCheck>().HasNoKey().ToView("vw_labor_contract_check","dbo");
  foreach(var type in new[]{typeof(Qualification),typeof(Employee),typeof(Document),typeof(DocumentType),typeof(RequirementRule),typeof(DocumentQuality),typeof(EmployeeQuality),typeof(DocumentRequirement),typeof(LaborContractCheck)})
   foreach(var p in m.Entity(type).Metadata.GetProperties())p.SetColumnName(Snake(p.Name));
 }
}