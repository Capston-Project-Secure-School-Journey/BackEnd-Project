namespace Api.Common.Utilities;

public static class ErrorMessages
{
    public const string AccessDenied = "Bạn không có quyền truy cập tài nguyên";
    public const string InvalidCredentials = "Sai tên đăng nhập hoặc mật khẩu";
    public const string NoStudentAdded = "Bạn chưa thêm bất kì học sinh nào";
    public const string NotLoggedIn = "Bạn Chưa đăng nhập. Hãy đăng nhập để tiếp tục sử dụng";
    public const string RequireAtLeast5VehiclePhotos = "Bạn cần phải chụp ít nhất 5 ảnh về phương tiện";
    public const string RequireBothSidesUploaded = "Bạn cần tải cả mặt trước và mặt sau";

    public const string InvalidScheduleAddTime =
        "Bạn không thể thêm lịch. Lịch các tuần phải được thêm vào trước ngày chủ nhật của tuần trước đó";

    public const string CannotAddPastSchedule = "Bạn không thể thêm lịch trong quá khứ";
    public const string MissingVehicleType = "Bạn vẫn chưa thực hiện nhập thông tin về loại xe";
    public const string MissingLicenseNumber = "Bạn vẫn chưa thực hiện nhập thông tin về số bằng lái";
    public const string MissingSeatNumber = "Bạn vẫn chưa thực hiện nhập thông tin về số chỗ ngồi của xe";
    public const string MissingLicenseImages = "Bạn vẫn chưa thực hiện nhập thông tin về ảnh 2 mặt của bằng lái";
    public const string AlreadyApplied = "Bạn đã ứng tuyển hồ sơ cho trường";
    public const string OnlyFirstParentCanEditAddress = "Chỉ có phụ huynh thêm con đầu tiên mới được chỉnh sửa địa chỉ";
    public const string EmailOrPhoneRequired = "Email và số điện thoại đều trống. Vui lòng điền ít nhất 1";
    public const string EmailExists = "Email đã được đăng kí";
    public const string TeacherNotFound = "Giáo viên không tồn tại";
    public const string StudentAlreadyAdded = "Học sinh đã được thêm trước đây";
    public const string InvalidArgumentType = "Invalid argument type";
    public const string CannotAcceptApplication = "Không thể chấp nhận hồ sơ";
    public const string CannotUpdateApplication = "Không thể cập nhập hồ sơ";
    public const string CannotCancelApplication = "Không thể hủy hồ sơ";
    public const string CannotSubmitApplication = "Không thể nộp hồ sơ";
    public const string CannotRejectApplication = "Không thể thực hiện từ chối hồ sơ";
    public const string CannotDeleteData = "Không thể xóa dữ liệu";
    public const string CannotDeleteApplication = "Không thể xóa hồ sơ";
    public const string CannotRegisterAccount = "Không thể đăng kí tài khoản";
    public const string ApplicationNotFound = "Không tìm thấy hồ sơ";
    public const string ScheduleNotFound = "Không tìm thấy lịch học";
    public const string UserNotFound = "Không tìm thấy người dùng";
    public const string StudentNotExist = "Không tồn tại học sinh";
    public const string AccountNotExist = "Không tồn tại tài khoản";
    public const string GradeIsEmpty = "Khối bị trống";
    public const string InvalidFileType = "Loại file không được chấp nhận";
    public const string InvalidSchoolType = "Loại trường không hợp lệ";
    public const string SystemError = "Lỗi hệ thống";
    public const string ClassIsEmpty = "Lớp học bị trống";
    public const string ClassNotFound = "Lớp học không tồn tại";
    public const string InvalidPreschoolClass = "Lớp không hợp lệ cho trường Mầm non";
    public const string InvalidPrimaryClass = "Lớp không hợp lệ cho trường Tiểu học";
    public const string InvalidSecondaryClass = "Lớp không hợp lệ cho trường Trung học cơ sở";
    public const string InvalidHighSchoolClass = "Lớp không hợp lệ cho trường Trung học phổ thông";
    public const string ClassNotExist = "Lớp không tồn tại";
    public const string InvalidSchoolId = "Please provide a valid school ID";
    public const string SchoolNotFound = "School is not found";
    public const string PhoneExists = "Số điện thoại đã được đăng kí";
    public const string NotificationNotFound = "Thông báo không tồn tại";

    public const string StudentInfoMismatch =
        "Thông tin học sinh không trùng khớp. Lưu ý nếu sai quá 5 lần bạn sẽ bị cấm trong 24h";

    public const string InvalidToken = "Token is not valid";
    public const string AccountNotVerified = "Tài khoản của bạn chưa được xác thực";
    public const string AccountLocked = "Tài khoản của bạn đã bị khóa";
    public const string DuplicateClassName = "Tên lớp bị trùng";
    public const string UsernameExists = "Tên đăng nhập đã tồn tại";
    public const string UploadLicenseFailed = "Tải ảnh bằng lái không thành công";
    public const string UploadVehiclePhotoFailed = "Tải ảnh xe không thành công";
    public const string AccountExists = "Đã tồn tại tài khoản";
    public const string ErrorDuringProcessing = "Đã xẩy ra lỗi trong quá trình xử lí";
    private const string FileCheckErrorByKeyFormat = "Error checking if the file exists with the key {0}";
    public static string FileCheckErrorByKey(string key) => string.Format(FileCheckErrorByKeyFormat, key);
    public const string FileDeleteError = "Error deleting file";
    public const string GeneratePreSignDownloadUrlError = "Error generating pre-signed download URL";
    public const string GeneratePreSignUploadUrlError = "Error generating pre-signed upload URL";
    public const string FileUploadError = "Error uploading file";
    public const string NotificationSendFailure = "Failed to send notification";
    private const string FileTooLargeLimitFormat = "File quá lớn. Yêu cầu file nhỏ hơn {0}Mb";
    public static string FileTooLargeLimit(int size) => string.Format(FileTooLargeLimitFormat, size);
    private const string FileNotFoundByIdFormat = "File with id {0} not found";
    public static string FileNotFound(Guid id) => string.Format(FileNotFoundByIdFormat, id);
    public const string NoClassFound = "Không có lớp nào được tìm thấy";
}