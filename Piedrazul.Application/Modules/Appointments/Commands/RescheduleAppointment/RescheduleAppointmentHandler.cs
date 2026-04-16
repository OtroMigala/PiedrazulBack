using MediatR;
using Piedrazul.Application.Common.Interfaces;
using Piedrazul.Domain.Exceptions;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Appointments.Commands.RescheduleAppointment;

public class RescheduleAppointmentHandler
    : IRequestHandler<RescheduleAppointmentCommand, RescheduleAppointmentResult>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public RescheduleAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RescheduleAppointmentResult> Handle(
        RescheduleAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Obtener la cita (incluye navegación al Doctor para verificar UserId)
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId);
        if (appointment is null)
            throw new KeyNotFoundException("La cita no fue encontrada.");

        // 2. Si el rol es Doctor, verificar que la cita pertenece a su agenda
        if (request.RequestingUserRole == "Doctor")
        {
            if (appointment.Doctor?.UserId != request.RequestingUserId)
                throw new UnauthorizedAccessException("Solo puede re-agendar sus propias citas.");
        }

        // 3. Obtener el médico con horarios para validar el nuevo slot
        var doctor = await _doctorRepository.GetByIdWithSchedulesAsync(appointment.DoctorId);
        if (doctor is null || !doctor.IsActive)
            throw new InvalidOperationException("El médico seleccionado no está disponible.");

        // 4. Verificar que la nueva hora corresponde a un slot válido del horario del médico
        if (!doctor.IsValidSlot(request.NewDate, request.NewTime))
            throw new SlotNotAvailableException(request.NewDate, request.NewTime);

        // 5. Verificar que el nuevo slot no esté ya ocupado
        var slotOccupied = await _appointmentRepository
            .ExistsAsync(appointment.DoctorId, request.NewDate, request.NewTime);
        if (slotOccupied)
            throw new SlotNotAvailableException(request.NewDate, request.NewTime);

        // 6. Re-agendar: marca la cita actual como Rescheduled y retorna la nueva cita vinculada
        var newAppointment = appointment.Reschedule(request.NewDate, request.NewTime, request.RequestingUserId);

        // 7. Persistir ambas operaciones en una transacción
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _appointmentRepository.UpdateAsync(appointment);
            await _appointmentRepository.AddAsync(newAppointment);
            await _auditService.LogAppointmentRescheduledAsync(
                performedByUserId: request.RequestingUserId,
                oldAppointmentId: appointment.Id,
                newAppointmentId: newAppointment.Id,
                patientId: appointment.PatientId,
                doctorId: appointment.DoctorId,
                newDate: request.NewDate,
                newTime: request.NewTime);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        return new RescheduleAppointmentResult(
            NewAppointmentId: newAppointment.Id,
            Message: "Cita re-agendada exitosamente.",
            NewDate: request.NewDate.ToString("dd/MM/yyyy"),
            NewTime: request.NewTime.ToString(@"hh\:mm"));
    }
}
