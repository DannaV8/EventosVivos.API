using FluentValidation;

namespace EventosVivos.Application.Models.Events;

public sealed class CreateEventValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MinimumLength(5).MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(10).MaximumLength(500);
        RuleFor(x => x.VenueId).GreaterThan(0);
        RuleFor(x => x.MaxCapacity).GreaterThan(0);
        RuleFor(x => x.StartDateTime).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.EndDateTime).GreaterThan(x => x.StartDateTime);
        RuleFor(x => x.TicketPrice).GreaterThan(0);
    }
}
