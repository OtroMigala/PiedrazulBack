using MediatR;

namespace Piedrazul.Application.Modules.Appointments.Queries.GetAppointmentsByDoctorAndDate;

// Lo que React envía como query params
public record GetAppointmentsByDoctorAndDateQuery(
    Guid DoctorId,
    DateTime Date)
    : IRequest<AppointmentsResult>;

// Lo que el Handler devuelve
public record AppointmentsResult(
    string Message,
    int Total,
    IEnumerable<AppointmentListItemDto> Appointments);

public record AppointmentListItemDto(
    Guid Id,
    string PatientName,
    string DocumentId,
    string Time,          // "08:00"
    string Specialty,
    string Status);