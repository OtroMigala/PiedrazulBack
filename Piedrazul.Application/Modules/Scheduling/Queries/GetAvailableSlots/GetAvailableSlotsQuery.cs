using MediatR;

namespace Piedrazul.Application.Modules.Scheduling.Queries.GetAvailableSlots;

public record GetAvailableSlotsQuery(Guid DoctorId, DateTime Date)
    : IRequest<AvailableSlotsResult>;

public record AvailableSlotsResult(
    Guid DoctorId,
    string DoctorName,
    string Date,
    IEnumerable<string> Slots);  // ["08:00", "08:30", "09:00"]