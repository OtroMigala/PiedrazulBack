using MediatR;

namespace Piedrazul.Application.Modules.Appointments.Queries.GetAppointmentsByDate;

public record GetAppointmentsByDateQuery(DateTime Date)
    : IRequest<IEnumerable<DailyAgendaItemDto>>;

public record DailyAgendaItemDto(
    Guid Id,
    string Date,         // "dd/MM/yyyy"
    string Time,         // "HH:mm"
    string PatientName,
    string DocumentId,
    string Phone,
    string Specialty,
    string DoctorName,
    Guid DoctorId,
    string? Observation,
    string Status);
