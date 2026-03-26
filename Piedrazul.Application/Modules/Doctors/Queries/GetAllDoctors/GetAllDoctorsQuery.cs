using MediatR;
using Piedrazul.Domain.Enums;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Doctors.Queries.GetAllDoctors;

/// <param name="Specialty">Filtro opcional. Si se omite, devuelve todos los médicos.</param>
/// <param name="IncludeInactive">Si true, incluye médicos inactivos (uso admin).</param>
public record GetAllDoctorsQuery(Specialty? Specialty = null, bool IncludeInactive = false) : IRequest<IEnumerable<DoctorDto>>;

public record ScheduleDto(
    Guid Id,
    string DayOfWeek,
    string StartTime,
    string EndTime);

public record DoctorDto(
    Guid Id,
    string FullName,
    string Type,
    string Specialty,
    int IntervalMinutes,
    bool IsActive,
    IEnumerable<ScheduleDto> Schedules);

public class GetAllDoctorsHandler : IRequestHandler<GetAllDoctorsQuery, IEnumerable<DoctorDto>>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetAllDoctorsHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<IEnumerable<DoctorDto>> Handle(
        GetAllDoctorsQuery request,
        CancellationToken cancellationToken)
    {
        var doctors = request.IncludeInactive
            ? await _doctorRepository.GetAllAsync()
            : await _doctorRepository.GetAllActiveAsync();

        if (request.Specialty.HasValue)
            doctors = doctors.Where(d => d.Specialty == request.Specialty.Value);

        return doctors.Select(d => new DoctorDto(
            Id: d.Id,
            FullName: d.FullName,
            Type: d.Type.ToString(),
            Specialty: d.Specialty.ToString(),
            IntervalMinutes: d.AppointmentIntervalMinutes,
            IsActive: d.IsActive,
            Schedules: d.Schedules.Select(s => new ScheduleDto(
                Id: s.Id,
                DayOfWeek: s.DayOfWeek.ToString(),
                StartTime: s.StartTime.ToString(@"hh\:mm"),
                EndTime: s.EndTime.ToString(@"hh\:mm")))));
    }
}