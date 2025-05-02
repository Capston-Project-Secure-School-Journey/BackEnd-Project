using Api.Common.Enums;
using Api.Domain.Models;
using Api.Extensions;

namespace Api.Domain;

public static class DbInitializer
{
    public static void SeedData(Context dbContext)
    {
        dbContext.Users.AddAsync(
            new User
            {
                Id = Guid.NewGuid(),
                UserType = UserType.Admin,
                UserTypeName = "Admin",
                UserName = "admin",
                AccountStatus = AccountStatus.Verified,
                Email = "admin@admin.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                CreatedAt = DateTimeHelper.GetDateTimeUtc7(),
                UpdatedAt = DateTimeHelper.GetDateTimeUtc7(),
                Address = "134-0091",
                PhoneNumber = "123456",
                FirstName = "",
                LastName = ""
            }
        );

        dbContext.SaveChanges();
    }
}