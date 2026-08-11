using System;
using FluentValidation;
using TaskManagement.Application.Dtos;

namespace TaskManagement.Application.Validators;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.DueDate)
            .Must(BeAValidFutureDate)
            .WithMessage("DueDate must be a future date.")
            .When(x => x.DueDate.HasValue);
    }

    private static bool BeAValidFutureDate(DateTime? dueDate)
    {
        return dueDate.HasValue && dueDate.Value > DateTime.UtcNow;
    }
}
