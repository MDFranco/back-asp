namespace WebAppiN5now.Data
{
    using Microsoft.EntityFrameworkCore;
    using System.Security;
    using Model;

    public class ApplicationDbContext : DbContext
    {
        public DbSet<PermissionType> PermissionTypes { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PermissionType>().ToTable("PermissionTypes");
            modelBuilder.Entity<Permission>().ToTable("Permissions");
        }
    }
}
