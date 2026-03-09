using MediatR;
using Piedrazul.Domain.Enums;

namespace Piedrazul.Application.Modules.Doctors.Commands.CreateDoctor;

public record CreateDoctorCommand(
    string FullName,
    DoctorType Type,
    Specialty Specialty,
    int AppointmentIntervalMinutes)
    : IRequest<CreateDoctorResult>;

public record CreateDoctorResult(
    Guid DoctorId,
    string FullName,
    string Specialty,
    int IntervalMinutes);
