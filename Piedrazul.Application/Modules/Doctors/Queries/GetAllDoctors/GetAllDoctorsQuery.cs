using MediatR;
using Piedrazul.Domain.Enums;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Doctors.Queries.GetAllDoctors;

/// <param name="Specialty">Filtro opcional. Si se omite, devuelve todos los médicos activos.</param>
public record GetAllDoctorsQuery(Specialty? Specialty = null) : IRequest<IEnumerable<DoctorDto>>;

public record DoctorDto(
    Guid Id,
    string FullName,
    string Type,
    string Specialty,
    int IntervalMinutes);

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
        var doctors = await _doctorRepository.GetAllActiveAsync();

        if (request.Specialty.HasValue)
            doctors = doctors.Where(d => d.Specialty == request.Specialty.Value);

        return doctors.Select(d => new DoctorDto(
            Id: d.Id,
            FullName: d.FullName,
            Type: d.Type.ToString(),
            Specialty: d.Specialty.ToString(),
            IntervalMinutes: d.AppointmentIntervalMinutes));
    }
}