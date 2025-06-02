using Microsoft.Extensions.DependencyInjection;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.Business.Logging;
using MumArchitecture.Business.Notification;
using MumArchitecture.Business.Result;
using MumArchitecture.DataAccess.Abstract;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.ListDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Services
{
    public class MailService : ServiceBase<MailBox>, IMailService, IAddScope
    {
        private readonly IRepository<Setting> _settingRepository;
        public MailService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _settingRepository = serviceProvider.GetRequiredService<IRepository<Setting>>();
        }

        public async Task<PaggingResult<MailBoxListDto>> GetAllMails(Filter<MailBox> filter)
        {
            var result = new PaggingResult<MailBoxListDto>();
            try
            {
                result.CurrentPage = filter.Page;
                result.ItemCount = filter.Count;
                var count = (await Repository.Count(filter.ConvertDbQuery()));
                result.AllCount = count;
                result.AllPageCount = (int)Math.Ceiling(count / filter.Count * 1.0);
                var mail = await Repository.GetAll(filter.ConvertDbQuery());
                result.Data = mail.Select(x => (MailBoxListDto)x).ToList();
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

        public async Task<SystemResult<MailBoxListDto>> GetMail(Filter<MailBox> filter)
        {
            var result = new SystemResult<MailBoxListDto>();
            try
            {
                var mail = await Repository.Get(filter.ConvertDbQuery());// ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                result.Data = mail;
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

        public async Task<SystemResult<Nothing>> OpenMail(int id)
        {
            var result = new SystemResult<Nothing>();
            try
            {
                var mail = await Repository.Get(x => x.Id == id) ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                mail.OpeningCount += 1;
                await Repository.Update(mail);
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

        public async Task<SystemResult<MailBoxListDto>> SaveMail(MailBoxDto dto)
        {
            var result = new SystemResult<MailBoxListDto>();
            try
            {
                var entity = (MailBox)dto;
                var oldEntity = await Repository.Get(x => x.Id == entity.Id);
                if (dto.SendNow)
                {
                    //await SendMail(Filter<MailBox>.CreateFilter(x => x.Id == entity.Id));
                    var smtpSetting = await GetSmtpSettings();
                    var sender = new EmailSender(smtpSetting);
                    var mailResult = sender.SendMail(new MailInfo
                    {
                        Email = dto.To ?? "",
                        Message =dto.Content ?? "",
                        Subject = dto.Subject,
                        IsHtml = true,
                        From = dto.From
                    });
                    if (!mailResult.IsSuccess || mailResult.Data == false)
                    {
                        entity.TryCount += 1;
                        entity.Status = Domain.Enums.ESendStatus.Fail;
                    }
                    else
                    {
                        entity.Status = Domain.Enums.ESendStatus.Send;
                    }
                }
                if (oldEntity != null)
                {
                    entity = await Repository.Update(entity);
                }
                else
                {
                    entity = await Repository.Add(entity);
                }
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

        public async Task<SystemResult<Nothing>> SendMail()
        {
            var result = new SystemResult<Nothing>();
            try
            {
                var smtpSetting = await GetSmtpSettings();
                var mails = await Repository.GetAll(x => x.Status == Domain.Enums.ESendStatus.Queed || (x.Status == Domain.Enums.ESendStatus.Fail && x.TryCount < 3));
                var sender = new EmailSender(smtpSetting);
                foreach (var mail in mails ?? new List<MailBox>())
                {
                    var mailResult = sender.SendMail(new MailInfo
                    {
                        Email = mail.To ?? "",
                        Message = mail.Content ?? "",
                        Subject = mail.Subject,
                        IsHtml = true,
                        From = mail.From??AppSettings.instance?.EmailSMTP?.From??""
                    });
                    if (!mailResult.IsSuccess || mailResult.Data == false)
                    {
                        mail.TryCount += 1;
                        mail.Status = Domain.Enums.ESendStatus.Fail;
                    }
                    else
                    {
                        mail.Status = Domain.Enums.ESendStatus.Send;
                    }
                    await Repository.Update(mail);
                }
                LogManager.NotifyUs("Tüm epostalar gönderildi. Task çalıştı");
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

        public async Task<SystemResult<Nothing>> SendMail(Filter<MailBox> filter)
        {
            var result = new SystemResult<Nothing>();
            try
            {
                var smtpSetting = await GetSmtpSettings();
                var mails = await Repository.GetAll(filter.ConvertDbQuery());
                var sender = new EmailSender(smtpSetting);
                foreach (var mail in mails ?? new List<MailBox>())
                {
                    var mailResult =  sender.SendMail(new MailInfo
                    {
                        Email = mail.To ?? "",
                        Message = mail.Content ?? "",
                        Subject = mail.Subject,
                        IsHtml = true,
                        From = mail.From
                    });
                    if (!mailResult.IsSuccess || mailResult.Data == false)
                    {
                        mail.TryCount += 1;
                        mail.Status = Domain.Enums.ESendStatus.Fail;
                    }
                    else
                    {
                        mail.Status = Domain.Enums.ESendStatus.Send;
                    }
                    await Repository.Update(mail);
                }
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

        private async Task<SmtpSettings> GetSmtpSettings()
        {
            return new SmtpSettings
            {
                SenderEmail = (await _settingRepository.Get(x => x.Key == "SmtpSenderMail"))?.Value,
                SenderPassword = (await _settingRepository.Get(x => x.Key == "SmtpSenderPassword"))?.Value,
                SmtpMail = (await _settingRepository.Get(x => x.Key == "SmtpSmtpMail"))?.Value,
                SmtpPort = Convert.ToInt32((await _settingRepository.Get(x => x.Key == "SmtpSmtpPort"))?.Value),
                SmtpServer = (await _settingRepository.Get(x => x.Key == "SmtpSmtpServer"))?.Value,
            };
        }
    }
}
