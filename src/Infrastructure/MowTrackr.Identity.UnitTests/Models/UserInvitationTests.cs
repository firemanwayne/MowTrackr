namespace MowTrackr.Identity.UnitTests.Models;

public sealed class UserInvitationTests
{
    private static readonly DateTimeOffset Now = new(2027, 1, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void GetStatus_BeforeExpiry_IsPending()
    {
        var invitation = CreateInvitation();

        Assert.Equal(InvitationStatus.Pending, invitation.GetStatus(Now));
    }

    [Fact]
    public void GetStatus_AtExpiry_IsExpired()
    {
        var invitation = CreateInvitation();

        Assert.Equal(InvitationStatus.Expired, invitation.GetStatus(invitation.ExpiresAt));
    }

    [Fact]
    public void GetStatus_AfterAcceptance_IsAcceptedEvenWhenLaterExpired()
    {
        var invitation = CreateInvitation();

        invitation.AcceptedAt = Now.AddDays(1);

        Assert.Equal(InvitationStatus.Accepted, invitation.GetStatus(Now.AddDays(30)));
    }

    [Fact]
    public void GetStatus_AfterRevocation_IsRevoked()
    {
        var invitation = CreateInvitation();

        invitation.RevokedAt = Now.AddHours(1);

        Assert.Equal(InvitationStatus.Revoked, invitation.GetStatus(Now.AddHours(2)));
    }

    private static UserInvitation CreateInvitation()
    {
        return new UserInvitation { Email = "new@example.com", ExpiresAt = Now.AddDays(7) };
    }
}