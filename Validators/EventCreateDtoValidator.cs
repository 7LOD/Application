using FluentValidation;
using MyEventsApi.Dtos;
public class EventCreateDtoValidator : AbstractValidator<EventCreateDto>
{
    public EventCreateDtoValidator()
    {
        RuleFor(e => e.Title)
            .NotEmpty().WithMessage("Title is required.");

        RuleFor(e => e.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(e => e.Location)
            .NotEmpty().WithMessage("Location is required.");


        RuleFor(e => e.Date)
            .GreaterThan(DateTime.Now).WithMessage("Event date must be in the future.");


        RuleFor(e => e.Capacity)
            .Must(c => c == null || c >= 0)
            .WithMessage("Capacity must be 0 (unlimited) or greater.");

    }
}

