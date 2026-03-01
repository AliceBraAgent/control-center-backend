using ControlCenter.Api.Data;
using ControlCenter.Api.Models.Dtos;
using ControlCenter.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControlCenter.Api.Services;

public class TaskService(ApplicationDbContext db) : ITaskService
{
    public async Task<IEnumerable<TaskDto>> GetAllAsync()
    {
        return await db.Tasks
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => ToDto(t))
            .ToListAsync();
    }

    public async Task<TaskDto?> GetByIdAsync(Guid id)
    {
        var task = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        return task is null ? null : ToDto(task);
    }

    public async Task<IEnumerable<TaskDto>> GetBySpaceIdAsync(Guid spaceId)
    {
        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.SpaceId == spaceId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => ToDto(t))
            .ToListAsync();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            SpaceId = request.SpaceId,
            AssignedToId = request.AssignedToId
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        return ToDto(task);
    }

    public async Task<TaskDto?> UpdateAsync(Guid id, UpdateTaskRequest request)
    {
        var task = await db.Tasks.FindAsync(id);
        if (task is null) return null;

        task.Title = request.Title;
        task.Description = request.Description;
        task.AssignedToId = request.AssignedToId;

        if (Enum.TryParse<TaskItemStatus>(request.Status, ignoreCase: true, out var status))
            task.Status = status;

        await db.SaveChangesAsync();
        return ToDto(task);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await db.Tasks.FindAsync(id);
        if (task is null) return false;

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return true;
    }

    private static TaskDto ToDto(TaskItem t) =>
        new(t.Id, t.Title, t.Description, t.Status.ToString(), t.SpaceId, t.AssignedToId, t.CreatedAt, t.UpdatedAt);
}
