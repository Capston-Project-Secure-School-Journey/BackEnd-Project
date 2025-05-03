using System.ComponentModel.DataAnnotations;

namespace Api.Common.Enums;

public enum ApplicationAction
{
    [Display(Name = "Nộp hồ sơ")] Submit = 1,
    [Display(Name = "Cập nhập hồ sơ")] Update = 2,
    [Display(Name = "Hủy hồ sơ")] Cancel = 3,
    [Display(Name = "Từ chối hồ sơ")] Reject = 4,
    [Display(Name = "Chấp nhận hồ sơ")] Approve = 5,

    [Display(Name = "Yêu cầu thêm thông tin")]
    RequestMoreInfo = 6,
    [Display(Name = "Xóa hồ sơ")] Delete = 7
}