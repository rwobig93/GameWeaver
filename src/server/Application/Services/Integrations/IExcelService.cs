namespace Application.Services.Integrations;

public interface IExcelService
{
    Task<string> ExportBase64Async<TData>(IEnumerable<TData> data, Dictionary<string, Func<TData, object>> dataMapping, string sheetName = "Sheet1");
}