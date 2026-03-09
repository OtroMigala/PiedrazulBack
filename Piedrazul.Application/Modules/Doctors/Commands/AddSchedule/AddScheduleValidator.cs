using FluentValidation;

namespace Piedrazul.Application.Modules.Doctors.Commands.AddSchedule;

public class AddScheduleValidator : AbstractValidator<AddScheduleCommand>
{
    public AddScheduleValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("El ID del médico es obligatorio.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("La hora de fin debe ser posterior a la hora de inicio.");
    }
}
