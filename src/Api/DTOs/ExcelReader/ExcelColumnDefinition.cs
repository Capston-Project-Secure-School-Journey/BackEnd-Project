using System.Reflection;

namespace Api.DTOs.ExcelReader;

public class ExcelColumnDefinition<T> where T : class
{
    public ExcelColumnDefinition(string columnName, string columnNameDisplay, string excelColumnLetter,
        string textSample)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentNullException(nameof(columnName));
        if (string.IsNullOrWhiteSpace(columnNameDisplay))
            throw new ArgumentNullException(nameof(columnNameDisplay));
        if (string.IsNullOrWhiteSpace(excelColumnLetter))
            throw new ArgumentNullException(nameof(excelColumnLetter));
        if (string.IsNullOrWhiteSpace(textSample))
            throw new ArgumentNullException(nameof(textSample));

        var property = typeof(T).GetProperties()
            .FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

        if (property == null)
            throw new ArgumentException($"Property '{columnName}' does not exist on type '{typeof(T).Name}'");

        if (!property.CanWrite)
            throw new InvalidOperationException($"Property '{property.Name}' does not have a setter");


        ColumnName = columnName;
        ColumnNameDisplay = columnNameDisplay;
        ExcelColumnLetter = excelColumnLetter.ToUpper();
        TextSample = textSample;
        Property = property;
    }

    public string ColumnName { get; set; }
    public string ColumnNameDisplay { get; set; }
    public string ExcelColumnLetter { get; set; }
    public string TextSample { get; set; }
    public PropertyInfo Property { get; }
    public int ColumnIndex => ConvertExcelColumnToIndex(ExcelColumnLetter);

    private static int ConvertExcelColumnToIndex(string columnLetter)
    {
        int sum = 0;
        foreach (char c in columnLetter.ToUpper())
        {
            sum *= 26;
            sum += (c - 'A' + 1);
        }

        return sum;
    }
}