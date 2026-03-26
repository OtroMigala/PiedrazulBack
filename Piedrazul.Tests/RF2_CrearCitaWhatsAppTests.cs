using FluentAssertions;
using Moq;
using Piedrazul.Application.Common.Interfaces;
using Piedrazul.Application.Modules.Appointments.Commands.CreateAppointment;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Enums;
using Piedrazul.Domain.Exceptions;
using Piedrazul.Domain.Interfaces;
using Piedrazul.Tests.Helpers;

namespace Piedrazul.Tests;

/// <summary>
/// RF2 — Crear cita desde WhatsApp.
/// Flujo: operador ingresa datos del paciente (doc, nombres, celular, género, fecha nac, email)
/// y datos de la cita (médico, hora). El sistema valida slot, crea paciente si no existe
/// y registra auditoría.
/// Handler bajo prueba: CreateAppointmentHandler
/// </summary>
public class RF2_CrearCitaWhatsAppTests
{
    private readonly Mock<IAppointmentRepository> _apptRepo = new();
    private readonly Mock<IDoctorRepository> _doctorRepo = new();
    private readonly Mock<IPatientRepository> _patientRepo = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly CreateAppointmentHandler _handler;

    private readonly Doctor _doctor;
    private readonly Patient _patient;
    private readonly DateTime _testDate;
    private readonly TimeSpan _testTime = TimeSpan.FromHours(8);

    public RF2_CrearCitaWhatsAppTests()
    {
        _handler = new CreateAppointmentHandler(
            _apptRepo.Object,
            _doctorRepo.Object,
            _patientRepo.Object,
            _auditService.Object);

        _testDate = EntityBuilder.NextWeekDay(DayOfWeek.Monday);
        _doctor = EntityBuilder.CreateDoctorWithSchedule(
            DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12));
        _patient = EntityBuilder.CreatePatient("10203040", "María González Pérez");

        // Setup por defecto del servicio de auditoría
        _auditService.Setup(a => a.LogAppointmentCreatedAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
    }

    // ── Helper ──────────────────────────────────────────────────────────────
    private CreateAppointmentCommand BuildCommand(
        string documentId = "10203040",
        TimeSpan? time = null) =>
        new(
            DocumentId: documentId,
            FullName: "María González Pérez",
            Phone: "3101234567",
            Gender: Gender.Female,
            BirthDate: new DateTime(1990, 5, 15),
            Email: "maria@example.com",
            DoctorId: _doctor.Id,
            Date: _testDate,
            Time: time ?? _testTime,
            CreatedByUserId: Guid.NewGuid());

    // ── Flujo exitoso ────────────────────────────────────────────────────────

    [Fact]
    public async Task Crea_cita_exitosamente_cuando_paciente_ya_existe_en_sistema()
    {
        // Arrange
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);
        _apptRepo.Setup(r => r.ExistsAsync(_doctor.Id, _testDate, _testTime)).ReturnsAsync(false);
        _patientRepo.Setup(r => r.GetByDocumentIdAsync("10203040")).ReturnsAsync(_patient);

        // Act
        var result = await _handler.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        result.AppointmentId.Should().NotBeEmpty();
        result.Message.Should().Be("Cita creada exitosamente.");
        result.DoctorName.Should().Be(_doctor.FullName);
        result.Time.Should().Be("08:00");
        result.Date.Should().Be(_testDate.ToString("dd/MM/yyyy"));

