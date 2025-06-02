using Microsoft.Extensions.DependencyInjection;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.Business.Logging;
using MumArchitecture.Business.Notification;
using MumArchitecture.Business.Result;
using MumArchitecture.DataAccess.Abstract;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Telegram.Bot.Types;
using User = MumArchitecture.Domain.Entities.User;

namespace MumArchitecture.Business.Services
{
    public class IdentityService : ServiceBase<Identity>, IIdentityService, IAddScope
    {
        private readonly IRepository<User> _userRepository;
        //private readonly IRepository<Content> _contentRepository;
        private readonly INotificationContentService _notificationContentService;
        private readonly IMailService _mailService;
        public IdentityService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _userRepository = serviceProvider.GetRequiredService<IRepository<User>>();
            
            //todo: _notificationContentService = serviceProvider.GetRequiredService<INotificationContentService>();
            _mailService = serviceProvider.GetRequiredService<IMailService>();
        }
        /// <summary>
        /// email ve şifrenin doğruluğunu test eder
        /// </summary>
        /// <param name="ıdentity"></param>
        /// <returns>hata, doğruysa true ve yanlışsa false döner</returns>
        public async Task<SystemResult<bool>> CheckPassword(IdentityCheckDto ıdentity)
        {
            var result = new SystemResult<bool>();
            try
            {
                var user = (await _userRepository.Get(x => x.Email == ıdentity.Email && x.IsActive)) ?? throw new UserException(Lang.Value("RecordNotFound"));
                var identity = (await Repository.Get(x => x.UserId == user.Id && x.IsActive)) ?? throw new UserException(Lang.Value("RecordNotFound"));
                var hash = CalculateHash(identity.PasswordSalt!, ıdentity.Password);
                result.Data = hash == identity.PasswordHash;
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                result.AddMessage(Lang.Value("AnErrorOccuraced"));
            }
            return result;
        }

        /// <summary>
        /// İlk aşama kullanıcı şifresini unutunca sadece eposta adresini girecek ve epostasına bir link iletilecek. 
        /// link bir key içerecek  linke tıklanınca diğer ForgatPassword methodu çağrılacak
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<SystemResult<bool>> ForgatPassword(string email)
        {
            var result = new SystemResult<bool>();
            try
            {
                var user = (await _userRepository.Get(x => x.Email == email)) ?? throw new UserException(Lang.Value("UserNotFound"));
                var link = AppSettings.instance?.ApplicationUrl + AppSettings.instance?.ForgatPasswordLink;
                link = link.Replace("//", "/");
                link = link.Replace("[KEY]", user.Key);
                var content = await _notificationContentService.Get(Filter<NotificationContent>.CreateFilter(x =>
                x.ContentTarget == EContentTarget.Email
                && x.ContentType == EContentType.ForgatPassword
                && x.ContentLang == Enum.Parse<EContentLang>(CultureInfo.CurrentCulture.TwoLetterISOLanguageName)),
                new Dictionary<string, string>
                {
                    {"[LINK]",link },
                    {"[USER_FULLNAME]",user.Name+" "+user.Surname},
                    {"[DATETIME]",DateTime.Now.ToString("g")},
                });

                if (!content.IsSuccess)
                {
                    result.AddMessage(content);
                    return result;
                }
                if (content.Data == null)
                {
                    throw new Exception("ForgatPassword content bulunamadı. Dil: " + CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
                }

                var mailResult = await _mailService.SaveMail(new MailBoxDto
                {
                    Content = content.Data.FirstOrDefault().Content,
                    Subject = content.Data.FirstOrDefault().Subject,
                    To = email,
                    SendNow = true
                });
                if (!mailResult.IsSuccess)
                {
                    result.AddMessage(mailResult);
                    return result;
                }
                result.Data = true;

            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }
            return result;
        }

        public async Task<SystemResult<bool>> ForgatPassword(string key, IdentityCheckDto ıdentity)
        {
            var result = new SystemResult<bool>();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var user = (await _userRepository.Get(x => x.Key == key)) ?? throw new UserException(Lang.Value("UserNotFound"));

                    var content = await _notificationContentService.Get(Filter<NotificationContent>.CreateFilter(x =>
                    x.ContentTarget == EContentTarget.Email
                    && x.ContentType == EContentType.PasswordChange
                    && x.ContentLang == Enum.Parse<EContentLang>(CultureInfo.CurrentCulture.TwoLetterISOLanguageName)),
                    new Dictionary<string, string>
                    {
                    {"[USER_FULLNAME]",user.Name+" "+user.Surname},
                    {"[DATETIME]",DateTime.Now.ToString("g")},
                    });

                    if (!content.IsSuccess)
                    {
                        result.AddMessage(content);
                        scope.Dispose();
                        return result;
                    }
                    if (content.Data == null)
                    {
                        throw new Exception("ForgatPassword content bulunamadı. Dil: " + CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
                    }

                    var identityResult = await Save(new IdentityDto
                    {
                        Password = ıdentity.Password,
                        UserId = user.Id
                    });
                    if (!identityResult.IsSuccess)
                    {
                        result.AddMessage(identityResult);
                        scope.Dispose();
                        return result;
                    }

                    user.Key = Guid.NewGuid().ToString();
                    await _userRepository.Update(user);

                    var mailResult = await _mailService.SaveMail(new MailBoxDto
                    {
                        Content = content.Data.FirstOrDefault().Content,
                        Subject = content.Data.FirstOrDefault().Subject,
                        To = user.Email,
                        SendNow = true
                    });
                    result.Data = true;
                    scope.Complete();
                }
                catch (UserException ex)
                {
                    result.AddMessage(ex.Message);
                }
                catch (Exception ex)
                {
                    LogManager.LogException(ex);
                    result.AddDefaultErrorMessage(ex);
                }
                scope.Dispose();
            }
            return result;
        }

        /// <summary>
        /// diğer servisler dışında çağrılmamalı kullanıcı idsine göre kullanıcıya yeni şifre ataması yapar
        /// </summary>
        /// <param name="ıdentity"></param>
        /// <returns></returns>
        public async Task<SystemResult<bool>> Save(IdentityDto ıdentity)
        {
            var result = new SystemResult<bool>();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var oldIdentities = await Repository.GetAll(x => x.UserId == ıdentity.UserId);
                    oldIdentities ??= new List<Identity>();
                    foreach (var oldIdentity in oldIdentities)
                    {
                        oldIdentity.IsActive = false;
                        await Repository.Update(oldIdentity);
                    }
                    var salt = ExtensionMethods.GenerateRandomString(128);
                    var newIdentity = new Identity
                    {
                        IsActive = true,
                        PasswordSalt = salt,
                        PasswordHash = CalculateHash(salt, ıdentity.Password ?? throw new UserException(Lang.Value("ValueMustBeEnter"))),
                        UserId = ıdentity.UserId,
                    };
                    await Repository.Add(newIdentity);
                    scope.Complete();
                }
                catch (UserException ex)
                {
                    result.AddMessage(ex.Message);
                }
                catch (Exception ex)
                {
                    result.AddMessage(Lang.Value("AnErrorOccuraced"));
                }
                scope.Dispose();
            }
            return result;
        }

        private string CalculateHash(string salt, string password)
        {
            return ExtensionMethods.CalculateMD5Hash(salt + password + salt);
        }

    }
}
