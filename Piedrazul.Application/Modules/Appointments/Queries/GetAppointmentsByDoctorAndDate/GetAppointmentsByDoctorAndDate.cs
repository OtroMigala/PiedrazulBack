using MediatR;

namespace Piedrazul.Application.Modules.Appointments.Queries.GetAppointmentsByDoctorAndDate;

/// <summary>
/// Query para obtener las citas de un médico en una fecha específica (HU1 principal).
/// Disparada desde <c>GET /api/appointments/by-doctor?doctorId=&amp;date=&amp;search=</c>
/// (roles <c>Admin</c>, <c>Scheduler</c>).
/// Los resultados vienen ordenados por hora de atención.
/// </summary>
/// <param name="DoctorId">ID UUID del médico cuya agenda se consulta.</param>
/// <param name="Date">Fecha a consultar.</param>
/// <param name="Search">
/// Filtro opcional de texto libre. El backend filtra por nombre del paciente
/// o número de documento. Si se omite, retorna todas las citas del día.
/// </param>
public record GetAppointmentsByDoctorAndDateQuery(
    Guid      DoctorId,
    DateTime  Date,
    string?   Search = null)
    : IRequest<AppointmentsResult>;

/// <summary>
/// Resultado de la consulta de citas por médico y fecha.
/// Corresponde al cuerpo de la respuesta <c>200 OK</c>.
/// </summary>
/// <param name="Message">Mensaje descriptivo del resultado (ej: <c>"5 cita(s) encontrada(s)."</c>).</param>
/// <param name="Total">Cantidad total de citas en el resultado.</param>
/// <param name="Appointments">Colección de citas, ordenada por hora de atención.</param>
public record AppointmentsResult(
    string                            Message,
    int                               Total,
    IEnumerable<AppointmentListItemDto> Appointments);

/// <summary>
/// Ítem individual de la lista de citas. Proyección plana para consumo directo del frontend.
/// </summary>
/// <param name="Id">ID UUID de la cita.</param>
/// <param name="PatientName">Nombre completo del paciente.</param>
/// <param name="DocumentId">Número de documento del paciente.</param>
/// <param name="Time">Hora de atención formateada como <c>HH:mm</c> (ej: <c>"08:00"</c>).</param>
/// <param name="Specialty">Especialidad del médico como string (ej: <c>"NeuralTherapy"</c>).</param>
/// <param name="Status">
/// Estado de la cita. Valores posibles:
/// <c>Scheduled</c> | <c>Completed</c> | <c>Cancelled</c> | <c>Rescheduled</c>.
/// </param>
public record AppointmentListItemDto(
    Guid   Id,
    string PatientName,
    string DocumentId,
    string Time,
    string Specialty,
    string Status);
