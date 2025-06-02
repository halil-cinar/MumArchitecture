using Microsoft.Extensions.DependencyInjection;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.Business.Logging;
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
using System.Transactions;

namespace MumArchitecture.Business.Services
{
    public class SettingService : ServiceBase<Setting>, ISettingService, IAddScope
    {
       
        public SettingService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<SystemResult<SettingListDto>> Get(int id)
        {
            var result = new SystemResult<SettingListDto>();
            try
            {
                var setting = await Repository.Get(x => x.Id == id);// ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                result.Data = setting;
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

        public async Task<SystemResult<SettingListDto>> Get(Filter<Setting> filter)
        {
            var result = new SystemResult<SettingListDto>();
            try
            {
                var setting = await Repository.Get(filter.ConvertDbQuery());// ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                result.Data = setting;
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

        public async Task<PaggingResult<SettingListDto>> GetAll(Filter<Setting> filter)
        {
            var result = new PaggingResult<SettingListDto>();
            try
            {
                result.CurrentPage = filter.Page;
                result.ItemCount = filter.Count;
                var count = (await Repository.Count(filter.ConvertDbQuery()));
                result.AllCount = count;
                result.AllPageCount = (int)Math.Ceiling(count / filter.Count * 1.0);
                var settings = await Repository.GetAll(filter.ConvertDbQuery());
                result.Data = settings.Select(x => (SettingListDto)x).ToList();
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

        public async Task<SystemResult<SettingListDto>> Save(SettingDto dto)
        {
            var result = new SystemResult<SettingListDto>();
            try
            {
                var entity = (Setting)dto;
                var oldEntity = await Repository.Get(x => x.Id == entity.Id || x.Key==dto.Key);
                entity.Key = oldEntity?.Key??entity.Key;
                entity.Category = oldEntity?.Category ?? entity.Category ?? "";
                entity.SettingType = oldEntity?.SettingType ?? entity.SettingType;
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
