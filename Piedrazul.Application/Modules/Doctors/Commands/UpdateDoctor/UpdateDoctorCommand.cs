using MediatR;
using Piedrazul.Domain.Enums;

namespace Piedrazul.Application.Modules.Doctors.Commands.UpdateDoctor;

public record UpdateDoctorCommand(
    Guid DoctorId,
    string FullName,
    Specialty Specialty,
    int AppointmentIntervalMinutes)
    : IRequest;
