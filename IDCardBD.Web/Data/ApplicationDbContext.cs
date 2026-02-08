using Microsoft.EntityFrameworkCore;
using IDCardBD.Web.Models;

namespace IDCardBD.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<SchoolClass> Classes { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<AcademicGroup> AcademicGroups { get; set; }
        public DbSet<CardTemplate> CardTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TPT (Table Per Type) or just rely on default TPH (Table Per Hierarchy)
            // Given requirements, separate tables might be cleaner?
            // "Create 'Student' class... Create 'Employee' class..."
            // But they inherit from IdentityBase. 
            // EF Core default is TPH (one table 'IdentityBase' with discriminator).
            // TPT (Table per Type) would create 'Students' and 'Employees' tables.
            // Let's use TPC (Table per Concrete Type) or TPT if we want to query bases.
            // But usually default is fine. Actually, default TPH is easiest.
            // I'll stick to default unless specified. Or wait, user requirement says:
            // "Create 'Student' class... Create 'Employee' class..."
            // It doesn't explicitly ask for separate tables but implies distinct entities.
            // I'll stick to default EF Core behavior for inheritance (TPH) for simplicity unless it complicates things.
            // Actually, TPH puts all columns in one table which is fine.

            // Optional: Seed data or further config here
        }
    }
}