        // No debe crear un paciente nuevo si ya existe
        _patientRepo.Verify(r => r.AddAsync(It.IsAny<Patient>()), Times.Never);
        _apptRepo.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Once);
    }

    [Fact]
    public async Task Crea_paciente_nuevo_y_cita_cuando_paciente_no_existe_en_sistema()
    {
        // Arrange — simula el flujo de WhatsApp donde el paciente es nuevo
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);
        _apptRepo.Setup(r => r.ExistsAsync(_doctor.Id, _testDate, _testTime)).ReturnsAsync(false);
        _patientRepo.Setup(r => r.GetByDocumentIdAsync("NUEVO-DOC-001")).ReturnsAsync((Patient?)null);

        // Act
        var result = await _handler.Handle(BuildCommand("NUEVO-DOC-001"), CancellationToken.None);

        // Assert
        result.AppointmentId.Should().NotBeEmpty();
        result.Message.Should().Be("Cita creada exitosamente.");

        // Debe crear el paciente antes de crear la cita
        _patientRepo.Verify(r =>
            r.AddAsync(It.Is<Patient>(p => p.DocumentId == "NUEVO-DOC-001")), Times.Once);
        _apptRepo.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Once);
    }

    [Fact]
    public async Task Registra_auditoria_con_datos_correctos_al_crear_cita()
    {
        // Arrange
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);
        _apptRepo.Setup(r => r.ExistsAsync(_doctor.Id, _testDate, _testTime)).ReturnsAsync(false);
        _patientRepo.Setup(r => r.GetByDocumentIdAsync("10203040")).ReturnsAsync(_patient);

        // Act
        await _handler.Handle(BuildCommand(), CancellationToken.None);

        // Assert — la auditoría debe llamarse exactamente una vez con la fecha/hora correcta
        _auditService.Verify(a => a.LogAppointmentCreatedAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            _testDate,
            _testTime), Times.Once);
    }

    // ── Validación del médico ────────────────────────────────────────────────

    [Fact]
    public async Task Lanza_excepcion_cuando_medico_no_existe()
    {
        // Arrange
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _handler.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no está disponible*");
    }

    [Fact]
    public async Task Lanza_excepcion_cuando_medico_esta_inactivo()
    {
        // Arrange
        _doctor.Deactivate();
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);

        // Act
        var act = async () => await _handler.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no está disponible*");
    }

    // ── Validación del slot ──────────────────────────────────────────────────

    [Fact]
    public async Task Lanza_excepcion_cuando_hora_esta_fuera_del_horario_del_medico()
    {
        // Arrange — horario 08:00-12:00, hora solicitada 15:00
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);

        var commandFueraDeHorario = BuildCommand(time: TimeSpan.FromHours(15));

        // Act
        var act = async () => await _handler.Handle(commandFueraDeHorario, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SlotNotAvailableException>();
    }

    [Fact]
    public async Task Lanza_excepcion_cuando_hora_no_esta_alineada_con_intervalo_del_medico()
    {
        // Arrange — intervalo 30 min, hora 08:15 no es múltiplo válido desde 08:00
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);

        var commandMalAlineado = BuildCommand(
            time: TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15)));

        // Act
        var act = async () => await _handler.Handle(commandMalAlineado, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SlotNotAvailableException>();
    }

    [Fact]
    public async Task Lanza_excepcion_cuando_slot_ya_esta_ocupado_por_otra_cita()
    {
        // Arrange
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);
        _apptRepo.Setup(r => r.ExistsAsync(_doctor.Id, _testDate, _testTime)).ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SlotNotAvailableException>();
    }

    // ── Validaciones de entidades de dominio (usadas en RF2) ────────────────

    [Fact]
    public void Patient_Create_lanza_excepcion_con_documento_vacio()
    {
        var act = () => Patient.Create("", "Nombre", "3001234567", Gender.Male);
        act.Should().Throw<ArgumentException>().WithMessage("*documento*");
    }

    [Fact]
    public void Patient_Create_lanza_excepcion_con_nombre_vacio()
    {
        var act = () => Patient.Create("12345", "", "3001234567", Gender.Male);
        act.Should().Throw<ArgumentException>().WithMessage("*nombre*");
    }

    [Fact]
    public void Appointment_Create_lanza_excepcion_con_fecha_en_el_pasado()
    {
        var ayer = DateTime.UtcNow.Date.AddDays(-1);
        var act = () => Appointment.Create(
            Guid.NewGuid(), Guid.NewGuid(), ayer,
            TimeSpan.FromHours(8), Specialty.Physiotherapy, Guid.NewGuid());

        act.Should().Throw<ArgumentException>().WithMessage("*fecha pasada*");
    }
}
