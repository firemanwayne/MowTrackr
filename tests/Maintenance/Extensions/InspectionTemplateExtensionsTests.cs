namespace MowTrackr.Maintenance.UnitTests.Extensions;

public sealed class InspectionTemplateExtensionsTests
{
    private static readonly Guid _chainSaw = Guid.NewGuid();

    private static readonly DateOnly _today = new(2026, 11, 21);

    [Fact]
    public void AppliesTo_HoursReached_ReturnsTrue()
    {
        var template = Template(hours: 500);

        Assert.True(template.AppliesTo(Equipment(hours: 500, purchased: new DateOnly(2026, 1, 1)), _today));
    }

    [Fact]
    public void AppliesTo_HoursNotReached_ReturnsFalse()
    {
        var template = Template(hours: 500);

        Assert.False(template.AppliesTo(Equipment(hours: 499.9m, purchased: new DateOnly(2026, 1, 1)), _today));
    }

    [Fact]
    public void AppliesTo_AgeReached_ReturnsTrue()
    {
        var template = Template(months: 36);

        Assert.True(template.AppliesTo(Equipment(hours: 0, purchased: new DateOnly(2023, 11, 21)), _today));
    }

    [Fact]
    public void AppliesTo_AgeNotReachedByADay_ReturnsFalse()
    {
        var template = Template(months: 36);

        Assert.False(template.AppliesTo(Equipment(hours: 0, purchased: new DateOnly(2023, 11, 22)), _today));
    }

    [Fact]
    public void AppliesTo_HoursAndAgeSet_EitherOneIsEnough()
    {
        var template = Template(hours: 500, months: 36);

        Assert.True(template.AppliesTo(Equipment(hours: 600, purchased: new DateOnly(2026, 1, 1)), _today));

        Assert.True(template.AppliesTo(Equipment(hours: 10, purchased: new DateOnly(2020, 1, 1)), _today));

        Assert.False(template.AppliesTo(Equipment(hours: 10, purchased: new DateOnly(2026, 1, 1)), _today));
    }

    [Fact]
    public void AppliesTo_OtherAssetType_ReturnsFalse()
    {
        var template = Template(hours: 1);

        var mower = Equipment(hours: 1000, purchased: new DateOnly(2010, 1, 1));

        mower.AssetTypeId = Guid.NewGuid();

        Assert.False(template.AppliesTo(mower, _today));
    }

    [Fact]
    public void AppliesTo_InactiveTemplate_ReturnsFalse()
    {
        var template = Template(hours: 1);

        template.IsActive = false;

        Assert.False(template.AppliesTo(Equipment(hours: 1000, purchased: new DateOnly(2010, 1, 1)), _today));
    }

    [Theory]
    [InlineData("2026-11-21", "2026-11-21", 0)]
    [InlineData("2026-10-21", "2026-11-21", 1)]
    [InlineData("2026-10-22", "2026-11-21", 0)]
    [InlineData("2025-11-21", "2026-11-21", 12)]
    [InlineData("2026-12-01", "2026-11-21", 0)]
    public void AgeInMonths_CountsWholeMonths(string from, string to, int expected)
    {
        Assert.Equal(expected, InspectionTemplateExtensions.AgeInMonths(DateOnly.Parse(from), DateOnly.Parse(to)));
    }

    [Fact]
    public void DisplayName_WithAssetType_PutsTheTypeFirst()
    {
        var template = Template(hours: 500);

        template.AssetType = new AssetType { Name = "Chain Saw" };

        Assert.Equal("Chain Saw 500 Hour Inspection", template.DisplayName());
    }

    private static InspectionTemplate Template(decimal? hours = null, int? months = null)
    {
        return new InspectionTemplate { Name = "500 Hour Inspection", AssetTypeId = _chainSaw, MinEngineHours = hours, MinAgeMonths = months };
    }

    private static Mower Equipment(decimal hours, DateOnly purchased)
    {
        return new Mower { Name = "Saw 1", AssetTypeId = _chainSaw, EngineHours = hours, PurchaseDate = purchased };
    }
}
