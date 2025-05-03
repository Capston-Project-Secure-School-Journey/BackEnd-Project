using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Api.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class PasswordStrengthAttribute : ValidationAttribute
{
    public int MinimumLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = true;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult("Mật khẩu không được để trống.");

        var password = value.ToString();

        if (password!.Length < MinimumLength)
            return new ValidationResult($"Mật khẩu phải có ít nhất {MinimumLength} ký tự.");

        if (RequireUppercase && !Regex.IsMatch(password, @"[A-Z]", RegexOptions.None, TimeSpan.FromMilliseconds(500)))
            return new ValidationResult("Mật khẩu phải chứa ít nhất một chữ hoa.");

        if (RequireLowercase && !Regex.IsMatch(password, @"[a-z]", RegexOptions.None, TimeSpan.FromMilliseconds(500)))
            return new ValidationResult("Mật khẩu phải chứa ít nhất một chữ thường.");

        if (RequireDigit && !Regex.IsMatch(password, @"\d", RegexOptions.None, TimeSpan.FromMilliseconds(500)))
            return new ValidationResult("Mật khẩu phải chứa ít nhất một chữ số.");

        if (RequireSpecialCharacter &&
            !Regex.IsMatch(password, @"[\W_]", RegexOptions.None, TimeSpan.FromMilliseconds(500)))
            return new ValidationResult("Mật khẩu phải chứa ít nhất một ký tự đặc biệt.");

        return ValidationResult.Success;
    }
}