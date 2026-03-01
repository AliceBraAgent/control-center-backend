using ControlCenter.Api.Models.Dtos;

namespace ControlCenter.Api.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
