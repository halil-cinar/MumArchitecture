using Microsoft.Extensions.DependencyInjection;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.Business.Logging;
using MumArchitecture.Business.Result;
using MumArchitecture.DataAccess.Abstract;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.ListDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Services
{
    public class ExcelDataService : ServiceBase<User>, IExcelDataService, IAddScope
    {
        public ExcelDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<SystemResult<ExcelDataListDto>> DownloadExcel<TEntity, TListDto>(Filter<TEntity> filter, Func<TEntity, TListDto> mapper, bool sample = false, params Expression<Func<TEntity, string>>[]? columns)
            where TEntity : Entity, new()
            where TListDto : ListDto, new()
        {
            var result = new SystemResult<ExcelDataListDto>();
            try
            {
                var repository = ServiceProvider.GetRequiredService<IRepository<TEntity>>();
                filter.All();
                var count = await repository.Count(filter.ConvertDbQuery());
                //if (count > AppSettings.instance!.ExcelMaxInRequestCount)
                //{

                //}
                //else
                //{
                var data = await repository.GetAll(filter.ConvertDbQuery());
                var listdtos = data.Select(x => mapper(x).ToDictionary()).ToList();
                //var i = 0;
                if (sample)
                {
                    listdtos = listdtos.Take(10).ToList();
                }
                foreach (var dto in listdtos)
                {
                    foreach (var d in dto)
                    {
                        if ((d.Value?.GetType()?.IsArray == true || d.Value?.GetType()?.IsClass == true) && d.Value.GetType() != typeof(string))
                        {
                            dto[d.Key] = JsonSerializer.Serialize(d.Value);
                        }
                    }
                }
                using var wb = new ClosedXML.Excel.XLWorkbook();
                var ws = wb.AddWorksheet("Data");

                var selectedColumns = (listdtos.FirstOrDefault()?.Keys.ToList() ?? new List<string>());

                for (int c = 0; c < selectedColumns.Count; c++)
                    ws.Cell(1, c + 1).Value = selectedColumns[c];

                for (int r = 0; r < listdtos.Count; r++)
                    for (int c = 0; c < selectedColumns.Count; c++)
                        ws.Cell(r + 2, c + 1).Value =
                            listdtos[r].TryGetValue(selectedColumns[c], out var v) ? v?.ToString() ?? "" : "";

                using var ms = new MemoryStream();
                wb.SaveAs(ms);

                result.Data = new ExcelDataListDto
                {
                    Name = $"{typeof(TEntity).Name}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx",
                    File = ms.ToArray(),
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    SendingEmail = false,
                };
                //}
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
            }
            return result;
        }

        public Task<SystemResult<ExcelDataListDto>> UploadExcel<TEntity, TDto, TListDto, TService>(Func<TDto, TEntity> mapper, Func<TDto, Task<SystemResult<TListDto>>> saveMethod)
            where TEntity : Entity, new()
            where TDto : Dto, new()
            where TListDto : ListDto, new()
        {
            throw new NotImplementedException();
        }
    }
}
