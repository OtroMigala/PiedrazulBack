using MediatR;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Doctors.Commands.DeleteDoctor;

public class DeleteDoctorHandler : IRequestHandler<DeleteDoctorCommand>
{
    private readonly IDoctorRepository _doctorRepository;

    public DeleteDoctorHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task Handle(DeleteDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId);
        if (doctor is null)
            throw new InvalidOperationException("El médico no existe.");

        doctor.Deactivate();
        await _doctorRepository.UpdateAsync(doctor);
    }
}
