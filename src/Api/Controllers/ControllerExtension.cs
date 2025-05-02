using Api.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public static class ControllerExtension
{
    public static Guid GetUserId(this ControllerBase controller)
    {
        if (controller.Request.Headers["Authorization-UserId"][0] == null)
            throw new UnauthorizedAccessException();
        return Guid.Parse(controller.Request.Headers["Authorization-UserId"][0]!);
    }

    public static UserType GetUserType(this ControllerBase controller)
    {
        if (controller.Request.Headers["Authorization-UserType"][0] == null)
            throw new UnauthorizedAccessException();
        return (UserType)
            Convert.ToInt16(controller.Request.Headers["Authorization-UserType"][0]);
    }

    public static AccountStatus GetAccountStatus(this ControllerBase controller)
    {
        if (controller.Request.Headers["Authorization-AccountStatus"][0] == null)
            throw new UnauthorizedAccessException();
        return (AccountStatus)
            Convert.ToInt16(controller.Request.Headers["Authorization-AccountStatus"][0]);
    }

    public static Guid GetSchoolId(this ControllerBase controller)
    {
        if (controller.Request.Headers["Authorization-SchoolId"][0] == null)
            throw new UnauthorizedAccessException();
        return Guid.Parse(controller.Request.Headers["Authorization-SchoolId"][0]!);
    }
}