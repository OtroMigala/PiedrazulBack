using MediatR;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Appointments.Queries.GetAppointmentHistory;

public class GetAppointmentHistoryHandler
    : IRequestHandler<GetAppointmentHistoryQuery, AppointmentHistoryResult>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserRepository _userRepository;

    public GetAppointmentHistoryHandler(
        IAppointmentRepository appointmentRepository,
        IUserRepository userRepository)
    {
        _appointmentRepository = appointmentRepository;
        _userRepository = userRepository;
    }

    public async Task<AppointmentHistoryResult> Handle(
        GetAppointmentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var chain = (await _appointmentRepository.GetHistoryChainAsync(request.AppointmentId)).ToList();

        if (!chain.Any())
            throw new KeyNotFoundException("La cita no fue encontrada.");

        // Recolectar ids de usuarios únicos que aparcen en RescheduledByUserId
        var userIds = chain
            .Where(a => a.RescheduledByUserId.HasValue)
            .Select(a => a.RescheduledByUserId!.Value)
            .Distinct()
            .ToList();

        // Cargar nombres de usuarios en paralelo
        var userNames = new Dictionary<Guid, string>();
        foreach (var userId in userIds)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is not null)
                userNames[userId] = user.FullName;
        }

        var dtos = chain.Select(a => new AppointmentHistoryItemDto(
            Id: a.Id,
            Date: a.Date.ToString("dd/MM/yyyy"),
            Time: a.Time.ToString(@"hh\:mm"),
            Status: a.Status.ToString(),
            RescheduledAt: a.RescheduledAt?.ToString("dd/MM/yyyy HH:mm"),
            RescheduledByName: a.RescheduledByUserId.HasValue && userNames.TryGetValue(a.RescheduledByUserId.Value, out var name)
                ? name
                : null,
            RescheduledFromId: a.RescheduledFromId))
            .ToList();

        return new AppointmentHistoryResult(Total: dtos.Count, History: dtos);
    }
}
