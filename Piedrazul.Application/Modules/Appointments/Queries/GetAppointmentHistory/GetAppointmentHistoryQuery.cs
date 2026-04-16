using MediatR;

namespace Piedrazul.Application.Modules.Appointments.Queries.GetAppointmentHistory;

public record GetAppointmentHistoryQuery(Guid AppointmentId)
    : IRequest<AppointmentHistoryResult>;

public record AppointmentHistoryResult(
    int Total,
    IEnumerable<AppointmentHistoryItemDto> History);

public record AppointmentHistoryItemDto(
    Guid Id,
    string Date,
    string Time,
    string Status,
    string? RescheduledAt,
    string? RescheduledByName,
    Guid? RescheduledFromId);
