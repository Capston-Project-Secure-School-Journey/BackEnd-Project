using FluentValidation;
using Api.Domain.Models;

namespace Api.Validators;

public class StudentValidator : AbstractValidator<Student>
{
    public StudentValidator()
    {
        RuleFor(student => student.SchoolId)
            .NotEmpty().WithMessage("Mã trường là bắt buộc.");

        RuleFor(student => student.FirstName)
            .NotEmpty().WithMessage("Tên là bắt buộc.")
            .MaximumLength(50).WithMessage("Tên không được vượt quá 50 ký tự.");

        RuleFor(student => student.LastName)
            .NotEmpty().WithMessage("Họ là bắt buộc.")
            .MaximumLength(50).WithMessage("Họ không được vượt quá 50 ký tự.");

        RuleFor(student => student.DateOfBirth)
            .NotEmpty().WithMessage("Ngày sinh là bắt buộc.")
            .LessThan(DateOnly.FromDateTime(DateTime.Now)).WithMessage("Ngày sinh phải nhỏ hơn ngày hiện tại.");

        RuleFor(student => student.ClassId)
            .NotEmpty().WithMessage("Mã lớp là bắt buộc.");

        RuleFor(student => student.Gender)
            .IsInEnum().WithMessage("Giới tính hợp lệ phải là (0 hoặc 1).");

        RuleFor(student => student.PickUpLocation)
            .MaximumLength(200).WithMessage("Địa chỉ đón không được vượt quá 200 ký tự.");
    }
}