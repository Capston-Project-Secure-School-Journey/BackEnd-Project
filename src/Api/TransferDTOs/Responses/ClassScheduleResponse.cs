using System.Text;
using Api.Common.Enums;
using Api.Extensions;

namespace Api.TransferDTOs.Responses;

public class ClassScheduleResponse
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public DateOnly Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public SessionType SessionType { get; set; }
    public ScheduleType ScheduleType { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Grade? Grade { get; set; }
    public string GradeName { get; set; } = string.Empty;
}

public class ClassScheduleResponseView
{
    public Guid SchoolId { get; set; }
    public DateOnly Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public SessionType SessionType { get; set; }
    public ScheduleType ScheduleType { get; set; }
    public Guid? ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Grade? Grade { get; set; }
    public List<Guid> ClassException { get; set; } = [];
    public List<string> ClassNameException { get; set; } = [];
    public List<Grade> GradeException { get; set; } = [];

    public List<string> TextDisplay => GetDisplayText();

    private List<string> GetDisplayText()
    {
        var result = new List<string>();
        switch (ScheduleType)
        {
            case ScheduleType.Class:
                result.Add($"Lớp {ClassName} học {SessionType.GetEnumDisplayName()}");
                break;
            case ScheduleType.Grade:
            case ScheduleType.School:
                var text = ScheduleType == ScheduleType.Grade
                    ? "Khối " + Grade!.Value.GetEnumDisplayName()
                    : "Toàn trường";

                result.Add(text +
                           $" học {SessionType.GetEnumDisplayName()}");

                if (GradeException.Count > 0)
                {
                    var builder = new StringBuilder();
                    builder.Append("Ngoại trừ các khối: ");
                    foreach (var i in GradeException) builder.Append($"{i.GetEnumDisplayName()}, ");
                    builder.Remove(builder.Length - 2, 2);
                    result.Add(builder.ToString());
                }
                
                if (ClassException.Count > 0)
                {
                    var builder = new StringBuilder();
                    builder.Append("Ngoại trừ các lớp: ");
                    foreach (var i in ClassNameException) builder.Append($"{i}, ");

                    builder.Remove(builder.Length - 2, 2);
                    result.Add(builder.ToString());
                }

                break;
        }

        return result;
    }
}

public class ClassSchedulePaginationResponse
{
    public Dictionary<DateOnly, IList<ClassScheduleResponseView>> ClassSchedules { get; set; } = new();
}