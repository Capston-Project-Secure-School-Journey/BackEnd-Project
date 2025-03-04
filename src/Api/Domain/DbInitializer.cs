using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Domain.Models;

namespace Api.Domain
{
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
                    Email = Constants.AdminEmail, 
                    Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                    CreatedAt = new DateTime(2022, 01, 01),
                    UpdatedAt = new DateTime(2022, 01, 01),
                    Address = "134-0091",
                    PhoneNumber = "123456",
                    FirstName = "",
                    LastName = ""
                }
            );
            
            dbContext.SaveChanges();
        }
    }
}