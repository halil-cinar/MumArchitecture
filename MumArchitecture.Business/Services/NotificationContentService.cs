using Microsoft.VisualStudio.Services.ExternalEvent;
using MumArchitecture.Business.Logging;
using Org.BouncyCastle.Math.EC;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.Business.Notification;
using MumArchitecture.Business.Result;
using MumArchitecture.DataAccess.Abstract;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.ListDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain;

namespace MumArchitecture.Business.Services
{
    public class NotificationContentService : ServiceBase<NotificationContent>, INotificationContentService, IAddScope
    {
        public NotificationContentService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<SystemResult<NotificationContentListDto>> Delete(int id)
        {
            var result = new SystemResult<NotificationContentListDto>();
            try
            {
                if (id <= 0)
                {
                    throw new UserException(Lang.Value(Messages.ParamaterNotValid));
                }
                var content = await Repository.Get(x => x.Id == id) ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                await Repository.Delete(x => x.Id == id);
                result.Data = content;
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

        public async Task<SystemResult<NotificationContentListDto>> Get(int id)
        {
            var result = new SystemResult<NotificationContentListDto>();
            try
            {
                if (id <= 0)
                {
                    throw new UserException(Lang.Value(Messages.ParamaterNotValid));
                }
                var content = await Repository.Get(x => x.Id == id) ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                result.Data = content;
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

        public async Task<SystemResult<NotificationContentListDto>> Get(Filter<NotificationContent> filter)
        {
            var result = new SystemResult<NotificationContentListDto>();
            try
            {
                var content = await Repository.Get(filter.ConvertDbQuery());
                result.Data = content;
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

        public async Task<SystemResult<List<NotificationContentListDto>>> Get(Filter<NotificationContent> filter, Dictionary<string, string> variables)

        {
            var result = new SystemResult<List<NotificationContentListDto>>();
            try
            {
                var notificationContents = await Repository.GetAll(filter.ConvertDbQuery());
                var dtoList = notificationContents.Select(x =>
                {
                    var dto = (NotificationContentListDto)x;
                    if (dto.Variables != null && dto.Variables.Any())
                    {
                        foreach (var variable in dto.Variables)
                        {
                            if (variables.ContainsKey(variable))
                            {
                                dto.Subject = dto.Subject?.Replace($"[{variable}]", variables[variable]);
                                dto.Content = dto.Content?.Replace($"[{variable}]", variables[variable]);
                            }
                        }
                    }
                    return dto;
                }).ToList();

                result.Data = dtoList;
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

        public async Task<PaggingResult<NotificationContentListDto>> GetAll(Filter<NotificationContent> filter)
        {
            var result = new PaggingResult<NotificationContentListDto>();
            try
            {
                result.CurrentPage = filter.Page;
                result.ItemCount = filter.Count;

                var count = await Repository.Count(filter.ConvertDbQuery());
                result.AllCount = count;
                result.AllPageCount = (int)Math.Ceiling(count / (filter.Count * 1.0));

                var notificationContents = await Repository.GetAll(filter.ConvertDbQuery());
                var contentDtos = notificationContents.Select(x => (NotificationContentListDto)x).ToList();

                result.Data = contentDtos;
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

        public async Task<SystemResult<NotificationContentListDto>> Save(NotificationContentDto dto)
        {
            var result = new SystemResult<NotificationContentListDto>();
            try
            {
                var entity = (NotificationContent)dto;
                var oldEntity = await Repository.Get(x => x.Id == entity.Id);
                var exist = await Repository.Count(x => x.ContentType == entity.ContentType && x.ContentLang == entity.ContentLang && x.ContentTarget == entity.ContentTarget) > 0;
                if (exist && oldEntity == null)
                {
                    throw new UserException(Lang.Value(Messages.RecordAlreadyExist));
                }
                if (oldEntity != null)
                {
                    await Repository.Update(entity);
                }
                else
                {
                    await Repository.Add(entity);
                }
                result.Data = entity;
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

        
    }
}







//TODO: notification content yazılacak 











