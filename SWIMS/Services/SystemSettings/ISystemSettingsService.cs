using System.Threading;
using System.Threading.Tasks;

namespace SWIMS.Services.SystemSettings
{
    public interface ISystemSettingsService
    {
        Task<SystemSettingsOverview> GetOverviewAsync(CancellationToken ct = default);
        Task<SystemSettingsSection> GetSectionAsync(string key, string? environment, CancellationToken ct = default);
        Task SaveSectionAsync(SystemSettingsSection section, string? environment, CancellationToken ct = default);
    }
}
