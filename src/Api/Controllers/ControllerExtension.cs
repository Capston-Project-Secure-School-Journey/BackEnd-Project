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

    public static AccountStatus GetAccountStatus(this ControllerBase controller)
    {
        if (controller.Request.Headers["Authorization-AccountStatus"].First() == null)
            throw new UnauthorizedAccessException();
        return (AccountStatus)
            Convert.ToInt16(controller.Request.Headers["Authorization-AccountStatus"].First());
    }

    public static Guid GetSchoolId(this ControllerBase controller)
    {
        if (controller.Request.Headers["Authorization-SchoolId"].First() == null)
            throw new UnauthorizedAccessException();
        return Guid.Parse(controller.Request.Headers["Authorization-SchoolId"].First()!);
    }
}