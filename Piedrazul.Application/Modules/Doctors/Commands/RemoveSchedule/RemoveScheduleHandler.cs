using MediatR;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Doctors.Commands.RemoveSchedule;

public class RemoveScheduleHandler : IRequestHandler<RemoveScheduleCommand>
{
    private readonly IDoctorRepository _doctorRepository;

    public RemoveScheduleHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task Handle(RemoveScheduleCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdWithSchedulesAsync(request.DoctorId);
        if (doctor is null)
            throw new InvalidOperationException("El médico no existe.");

        doctor.RemoveSchedule(request.ScheduleId);
        await _doctorRepository.RemoveScheduleAsync(request.ScheduleId);
    }
}
