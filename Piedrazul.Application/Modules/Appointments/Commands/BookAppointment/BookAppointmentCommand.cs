using MediatR;

namespace Piedrazul.Application.Modules.Appointments.Commands.BookAppointment;

/// <summary>
/// Comando para que un paciente autenticado agende su propia cita (autoagendado).
/// Disparado desde <c>POST /api/appointments/book</c> (rol <c>Patient</c>).
/// </summary>
/// <param name="UserId">ID UUID del usuario paciente que realiza el agendamiento.</param>
/// <param name="DoctorId">ID UUID del médico seleccionado para la cita.</param>
/// <param name="Date">Fecha deseada para la cita (componente de fecha).</param>
/// <param name="Time">Hora deseada para la cita (componente de tiempo, ej: <c>08:00:00</c>).</param>
/// <param name="CaptchaToken">
/// Token anti-bot. En esta iteración se valida que no esté vacío (mock funcional).
/// Integrar con reCAPTCHA v3 o hCaptcha antes de salir a producción.
/// </param>
public record BookAppointmentCommand(
    Guid     UserId,
    Guid     DoctorId,
    DateTime Date,
    TimeSpan Time,
    string   CaptchaToken)
    : IRequest<BookAppointmentResult>;

/// <summary>
/// Resultado retornado tras el agendamiento exitoso de una cita por el paciente.
/// Corresponde al cuerpo de la respuesta <c>201 Created</c>.
/// </summary>
/// <param name="AppointmentId">ID UUID de la cita recién creada.</param>
/// <param name="Message">Mensaje de confirmación legible para el usuario.</param>
/// <param name="Date">Fecha de la cita formateada como <c>YYYY-MM-DD</c>.</param>
/// <param name="Time">Hora de la cita formateada como <c>HH:mm</c>.</param>
/// <param name="DoctorName">Nombre completo del médico asignado.</param>
/// <param name="Specialty">Especialidad del médico como string (ej: <c>"NeuralTherapy"</c>).</param>
public record BookAppointmentResult(
    Guid   AppointmentId,
    string Message,
    string Date,
    string Time,
    string DoctorName,
    string Specialty);
