using MediatR;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Doctors.Commands.UpdateDoctor;

public class UpdateDoctorHandler : IRequestHandler<UpdateDoctorCommand>
{
    private readonly IDoctorRepository _doctorRepository;

    public UpdateDoctorHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId);
        if (doctor is null)
            throw new InvalidOperationException("El médico no existe.");

        doctor.Update(request.FullName, request.Specialty, request.AppointmentIntervalMinutes);
        await _doctorRepository.UpdateAsync(doctor);
    }
}
