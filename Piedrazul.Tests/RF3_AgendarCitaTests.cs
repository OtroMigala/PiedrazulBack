using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Piedrazul.Application.Common.Options;
using Piedrazul.Application.Modules.Appointments.Commands.BookAppointment;
using Piedrazul.Application.Modules.Scheduling.Queries.GetAvailableSlots;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Exceptions;
using Piedrazul.Domain.Interfaces;
using Piedrazul.Tests.Helpers;

namespace Piedrazul.Tests;

/// <summary>
/// RF3 — Agendar cita desde UI web con franjas disponibles.
/// Prueba dos handlers:
///   1. GetAvailableSlotsHandler — calcula qué franjas están libres
///   2. BookAppointmentHandler   — reserva una franja para el paciente autenticado
/// </summary>
public class RF3_AgendarCitaTests
{
    // ── Dependencias GetAvailableSlots ───────────────────────────────────────
    private readonly Mock<IDoctorRepository> _doctorRepo = new();
    private readonly Mock<IAppointmentRepository> _apptRepo = new();
    private readonly IOptions<SchedulingOptions> _options =
        Options.Create(new SchedulingOptions { WeeksAhead = 4 });
    private readonly GetAvailableSlotsHandler _slotsHandler;

    // ── Dependencias BookAppointment ─────────────────────────────────────────
    private readonly Mock<IPatientRepository> _patientRepo = new();
    private readonly BookAppointmentHandler _bookHandler;

    // ── Datos de prueba compartidos ──────────────────────────────────────────
    // Horario lunes 08:00–10:00, intervalo 30 min → 4 slots: 08:00, 08:30, 09:00, 09:30
    private readonly Doctor _doctor;
    private readonly Patient _patient;
    private readonly DateTime _testDate;

    public RF3_AgendarCitaTests()
    {
        _slotsHandler = new GetAvailableSlotsHandler(
            _doctorRepo.Object, _apptRepo.Object, _options);
        _bookHandler = new BookAppointmentHandler(
            _apptRepo.Object, _doctorRepo.Object, _patientRepo.Object);

        _testDate = EntityBuilder.NextWeekDay(DayOfWeek.Monday);
        _doctor = EntityBuilder.CreateDoctorWithSchedule(
            DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(10), 30);
        _patient = EntityBuilder.CreatePatient();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  GetAvailableSlots
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Devuelve_todos_los_slots_cuando_no_hay_citas_agendadas()
    {
        // Arrange
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);
        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(_doctor.Id, _testDate))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = await _slotsHandler.Handle(
            new GetAvailableSlotsQuery(_doctor.Id, _testDate), CancellationToken.None);

