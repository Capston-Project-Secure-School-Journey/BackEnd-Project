using FluentValidation;
using Api.Domain.Models;

namespace Api.Validators;

public class TeacherValidator : AbstractValidator<Teacher>
{
    public TeacherValidator()
    {
        RuleFor(teacher => teacher.SchoolId)
            .NotEmpty().WithMessage("Mã trường là bắt buộc.");

        RuleFor(teacher => teacher.FirstName)
            .NotEmpty().WithMessage("Tên là bắt buộc.")
            .MaximumLength(200).WithMessage("Tên không được vượt quá 200 ký tự.");

        RuleFor(teacher => teacher.LastName)
            .NotEmpty().WithMessage("Họ là bắt buộc.")
            .MaximumLength(200).WithMessage("Họ không được vượt quá 200 ký tự.");

        RuleFor(teacher => teacher.DateOfBirth)
            .NotEmpty().WithMessage("Ngày sinh là bắt buộc.")
            .LessThan(DateOnly.FromDateTime(DateTime.Now)).WithMessage("Ngày sinh phải nhỏ hơn ngày hiện tại.");

        RuleFor(teacher => teacher.Gender)
            .IsInEnum().WithMessage("Giới tính hợp lệ phải là (0 hoặc 1).");

        RuleFor(teacher => teacher.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại là bắt buộc.")
            .Matches(@"^\d{10,11}$").WithMessage("Số điện thoại phải có 10 hoặc 11 chữ số.");

        RuleFor(teacher => teacher.Email)
            .NotEmpty().WithMessage("Email là bắt buộc.")
            .EmailAddress().WithMessage("Email phải đúng định dạng.")
            .MaximumLength(100).WithMessage("Email không được vượt quá 100 ký tự.");
    }
}