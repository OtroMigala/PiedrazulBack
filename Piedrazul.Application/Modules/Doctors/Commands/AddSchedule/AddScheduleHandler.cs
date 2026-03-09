using MediatR;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Doctors.Commands.AddSchedule;

public class AddScheduleHandler : IRequestHandler<AddScheduleCommand, AddScheduleResult>
{
    private readonly IDoctorRepository _doctorRepository;

    public AddScheduleHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<AddScheduleResult> Handle(
        AddScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdWithSchedulesAsync(request.DoctorId);
        if (doctor is null)
            throw new InvalidOperationException("El médico no existe.");

        var schedule = DoctorSchedule.Create(
            request.DoctorId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime);

        doctor.AddSchedule(schedule);
        await _doctorRepository.AddScheduleAsync(schedule);

        return new AddScheduleResult(
            ScheduleId: schedule.Id,
            DoctorId: doctor.Id,
            DayOfWeek: schedule.DayOfWeek.ToString(),
            StartTime: schedule.StartTime.ToString(@"hh\:mm"),
            EndTime: schedule.EndTime.ToString(@"hh\:mm"));
    }
}
