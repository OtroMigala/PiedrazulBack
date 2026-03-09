using MediatR;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Exceptions;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Appointments.Commands.BookAppointment;

public class BookAppointmentHandler : IRequestHandler<BookAppointmentCommand, BookAppointmentResult>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPatientRepository _patientRepository;

    public BookAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IPatientRepository patientRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
    }

    public async Task<BookAppointmentResult> Handle(
        BookAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Obtener el paciente vinculado al usuario autenticado
        var patient = await _patientRepository.GetByUserIdAsync(request.UserId);
        if (patient is null)
            throw new InvalidOperationException(
                "No se encontró un perfil de paciente asociado a este usuario. " +
                "Por favor contacte al administrador.");

        // 2. Verificar que el médico existe y está activo
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId);
        if (doctor is null || !doctor.IsActive)
            throw new InvalidOperationException("El médico seleccionado no está disponible.");

        // 3. Verificar que el slot no está ocupado
        var slotOccupied = await _appointmentRepository
            .ExistsAsync(request.DoctorId, request.Date, request.Time);
        if (slotOccupied)
            throw new SlotNotAvailableException(request.Date, request.Time);

        // 4. Crear la cita
        var appointment = Appointment.Create(
            patientId: patient.Id,
            doctorId: doctor.Id,
            date: request.Date,
            time: request.Time,
            specialty: doctor.Specialty,
            createdByUserId: request.UserId);

        await _appointmentRepository.AddAsync(appointment);

        return new BookAppointmentResult(
            AppointmentId: appointment.Id,
            Message: "Cita agendada exitosamente.",
            Date: request.Date.ToString("dd/MM/yyyy"),
            Time: request.Time.ToString(@"hh\:mm"),
            DoctorName: doctor.FullName);
    }
}
