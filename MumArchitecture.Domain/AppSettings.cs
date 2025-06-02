using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain
{
    public class AppSettings
    {
        public static AppSettings? instance;
        public static void Initialize(AppSettings appSettings)
        {
            instance = appSettings;
        }
        public LoggingSettings? Logging { get; set; }
        public string? AllowedHosts { get; set; }
        public TelegramSettings? Telegram { get; set; }
        public EmailSmtpSettings? EmailSMTP { get; set; }
        public ConnectionStrings? ConnectionStrings { get; set; }
        public string? ApplicationUrl { get; set; }
        public string? ForgatPasswordLink { get; set; }
        public string? MediaBaseDirectory { get; set; }
        public string? JwtSecret { get; set; }
        public string? LocalizationLangs { get; set; }
        public string? DefaultCulture { get; set; }
        public IServiceProvider? serviceProvider { get; set; }
    }

    public class LoggingSettings
    {
        public LogLevelSettings? LogLevel { get; set; }
    }

    public class LogLevelSettings
    {
        public string? Default { get; set; }
        public string? MicrosoftAspNetCore { get; set; }
    }

    public class TelegramSettings
    {
        public string? ApiToken { get; set; }
        public long? DevChatId { get; set; }
    }

    public class EmailSmtpSettings
    {
        public string? SmtpServer { get; set; }
        public string? SmtpMail { get; set; }
        public int? SmtpPort { get; set; }
        public string? SmtpUserName { get; set; }
        public string? SmtpPassword { get; set; }
        public string? From { get; set; }
        public string? DevTo { get; set; }

    }
    public class ConnectionStrings
    {
        public string? DefaultConnection { get; set; }
    }

}
