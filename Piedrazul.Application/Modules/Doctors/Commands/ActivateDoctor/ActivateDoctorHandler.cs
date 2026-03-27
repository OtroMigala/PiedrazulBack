using MediatR;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Doctors.Commands.ActivateDoctor;

public class ActivateDoctorHandler : IRequestHandler<ActivateDoctorCommand>
{
    private readonly IDoctorRepository _doctorRepository;

    public ActivateDoctorHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task Handle(ActivateDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId);
        if (doctor is null)
            throw new InvalidOperationException("El médico no existe.");

        doctor.Activate();
        await _doctorRepository.UpdateAsync(doctor);
    }
}
