using MediatR;

namespace Piedrazul.Application.Modules.Doctors.Commands.AddSchedule;

public record AddScheduleCommand(
    Guid DoctorId,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime)
    : IRequest<AddScheduleResult>;

public record AddScheduleResult(
    Guid ScheduleId,
    Guid DoctorId,
    string DayOfWeek,
    string StartTime,
    string EndTime);
