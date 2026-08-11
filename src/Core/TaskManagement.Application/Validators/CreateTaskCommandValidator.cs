using System;
using FluentValidation;
using TaskManagement.Application.Features.Tasks.Commands;

namespace TaskManagement.Application.Validators;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Task.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Task.DueDate)
            .Must(BeAValidFutureDate)
            .WithMessage("DueDate must be a future date.")
            .When(x => x.Task.DueDate.HasValue);
    }

    private static bool BeAValidFutureDate(DateTime? dueDate)
    {
        return dueDate.HasValue && dueDate.Value > DateTime.UtcNow;
    }
}
