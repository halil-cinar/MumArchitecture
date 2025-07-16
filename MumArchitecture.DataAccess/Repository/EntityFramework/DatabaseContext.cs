using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.DependencyInjection;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.ListDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MumArchitecture.DataAccess.Repository.EntityFramework
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            ChangeTracker.LazyLoadingEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(AppSettings.instance!.ConnectionStrings!.DefaultConnection);
            //optionsBuilder.UseMySql(AppSettings.instance!.ConnectionStrings!.DefaultConnection, ServerVersion.AutoDetect(AppSettings.instance!.ConnectionStrings!.DefaultConnection));
            optionsBuilder.UseNpgsql(AppSettings.instance!.ConnectionStrings!.PostgreSQLConnection, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(0);//, TimeSpan.FromSeconds(10), null);
            });
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            //optionsBuilder.EnableSensitiveDataLogging();
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

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (AppSettings.instance?.AuditLogEnabled == true)
            {
                ChangeTracker.DetectChanges();
                var entries = ChangeTracker.Entries();
                foreach (var entry in entries)
                {
                    if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    {
                        continue;
                    }
                    if (!entry.Entity.GetType().IsDefined(typeof(AuditLogAttribute), true))
                    {
                        continue;
                    }
                    var entity = entry.Entity;
                    object? oldEntity = null;
                    int? id = null;
                    if (entity is Entity e)
                    {
                        oldEntity = this.FindAsync(entity.GetType(), e.Id);
                        id = e.Id;
                    }
                    string DiffAsJson(object oldObj, object newObj, Type type)
                    {
                        var diffs = new List<Dictionary<string, object?>>();

                        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            var oldVal = prop.GetValue(oldObj);
                            var newVal = prop.GetValue(newObj);
                            if ((oldVal == null && newVal == null) || (oldVal?.Equals(newVal) ?? false)) continue;

                            diffs.Add(new Dictionary<string, object?>
                            {
                                [prop.Name] = new { newValue = newVal, oldValue = oldVal }
                            });
                        }

                        return JsonSerializer.Serialize(diffs, new JsonSerializerOptions { WriteIndented = true });
                    }
                    var context = AppSettings.instance?.serviceProvider?.GetRequiredService<IHttpContextAccessor>()?.HttpContext;
                    var session = (SessionListDto?)context?.Items?["UserSession"];
                    var auditEntity = new AuditLog()
                    {
                        EntityType = entry.Metadata.GetTableName() ?? entry.Metadata.Name,
                        Action = entry.State.ToString(),
                        EntityId = id ?? 0,
                        EntityJson = JsonSerializer.Serialize(entity),
                        OldEntityJson = JsonSerializer.Serialize(oldEntity),
                        UpdateDate = DateTime.UtcNow,
                        CreateDate = DateTime.UtcNow,
                        Date = DateTime.UtcNow,
                        UpdatedProperties = oldEntity != null ? DiffAsJson(oldEntity, entity, entity.GetType()) : "",
                        UserId = session?.UserId,
                        UserEmail = session?.User?.Email ?? "",
                        IsDeleted = false
                    };
                    this.Set<AuditLog>().Add(auditEntity);
                }
            }
            return base.SaveChangesAsync(cancellationToken);
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
        public DbSet<Menu> Menus { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }


}
