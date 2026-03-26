using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Enums;

namespace Piedrazul.Tests.Helpers;

/// <summary>
/// Fábrica de entidades de dominio para pruebas unitarias.
/// Usa reflexión para asignar propiedades de navegación privadas (Patient, Doctor)
/// en Appointment, simulando lo que hace EF Core en tiempo de ejecución.
/// </summary>
public static class EntityBuilder
{
    /// <summary>
    /// Crea un Doctor activo con un horario configurado para el día especificado.
    /// Por defecto: lunes 08:00–12:00, intervalo 30 min, especialidad Fisioterapia.
    /// </summary>
    public static Doctor CreateDoctorWithSchedule(
        DayOfWeek day = DayOfWeek.Monday,
        TimeSpan? start = null,
        TimeSpan? end = null,
        int intervalMinutes = 30,
        Specialty specialty = Specialty.Physiotherapy)
    {
        var doctor = Doctor.Create("Dra. Ana Torres", DoctorType.Doctor, specialty, intervalMinutes);
        var schedule = DoctorSchedule.Create(
            doctor.Id,
            day,
            start ?? TimeSpan.FromHours(8),
            end ?? TimeSpan.FromHours(12));
        doctor.AddSchedule(schedule);
        return doctor;
    }

    /// <summary>
    /// Crea un Paciente con datos básicos de prueba.
    /// </summary>
    public static Patient CreatePatient(
        string documentId = "12345678",
        string fullName = "Juan Pérez López",
        string phone = "3001234567",
        Gender gender = Gender.Male)
    {
        return Patient.Create(documentId, fullName, phone, gender);
    }

    /// <summary>
    /// Crea un Appointment y asigna las propiedades de navegación (Patient, Doctor)
    /// mediante reflexión, simulando lo que haría EF Core al cargar desde base de datos.
    /// </summary>
    public static Appointment CreateAppointmentWithNavigation(
        Patient patient,
        Doctor doctor,
        DateTime? date = null,
        TimeSpan? time = null)
    {
        var apptDate = date ?? DateTime.UtcNow.Date.AddDays(1);
        var apptTime = time ?? TimeSpan.FromHours(8);

        var appt = Appointment.Create(
            patientId: patient.Id,
            doctorId: doctor.Id,
            date: apptDate,
            time: apptTime,
            specialty: doctor.Specialty,
            createdByUserId: Guid.NewGuid());

        // Asignar propiedades de navegación con setter privado (EF Core lo hace internamente)
        typeof(Appointment).GetProperty("Patient")!.SetValue(appt, patient);
        typeof(Appointment).GetProperty("Doctor")!.SetValue(appt, doctor);

        return appt;
    }

    /// <summary>
    /// Devuelve la próxima fecha que cae en el DayOfWeek indicado (mínimo mañana).
    /// Útil para crear fechas válidas que no estén en el pasado.
    /// </summary>
    public static DateTime NextWeekDay(DayOfWeek day)
    {
        var date = DateTime.UtcNow.Date.AddDays(1);
        while (date.DayOfWeek != day)
            date = date.AddDays(1);
        return date;
    }
}
