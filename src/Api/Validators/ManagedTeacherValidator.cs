using FluentValidation;
using Api.Domain.Models;

namespace Api.Validators;

public class ManagedTeacherValidator : AbstractValidator<ManagedTeacher>
{
    public ManagedTeacherValidator()
    {
        RuleFor(mt => mt.ManagedTeacherId)
            .NotEmpty().WithMessage("Mã giáo viên quản lý là bắt buộc.");
    }
}