using MediatR;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Exceptions;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Appointments.Commands.CreateAppointment;

public class CreateAppointmentHandler
    : IRequestHandler<CreateAppointmentCommand, CreateAppointmentResult>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPatientRepository _patientRepository;

    public CreateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IPatientRepository patientRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
    }

    public async Task<CreateAppointmentResult> Handle(
        CreateAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verificar que el médico existe, está activo y cargar sus horarios configurados
        var doctor = await _doctorRepository.GetByIdWithSchedulesAsync(request.DoctorId);
        if (doctor is null || !doctor.IsActive)
            throw new InvalidOperationException("El médico seleccionado no está disponible.");

        // 2. Verificar que la hora solicitada corresponde a un slot válido del horario del médico
        //    (CA2.5: slots generados según intervalo; CA2.6: no asignar fuera de horario)
        if (!doctor.IsValidSlot(request.Date, request.Time))
            throw new SlotNotAvailableException(request.Date, request.Time);

        // 3. Verificar que el slot no está ya ocupado por otra cita
        var slotOccupied = await _appointmentRepository
            .ExistsAsync(request.DoctorId, request.Date, request.Time);
        if (slotOccupied)
            throw new SlotNotAvailableException(request.Date, request.Time);

        // 4. Buscar paciente existente o crear uno nuevo
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

        // 5. Crear la cita
        var appointment = Appointment.Create(
            patientId: patient.Id,
            doctorId: doctor.Id,
            date: request.Date,
            time: request.Time,
            specialty: doctor.Specialty,
            createdByUserId: request.CreatedByUserId);

        await _appointmentRepository.AddAsync(appointment);

        return new CreateAppointmentResult(
            AppointmentId: appointment.Id,
            Message: "Cita creada exitosamente.",
            Date: request.Date.ToString("dd/MM/yyyy"),
            Time: request.Time.ToString(@"hh\:mm"),
            DoctorName: doctor.FullName);
    }
}