using System.Globalization;
using System.Reflection;
using Api.Common.Exceptions;
using Api.Domain.Models;
using Api.DTOs.ExcelReader;
using ClosedXML.Excel;
using FluentValidation;

namespace Api.Extensions;

public class ExcelReader<T>(IServiceProvider serviceProvider) where T : class
{
    public MemoryStream GetTemplateFile(List<ExcelColumnDefinition<T>> excelColumns)
    {
        using var workbook = new XLWorkbook();
        var descriptor = serviceProvider.GetRequiredService<IValidator<T>>().CreateDescriptor();
        workbook.Worksheets.Add("Sample");
        var worksheet = workbook.Worksheet("Sample");
        var maxColumnIndex = excelColumns.Max(x => x.ColumnIndex);
        foreach (var excelColumn in excelColumns)
        {
            worksheet.Columns(excelColumn.ExcelColumnLetter).AdjustToContents();
            worksheet.Cell(1, excelColumn.ColumnIndex).Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Cell(1, excelColumn.ColumnIndex).SetValue(excelColumn.ColumnNameDisplay);
        }

        for (int i = 2; i <= 11; i++)
        {
            foreach (var excelColumn in excelColumns)
            {
                worksheet.Cell(i, excelColumn.ColumnIndex).SetValue(excelColumn.TextSample);
            }
        }

        var cell = worksheet.Cell(1, maxColumnIndex + 1)
            .SetValue("Yêu cầu đầu vào cho mỗi cột");
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontColor = XLColor.Red;
        cell.Style.Font.FontSize = 14;
        cell.Style.Font.Italic = true;

        var index = 2;
        foreach (var excelColumn in excelColumns)
        {
            cell = worksheet.Cell(index, maxColumnIndex + 1)
                .SetValue($"Đối với cột: {excelColumn.ColumnNameDisplay}");
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.Red;
            cell.Style.Font.FontSize = 12;

            index++;
            foreach (var rule in descriptor.GetRulesForMember(excelColumn.ColumnName))
            {
                foreach (var component in rule.Components)
                {
                    var field = component.GetType()
                        .GetRuntimeFields()
                        .FirstOrDefault(f => f.Name == "_errorMessage");

                    if (field == null) continue;
                    cell = worksheet.Cell(index, maxColumnIndex + 2)
                        .SetValue($"{field.GetValue(component)}");
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.Red;
                    cell.Style.Font.FontSize = 10;
                    index++;
                }
            }
        }

        worksheet.Range(1, maxColumnIndex + 1, index, maxColumnIndex + 2)
            .Style
            .Fill.SetBackgroundColor(XLColor.LightGray)
            .Border.SetInsideBorder(XLBorderStyleValues.Thin)
            .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        worksheet.Range(1, 1, 100, maxColumnIndex)
            .Style
            .Border.SetInsideBorder(XLBorderStyleValues.Thin)
            .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        worksheet.Columns(1, maxColumnIndex + 4).AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }

    public List<T> ReadExcel(Stream fileStream, List<ExcelColumnDefinition<T>> excelColumns)
    {
        var result = new List<T>();
        var validator = serviceProvider.GetRequiredService<IValidator<T>>();
        var maxColumnIndex = excelColumns.Max(x => x.ColumnIndex);
        var index = 0;
        try
        {
            using var workbook = new XLWorkbook(fileStream);

            if (workbook.Worksheets.Count == 0)
                throw new ArgumentException("File excel không đúng định dạng.");
            var worksheet = workbook.Worksheet(1);
            var dataRange = worksheet
                .Range(2, 1, 100000, maxColumnIndex)
                .RangeUsed();

            if (!dataRange.Rows().Any())
                throw new ArgumentException("File excel không tồn tại data.");

            foreach (var excelColumn in excelColumns)
            {
                var columnIndex = excelColumn.ColumnIndex;
                if (worksheet.Cell(1, columnIndex).GetText() != excelColumn.ColumnNameDisplay)
                    throw new ArgumentException("File của bạn không đúng định dạng." +
                                                $"Thiếu cột {excelColumn.ColumnNameDisplay}");
            }

            foreach (var row in dataRange.Rows())
            {
                index = row.RowNumber();
                var obj = Activator.CreateInstance<T>();
                foreach (var excelColumn in excelColumns)
                {
                    var value = row.Cell(excelColumn.ColumnIndex).GetText();
                    SetPropertyValue(obj, excelColumn.Property, excelColumn.ColumnNameDisplay, value);
                }

                var error = validator.Validate(obj);

                if (!error.IsValid)
                {
                    var columnError = error.Errors
                        .FirstOrDefault(e =>
                            excelColumns.Select(col => col.ColumnName).Contains(e.PropertyName));
                    if (columnError != null)
                        throw new ValidationException(columnError.ErrorMessage);
                }

                result.Add(obj);
            }

            return result;
        }
        catch (ValidationException e)
        {
            throw new ValidationException($"Lỗi tại dòng: {index}. Chi tiết lỗi: {e.Message}");
        }
        catch (Exception e)
        {
            throw new BadRequestException(e.Message);
        }
    }

    private static void SetPropertyValue(T obj, PropertyInfo prop, string displayName, object value)
    {
        try
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (prop == null)
                throw new ArgumentException($"Property not found on type {typeof(T).Name}");

            var convertedValue = ConvertToType(value, prop.PropertyType);

            prop.SetValue(obj, convertedValue);
        }
        catch (Exception)
        {
            throw new ValidationException($"Giá trị cột {displayName} không đúng, Giá trị = {value}.");
        }
    }

    private static object ConvertToType(object value, Type targetType)
    {
        var culture = new CultureInfo("vi-VN");
        if (targetType.IsInstanceOfType(value))
            return value;

        if (targetType.IsEnum)
        {
            if (value is string s)
                return Enum.Parse(targetType, s, ignoreCase: true);
            return Enum.ToObject(targetType, value);
        }

        if (targetType == typeof(Guid))
        {
            return Guid.Parse(value.ToString()!);
        }

        if (targetType == typeof(DateTime))
        {
            return DateTime.Parse(value.ToString()!, culture);
        }

        if (targetType == typeof(DateOnly))
        {
            return DateOnly.Parse(value.ToString()!, culture);
        }

        if (targetType == typeof(DateTimeOffset))
        {
            return DateTimeOffset.Parse(value.ToString()!, culture);
        }

        if (targetType == typeof(List<ManagedTeacher>))
        {
            if (string.IsNullOrWhiteSpace(value.ToString()))
                return new List<object>();

            var result = value.ToString()!.Split(",")
                .Select(s => new ManagedTeacher()
                {
                    ManagedTeacherId = Guid.Parse(s),
                })
                .ToList();
            return result;
        }

        return Convert.ChangeType(value, targetType);
    }
}