using Microsoft.EntityFrameworkCore;
using RoutingV3.Domain.Enums;
using RoutingV3.Domain.Models;

namespace RoutingV3.Infrastructure;

public class RoutingDbContext : DbContext
{
    public RoutingDbContext(DbContextOptions<RoutingDbContext> options) : base(options) { }

    public DbSet<Hub> Hubs => Set<Hub>();
    public DbSet<PostalCodeArea> PostalCodeAreas => Set<PostalCodeArea>();
    public DbSet<Line> Lines => Set<Line>();
    public DbSet<Mandate> Mandates => Set<Mandate>();
    public DbSet<LineMandate> LineMandates => Set<LineMandate>();
    public DbSet<ScheduleRule> ScheduleRules => Set<ScheduleRule>();
    public DbSet<ScheduleExecution> ScheduleExecutions => Set<ScheduleExecution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hub>(e =>
        {
            e.HasIndex(h => h.Code).IsUnique();
        });

        modelBuilder.Entity<PostalCodeArea>(e =>
        {
            e.HasIndex(p => new { p.Country, p.Pattern });
        });

        modelBuilder.Entity<Mandate>(e =>
        {
            e.HasIndex(m => m.Code).IsUnique();
        });

        modelBuilder.Entity<Line>(e =>
        {
            e.HasOne(l => l.OriginHub)
                .WithMany(h => h.OriginLines)
                .HasForeignKey(l => l.OriginHubId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(l => l.DestinationHub)
                .WithMany(h => h.DestinationLines)
                .HasForeignKey(l => l.DestinationHubId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(l => l.OriginPostalCodeArea)
                .WithMany(p => p.OriginLines)
                .HasForeignKey(l => l.OriginPostalCodeAreaId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(l => l.DestinationPostalCodeArea)
                .WithMany(p => p.DestinationLines)
                .HasForeignKey(l => l.DestinationPostalCodeAreaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<LineMandate>(e =>
        {
            e.HasKey(lm => new { lm.LineId, lm.MandateId });

            e.HasOne(lm => lm.Line)
                .WithMany(l => l.LineMandates)
                .HasForeignKey(lm => lm.LineId);

            e.HasOne(lm => lm.Mandate)
                .WithMany(m => m.LinesMandates)
                .HasForeignKey(lm => lm.MandateId);
        });

        modelBuilder.Entity<ScheduleRule>(e =>
        {
            e.HasOne(s => s.Line)
                .WithMany(l => l.ScheduleRules)
                .HasForeignKey(s => s.LineId);
        });

        modelBuilder.Entity<ScheduleExecution>(e =>
        {
            e.HasOne(s => s.Line)
                .WithMany(l => l.ScheduleExecutions)
                .HasForeignKey(s => s.LineId);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Hubs
        modelBuilder.Entity<Hub>().HasData(
            new Hub { Id = 1, Code = "AT-LNZ", Name = "Linz", Country = "AT" },
            new Hub { Id = 2, Code = "AT-WND", Name = "Wiener Neudorf", Country = "AT" },
            new Hub { Id = 3, Code = "DE.AUR", Name = "Aurich", Country = "DE" }
        );

        // Postal Code Areas
        modelBuilder.Entity<PostalCodeArea>().HasData(
            new PostalCodeArea { Id = 1, Country = "AT", Pattern = "4.*" },
            new PostalCodeArea { Id = 2, Country = "AT", Pattern = "1.*" },
            new PostalCodeArea { Id = 3, Country = "DE", Pattern = "4.*", SubHubCode = "DE-41379" },
            new PostalCodeArea { Id = 4, Country = "DE", Pattern = "1.*", SubHubCode = "DE-10115" }
        );

        // Mandates
        modelBuilder.Entity<Mandate>().HasData(
            new Mandate { Id = 1, Code = "AT-LNZ", Name = "Linz" },
            new Mandate { Id = 2, Code = "AT-WND", Name = "Wiener Neudorf" },
            new Mandate { Id = 3, Code = "AT-SEI", Name = "Seiersberg" },
            new Mandate { Id = 4, Code = "AT-GRZ", Name = "Graz" },
            new Mandate { Id = 5, Code = "AT-KLG", Name = "Klagenfurt" },
            new Mandate { Id = 6, Code = "AT-IBK", Name = "Innsbruck" }
        );

        // Lines from BRAINDUMP.md
        // Line 1: NV AT.LNZ Collection (AT-4* -> AT-LNZ)
        modelBuilder.Entity<Line>().HasData(new Line
        {
            Id = 1,
            Code = "NV AT.LNZ-C",
            Type = LegType.Collection,
            OriginPostalCodeAreaId = 1,  // AT-4*
            DestinationHubId = 1,        // AT-LNZ
            AttributesJson = "[\"ADR\"]",
            Department = "Rollfuhr",
            PricingRef = "ILV-Pricing",
            PricingIncludedInDelivery = false
        });

        // Line 2: NV AT.LNZ Delivery (AT-LNZ -> AT-4*)
        modelBuilder.Entity<Line>().HasData(new Line
        {
            Id = 2,
            Code = "NV AT.LNZ-D",
            Type = LegType.Delivery,
            OriginHubId = 1,             // AT-LNZ
            DestinationPostalCodeAreaId = 1, // AT-4*
            AttributesJson = "[\"ADR\"]",
            Department = "Rollfuhr",
            PricingRef = "ILV-Pricing",
            PricingIncludedInDelivery = false
        });

        // Line 3: NV AT.WND Collection (AT-1* -> AT-WND)
        modelBuilder.Entity<Line>().HasData(new Line
        {
            Id = 3,
            Code = "NV AT.WND-C",
            Type = LegType.Collection,
            OriginPostalCodeAreaId = 2,  // AT-1*
            DestinationHubId = 2,        // AT-WND
            AttributesJson = "[\"ADR\"]",
            Department = "Rollfuhr",
            PricingRef = "ILV-Pricing",
            PricingIncludedInDelivery = false
        });

        // Line 4: NV AT.WND Delivery (AT-WND -> AT-1*)
        modelBuilder.Entity<Line>().HasData(new Line
        {
            Id = 4,
            Code = "NV AT.WND-D",
            Type = LegType.Delivery,
            OriginHubId = 2,             // AT-WND
            DestinationPostalCodeAreaId = 2, // AT-1*
            AttributesJson = "[\"ADR\"]",
            Department = "Rollfuhr",
            PricingRef = "ILV-Pricing",
            PricingIncludedInDelivery = false
        });

        // Line 5: AT.LNZ-AT.WND Linehaul
        modelBuilder.Entity<Line>().HasData(new Line
        {
            Id = 5,
            Code = "AT.LNZ-AT.WND",
            Type = LegType.Linehaul,
            OriginHubId = 1,     // AT-LNZ
            DestinationHubId = 2, // AT-WND
            AttributesJson = "[\"ADR\"]",
            Department = "EAS",
            PricingRef = "ILV-Pricing",
            PricingIncludedInDelivery = false
        });

        // Line 6: AT.WND-AT.LNZ Linehaul
        modelBuilder.Entity<Line>().HasData(new Line
        {
            Id = 6,
            Code = "AT.WND-AT.LNZ",
            Type = LegType.Linehaul,
            OriginHubId = 2,     // AT-WND
            DestinationHubId = 1, // AT-LNZ
            AttributesJson = "[\"ADR\"]",
            Department = "EAS",
            PricingRef = "ILV-Pricing",
            PricingIncludedInDelivery = false
        });

        // Line 7: AT.LNZ-DE.AUR Linehaul
        modelBuilder.Entity<Line>().HasData(new Line
        {
            Id = 7,
            Code = "AT.LNZ-DE.AUR",
            Type = LegType.Linehaul,
            OriginHubId = 1,     // AT-LNZ
            DestinationHubId = 3, // DE.AUR
            AttributesJson = "[\"ADR\"]",
            Department = "EAS",
            Partner = "CTL",
            PricingRef = "AT.LNZ-DE.AUR-LH",
            PricingIncludedInDelivery = false
        });

        // Line 8: DE.AUR -> DE-4* Delivery
        modelBuilder.Entity<Line>().HasData(new Line
        {
            Id = 8,
            Code = "DE.AUR-DE4-D",
            Type = LegType.Delivery,
            OriginHubId = 3,             // DE.AUR
            DestinationPostalCodeAreaId = 3, // DE-4*
            AttributesJson = "[\"ADR\"]",
            Department = "EAS",
            Partner = "CTL",
            PricingRef = "DE.AUR-DE-Delivery",
            PricingIncludedInDelivery = false
        });

        // Line 9: DE.AUR -> DE-1* Delivery
        modelBuilder.Entity<Line>().HasData(new Line
        {
            Id = 9,
            Code = "DE.AUR-DE1-D",
            Type = LegType.Delivery,
            OriginHubId = 3,             // DE.AUR
            DestinationPostalCodeAreaId = 4, // DE-1*
            AttributesJson = "[\"ADR\"]",
            Department = "EAS",
            Partner = "CTL",
            PricingRef = "DE.AUR-DE-Delivery",
            PricingIncludedInDelivery = false
        });

        // Seed LineMandates
        var lineMandates = new List<object>();
        for (var lineId = 1; lineId <= 9; lineId++)
        {
            for (var mandateId = 1; mandateId <= 6; mandateId++)
            {
                lineMandates.Add(new { LineId = lineId, MandateId = mandateId });
            }
        }
        modelBuilder.Entity<LineMandate>().HasData(lineMandates.ToArray());

        // Schedule Rules
        var weekdays = string.Join(",", new[] { 1, 2, 3, 4, 5 }); // Mon-Fri

        // Collection lines: Mon-Fri 08:00-17:00 collection, arrival 18:00
        modelBuilder.Entity<ScheduleRule>().HasData(
            new ScheduleRule { Id = 1, LineId = 1, DaysOfWeek = weekdays, DepartureTime = new TimeOnly(8, 0), ArrivalTime = new TimeOnly(18, 0), ArrivalDayOffset = 0 },
            new ScheduleRule { Id = 2, LineId = 2, DaysOfWeek = weekdays, DepartureTime = new TimeOnly(6, 0), ArrivalTime = new TimeOnly(17, 0), ArrivalDayOffset = 0 },
            new ScheduleRule { Id = 3, LineId = 3, DaysOfWeek = weekdays, DepartureTime = new TimeOnly(8, 0), ArrivalTime = new TimeOnly(18, 0), ArrivalDayOffset = 0 },
            new ScheduleRule { Id = 4, LineId = 4, DaysOfWeek = weekdays, DepartureTime = new TimeOnly(6, 0), ArrivalTime = new TimeOnly(17, 0), ArrivalDayOffset = 0 },
            // Linehaul AT.LNZ-AT.WND: Mon-Fri 18:00 dep, 04:00+1 arr
            new ScheduleRule { Id = 5, LineId = 5, DaysOfWeek = weekdays, DepartureTime = new TimeOnly(18, 0), ArrivalTime = new TimeOnly(4, 0), ArrivalDayOffset = 1 },
            // Linehaul AT.WND-AT.LNZ: Mon-Fri 18:00 dep, 04:00+1 arr
            new ScheduleRule { Id = 6, LineId = 6, DaysOfWeek = weekdays, DepartureTime = new TimeOnly(18, 0), ArrivalTime = new TimeOnly(4, 0), ArrivalDayOffset = 1 },
            // Linehaul AT.LNZ-DE.AUR: Mon-Fri 18:00 dep, 01:00+1 arr
            new ScheduleRule { Id = 7, LineId = 7, DaysOfWeek = weekdays, DepartureTime = new TimeOnly(18, 0), ArrivalTime = new TimeOnly(1, 0), ArrivalDayOffset = 1 },
            // Delivery DE.AUR->DE-4*: Mon-Fri 01:00 dep, 08:00-17:00 arr
            new ScheduleRule { Id = 8, LineId = 8, DaysOfWeek = weekdays, DepartureTime = new TimeOnly(1, 0), ArrivalTime = new TimeOnly(17, 0), ArrivalDayOffset = 0 },
            // Delivery DE.AUR->DE-1*: Mon-Fri 01:00 dep, 08:00-17:00 arr
            new ScheduleRule { Id = 9, LineId = 9, DaysOfWeek = weekdays, DepartureTime = new TimeOnly(1, 0), ArrivalTime = new TimeOnly(17, 0), ArrivalDayOffset = 0 }
        );
    }
}
