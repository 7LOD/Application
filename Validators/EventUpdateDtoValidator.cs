using FluentValidation;
using MyEventsApi.Dtos;


namespace MyEventsApi.Validators
{
    public class EventUpdateDtoValidator : AbstractValidator<EventUpdateDto>
    {
        public EventUpdateDtoValidator()
        {
            When(e => e.Title is not null, () =>
            {
                RuleFor(e => e.Title)
                    .NotEmpty().WithMessage("Title cannot be empty.");
            });
            When(e => e.Description is not null, () =>
            {
                RuleFor(e => e.Description)
                    .NotEmpty().WithMessage("Description cannot be empty.");
            });
            When(e => e.Location is not null, () =>
            {
                RuleFor(e => e.Location)
                    .NotEmpty().WithMessage("Location cannot be empty.");
            });
            When(e => e.Date is not null, () =>
            {
                RuleFor(e => e.Date)
                    .Must(d => d >= DateTime.Now)
                    .WithMessage("Event date must be in the future.");
            });
            When(e => e.Capacity is not null, () =>
            {
                RuleFor(e => e.Capacity)
                    .Must(c => c == null || c >= 0)
                    .WithMessage("Capacity must be 0 (unlimited) or greater.");
            });
        }
    }
}
