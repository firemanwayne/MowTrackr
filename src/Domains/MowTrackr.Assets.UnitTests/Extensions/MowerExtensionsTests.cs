using MowTrackr.Assets.Extensions;

namespace MowTrackr.Assets.UnitTests.Extensions;

public sealed class MowerExtensionsTests
{
    [Fact]
    public void StatusFor_ActiveWithNoOpenRepair_IsAvailable()
    {
        Assert.Equal(AssetStatus.Available, Mower(isActive: true).StatusFor(needsRepair: false));
    }

    [Fact]
    public void StatusFor_ActiveWithAnOpenRepair_NeedsRepair()
    {
        Assert.Equal(AssetStatus.NeedsRepair, Mower(isActive: true).StatusFor(needsRepair: true));
    }

    [Fact]
    public void StatusFor_RetiredUnit_IsRetiredEvenWithAnOpenRepair()
    {
        Assert.Equal(AssetStatus.Retired, Mower(isActive: false).StatusFor(needsRepair: true));
    }

    [Fact]
    public void ToResponse_UnitWithAnOpenRepair_CarriesTheNeedsRepairStatus()
    {
        var response = Mower(isActive: true).ToResponse(needsRepair: true);

        Assert.Equal(AssetStatus.NeedsRepair, response.Status);
    }

    private static Mower Mower(bool isActive)
    {
        return new Mower { Name = "Mower 1", IsActive = isActive };
    }
}
