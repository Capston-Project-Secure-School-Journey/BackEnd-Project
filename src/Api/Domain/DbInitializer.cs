using Api.Common.Enums;
using Api.Domain.Models;
using Api.Extensions;

namespace Api.Domain;

public static class DbInitializer
{
    public static async Task SeedData(Context dbContext)
    {
        await dbContext.Users.AddAsync(
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

        await dbContext.SaveChangesAsync();
    }
}