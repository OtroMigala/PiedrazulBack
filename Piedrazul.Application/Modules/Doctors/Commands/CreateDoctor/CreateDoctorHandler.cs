using MediatR;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Doctors.Commands.CreateDoctor;

public class CreateDoctorHandler : IRequestHandler<CreateDoctorCommand, CreateDoctorResult>
{
    private readonly IDoctorRepository _doctorRepository;

    public CreateDoctorHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<CreateDoctorResult> Handle(
        CreateDoctorCommand request,
        CancellationToken cancellationToken)
    {
        var doctor = Doctor.Create(
            request.FullName,
            request.Type,
            request.Specialty,
            request.AppointmentIntervalMinutes);

        await _doctorRepository.AddAsync(doctor);

        return new CreateDoctorResult(
            DoctorId: doctor.Id,
            FullName: doctor.FullName,
            Specialty: doctor.Specialty.ToString(),
            IntervalMinutes: doctor.AppointmentIntervalMinutes);
    }
}
