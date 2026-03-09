using MediatR;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Appointments.Queries.GetAppointmentsByDate;

public class GetAppointmentsByDateHandler
    : IRequestHandler<GetAppointmentsByDateQuery, IEnumerable<DailyAgendaItemDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentsByDateHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IEnumerable<DailyAgendaItemDto>> Handle(
        GetAppointmentsByDateQuery request,
        CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetByDateAsync(request.Date);

        return appointments.Select(a => new DailyAgendaItemDto(
            Id: a.Id,
            Date: a.Date.ToString("dd/MM/yyyy"),
            Time: a.Time.ToString(@"hh\:mm"),
            PatientName: a.Patient!.FullName,
            DocumentId: a.Patient.DocumentId,
            Phone: a.Patient.Phone,
            Specialty: a.Specialty.ToString(),
            DoctorName: a.Doctor!.FullName,
            DoctorId: a.DoctorId,
            Observation: null,
            Status: a.Status.ToString()));
    }
}
