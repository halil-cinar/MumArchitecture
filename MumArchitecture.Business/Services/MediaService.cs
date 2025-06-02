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
using System.Security.Policy;
using System.Transactions;
using System.Web;

namespace MumArchitecture.Business.Services
{
    public class MediaService : ServiceBase<Media>, IMediaService, IAddScope
    {
        private readonly IRepository<User> _userRepository;
        private readonly string baseDirectoryUrl;
        public MediaService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _userRepository = serviceProvider.GetRequiredService<IRepository<User>>();
            baseDirectoryUrl = AppSettings.instance!.MediaBaseDirectory!;
            if (Directory.Exists(baseDirectoryUrl))
            {
                Directory.CreateDirectory(baseDirectoryUrl);
            }
        }

        public async Task<SystemResult<MediaListDto>> Delete(string url)
        {
            var result = new SystemResult<MediaListDto>();
            try
            {
                var media = await Repository.Get(x => x.Url == url) ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                //var location=Path.Combine(baseDirectoryUrl, media.Location!);
                File.Delete(media.Location!);
                await Repository.Delete(x => x.Url == url);
                result.Data = new MediaListDto
                {
                    Name = media.Name,
                    Id = media.Id,
                    Url = media.Url,
                };
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

        public async Task<SystemResult<MediaListDto>> Get(int id)
        {
            var result = new SystemResult<MediaListDto>();
            try
            {
                var media = await Repository.Get(x => x.Id == id) ?? throw new UserException(Lang.Value(Messages.RecordNotFound));

                if (File.Exists(media.Location!))
                {
                    var data = File.ReadAllBytes(media.Location!);
                    result.Data = new MediaListDto
                    {
                        Name = media.Name,
                        Id = media.Id,
                        Url = media.Url,
                        File = data,
                        ContentType = media.ContentType,
                    };
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

        public async Task<SystemResult<MediaListDto>> Get(string url)
        {
            var result = new SystemResult<MediaListDto>();
            try
            {
                var media = await Repository.Get(x => x.Url == url) ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                if (File.Exists(media.Location!))
                {
                    var data = File.ReadAllBytes(media.Location!);
                    result.Data = new MediaListDto
                    {
                        Name = media.Name,
                        Id = media.Id,
                        Url = media.Url,
                        File = data,
                        ContentType = media.ContentType,
                    };
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

        public async Task<SystemResult<MediaListDto>> Save(MediaDto dto)
        {
            var result = new SystemResult<MediaListDto>();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (dto.File == null || dto.File.Length == 0)
                    {
                        throw new UserException(Lang.Value(Messages.ParamaterNotValid));
                    }
                    var url = Path.Combine(baseDirectoryUrl, Guid.NewGuid().ToString());
                    var filename = dto.File?.FileName ?? "";
                    filename = Path.GetFileName(filename);
                    if (dto.File.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                    {
                        dto.Message ??= "";
                        dto.Message += " video";
                    }

                    dto.Message ??= "";
                    dto.Message += Path.GetExtension(filename);

                   

                    var entity = new Media
                    {
                        ContentType = dto.File.ContentType,
                        Id = dto.Id,
                        Url = "/Media?key=" + HttpUtility.UrlEncode(Guid.NewGuid().ToString() + (dto.Message?.Replace(" ", "-") ?? "")),
                        Location = url,
                        Name = filename,
                        SavedUserId = dto.SavedUserId>0?dto.SavedUserId:AuthUserId,
                    };
                    if (dto.Id > 0)
                    {
                        var oldEntity = await Repository.Get(x => x.Id == dto.Id) ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                        entity.Url = oldEntity.Url;
                        url = oldEntity.Location;
                        await Repository.Update(entity);
                    }
                    else
                    {
                        await Repository.Add(entity);
                    }
                    using (var stream = new FileStream(url, FileMode.Create))
                    {
                        await dto.File!.CopyToAsync(stream);
                    }
                    scope.Complete();
                    result.Data = new MediaListDto
                    {
                        Id = entity.Id,
                        Url = entity.Url,
                        ContentType = entity.ContentType,
                        Name = entity.Name,
                    };
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

        async Task<bool> IMediaService.Exist(int id)
        {
            var exist = await Repository.Count(x => x.Id == id) > 0;
            return exist;
        }
    }
}
