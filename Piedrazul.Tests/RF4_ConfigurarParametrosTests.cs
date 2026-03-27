using FluentAssertions;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Enums;
using Piedrazul.Tests.Helpers;

namespace Piedrazul.Tests;

/// <summary>
/// RF4 — Configurar parámetros de agendamiento.
/// Prueba las entidades de dominio involucradas en la configuración:
///   - SchedulingSettings: ventana de tiempo y días laborales
///   - Doctor + DoctorSchedule: franjas horarias e intervalos
///   - Doctor.IsValidSlot(): validación de hora contra horario configurado
///   - Doctor.GetAvailableSlots(): cálculo de franjas disponibles
/// No requiere mocks — son pruebas puras de lógica de dominio.
/// </summary>
public class RF4_ConfigurarParametrosTests
{
    // ══════════════════════════════════════════════════════════════════════════
    //  SchedulingSettings — ventana de agendamiento
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void SchedulingSettings_default_tiene_4_semanas_de_ventana()
    {
        var settings = SchedulingSettings.CreateDefault();

        settings.WeeksAhead.Should().Be(4);
        settings.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void UpdateWeeksAhead_actualiza_la_ventana_correctamente()
    {
        var settings = SchedulingSettings.CreateDefault();

        settings.UpdateWeeksAhead(8);

        settings.WeeksAhead.Should().Be(8);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public void UpdateWeeksAhead_lanza_excepcion_con_valor_cero_o_negativo(int valorInvalido)
    {
        var settings = SchedulingSettings.CreateDefault();

        var act = () => settings.UpdateWeeksAhead(valorInvalido);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*mayor a 0*");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(12)]
    [InlineData(52)]
    public void UpdateWeeksAhead_acepta_cualquier_valor_positivo(int valorValido)
    {
        var settings = SchedulingSettings.CreateDefault();

        var act = () => settings.UpdateWeeksAhead(valorValido);

        act.Should().NotThrow();
        settings.WeeksAhead.Should().Be(valorValido);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  DoctorSchedule — configuración de franjas horarias
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void DoctorSchedule_se_crea_correctamente_con_horario_valido()
    {
        var doctorId = Guid.NewGuid();

        var schedule = DoctorSchedule.Create(
            doctorId, DayOfWeek.Wednesday,
            TimeSpan.FromHours(8), TimeSpan.FromHours(17));

        schedule.Id.Should().NotBeEmpty();
        schedule.DoctorId.Should().Be(doctorId);
        schedule.DayOfWeek.Should().Be(DayOfWeek.Wednesday);
        schedule.StartTime.Should().Be(TimeSpan.FromHours(8));
        schedule.EndTime.Should().Be(TimeSpan.FromHours(17));
    }

    [Fact]
    public void DoctorSchedule_lanza_excepcion_si_hora_fin_es_anterior_a_hora_inicio()
    {
        var act = () => DoctorSchedule.Create(
            Guid.NewGuid(), DayOfWeek.Monday,
            TimeSpan.FromHours(17), TimeSpan.FromHours(8));

        act.Should().Throw<ArgumentException>()
            .WithMessage("*posterior*");
    }

    [Fact]
    public void DoctorSchedule_lanza_excepcion_si_hora_fin_es_igual_a_hora_inicio()
    {
        var act = () => DoctorSchedule.Create(
            Guid.NewGuid(), DayOfWeek.Monday,
            TimeSpan.FromHours(8), TimeSpan.FromHours(8));

        act.Should().Throw<ArgumentException>();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Doctor — gestión de horarios (días laborales e intervalos)
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Doctor_se_crea_activo_con_datos_correctos()
    {
        var doctor = Doctor.Create("Dr. Roberto Silva", DoctorType.Therapist, Specialty.Chiropractic, 45);

        doctor.Id.Should().NotBeEmpty();
        doctor.FullName.Should().Be("Dr. Roberto Silva");
        doctor.Type.Should().Be(DoctorType.Therapist);
        doctor.Specialty.Should().Be(Specialty.Chiropractic);
        doctor.AppointmentIntervalMinutes.Should().Be(45);
        doctor.IsActive.Should().BeTrue();
        doctor.Schedules.Should().BeEmpty();
    }

    [Fact]
    public void Doctor_Create_lanza_excepcion_con_nombre_vacio()
    {
        var act = () => Doctor.Create("", DoctorType.Doctor, Specialty.Physiotherapy, 30);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*nombre*");
    }

    [Fact]
    public void Doctor_Create_lanza_excepcion_con_intervalo_cero_o_negativo()
    {
        var act = () => Doctor.Create("Dr. Test", DoctorType.Doctor, Specialty.Physiotherapy, 0);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*mayor a 0*");
    }

    [Fact]
    public void Doctor_agrega_horario_para_un_dia_laboral()
    {
        var doctor = Doctor.Create("Dra. López", DoctorType.Doctor, Specialty.NeuralTherapy, 30);
        var schedule = DoctorSchedule.Create(
            doctor.Id, DayOfWeek.Tuesday, TimeSpan.FromHours(9), TimeSpan.FromHours(13));

        doctor.AddSchedule(schedule);

        doctor.Schedules.Should().HaveCount(1);
        doctor.Schedules.First().DayOfWeek.Should().Be(DayOfWeek.Tuesday);
    }

    [Fact]
    public void Doctor_puede_configurar_horarios_en_multiples_dias_laborales()
    {
        var doctor = Doctor.Create("Dr. Multiday", DoctorType.Doctor, Specialty.Physiotherapy, 30);

        doctor.AddSchedule(DoctorSchedule.Create(doctor.Id, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12)));
        doctor.AddSchedule(DoctorSchedule.Create(doctor.Id, DayOfWeek.Wednesday, TimeSpan.FromHours(14), TimeSpan.FromHours(18)));
        doctor.AddSchedule(DoctorSchedule.Create(doctor.Id, DayOfWeek.Friday, TimeSpan.FromHours(8), TimeSpan.FromHours(12)));

        doctor.Schedules.Should().HaveCount(3);
    }

    [Fact]
    public void Doctor_lanza_excepcion_al_agregar_horario_duplicado_para_el_mismo_dia()
    {
        var doctor = EntityBuilder.CreateDoctorWithSchedule(DayOfWeek.Monday);
        var duplicado = DoctorSchedule.Create(
            doctor.Id, DayOfWeek.Monday, TimeSpan.FromHours(14), TimeSpan.FromHours(18));

        var act = () => doctor.AddSchedule(duplicado);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Ya existe*");
    }

    [Fact]
    public void Doctor_elimina_horario_por_id_correctamente()
    {
        var doctor = Doctor.Create("Dr. Test", DoctorType.Doctor, Specialty.Physiotherapy, 30);
        var schedule = DoctorSchedule.Create(doctor.Id, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12));
        doctor.AddSchedule(schedule);

        doctor.RemoveSchedule(schedule.Id);

        doctor.Schedules.Should().BeEmpty();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Doctor.IsValidSlot — validación de hora contra horario configurado
    // ══════════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(8, 0)]    // inicio exacto
    [InlineData(8, 30)]   // segundo slot
    [InlineData(9, 0)]    // tercer slot
    [InlineData(11, 30)]  // último slot antes del cierre 12:00
    public void IsValidSlot_retorna_true_para_slots_validos_alineados(int hora, int minuto)
    {
        // Horario lunes 08:00–12:00, intervalo 30 min
        var doctor = EntityBuilder.CreateDoctorWithSchedule(
            DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 30);
        var lunes = EntityBuilder.NextWeekDay(DayOfWeek.Monday);

        var esValido = doctor.IsValidSlot(lunes, new TimeSpan(hora, minuto, 0));

        esValido.Should().BeTrue($"el slot {hora:D2}:{minuto:D2} es válido dentro del horario");
    }

    [Theory]
    [InlineData(7, 0)]    // antes de apertura
    [InlineData(7, 59)]   // un minuto antes
    [InlineData(12, 0)]   // EndTime es exclusivo
    [InlineData(15, 0)]   // muy tarde
    public void IsValidSlot_retorna_false_para_horas_fuera_del_horario(int hora, int minuto)
    {
        var doctor = EntityBuilder.CreateDoctorWithSchedule(
            DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 30);
        var lunes = EntityBuilder.NextWeekDay(DayOfWeek.Monday);

        var esValido = doctor.IsValidSlot(lunes, new TimeSpan(hora, minuto, 0));

        esValido.Should().BeFalse($"el slot {hora:D2}:{minuto:D2} está fuera del horario");
    }

    [Theory]
    [InlineData(8, 15)]   // no múltiplo de 30 desde las 08:00
    [InlineData(8, 45)]   // idem
    [InlineData(9, 10)]   // idem
    public void IsValidSlot_retorna_false_para_horas_no_alineadas_con_intervalo(int hora, int minuto)
    {
        var doctor = EntityBuilder.CreateDoctorWithSchedule(
            DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12), 30);
        var lunes = EntityBuilder.NextWeekDay(DayOfWeek.Monday);

        var esValido = doctor.IsValidSlot(lunes, new TimeSpan(hora, minuto, 0));

        esValido.Should().BeFalse($"{hora:D2}:{minuto:D2} no está alineado con el intervalo de 30 min");
    }

    [Fact]
    public void IsValidSlot_retorna_false_si_medico_esta_inactivo()
    {
        var doctor = EntityBuilder.CreateDoctorWithSchedule(DayOfWeek.Monday);
        doctor.Deactivate();
        var lunes = EntityBuilder.NextWeekDay(DayOfWeek.Monday);

        doctor.IsValidSlot(lunes, TimeSpan.FromHours(8)).Should().BeFalse();
    }

    [Fact]
    public void IsValidSlot_retorna_false_si_dia_no_tiene_horario_configurado()
    {
        // Doctor solo trabaja lunes, se consulta martes
        var doctor = EntityBuilder.CreateDoctorWithSchedule(DayOfWeek.Monday);
        var martes = EntityBuilder.NextWeekDay(DayOfWeek.Tuesday);

        doctor.IsValidSlot(martes, TimeSpan.FromHours(8)).Should().BeFalse();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Doctor.GetAvailableSlots — cálculo de franjas disponibles
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void GetAvailableSlots_devuelve_todos_los_slots_si_ninguno_esta_ocupado()
    {
        // 08:00–10:00 con intervalo 30 min = 4 slots
        var doctor = EntityBuilder.CreateDoctorWithSchedule(
            DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(10), 30);
        var lunes = EntityBuilder.NextWeekDay(DayOfWeek.Monday);

        var slots = doctor.GetAvailableSlots(lunes, Enumerable.Empty<TimeSpan>()).ToList();

        slots.Should().HaveCount(4);
        slots.Should().ContainInOrder(
            TimeSpan.FromHours(8),
            new TimeSpan(8, 30, 0),
            TimeSpan.FromHours(9),
            new TimeSpan(9, 30, 0));
    }

    [Fact]
    public void GetAvailableSlots_excluye_correctamente_slots_ya_ocupados()
    {
        var doctor = EntityBuilder.CreateDoctorWithSchedule(
            DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(10), 30);
        var lunes = EntityBuilder.NextWeekDay(DayOfWeek.Monday);
        var ocupados = new[] { TimeSpan.FromHours(8), TimeSpan.FromHours(9) };

        var slots = doctor.GetAvailableSlots(lunes, ocupados).ToList();

        slots.Should().HaveCount(2);
        slots.Should().NotContain(TimeSpan.FromHours(8));
        slots.Should().NotContain(TimeSpan.FromHours(9));
        slots.Should().Contain(new TimeSpan(8, 30, 0));
        slots.Should().Contain(new TimeSpan(9, 30, 0));
    }

    [Fact]
    public void GetAvailableSlots_devuelve_vacio_para_dia_sin_horario_configurado()
    {
        var doctor = EntityBuilder.CreateDoctorWithSchedule(DayOfWeek.Monday);
        var martes = EntityBuilder.NextWeekDay(DayOfWeek.Tuesday);

        var slots = doctor.GetAvailableSlots(martes, Enumerable.Empty<TimeSpan>());

        slots.Should().BeEmpty("el médico no trabaja los martes");
    }

    [Fact]
    public void GetAvailableSlots_devuelve_vacio_si_medico_inactivo()
    {
        var doctor = EntityBuilder.CreateDoctorWithSchedule(DayOfWeek.Monday);
        doctor.Deactivate();
        var lunes = EntityBuilder.NextWeekDay(DayOfWeek.Monday);

        var slots = doctor.GetAvailableSlots(lunes, Enumerable.Empty<TimeSpan>());

        slots.Should().BeEmpty("un médico inactivo no tiene disponibilidad");
    }

    [Fact]
    public void GetAvailableSlots_calcula_correctamente_con_intervalo_de_60_minutos()
    {
        // 08:00–12:00 con intervalo 60 min = 4 slots: 08:00, 09:00, 10:00, 11:00
        var doctor = Doctor.Create("Dr. Hora", DoctorType.Doctor, Specialty.Physiotherapy, 60);
        var schedule = DoctorSchedule.Create(
            doctor.Id, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12));
        doctor.AddSchedule(schedule);
        var lunes = EntityBuilder.NextWeekDay(DayOfWeek.Monday);

        var slots = doctor.GetAvailableSlots(lunes, Enumerable.Empty<TimeSpan>()).ToList();

        slots.Should().HaveCount(4);
        slots.Should().Contain(TimeSpan.FromHours(8));
        slots.Should().Contain(TimeSpan.FromHours(9));
        slots.Should().Contain(TimeSpan.FromHours(10));
        slots.Should().Contain(TimeSpan.FromHours(11));
        slots.Should().NotContain(TimeSpan.FromHours(12), "EndTime es exclusivo");
    }
}
