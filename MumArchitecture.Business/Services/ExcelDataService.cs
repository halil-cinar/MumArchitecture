using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
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
using System.ComponentModel.DataAnnotations;
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

        public async Task<SystemResult<ExcelDataListDto>> DownloadExcel<TEntity, TListDto>(Filter<TEntity> filter, Func<TEntity, TListDto> mapper, params Expression<Func<TEntity, string>>[]? columns)
            where TEntity : Entity, new()
        {
            var result = new SystemResult<ExcelDataListDto>();
            try
            {
                var repository = ServiceProvider.GetRequiredService<IRepository<TEntity>>();
                //filter.All();
                var count = await repository.Count(filter.ConvertDbQuery());
                //if (count > AppSettings.instance!.ExcelMaxInRequestCount)
                //{

                //}
                //else
                //{
                var data = await repository.GetAll(filter.ConvertDbQuery());
                var listdtos = data.Select(x => mapper(x).ToDictionary()).ToList();
                //var i = 0;

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


        public Task<SystemResult<ExcelDataListDto>> DownloadSampleExcel<TEntity, TDto>(Filter<TEntity> filter, Func<TEntity, TDto> mapper, params Expression<Func<TEntity, string>>[]? columns)
            where TEntity : Entity, new()
            where TDto : Dto, new()
        {
            filter.GetPage(0, 10);
            return DownloadExcel(filter, mapper, columns);
        }
        public async Task<SystemResult<ExcelDataListDto>> UploadExcel<TEntity, TDto, TListDto, TService>(Func<TDto, TEntity> mapper, Func<TDto, Task<SystemResult<TListDto>>> saveMethod, IFormFile excelFile)
            where TEntity : Entity, new()
            where TDto : Dto, new()
            where TListDto : ListDto, new()
        {
            var result = new SystemResult<ExcelDataListDto>();
            try
            {
                var data = ToDictionaryList(excelFile);
                var dtos = data.Select(x => x?.ToObject<TDto>()).ToList();
                var results = new Dictionary<int, string>();
                var colors = new Dictionary<int, XLColor>();
                var i = 2;
                foreach (var dto in dtos)
                {
                    if (dto == null) continue;
                    var validateResult = ValidateDto(dto);
                    if (!validateResult.IsSuccess)
                    {
                        results.Add(i, string.Join(" - ", validateResult.Messages.Select(x => x.Message).ToList()));
                        colors.Add(i, XLColor.LightPink);
                        i++;
                        continue;
                    }
                    var saveResult = await saveMethod(dto);
                    results.Add(i,  saveResult.IsSuccess?Lang.Value("Success"): string.Join(" - ", saveResult.Messages.Select(x => x.Message).ToList()));
                    colors.Add(i, saveResult.IsSuccess ? XLColor.LightGreen : XLColor.LightPink);
                    i++;
                }
                var excelbyte = WriteColumn(excelFile, data.FirstOrDefault()?.Keys?.Count + 1 ?? 1, results, colors, XLColor.LightGray, "Data");
                result.Data = new ExcelDataListDto
                {
                    Name = $"Results_{typeof(TEntity).Name}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx",
                    File = excelbyte,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    SendingEmail = false,
                };
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
            }
            return result;
        }


        private List<Dictionary<string, object?>> ToDictionaryList(IFormFile file)
        {
            var result = new List<Dictionary<string, object?>>();

            using var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var headerCells = worksheet.Row(1).CellsUsed().ToList();
            var headers = headerCells.Select(c => c.GetString()).ToList();

            int firstDataRow = 2;
            int lastRow = worksheet.LastRowUsed().RowNumber();

            for (int rowIndex = firstDataRow; rowIndex <= lastRow; rowIndex++)
            {
                var row = worksheet.Row(rowIndex);
                if (row.IsEmpty()) continue;

                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                for (int col = 1; col <= headers.Count; col++)
                {
                    string header = headers[col - 1];
                    var val = row.Cell(col).GetString();
                    dict[header] = val;
                }
                result.Add(dict);
            }

            return result;
        }
        private byte[] WriteColumn(
         byte[] workbookBytes,
         int column,
         IDictionary<int, string> rowTextMap,
         IDictionary<int, XLColor>? rowFillColorMap = null,
         XLColor? defaultFontColor = null,
         string? sheetName = null)
        {
            using var stream = new MemoryStream(workbookBytes, writable: true);
            using var workbook = new XLWorkbook(stream);
            var ws = sheetName is null ? workbook.Worksheets.First() : workbook.Worksheet(sheetName);

            foreach (var kvp in rowTextMap)
            {
                var cell = ws.Cell(kvp.Key, column);
                cell.Value = kvp.Value;

                if (defaultFontColor is not null)
                    cell.Style.Font.FontColor = defaultFontColor;

                if (rowFillColorMap is not null && rowFillColorMap.TryGetValue(kvp.Key, out var fill))
                    cell.Style.Fill.BackgroundColor = fill;
            }

            using var output = new MemoryStream();
            workbook.SaveAs(output);
            return output.ToArray();
        }


        private byte[] WriteColumn(
            IFormFile inputFile,
            int column,
         IDictionary<int, string> rowTextMap,
         IDictionary<int, XLColor>? rowFillColorMap = null,
         XLColor? defaultFontColor = null,
         string? sheetName = null)
        {
            var inStream = new MemoryStream();
            inputFile.CopyTo(inStream);
            inStream.Position = 0;
            return WriteColumn(
                inStream.ToArray(),
                column,
                rowTextMap,
                rowFillColorMap,
                XLColor.Black,
                sheetName);
        }

        private static SystemResult<object> ValidateDto<TDto>(TDto model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            var result = new SystemResult<object>();

            if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
            {
                var errors = validationResults
                    .GroupBy(e => e.MemberNames.FirstOrDefault() ?? string.Empty)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                result.Messages = errors
                    .SelectMany(e => e.Value.Select(x => new StateMessage
                    {
                        Message = x,
                        Priority = EPriority.Error
                    }))
                    .ToList();
            }

            return result;
        }

    }


}
