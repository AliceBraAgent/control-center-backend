using ControlCenter.Api.Data;
using ControlCenter.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DevController(ApplicationDbContext db, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("seed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Seed()
    {
        if (!env.IsDevelopment())
            return BadRequest(new { error = "Seeding is only available in the Development environment." });

        if (await db.Spaces.AnyAsync())
            return Conflict(new { error = "Data already exists. Delete existing data before re-seeding." });

        // -- Users --
        var users = new[]
        {
            new User { FirstName = "Max", LastName = "Müller", Email = "m.mueller@brabender.com" },
            new User { FirstName = "Anna", LastName = "Schmidt", Email = "a.schmidt@brabender.com" },
            new User { FirstName = "Thomas", LastName = "Weber", Email = "t.weber@brabender.com" },
        };
        db.Users.AddRange(users);

        // -- Root spaces --
        var brabender = new Space { Name = "BRABENDER", Description = "BRABENDER Group – company-wide space" };
        db.Spaces.Add(brabender);
        await db.SaveChangesAsync(); // persist so FKs resolve

        var instruments = new Space { Name = "Instruments", Description = "Product line: measurement instruments", ParentSpaceId = brabender.Id };
        var software = new Space { Name = "Software", Description = "Software products & digital services", ParentSpaceId = brabender.Id };
        var operations = new Space { Name = "Operations", Description = "Internal operations & processes", ParentSpaceId = brabender.Id };
        db.Spaces.AddRange(instruments, software, operations);
        await db.SaveChangesAsync();

        // -- Level 3 spaces --
        var rheometers = new Space { Name = "Rheometers", Description = "Torque rheometers & accessories", ParentSpaceId = instruments.Id };
        var extruders = new Space { Name = "Extruders", Description = "Lab & pilot-scale extruders", ParentSpaceId = instruments.Id };
        var controlCenter = new Space { Name = "Control Center", Description = "BRABENDER Control Center web platform", ParentSpaceId = software.Id };
        var metabridge = new Space { Name = "MetaBridge", Description = "Data integration & analytics platform", ParentSpaceId = software.Id };
        var hr = new Space { Name = "HR", Description = "Human resources", ParentSpaceId = operations.Id };
        db.Spaces.AddRange(rheometers, extruders, controlCenter, metabridge, hr);
        await db.SaveChangesAsync();

        // -- Cross-references --
        db.SpaceRelations.AddRange(
            new SpaceRelation { SourceSpaceId = controlCenter.Id, TargetSpaceId = metabridge.Id, RelationType = "integrates_with" },
            new SpaceRelation { SourceSpaceId = rheometers.Id, TargetSpaceId = metabridge.Id, RelationType = "data_source" }
        );

        // -- Documents --
        db.Documents.AddRange(
            new Document { SpaceId = brabender.Id, Title = "Company Overview", Slug = "company-overview", Content = "BRABENDER is a leading manufacturer of laboratory and industrial measuring and processing instruments.", DocumentType = "wiki", SortOrder = 0 },
            new Document { SpaceId = controlCenter.Id, Title = "Architecture Decision Record", Slug = "architecture-decision-record", Content = "## ADR-001: .NET 8 Web API\n\nWe chose .NET 8 for the backend because of performance, ecosystem maturity, and team expertise.", DocumentType = "adr", SortOrder = 0 },
            new Document { SpaceId = controlCenter.Id, Title = "API Design Guidelines", Slug = "api-design-guidelines", Content = "All endpoints follow RESTful conventions. Resources are plural nouns. Use kebab-case for multi-word URL segments.", DocumentType = "guideline", SortOrder = 1 },
            new Document { SpaceId = metabridge.Id, Title = "Integration Spec", Slug = "integration-spec", Content = "MetaBridge exposes a gRPC API for real-time instrument data streaming.", DocumentType = "spec", SortOrder = 0 }
        );

        // -- Tasks --
        db.Tasks.AddRange(
            new TaskItem { Title = "Set up CI/CD pipeline", Description = "Configure GitHub Actions for build, test, deploy", Status = TaskItemStatus.InProgress, SpaceId = controlCenter.Id, AssignedToId = users[0].Id },
            new TaskItem { Title = "Design dashboard wireframes", Description = "Create Figma wireframes for the main dashboard view", Status = TaskItemStatus.Concept, SpaceId = controlCenter.Id, AssignedToId = users[1].Id },
            new TaskItem { Title = "Evaluate gRPC vs REST for MetaBridge", Description = "Compare performance and developer experience", Status = TaskItemStatus.Refinement, SpaceId = metabridge.Id },
            new TaskItem { Title = "Onboarding process documentation", Description = "Document the new-hire onboarding flow", Status = TaskItemStatus.Idea, SpaceId = hr.Id, AssignedToId = users[2].Id },
            new TaskItem { Title = "Rheometer data export format", Description = "Define CSV/JSON export schema for measurement results", Status = TaskItemStatus.Idea, SpaceId = rheometers.Id }
        );

        await db.SaveChangesAsync();

        return Ok(new
        {
            message = "Seed data created successfully.",
            spaces = await db.Spaces.CountAsync(),
            documents = await db.Documents.CountAsync(),
            tasks = await db.Tasks.CountAsync(),
            users = await db.Users.CountAsync()
        });
    }
}