        // Assert — horario 08:00-10:00 con intervalo 30 min = 4 slots
        result.Slots.Should().HaveCount(4);
        result.Slots.Should().Contain("08:00");
        result.Slots.Should().Contain("08:30");
        result.Slots.Should().Contain("09:00");
        result.Slots.Should().Contain("09:30");
        result.DoctorName.Should().Be(_doctor.FullName);
        result.DoctorId.Should().Be(_doctor.Id);
    }

    [Fact]
    public async Task Slots_ocupados_no_aparecen_en_la_lista_de_disponibles()
    {
        // Arrange — 08:00 ya tiene una cita, quedan 3 slots libres
        var citaExistente = EntityBuilder.CreateAppointmentWithNavigation(
            _patient, _doctor, _testDate, TimeSpan.FromHours(8));

        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);
        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(_doctor.Id, _testDate))
            .ReturnsAsync(new List<Appointment> { citaExistente });

        // Act
        var result = await _slotsHandler.Handle(
            new GetAvailableSlotsQuery(_doctor.Id, _testDate), CancellationToken.None);

        // Assert
        result.Slots.Should().HaveCount(3);
        result.Slots.Should().NotContain("08:00", "ese slot ya está ocupado");
        result.Slots.Should().Contain("08:30");
        result.Slots.Should().Contain("09:00");
        result.Slots.Should().Contain("09:30");
    }

    [Fact]
    public async Task Devuelve_lista_vacia_cuando_todos_los_slots_estan_ocupados()
    {
        // Arrange — los 4 slots ocupados
        var citas = new[]
        {
            EntityBuilder.CreateAppointmentWithNavigation(_patient, _doctor, _testDate, TimeSpan.FromHours(8)),
            EntityBuilder.CreateAppointmentWithNavigation(_patient, _doctor, _testDate, TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30))),
            EntityBuilder.CreateAppointmentWithNavigation(_patient, _doctor, _testDate, TimeSpan.FromHours(9)),
            EntityBuilder.CreateAppointmentWithNavigation(_patient, _doctor, _testDate, TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(30))),
        };

        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);
        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(_doctor.Id, _testDate))
            .ReturnsAsync(citas.ToList());

        // Act
        var result = await _slotsHandler.Handle(
            new GetAvailableSlotsQuery(_doctor.Id, _testDate), CancellationToken.None);

        // Assert
        result.Slots.Should().BeEmpty("la agenda está completa para ese día");
    }

    [Fact]
    public async Task Devuelve_lista_vacia_cuando_medico_no_tiene_horario_en_el_dia_consultado()
    {
        // Arrange — doctor con horario solo lunes, se consulta martes
        var martes = EntityBuilder.NextWeekDay(DayOfWeek.Tuesday);

        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);
        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(_doctor.Id, martes))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = await _slotsHandler.Handle(
            new GetAvailableSlotsQuery(_doctor.Id, martes), CancellationToken.None);

        // Assert
        result.Slots.Should().BeEmpty("el médico no trabaja los martes");
    }

    [Fact]
    public async Task Lanza_excepcion_cuando_fecha_supera_la_ventana_de_agendamiento()
    {
        // Arrange — 10 semanas en el futuro, ventana configurada es 4 semanas
        var fechaFuera = DateTime.UtcNow.Date.AddDays(70);

        // Act
        var act = async () => await _slotsHandler.Handle(
            new GetAvailableSlotsQuery(_doctor.Id, fechaFuera), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*próximos*");
    }

    [Fact]
    public async Task Lanza_excepcion_cuando_medico_no_existe()
    {
        // Arrange
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _slotsHandler.Handle(
            new GetAvailableSlotsQuery(Guid.NewGuid(), _testDate), CancellationToken.None);

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
        var act = async () => await _slotsHandler.Handle(
            new GetAvailableSlotsQuery(_doctor.Id, _testDate), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no está disponible*");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  BookAppointment (paciente reserva su cita desde UI web)
    // ══════════════════════════════════════════════════════════════════════════

    private BookAppointmentCommand BuildBookCommand(TimeSpan? time = null) =>
        new(
            UserId: Guid.NewGuid(),
            DoctorId: _doctor.Id,
            Date: _testDate,
            Time: time ?? TimeSpan.FromHours(8),
            CaptchaToken: "token-test-valido");

    [Fact]
    public async Task Paciente_agenda_cita_exitosamente_en_slot_disponible()
    {
        // Arrange
        var cmd = BuildBookCommand();
        _patientRepo.Setup(r => r.GetByUserIdAsync(cmd.UserId)).ReturnsAsync(_patient);
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);
        _apptRepo.Setup(r => r.PatientHasAppointmentAsync(_patient.Id, _doctor.Id, _testDate))
            .ReturnsAsync(false);
        _apptRepo.Setup(r => r.ExistsAsync(_doctor.Id, _testDate, cmd.Time)).ReturnsAsync(false);

        // Act
        var result = await _bookHandler.Handle(cmd, CancellationToken.None);

        // Assert
        result.AppointmentId.Should().NotBeEmpty();
        result.Message.Should().Be("Cita agendada exitosamente.");
        result.DoctorName.Should().Be(_doctor.FullName);
        result.Specialty.Should().NotBeNullOrEmpty();
        _apptRepo.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Once);
    }

    [Fact]
    public async Task Lanza_excepcion_cuando_usuario_no_tiene_perfil_de_paciente()
    {
        // Arrange
        var cmd = BuildBookCommand();
        _patientRepo.Setup(r => r.GetByUserIdAsync(cmd.UserId)).ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _bookHandler.Handle(cmd, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*perfil de paciente*");
    }

    [Fact]
    public async Task Lanza_excepcion_cuando_paciente_ya_tiene_cita_con_ese_medico_ese_dia()
    {
        // Arrange — CA3.7: impedir cita duplicada mismo paciente/médico/día
        var cmd = BuildBookCommand(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30)));
        _patientRepo.Setup(r => r.GetByUserIdAsync(cmd.UserId)).ReturnsAsync(_patient);
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);
        _apptRepo.Setup(r => r.PatientHasAppointmentAsync(_patient.Id, _doctor.Id, _testDate))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _bookHandler.Handle(cmd, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya tienes una cita*");
    }

    [Fact]
    public async Task Lanza_excepcion_cuando_slot_ya_fue_tomado_por_otro_paciente()
    {
        // Arrange — race condition: slot válido pero ya ocupado
        var cmd = BuildBookCommand();
        _patientRepo.Setup(r => r.GetByUserIdAsync(cmd.UserId)).ReturnsAsync(_patient);
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);
        _apptRepo.Setup(r => r.PatientHasAppointmentAsync(_patient.Id, _doctor.Id, _testDate))
            .ReturnsAsync(false);
        _apptRepo.Setup(r => r.ExistsAsync(_doctor.Id, _testDate, cmd.Time)).ReturnsAsync(true);

        // Act
        var act = async () => await _bookHandler.Handle(cmd, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SlotNotAvailableException>();
    }

    [Fact]
    public async Task Lanza_excepcion_cuando_hora_solicitada_no_es_slot_valido_del_medico()
    {
        // Arrange — hora fuera del horario 08:00-10:00
        var cmd = BuildBookCommand(TimeSpan.FromHours(14));
        _patientRepo.Setup(r => r.GetByUserIdAsync(cmd.UserId)).ReturnsAsync(_patient);
        _doctorRepo.Setup(r => r.GetByIdWithSchedulesAsync(_doctor.Id)).ReturnsAsync(_doctor);

        // Act
        var act = async () => await _bookHandler.Handle(cmd, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SlotNotAvailableException>();
    }
}
