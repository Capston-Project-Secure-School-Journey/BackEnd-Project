using Api.Common.Enums;
using Api.Domain.Models;

namespace Api.Transfers.Responses
{
    public class AuthenticateResponse
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public int UserType { get; set; }
        public string Token { get; set; }
        public AuthenticateResponse(User user, string AccessToken)
        {
            Id = user.Id;
            UserName = user.UserName;
            AccountStatus = user.AccountStatus;
            UserType = (int)user.UserType;
            Token = AccessToken;
        }
    }
}