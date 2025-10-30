using FluentValidation;
using MyEventsApi.Dto;
public class EventCreateDtoValidator : AbstractValidator<EventCreateDto>
{
    public EventCreateDtoValidator()
    {
        RuleFor(e => e.Title)
            .NotEmpty().WithMessage("Title is required.");



        RuleFor(e => e.Description)
            .NotEmpty().WithMessage("Description is required.");
            

        RuleFor(e => e.Date)
            .GreaterThan(DateTime.Now).WithMessage("Event date must be in the future.");
    }
}

