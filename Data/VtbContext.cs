using Microsoft.EntityFrameworkCore;
namespace Vtb.Hr.Web.Data;
// Mapping derived from schema.sql; replace only after reviewing a live Database First scaffold.
public partial class VtbContext(DbContextOptions<VtbContext> options) : DbContext(options)
{
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<Department> Departments => Set<Department>();
    protected override void OnModelCreating(ModelBuilder m) {
        ConfigureRecords(m);
        m.Entity<Department>(e => {
            e.ToTable("department", "dbo");
            e.HasKey(x => x.DepartmentId);
            e.Property(x => x.DepartmentId).HasColumnName("department_id").UseIdentityColumn();
            e.Property(x => x.DivisionId).HasColumnName("division_id");
            e.Property(x => x.DepartmentCode).HasColumnName("department_code").HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.DepartmentName).HasColumnName("department_name").HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.DepartmentCode).IsUnique();
            e.HasOne(x => x.Division).WithMany().HasForeignKey(x => x.DivisionId).OnDelete(DeleteBehavior.NoAction);
        });
        m.Entity<Division>(e => { e.ToTable("division", "dbo"); e.HasKey(x=>x.DivisionId);
            e.Property(x=>x.DivisionId).HasColumnName("division_id").UseIdentityColumn();
            e.Property(x=>x.DivisionCode).HasColumnName("division_code").HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x=>x.DivisionName).HasColumnName("division_name").HasMaxLength(200).IsRequired();
            e.HasIndex(x=>x.DivisionCode).IsUnique(); e.HasIndex(x=>x.DivisionName).IsUnique(); });
    }
}