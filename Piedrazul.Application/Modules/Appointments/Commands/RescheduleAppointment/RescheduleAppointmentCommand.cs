using MediatR;

namespace Piedrazul.Application.Modules.Appointments.Commands.RescheduleAppointment;

public record RescheduleAppointmentCommand(
    Guid AppointmentId,
    DateTime NewDate,
    TimeSpan NewTime,
    Guid RequestingUserId,
    string RequestingUserRole) : IRequest<RescheduleAppointmentResult>;

public record RescheduleAppointmentResult(
    Guid NewAppointmentId,
    string Message,
    string NewDate,
    string NewTime);
