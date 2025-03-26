namespace Api.Common.Utilities;

public static class ConstantErrorMessage
{
    public static string INVALID_EMAIL_PASSWORD(string lang)
    {
        return lang == "en" ? "Invalid email or password" : "Sai tên đăng nhập hoặc mật khẩu";
    }

    public static string EMAIL_ALREADY_EXIST(string lang)
    {
        return lang == "en" ? "Email already in use" : "Tên đăng nhập đã tồn tại";
    }

    public static string DISPLAY_NAME_ALREADY_EXIST(string lang)
    {
        return lang == "en" ? "Display name already in use" : "Tên hiển thị đã tồn tại";
    }

    public static string ADMIN_NOT_SET_QUOTATION(string lang)
    {
        return lang == "en"
            ? "The administrator has not set up a quotaion for this user"
            : "Quản trị viên chưa thiết lập hạn ngạch cho người dùng này";
    }

    public static string ENTERED_OLD_PASSWORD(string lang)
    {
        return lang == "en" ? "You  entered an old password" : "Bạn đã nhập mật khẩu cũ";
    }

    public static string INVALID_OLD_PASSWORD(string lang)
    {
        return lang == "en" ? "The old password you have entered is incorrect." : "Mật khẩu cũ chưa chính xác";
    }

    public static string NOT_COMPLETE_PROFILE(string lang)
    {
        return lang == "en"
            ? "Update your profile to perform this action"
            : "Cập nhật thông tin của bạn để thực hiện hành động này";
    }

    public static string ACCOUNT_NOT_PUBLIC(string lang)
    {
        return lang == "en"
            ? "You can not perform this action because your account is not public"
            : "Không thể thực hiện hành động này bởi vì tài khoản này không được phát hành công cộng";
    }
}