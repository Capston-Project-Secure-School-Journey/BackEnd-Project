using Api.Common.Enums;
using Api.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public static class ControllerExtension
{
    public static Guid GetUserId(this ControllerBase controller)
    {
        if (controller.Request.Headers["Authorization-UserId"].First() == null)
            throw new UnauthorizedAccessException();
        return Guid.Parse(controller.Request.Headers["Authorization-UserId"].First()!);

    }
    
    public static UserType GetUserType(this ControllerBase controller)
    {
        if (controller.Request.Headers["Authorization-UserType"].First() == null)
            throw new UnauthorizedAccessException();
        return (UserType)
            Convert.ToInt16(controller.Request.Headers["Authorization-UserType"].First());

    }

    public static Guid GetSchoolId(this ControllerBase controller, Context context, Guid userId)
    {
        return context.SchoolPersons.FirstOrDefault(person => person.Id == userId)!.SchoolId;
    }
}