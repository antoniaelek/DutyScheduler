using System.Linq;
using DutyScheduler.ViewModels;
using Microsoft.AspNetCore.Identity;
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

        public async void EnsureSeedData(UserManager<User> userMgr, RegisterViewModel defaultAdmin, string password)
        {
            if (!Users.Any(u => u.IsAdmin))
            {
                // create admin user
                var adminUser = new User
                {
                    UserName = defaultAdmin.UserName,
                    Email = defaultAdmin.Email,
                    IsAdmin = true,
                    Name = defaultAdmin.Name,
                    LastName = defaultAdmin.LastName,
                    Office = defaultAdmin.Office,
                    Phone = defaultAdmin.Phone
                };
                await userMgr.CreateAsync(adminUser, password);
            }
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
