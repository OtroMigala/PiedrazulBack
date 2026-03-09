using MediatR;

namespace Piedrazul.Application.Modules.Doctors.Commands.RemoveSchedule;

public record RemoveScheduleCommand(Guid DoctorId, Guid ScheduleId) : IRequest;
