using Microsoft.EntityFrameworkCore;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.DataAccess.Repository.EntityFramework
{
    public class FirstStart
    {
        private static string[] entityNames = new string[]
        {
            "Identity",
            "MailBox",
            "Media",
            "Method",
            "NotificationContent",
            "Role",
            "RoleMethod",
            "RoleUser",
            "Session",
            "Setting",
            "User",
        };

        private static string[] methodTypes = new string[]
        {
            "List",
            "Get",
            "Add",
            "Update",
            "Delete",
        };
        private static DbContextOptions<DatabaseContext> options
        {
            get
            {
                var sqltype = AppSettings.instance?.ConnectionStrings?.SqlType ?? "mssql";
                if (sqltype == "mssql")
                {
                    return new DbContextOptionsBuilder<DatabaseContext>()
                        .UseSqlServer(AppSettings.instance!.ConnectionStrings!.DefaultConnection)
                        .Options;
                }
                if (sqltype == "postgre")
                {
                    return new DbContextOptionsBuilder<DatabaseContext>()
                        .UseNpgsql(AppSettings.instance!.ConnectionStrings!.PostgreSQLConnection)
                        .Options;
                }
                return new DbContextOptionsBuilder<DatabaseContext>()
                        .UseSqlServer(AppSettings.instance!.ConnectionStrings!.DefaultConnection)
                        .Options;
            }
        }
        private static DatabaseContext context = new DatabaseContext(options);

        public static void CreateDB()
        {
            context.Database.Migrate();
            if (context.Users.Count() == 0)
            {
                // Create Roles
                var roles = new[]{
                        new Role
                        {
                            Name = "Admin",
                            Description = "Admin",
                            Key = "Admin",
                            CreateDate = DateTime.UtcNow,
                            IsDeleted = false,
                        },
                        new Role
                        {
                            Name = "User",
                            Description = "User",
                            Key = "User",
                            CreateDate = DateTime.UtcNow,
                            IsDeleted = false,
                        },
                    };
                foreach (var role in roles)
                    context.Roles.Add(role);
                context.SaveChanges();

                // Create Methods
                var methods = entityNames.Select(entityName => methodTypes.Select(methodType => new Method
                {
                    Name = $"{entityName}.{methodType}",
                    CreateDate = DateTime.UtcNow,
                    IsDeleted = false,
                })).SelectMany(x => x).ToList();
                foreach (var method in methods)
                    context.Methods.Add(method);
                context.SaveChanges();

                // Create RoleMethods
                var roleMethods = context.Roles.ToList().Select(role => context.Methods.Select(method => new RoleMethod
                {
                    RoleId = role.Id,
                    MethodId = method.Id,
                    CreateDate = DateTime.UtcNow,
                    IsDeleted = false,
                })).SelectMany(x => x).ToList();
                foreach (var roleMethod in roleMethods)
                    context.RoleMethods.Add(roleMethod);
                context.SaveChanges();

                // Create Admin User
                var adminUser = new User
                {
                    Name = "Admin",
                    Surname = "User",
                    Email = "admin@example.com",
                    Phone = "5555555555",
                    Key = Guid.NewGuid().ToString(),
                    CreateDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };
                context.Users.Add(adminUser);
                context.SaveChanges();

                // Create Admin Identity
                var salt = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var adminIdentity = new Identity
                {
                    UserId = adminUser.Id,
                    PasswordSalt = salt,
                    PasswordHash = "dde36a38204964f7c1c1aff83cda4efb",//ExtensionMethods.CalculateMD5Hash(salt + "Admin123" + salt),
                    IsActive = true,
                    CreateDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                context.Identities.Add(adminIdentity);
                context.SaveChanges();

                // Create Admin RoleUser
                var adminRoleUser = new RoleUser
                {
                    UserId = adminUser.Id,
                    RoleId = roles.First(r => r.Key == "Admin").Id,
                    CreateDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                context.RoleUsers.Add(adminRoleUser);
                context.SaveChanges();

                // Create Settings
                //var settings = new[]
                //{
                //        //new Setting { Key = "SiteTitle", Value = "Real Estate Management System", CreateDate = DateTime.UtcNow, IsDeleted = false,SettingType=ESetting.TEXT },
                //        //new Setting { Key = "SiteDescription", Value = "Professional Real Estate Management Solution", CreateDate = DateTime.UtcNow, IsDeleted = false , SettingType = ESetting.TEXT},
                //        //new Setting { Key = "ContactEmail", Value = "contact@example.com", CreateDate = DateTime.UtcNow, IsDeleted = false,SettingType = ESetting.TEXT },
                //        //new Setting { Key = "ContactPhone", Value = "+90 555 555 5555", CreateDate = DateTime.UtcNow, IsDeleted = false ,SettingType = ESetting.TEXT},
                //        //new Setting { Key = "SmtpSenderMail",     Value = "no-reply@yourdomain.com", CreateDate = DateTime.UtcNow, IsDeleted = false, SettingType = ESetting.TEXT },
                //        //new Setting { Key = "SmtpSenderPassword", Value = "your_smtp_password",      CreateDate = DateTime.UtcNow, IsDeleted = false, SettingType = ESetting.TEXT },
                //        //new Setting { Key = "SmtpSmtpMail",       Value = "smtp@yourdomain.com",     CreateDate = DateTime.UtcNow, IsDeleted = false, SettingType = ESetting.TEXT },
                //        //new Setting { Key = "SmtpSmtpPort",       Value = "587",                     CreateDate = DateTime.UtcNow, IsDeleted = false, SettingType = ESetting.TEXT },
                //        //new Setting { Key = "SmtpSmtpServer",     Value = "smtp.yourdomain.com",     CreateDate = DateTime.UtcNow, IsDeleted = false, SettingType = ESetting.TEXT }

                //    };
                //foreach (var setting in settings)
                //    context.Settings.Add(setting);
                //context.SaveChanges();

            }
        }
    }
}
