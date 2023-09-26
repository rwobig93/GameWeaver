using System.Collections;
using System.Drawing;
using Application.Services.Integrations;
using Application.Services.Lifecycle;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Infrastructure.Services.Integrations;

public class ExcelService : IExcelService
{
    private readonly IRunningServerState _serverState;

    public ExcelService(IRunningServerState serverState)
    {
        _serverState = serverState;
    }

    private static void ConfigureWorkSheetHeaders(List<string> workSheetHeaders, ExcelWorksheet workSheet)
    {
        var columnIndex = 1;
        
        foreach (var header in workSheetHeaders)
        {
            var currentCell = workSheet.Cells[1, columnIndex];

            var cellFill = currentCell.Style.Fill;
            cellFill.PatternType = ExcelFillStyle.Solid;
            cellFill.BackgroundColor.SetColor(Color.DarkGray);

            var cellBorder = currentCell.Style.Border;
            cellBorder.Bottom.Style = ExcelBorderStyle.Thin;
            cellBorder.Top.Style = ExcelBorderStyle.Thin;
            cellBorder.Left.Style = ExcelBorderStyle.Thin;
            cellBorder.Right.Style = ExcelBorderStyle.Thin;

            currentCell.Value = header;

            columnIndex++;
        }
    }

    private static int PopulateDataIntoCells<TData>(IEnumerable<TData> data, IReadOnlyDictionary<string, Func<TData, object>> dataMapping,
        IReadOnlyCollection<string> workSheetHeaders,
        ExcelWorksheet workSheet)
    {
        var dataList = data.ToList();
        var rowIndex = 1;

        foreach (var item in dataList)
        {
            var columnIndex = 1;
            rowIndex++;

            var rowData = workSheetHeaders.Select(header => dataMapping[header](item));

            foreach (var row in rowData)
            {
                workSheet.Cells[rowIndex, columnIndex++].Value = row;
            }
        }

        return dataList.Count;
    }

    private static void AutoFitWorksheetColumns(ExcelWorksheet workSheet, int dataCount, ICollection workSheetHeaders)
    {
        using var workSheetCellRange = workSheet.Cells[1, 1, dataCount + 1, workSheetHeaders.Count];
        
        workSheetCellRange.AutoFilter = true;
        workSheetCellRange.AutoFitColumns();
    }

    public async Task<string> ExportBase64Async<TData>(IEnumerable<TData> data, Dictionary<string, Func<TData, object>> dataMapping,
        string sheetName = "Sheet1")
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var excelPackage = new ExcelPackage();
        excelPackage.Workbook.Properties.Author = _serverState.ApplicationName;
        excelPackage.Workbook.Worksheets.Add(sheetName);
        
        var workSheet = excelPackage.Workbook.Worksheets[0];
        workSheet.Name = sheetName;
        workSheet.Cells.Style.Font.Size = 11;
        workSheet.Cells.Style.Font.Name = "Calibri";

        var workSheetHeaders = dataMapping.Keys.Select(x => x).ToList();

        ConfigureWorkSheetHeaders(workSheetHeaders, workSheet);

        var dataCount = PopulateDataIntoCells(data, dataMapping, workSheetHeaders, workSheet);

        AutoFitWorksheetColumns(workSheet, dataCount, workSheetHeaders);

        var byteArray = await excelPackage.GetAsByteArrayAsync();
        return Convert.ToBase64String(byteArray);
    }
}