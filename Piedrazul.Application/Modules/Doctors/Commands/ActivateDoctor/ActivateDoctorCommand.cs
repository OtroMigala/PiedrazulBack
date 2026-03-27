using MediatR;

namespace Piedrazul.Application.Modules.Doctors.Commands.ActivateDoctor;

public record ActivateDoctorCommand(Guid DoctorId) : IRequest;
