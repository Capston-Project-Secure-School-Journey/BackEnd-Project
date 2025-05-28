using Api.Common.Enums;
using Api.Common.Exceptions;
using Api.Common.Utilities;
using Api.Security.CurrentUserProvider;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public static class ControllerExtension
{
    public static Guid GetUserId(this ControllerBase controller)
    {
        var currentUserProvider = controller.HttpContext.RequestServices.GetService<ICurrentUserProvider>();
        var user = currentUserProvider!.GetCurrentUser();
        return user.UserId;
    }

    public static UserType GetUserType(this ControllerBase controller)
    {
        var currentUserProvider = controller.HttpContext.RequestServices.GetService<ICurrentUserProvider>();
        var user = currentUserProvider!.GetCurrentUser();
        return user.UserType;
    }

    public static AccountStatus GetAccountStatus(this ControllerBase controller)
    {
        var currentUserProvider = controller.HttpContext.RequestServices.GetService<ICurrentUserProvider>();
        var user = currentUserProvider!.GetCurrentUser();
        return user.AccountStatus;
    }

    public static Guid GetSchoolId(this ControllerBase controller)
    {
        var currentUserProvider = controller.HttpContext.RequestServices.GetService<ICurrentUserProvider>();
        var user = currentUserProvider!.GetCurrentUser();
        return user.SchoolId ?? throw new UnAuthorizedException(ErrorMessages.AccountExists);
    }
}