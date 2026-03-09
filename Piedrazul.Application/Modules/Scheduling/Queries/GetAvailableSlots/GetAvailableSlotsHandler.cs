using MediatR;
using Microsoft.Extensions.Configuration;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Scheduling.Queries.GetAvailableSlots;

public class GetAvailableSlotsHandler
    : IRequestHandler<GetAvailableSlotsQuery, AvailableSlotsResult>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IConfiguration _configuration;

    public GetAvailableSlotsHandler(
        IDoctorRepository doctorRepository,
        IAppointmentRepository appointmentRepository,
        IConfiguration configuration)
    {
        _doctorRepository = doctorRepository;
        _appointmentRepository = appointmentRepository;
        _configuration = configuration;
    }

    public async Task<AvailableSlotsResult> Handle(
        GetAvailableSlotsQuery request,
        CancellationToken cancellationToken)
    {
        // 0. Validar ventana de agendamiento
        var weeksAheadStr = _configuration["Scheduling:WeeksAhead"];
        var weeksAhead = int.TryParse(weeksAheadStr, out var w) ? w : 4;
        var maxDate = DateTime.UtcNow.Date.AddDays(weeksAhead * 7);
        if (request.Date.Date > maxDate)
            throw new InvalidOperationException(
                $"Solo se pueden consultar slots dentro de los próximos {weeksAhead} semanas.");

        // 1. Cargar médico con sus horarios configurados
        var doctor = await _doctorRepository.GetByIdWithSchedulesAsync(request.DoctorId);
        if (doctor is null || !doctor.IsActive)
            throw new InvalidOperationException("El médico no está disponible.");

        // 2. Obtener los slots ya ocupados para ese día
        var existingAppointments = await _appointmentRepository
            .GetByDoctorAndDateAsync(request.DoctorId, request.Date);
        var occupiedSlots = existingAppointments.Select(a => a.Time);

        // 3. El método GetAvailableSlots en la entidad Doctor
        //    calcula automáticamente los slots libres
        var availableSlots = doctor
            .GetAvailableSlots(request.Date, occupiedSlots)
            .Select(s => s.ToString(@"hh\:mm"))
            .ToList();

        return new AvailableSlotsResult(
            DoctorId: doctor.Id,
            DoctorName: doctor.FullName,
            Date: request.Date.ToString("dd/MM/yyyy"),
            Slots: availableSlots);
    }
}