using MediatR;
using Piedrazul.Domain.Enums;

namespace Piedrazul.Application.Modules.Doctors.Commands.UpdateDoctor;

/// <summary>
/// Comando para actualizar los datos editables de un médico existente.
/// Disparado desde <c>PUT /api/doctors/{id}</c> (rol <c>Admin</c>).
/// Retorna <see cref="Unit"/> — sin payload de respuesta (<c>204 No Content</c>).
/// </summary>
/// <param name="DoctorId">ID UUID del médico a actualizar.</param>
/// <param name="FullName">Nuevo nombre completo con título (ej: <c>"Dr. García"</c>).</param>
/// <param name="Specialty">Nueva especialidad: <c>NeuralTherapy</c> | <c>Chiropractic</c> | <c>Physiotherapy</c>.</param>
/// <param name="AppointmentIntervalMinutes">Nueva duración en minutos de cada franja de atención (ej: <c>30</c>).</param>
public record UpdateDoctorCommand(
    Guid      DoctorId,
    string    FullName,
    Specialty Specialty,
    int       AppointmentIntervalMinutes)
    : IRequest;
