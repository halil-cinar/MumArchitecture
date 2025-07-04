using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Targets;
using NLog.Targets.Wrappers;
using MumArchitecture.Business.Notification;
using MumArchitecture.Business.Result;
using MumArchitecture.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Logging
{
    public class LogManager
    {
        public static Logger Logger;
        public static TelegramNotify Telegram;
        public static void Config()
        {
            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget("fileTarget")
            {
                FileName = "logs/logfile.txt",
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}"
            };
            var asyncFileTarget = new AsyncTargetWrapper(fileTarget)
            {
                Name = "asyncFileTarget",
                QueueLimit = 5000,
                OverflowAction = AsyncTargetWrapperOverflowAction.Discard
            };
            var mailTarget = new MailTarget("mailTarget")
            {
                SmtpServer = "smtp.your-email-provider.com",
                SmtpPort = 587,
                SmtpAuthentication = SmtpAuthenticationMode.Basic,
                SmtpUserName = "your-email@example.com",
                SmtpPassword = "your-email-password",
                EnableSsl = true,
                From = "your-email@example.com",
                To = "recipient-email@example.com",
                Subject = "Yüksek Seviye Log Bildirimi",
                Html = true,
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}"
            };
            var asyncMailTarget = new AsyncTargetWrapper(mailTarget)
            {
                Name = "asyncMailTarget",
                QueueLimit = 100,
                OverflowAction = AsyncTargetWrapperOverflowAction.Grow
            };

            var infoFileTarget = new FileTarget("infoFileTarget")
            {
                FileName = "logs/info/logfile-${shortdate}.txt",
                Layout = "${longdate}|${level:uppercase=true}|${ip-address}|${logger}|${message}",
                ArchiveEvery = FileArchivePeriod.Day,
                MaxArchiveFiles = 30, // **30 gün boyunca arşivleri sakla**
                ArchiveNumbering = ArchiveNumberingMode.Date,
                ConcurrentWrites = true,
                KeepFileOpen = false
            };
            config.AddTarget(asyncFileTarget);
            config.AddTarget(asyncMailTarget);
            //config.AddTarget(infoFileTarget);

            config.AddRuleForAllLevels(asyncFileTarget);
            // Error ve üzeri seviyeler için e-posta gönderimi
            config.AddRule(LogLevel.Error, LogLevel.Fatal, asyncMailTarget);
            var ruleInfo = new LoggingRule("*", LogLevel.Trace, LogLevel.Info, infoFileTarget);
            config.LoggingRules.Add(ruleInfo);

            NLog.LogManager.Configuration = config;

            //logger= NLog.LogManager.GetCurrentClassLogger();
        }

        static LogManager()
        {
            Config();
            Logger = NLog.LogManager.GetCurrentClassLogger();
            try
            {
                Telegram = new TelegramNotify();
            }
            catch
            {

            }
        }

        public static void LogException(UserException ex)
        {
            Logger.Warn(ex);
            Exception? innerException = ex.InnerException;
            while (innerException != null)
            {
                Logger.Warn(innerException, "User Exception => " + ex.Message);
                innerException = innerException.InnerException;
            }
        }
        public static void LogException(Exception ex)
        {
            Logger.Error(ex);
            NotifyUs(ex);
            Exception? innerException = ex.InnerException;
            while (innerException != null)
            {
                Logger.Error(innerException, "Main Exception => " + ex.Message);
                innerException = innerException.InnerException;
            }
        }
        public static void LogInfo(string message)
        {
            Logger.Info(message);
        }
        public static void LogDifferences<T>(string info, T newObject, T? oldObject = null)
            where T : class, new()
        {
            //if (newObject == null)
            //{
            //    Logger.Warn("Karşılaştırılacak nesneler null.");
            //    return;
            //}
            //if (oldObject == null)
            //{
            //    oldObject = new T();
            //}
            //Type type = typeof(T);
            //PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            //var message = "";
            //message += info + " " + type.FullName;

            //foreach (PropertyInfo property in properties)
            //{
            //    object? oldValue = property.GetValue(oldObject!);
            //    object? newValue = property.GetValue(newObject!);
            //    if (!Equals(oldValue, newValue) || property.Name == "Id")
            //    {
            //        message += $"\nÖzellik '{property.Name}' değişti: Eski Değer = '{oldValue}', Yeni Değer = '{newValue}'";
            //    }
            //    Logger.Info(message);
            //}
        }

        public static void NotifyUs(string message)
        {
            Telegram.SendMessage("Proje url: " + AppSettings.instance!.ApplicationUrl + "\n" + message).Wait();
        }
        public static void NotifyUs(Exception ex)
        {
            Telegram.SendMessage("Bir hata oluştu: \n Proje url: " + AppSettings.instance!.ApplicationUrl + "\n" + ex.Message + "    \n" + JsonSerializer.Serialize(ex)).Wait();
        }

    }

}
