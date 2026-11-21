using MowTrackr.Core.Enums;
using MowTrackr.Maintenance.Validators;

namespace MowTrackr.Maintenance.UnitTests.Validators;

public sealed class CheckAnswerValidatorTests
{
    [Theory]
    [InlineData("Yes")]
    [InlineData("no")]
    public void Validate_YesNoItemWithYesOrNo_IsValid(string answer)
    {
        Assert.Null(CheckAnswerValidator.Validate(Item(CheckItemType.YesNo), answer));
    }

    [Fact]
    public void Validate_YesNoItemWithSomethingElse_ReturnsAnError()
    {
        Assert.NotNull(CheckAnswerValidator.Validate(Item(CheckItemType.YesNo), "Maybe"));
    }

    [Fact]
    public void Validate_ChoiceItemWithOneOfTheChoices_IsValidIgnoringCase()
    {
        var item = Item(CheckItemType.Choice, choices: "Full, Half, None");

        Assert.Null(CheckAnswerValidator.Validate(item, "half"));
    }

    [Fact]
    public void Validate_ChoiceItemWithAnUnlistedAnswer_ReturnsAnError()
    {
        var item = Item(CheckItemType.Choice, choices: "Full, Half, None");

        Assert.NotNull(CheckAnswerValidator.Validate(item, "Quarter"));
    }

    [Theory]
    [InlineData("125.5", true)]
    [InlineData("0", true)]
    [InlineData("-3", false)]
    [InlineData("lots", false)]
    public void Validate_EngineHoursItem_AcceptsOnlyNonNegativeNumbers(string answer, bool valid)
    {
        var error = CheckAnswerValidator.Validate(Item(CheckItemType.EngineHours), answer);

        Assert.Equal(valid, error is null);
    }

    [Fact]
    public void Canonical_ChoiceAnswer_UsesTheTemplatesSpelling()
    {
        var item = Item(CheckItemType.Choice, choices: "Full, Half, None");

        Assert.Equal("Half", CheckAnswerValidator.Canonical(item, "HALF"));
    }

    [Fact]
    public void IsFinding_AnswerListedInRepairOn_ReturnsTrue()
    {
        var item = Item(CheckItemType.YesNo, repairOn: "No");

        item.Answer = "No";

        Assert.True(CheckAnswerValidator.IsFinding(item));
    }

    [Fact]
    public void IsFinding_GoodAnswer_ReturnsFalse()
    {
        var item = Item(CheckItemType.YesNo, repairOn: "No");

        item.Answer = "Yes";

        Assert.False(CheckAnswerValidator.IsFinding(item));
    }

    [Fact]
    public void IsFinding_NoAnswerYet_ReturnsFalse()
    {
        Assert.False(CheckAnswerValidator.IsFinding(Item(CheckItemType.YesNo, repairOn: "No")));
    }

    [Fact]
    public void IsFinding_ItemWithoutRepairAnswers_ReturnsFalse()
    {
        var item = Item(CheckItemType.YesNo);

        item.Answer = "No";

        Assert.False(CheckAnswerValidator.IsFinding(item));
    }

    private static WorkOrderTask Item(CheckItemType type, string? choices = null, string? repairOn = null)
    {
        return new WorkOrderTask { Description = "Check", ItemType = type, Choices = choices, RepairOn = repairOn };
    }
}
