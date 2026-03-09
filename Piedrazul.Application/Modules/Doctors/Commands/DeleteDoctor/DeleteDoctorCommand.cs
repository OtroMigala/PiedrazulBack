using MediatR;

namespace Piedrazul.Application.Modules.Doctors.Commands.DeleteDoctor;

public record DeleteDoctorCommand(Guid DoctorId) : IRequest;
