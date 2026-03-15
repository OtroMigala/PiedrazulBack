using MediatR;

namespace Piedrazul.Application.Modules.Appointments.Commands.BookAppointment;

public record BookAppointmentCommand(
    Guid UserId,
    Guid DoctorId,
    DateTime Date,
    TimeSpan Time,
    /// <summary>
    /// Token anti-bot. En esta iteración se valida que no esté vacío (mock funcional).
    /// Integrar con reCAPTCHA v3 o hCaptcha en producción.
    /// </summary>
    string CaptchaToken)
    : IRequest<BookAppointmentResult>;

public record BookAppointmentResult(
    Guid AppointmentId,
    string Message,
    string Date,
    string Time,
    string DoctorName,
    string Specialty);
