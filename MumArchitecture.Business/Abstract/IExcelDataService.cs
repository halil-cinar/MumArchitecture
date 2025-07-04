using MumArchitecture.Business.Result;
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.ListDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Abstract
{
    public interface IExcelDataService
    {
        public Task<SystemResult<ExcelDataListDto>> DownloadExcel<TEntity, TListDto>(Filter<TEntity> filter, Func<TEntity, TListDto> mapper, bool sample = false, params Expression<Func<TEntity, string>>[]? columns)
            where TEntity : Entity, new()
            where TListDto : ListDto, new();

        public Task<SystemResult<ExcelDataListDto>> UploadExcel<TEntity, TDto, TListDto, TService>(Func<TDto, TEntity> mapper, Func<TDto, Task<SystemResult<TListDto>>> saveMethod)
            where TEntity : Entity, new()
            where TDto : Dto, new()
            where TListDto : ListDto, new();
    }
}
