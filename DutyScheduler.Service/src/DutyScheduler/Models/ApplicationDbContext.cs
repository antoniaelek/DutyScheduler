using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using ModelBuilder = Microsoft.EntityFrameworkCore.ModelBuilder;

namespace DutyScheduler.Models
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.HasPostgresExtension("uuid-ossp");
            //builder.Entity<User>().HasMany(u => u.Preferences).WithOne(p => p.User);//.OnDelete(DeleteBehavior.SetNull);
        }
        public DbSet<Preference> Preference { get; set; }
        public DbSet<ReplacementHistory> ReplacementHistory { get; set; }
        public DbSet<ReplacementRequest> ReplacementRequest { get; set; }
        public DbSet<Shift> Shift { get; set; }
        // public DbSet<SchedulerLog> SchedulerLog { get; set; }
    }
}
