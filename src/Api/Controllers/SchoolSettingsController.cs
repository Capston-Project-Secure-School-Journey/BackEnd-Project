using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("school-settings")]
public class SchoolSettingsController(Context context) : ControllerBase
{
    [HttpPut("{schoolId}/start-date")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult> SetSchoolStartDate([FromRoute] Guid schoolId,
        [FromBody] DateOnly date)
    {
        if (this.GetSchoolId() != schoolId)
            throw new ForbiddenException(ErrorMessages.AccessDenied);

        var startDate = await context.SystemVariables
            .FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.Name == "START_DATE");

        if (startDate == null)
            await context.SystemVariables.AddAsync(new SystemVariable()
                { Name = "START_DATE", SchoolId = schoolId, Value = date.ToString() });
        else
            startDate.Value = date.ToString();

        await context.SaveChangesAsync();
        return Ok();
    }
}