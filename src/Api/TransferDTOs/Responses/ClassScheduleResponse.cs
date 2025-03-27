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
    
    public string TextDisplay => GetDisplayText();

    private string GetDisplayText()
    {
        StringBuilder builder = new StringBuilder();
        if (ScheduleType == ScheduleType.Class)
        {
            if (SessionType == SessionType.FullDay)
                builder.Append($"{ClassName} học cả ngày");
            else
                builder.Append($"{ClassName} học vào buổi {EnumExtension.GetEnumDisplayName(SessionType)}");
        }
        else if (ScheduleType == ScheduleType.Grade)
        {
            if (SessionType == SessionType.FullDay)
                builder.Append($"Khối {EnumExtension.GetEnumDisplayName(Grade!.Value)} học cả ngày\n");
            else
                builder.Append($"Khối {EnumExtension.GetEnumDisplayName(Grade!.Value)} " +
                               $"học vào buổi {EnumExtension.GetEnumDisplayName(SessionType)}\n");

            if (ClassException.Count > 0)
            {
                builder.Append("Ngoại trừ các lớp: ");
                foreach (var i in ClassNameException)
                {
                    builder.Append($"{i}, ");
                }
                builder.Remove(builder.Length - 2 , 2);
            }
        }
        else
        {
            if (SessionType == SessionType.FullDay)
                builder.Append($"Toàn trường học cả ngày\n");
            else
                builder.Append($"Toàn trường " +
                               $"học vào buổi {EnumExtension.GetEnumDisplayName(SessionType)}\n");

            if (ClassException.Count > 0)
            {
                builder.Append("Ngoại trừ các lớp: ");
                foreach (var i in ClassNameException)
                {
                    builder.Append($"{i}, ");
                }
                builder.Remove(builder.Length - 2 , 2);
            }
        }
        
        return builder.ToString();  
    }
}

public class ClassSchedulePaginationResponse
{
    public Dictionary<DateOnly, IEnumerable<ClassScheduleResponseView>> ClassSchedules { get; set; } = new Dictionary<DateOnly, IEnumerable<ClassScheduleResponseView>>();
}