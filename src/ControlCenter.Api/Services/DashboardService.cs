using ControlCenter.Api.Data;
using ControlCenter.Api.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ControlCenter.Api.Services;

public class DashboardService(ApplicationDbContext db) : IDashboardService
{
    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var totalSpaces = await db.Spaces.CountAsync();
        var totalTasks = await db.Tasks.CountAsync();
        var totalDocuments = await db.Documents.CountAsync();

        var tasksByStatus = await db.Tasks
            .AsNoTracking()
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var completedTasks = tasksByStatus.GetValueOrDefault("Done", 0);

        var recentTasks = await db.Tasks
            .AsNoTracking()
            .Include(t => t.Space)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new RecentTaskDto(
                t.Id, t.Title, t.Status.ToString(),
                t.SpaceId, t.Space.Name, t.CreatedAt))
            .ToListAsync();

        var recentDocuments = await db.Documents
            .AsNoTracking()
            .Include(d => d.Space)
            .OrderByDescending(d => d.CreatedAt)
            .Take(5)
            .Select(d => new RecentDocumentDto(
                d.Id, d.Title, d.SpaceId, d.Space.Name, d.CreatedAt))
            .ToListAsync();

        return new DashboardStatsDto(
            totalSpaces,
            totalTasks,
            tasksByStatus,
            completedTasks,
            totalDocuments,
            recentTasks,
            recentDocuments);
    }
}
