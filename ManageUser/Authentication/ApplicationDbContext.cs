using ManageUser.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageUser.Authentication
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }
        }
        public DbSet<UserInfo> UserInfo { get; set; }
        public DbSet<ApplicationUser> User { get; set; }
        public DbSet<AdvanceMoney> AdvanceMoney { get; set; }
        public DbSet<Bonus> Bonus { get; set; }
        public DbSet<DayOff> DayOff { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<SalaryOfMonth> SalaryOfMonth { get; set; }
        public DbSet<Position> Position { get; set; }
        public DbSet<HistoryOfSalary> HistoryOfSalary { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
    }
}
