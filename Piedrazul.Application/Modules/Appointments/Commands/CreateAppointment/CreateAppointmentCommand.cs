using MediatR;
using Piedrazul.Domain.Enums;

namespace Piedrazul.Application.Modules.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand(
    // Datos del paciente
    string DocumentId,
    string FullName,
    string Phone,
    Gender Gender,
    DateTime? BirthDate,
    string? Email,
    // Datos de la cita
    Guid DoctorId,
    DateTime Date,
    TimeSpan Time,
    // Usuario que agenda
    Guid CreatedByUserId)
    : IRequest<CreateAppointmentResult>;

public record CreateAppointmentResult(
    Guid AppointmentId,
    string Message,
    string Date,
    string Time,
    string DoctorName);