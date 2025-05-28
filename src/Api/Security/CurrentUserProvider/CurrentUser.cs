using Api.Common.Enums;

namespace Api.Security.CurrentUserProvider;

public record CurrentUser(
    Guid UserId,
    UserType UserType,
    Guid? SchoolId,
    AccountStatus AccountStatus
);