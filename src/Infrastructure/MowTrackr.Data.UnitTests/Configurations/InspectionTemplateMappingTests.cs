using MowTrackr.Maintenance.Models;

namespace MowTrackr.Data.UnitTests.Configurations;

public sealed class InspectionTemplateMappingTests
{
    [Fact]
    public void Timing_HasADatabaseDefault_SoItNeedsASentinelThatIsNotAnEnumValue()
    {
        // Before is the first enum value. Without a sentinel EF reads it as "not set" and saves the database default
        // (After) instead, which silently turned every pre-check into a post-check.
        using var context = CreateContext();

        var property = context.Model
            .FindEntityType(typeof(InspectionTemplate))!
            .FindProperty(nameof(InspectionTemplate.Timing))!;

        Assert.NotNull(property.GetDefaultValue());

        Assert.False(Enum.IsDefined(typeof(Core.Enums.InspectionTiming), property.Sentinel!));
    }

    [Fact]
    public void EnumsWithADatabaseDefault_EitherDefaultToTheirFirstValueOrHaveASentinel()
    {
        using var context = CreateContext();

        var mismatched = context.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType.IsEnum && p.GetDefaultValue() is not null)
            .Where(p => !Equals(p.GetDefaultValue(), Activator.CreateInstance(p.ClrType)) && Equals(p.Sentinel, Activator.CreateInstance(p.ClrType)))
            .Select(p => $"{p.DeclaringType.DisplayName()}.{p.Name}")
            .ToList();

        Assert.Empty(mismatched);
    }

    private static MowTrackrDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MowTrackrDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MowTrackrDbContext(options);
    }
}
