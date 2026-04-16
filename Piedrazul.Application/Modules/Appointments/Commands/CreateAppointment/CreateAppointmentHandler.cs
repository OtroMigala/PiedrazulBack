using MediatR;
using Piedrazul.Application.Common.Interfaces;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Exceptions;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Appointments.Commands.CreateAppointment;

/// <summary>
/// Handler para la creación manual de citas por parte del agendador o administrador.
/// Implementa el flujo completo de CA2: validación de médico, slot, disponibilidad,
/// upsert de paciente, persistencia de cita y registro de auditoría.
/// </summary>
public class CreateAppointmentHandler
    : IRequestHandler<CreateAppointmentCommand, CreateAppointmentResult>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository      _doctorRepository;
    private readonly IPatientRepository     _patientRepository;
    private readonly IAuditService          _auditService;

    public CreateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository      doctorRepository,
        IPatientRepository     patientRepository,
        IAuditService          auditService)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository      = doctorRepository;
        _patientRepository     = patientRepository;
        _auditService          = auditService;
    }

    /// <summary>
    /// Ejecuta el flujo de creación de cita en 5 pasos secuenciales.
    /// Lanza excepciones de dominio en caso de validaciones fallidas;
    /// el middleware de excepciones las traduce a respuestas HTTP.
    /// </summary>
    public async Task<CreateAppointmentResult> Handle(
        CreateAppointmentCommand request,
        CancellationToken        cancellationToken)
    {
        // ── Paso 1: Médico existe y está activo ───────────────────────────────
        // Se cargan los horarios configurados para validar el slot en el paso 2.
        var doctor = await _doctorRepository.GetByIdWithSchedulesAsync(request.DoctorId);
        if (doctor is null || !doctor.IsActive)
            throw new InvalidOperationException("El médico seleccionado no está disponible.");

        // ── Paso 2: El slot es válido según el horario del médico ─────────────
        // CA2.5: slots generados según intervalo configurado.
        // CA2.6: no se permite asignar fuera del horario de atención.
        if (!doctor.IsValidSlot(request.Date, request.Time))
            throw new SlotNotAvailableException(request.Date, request.Time);

        // ── Paso 3: El slot no está ocupado por otra cita ─────────────────────
        var slotOccupied = await _appointmentRepository
            .ExistsAsync(request.DoctorId, request.Date, request.Time);
        if (slotOccupied)
            throw new SlotNotAvailableException(request.Date, request.Time);

        // ── Paso 4: Upsert de paciente ────────────────────────────────────────
        // Si el paciente ya existe por documento se reutiliza; si no, se crea.
        var patient = await _patientRepository.GetByDocumentIdAsync(request.DocumentId);
        if (patient is null)
        {
            patient = Patient.Create(
                request.DocumentId,
                request.FullName,
                request.Phone,
                request.Gender,
                request.BirthDate,
                request.Email);

            await _patientRepository.AddAsync(patient);
        }

        // ── Paso 5: Crear cita y registrar auditoría ──────────────────────────
        var appointment = Appointment.Create(
            patientId:       patient.Id,
            doctorId:        doctor.Id,
            date:            request.Date,
            time:            request.Time,
            specialty:       doctor.Specialty,
            createdByUserId: request.CreatedByUserId);

        await _appointmentRepository.AddAsync(appointment);

        await _auditService.LogAppointmentCreatedAsync(
            performedByUserId: request.CreatedByUserId,
            appointmentId:     appointment.Id,
            patientId:         patient.Id,
            doctorId:          doctor.Id,
            date:              request.Date,
            time:              request.Time);

        return new CreateAppointmentResult(
            AppointmentId: appointment.Id,
            Message:       "Cita creada exitosamente.",
            Date:          request.Date.ToString("dd/MM/yyyy"),
            Time:          request.Time.ToString(@"hh\:mm"),
            DoctorName:    doctor.FullName);
    }
}
