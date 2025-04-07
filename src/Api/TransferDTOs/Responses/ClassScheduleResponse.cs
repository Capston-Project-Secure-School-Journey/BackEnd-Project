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
    public List<Guid> ClassException = [];
    public List<string> ClassNameException = [];
    
    public List<string> TextDisplay => GetDisplayText();

    private List<string> GetDisplayText()
    {
        var result = new List<string>();
        if (ScheduleType == ScheduleType.Class)
        {
            if (SessionType == SessionType.FullDay)
                result.Add($"{ClassName} học cả ngày");
            else
                result.Add($"{ClassName} học vào buổi {SessionType.GetEnumDisplayName()}");
        }
        else if (ScheduleType == ScheduleType.Grade)
        {
            if (SessionType == SessionType.FullDay)
                result.Add($"Khối {Grade!.Value.GetEnumDisplayName()} học cả ngày");
            else
                result.Add($"Khối {Grade!.Value.GetEnumDisplayName()} " +
                           $"học vào buổi {SessionType.GetEnumDisplayName()}");

            if (ClassException.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("Ngoại trừ các lớp: ");
                foreach (var i in ClassNameException)
                {
                    builder.Append($"{i}, ");
                }
                builder.Remove(builder.Length - 2 , 2);
                
                result.Add(builder.ToString());
            }
        }
        else
        {
            if (SessionType == SessionType.FullDay)
                result.Add($"Toàn trường học cả ngày");
            else
                result.Add($"Toàn trường " +
                           $"học vào buổi {SessionType.GetEnumDisplayName()}");

            if (ClassException.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("Ngoại trừ các lớp: ");
                foreach (var i in ClassNameException)
                {
                    builder.Append($"{i}, ");
                }
                builder.Remove(builder.Length - 2 , 2);
                result.Add(builder.ToString());
            }
        }
        return result;
    }
}

public class ClassSchedulePaginationResponse
{
    public Dictionary<DateOnly, IEnumerable<ClassScheduleResponseView>> ClassSchedules { get; set; } = new Dictionary<DateOnly, IEnumerable<ClassScheduleResponseView>>();
}