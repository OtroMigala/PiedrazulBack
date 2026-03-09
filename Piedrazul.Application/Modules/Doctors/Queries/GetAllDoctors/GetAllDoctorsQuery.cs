using MediatR;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Doctors.Queries.GetAllDoctors;

public record GetAllDoctorsQuery : IRequest<IEnumerable<DoctorDto>>;

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

        return doctors.Select(d => new DoctorDto(
            Id: d.Id,
            FullName: d.FullName,
            Type: d.Type.ToString(),
            Specialty: d.Specialty.ToString(),
            IntervalMinutes: d.AppointmentIntervalMinutes));
    }
}