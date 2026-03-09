using MediatR;

namespace Piedrazul.Application.Modules.Appointments.Commands.BookAppointment;

public record BookAppointmentCommand(
    Guid UserId,
    Guid DoctorId,
    DateTime Date,
    TimeSpan Time)
    : IRequest<BookAppointmentResult>;

public record BookAppointmentResult(
    Guid AppointmentId,
    string Message,
    string Date,
    string Time,
    string DoctorName);
