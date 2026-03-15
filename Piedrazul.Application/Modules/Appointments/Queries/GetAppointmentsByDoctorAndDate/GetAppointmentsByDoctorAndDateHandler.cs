using MediatR;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Appointments.Queries.GetAppointmentsByDoctorAndDate;

public class GetAppointmentsByDoctorAndDateHandler
    : IRequestHandler<GetAppointmentsByDoctorAndDateQuery, AppointmentsResult>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentsByDoctorAndDateHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<AppointmentsResult> Handle(
        GetAppointmentsByDoctorAndDateQuery request,
        CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository
            .GetByDoctorAndDateAsync(request.DoctorId, request.Date);

        var dtos = appointments
            .OrderBy(a => a.Time)
            .Select(a => new AppointmentListItemDto(
                Id: a.Id,
                PatientName: a.Patient!.FullName,
                DocumentId: a.Patient.DocumentId,
                Time: a.Time.ToString(@"hh\:mm"),
                Specialty: a.Specialty.ToString(),
                Status: a.Status.ToString()))
            .ToList();

        var message = dtos.Any()
            ? $"{dtos.Count} cita(s) encontrada(s)."
            : "No hay citas registradas para esta búsqueda";

        return new AppointmentsResult(
            Message: message,
            Total: dtos.Count,
            Appointments: dtos);
    }
}