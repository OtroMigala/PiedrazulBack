using Piedrazul.Domain.Entities;

namespace Piedrazul.Domain.Interfaces;

public interface ISchedulingSettingsRepository
{
    Task<SchedulingSettings?> GetAsync();
    Task SaveAsync(SchedulingSettings settings);
}