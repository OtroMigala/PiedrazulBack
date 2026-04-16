using MediatR;
using Piedrazul.Domain.Enums;

namespace Piedrazul.Application.Modules.Doctors.Commands.CreateDoctor;

/// <summary>
/// Comando para crear un nuevo médico en el sistema.
/// Disparado desde <c>POST /api/doctors</c> (rol <c>Admin</c>).
/// </summary>
/// <param name="FullName">Nombre completo con título (ej: <c>"Dr. García"</c>).</param>
/// <param name="Type">Tipo de profesional: <c>Doctor</c> u otros valores de <see cref="DoctorType"/>.</param>
/// <param name="Specialty">Especialidad: <c>NeuralTherapy</c> | <c>Chiropractic</c> | <c>Physiotherapy</c>.</param>
/// <param name="AppointmentIntervalMinutes">Duración en minutos de cada franja de atención (ej: <c>30</c>).</param>
public record CreateDoctorCommand(
    string     FullName,
    DoctorType Type,
    Specialty  Specialty,
    int        AppointmentIntervalMinutes)
    : IRequest<CreateDoctorResult>;

/// <summary>
/// Resultado retornado tras la creación exitosa de un médico.
/// Corresponde al cuerpo de la respuesta <c>201 Created</c>.
/// </summary>
/// <param name="DoctorId">ID UUID asignado al médico recién creado.</param>
/// <param name="FullName">Nombre completo del médico registrado.</param>
/// <param name="Specialty">Especialidad del médico como string (ej: <c>"NeuralTherapy"</c>).</param>
/// <param name="IntervalMinutes">Duración en minutos de cada franja de atención.</param>
public record CreateDoctorResult(
    Guid   DoctorId,
    string FullName,
    string Specialty,
    int    IntervalMinutes);
