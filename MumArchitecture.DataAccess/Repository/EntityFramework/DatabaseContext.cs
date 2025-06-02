using Microsoft.EntityFrameworkCore;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.DataAccess.Repository.EntityFramework
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(AppSettings.instance!.ConnectionStrings!.DefaultConnection);
            optionsBuilder.EnableSensitiveDataLogging();
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                 .SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
            }
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Identity> Identities { get; set; }
        public DbSet<MailBox> MailBoxes { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<Method> Methods { get; set; }
        public DbSet<NotificationContent> NotificationContents { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleMethod> RoleMethods { get; set; }
        public DbSet<RoleUser> RoleUsers { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
