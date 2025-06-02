using MumArchitecture.Business.Result;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;

namespace MumArchitecture.Business.Abstract
{
    public interface IMediaService
    {
        public Task<SystemResult<MediaListDto>> Save(MediaDto dto);
        public Task<SystemResult<MediaListDto>> Get(int id);
        public Task<SystemResult<MediaListDto>> Get(string url);
        public Task<SystemResult<MediaListDto>> Delete(string url);
        internal Task<bool> Exist(int id);
        //internal Task<SystemResult<MediaListDto>> Delete(Media entity);
             
    }
}