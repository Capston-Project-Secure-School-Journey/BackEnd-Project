using FluentValidation;
using Api.Domain.Models;

namespace Api.Validators;

public class ClassValidator : AbstractValidator<Class>
{
    public ClassValidator()
    {
        RuleFor(c => c.SchoolId)
            .NotEmpty().WithMessage("Mã trường là bắt buộc.");

        RuleFor(c => c.ClassName)
            .NotEmpty().WithMessage("Tên lớp là bắt buộc.")
            .MaximumLength(200).WithMessage("Tên lớp không được vượt quá 200 ký tự.");

        RuleFor(c => c.Grade)
            .IsInEnum().WithMessage("Khối lớp phải là số");

        RuleFor(c => c.NumberOfStudent)
            .GreaterThanOrEqualTo(0).WithMessage("Số lượng học sinh phải lớn hơn hoặc bằng 0.");

        RuleForEach(c => c.ManagedTeachers)
            .SetValidator(new ManagedTeacherValidator());
    }
}