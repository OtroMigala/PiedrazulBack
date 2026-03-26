using FluentAssertions;
using Moq;
using Piedrazul.Application.Modules.Appointments.Queries.GetAppointmentsByDoctorAndDate;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Interfaces;
using Piedrazul.Tests.Helpers;

namespace Piedrazul.Tests;

/// <summary>
/// RF1 — Listar citas de un médico en una fecha específica.
/// Prueba: filtros de búsqueda, resultados vacíos, mensajes de respuesta y orden por hora.
/// Handler bajo prueba: GetAppointmentsByDoctorAndDateHandler
/// </summary>
public class RF1_ListarCitasMedicoTests
{
    private readonly Mock<IAppointmentRepository> _apptRepo = new();
    private readonly GetAppointmentsByDoctorAndDateHandler _handler;

    private readonly Guid _doctorId = Guid.NewGuid();
    private readonly DateTime _testDate;
    private readonly Doctor _doctor;
    private readonly Patient _patient;

    public RF1_ListarCitasMedicoTests()
    {
        _handler = new GetAppointmentsByDoctorAndDateHandler(_apptRepo.Object);
        _testDate = EntityBuilder.NextWeekDay(DayOfWeek.Monday);
        _doctor = EntityBuilder.CreateDoctorWithSchedule(DayOfWeek.Monday);
        _patient = EntityBuilder.CreatePatient("10203040", "Carlos Ruiz Mora");
    }

    [Fact]
    public async Task Devuelve_cita_con_datos_correctos_del_paciente()
    {
        // Arrange
        var appointment = EntityBuilder.CreateAppointmentWithNavigation(
            _patient, _doctor, _testDate, TimeSpan.FromHours(8));

        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(_doctorId, _testDate))
            .ReturnsAsync(new List<Appointment> { appointment });

        var query = new GetAppointmentsByDoctorAndDateQuery(_doctorId, _testDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Total.Should().Be(1);
        result.Message.Should().Contain("1");
        var item = result.Appointments.Single();
        item.PatientName.Should().Be(_patient.FullName);
        item.DocumentId.Should().Be(_patient.DocumentId);
        item.Time.Should().Be("08:00");
        item.Specialty.Should().NotBeNullOrEmpty();
        item.Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Devuelve_resultado_vacio_cuando_no_hay_citas_para_la_fecha()
    {
        // Arrange
        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(_doctorId, _testDate))
            .ReturnsAsync(new List<Appointment>());

        var query = new GetAppointmentsByDoctorAndDateQuery(_doctorId, _testDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Total.Should().Be(0);
        result.Appointments.Should().BeEmpty();
        result.Message.Should().Contain("No hay citas");
    }

    [Fact]
    public async Task Filtra_citas_por_nombre_de_paciente_case_insensitive()
    {
        // Arrange
        var patient2 = EntityBuilder.CreatePatient("99887766", "Pedro Gomez Villa");
        var appt1 = EntityBuilder.CreateAppointmentWithNavigation(
            _patient, _doctor, _testDate, TimeSpan.FromHours(8));
        var appt2 = EntityBuilder.CreateAppointmentWithNavigation(
            patient2, _doctor, _testDate, TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30)));

        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(_doctorId, _testDate))
            .ReturnsAsync(new List<Appointment> { appt1, appt2 });

        var query = new GetAppointmentsByDoctorAndDateQuery(_doctorId, _testDate, Search: "CARLOS");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Total.Should().Be(1);
        result.Appointments.Single().PatientName.Should().Be(_patient.FullName);
    }

    [Fact]
    public async Task Filtra_citas_por_numero_de_documento()
    {
        // Arrange
        var patient2 = EntityBuilder.CreatePatient("99887766", "Pedro Gomez Villa");
        var appt1 = EntityBuilder.CreateAppointmentWithNavigation(
            _patient, _doctor, _testDate, TimeSpan.FromHours(8));
        var appt2 = EntityBuilder.CreateAppointmentWithNavigation(
            patient2, _doctor, _testDate, TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30)));

        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(_doctorId, _testDate))
            .ReturnsAsync(new List<Appointment> { appt1, appt2 });

        var query = new GetAppointmentsByDoctorAndDateQuery(_doctorId, _testDate, Search: "99887766");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Total.Should().Be(1);
        result.Appointments.Single().PatientName.Should().Be(patient2.FullName);
    }

    [Fact]
    public async Task Sin_filtro_devuelve_todas_las_citas_ordenadas_por_hora_ascendente()
    {
        // Arrange — citas desordenadas intencionalmente
        var appt1 = EntityBuilder.CreateAppointmentWithNavigation(
            _patient, _doctor, _testDate, TimeSpan.FromHours(10));
        var appt2 = EntityBuilder.CreateAppointmentWithNavigation(
            _patient, _doctor, _testDate, TimeSpan.FromHours(8));
        var appt3 = EntityBuilder.CreateAppointmentWithNavigation(
            _patient, _doctor, _testDate, TimeSpan.FromHours(9));

        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(_doctorId, _testDate))
            .ReturnsAsync(new List<Appointment> { appt1, appt2, appt3 });

        var query = new GetAppointmentsByDoctorAndDateQuery(_doctorId, _testDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Total.Should().Be(3);
        var times = result.Appointments.Select(a => a.Time).ToList();
        times.Should().BeInAscendingOrder("las citas deben listarse ordenadas cronológicamente");
    }

    [Fact]
    public async Task Busqueda_sin_coincidencias_devuelve_lista_vacia_con_mensaje()
    {
        // Arrange
        var appt = EntityBuilder.CreateAppointmentWithNavigation(
            _patient, _doctor, _testDate, TimeSpan.FromHours(8));

        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(_doctorId, _testDate))
            .ReturnsAsync(new List<Appointment> { appt });

        var query = new GetAppointmentsByDoctorAndDateQuery(_doctorId, _testDate, Search: "zzz_inexistente");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Total.Should().Be(0);
        result.Appointments.Should().BeEmpty();
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Multiples_citas_devuelven_total_correcto()
    {
        // Arrange
        var citas = Enumerable.Range(0, 5).Select(i =>
            EntityBuilder.CreateAppointmentWithNavigation(
                EntityBuilder.CreatePatient($"DOC{i:D3}", $"Paciente {i}"),
                _doctor,
                _testDate,
                TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30 * i)))
        ).ToList();

        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(_doctorId, _testDate))
            .ReturnsAsync(citas);

        var query = new GetAppointmentsByDoctorAndDateQuery(_doctorId, _testDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Total.Should().Be(5);
        result.Appointments.Should().HaveCount(5);
        result.Message.Should().Contain("5");
    }
}
