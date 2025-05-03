using Api.Common.Utilities;
using Api.Domain;
using Api.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Api.Jobs;

public class CreatePickupScheduleJob(IServiceProvider serviceProvider) : IJob
{
    public async Task ExecuteAsync(params object[] args)
    {
        var schoolId = Guid.Parse(args[0].ToString()!);

        if (schoolId == Guid.Empty)
            throw new InvalidDataException(ErrorMessages.InvalidSchoolId);

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Context>();

        var startDate = await db.SystemVariables
            .FirstOrDefaultAsync(e => e.Name == "START_DATE" && e.SchoolId == schoolId);

        if (startDate is null)
            return;
        if (Convert.ToDateTime(startDate.Value) > DateTimeHelper.GetDateTimeUtc7())
            return;

        var studentMissingAddress = await db.Students
            .AsNoTracking()
            .AsQueryable()
            .Include(s => s.Class)
            .Where(s => s.SchoolId == schoolId)
            .Where(s => s.NeedsPickup)
            .Where(s => string.IsNullOrEmpty(s.PickUpLocation) || s.PickUpLat == 0 || s.PickUpLng == 0)
            .ToListAsync();

        if (studentMissingAddress.Count > 0)
            return;
    }
}