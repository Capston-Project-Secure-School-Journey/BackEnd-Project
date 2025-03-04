namespace Api.Domain.ModelSettings
{
    public class TokenSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpirationHours { get; set; }
        public string SigninCredentials { get; set; } = string.Empty;
    }
}