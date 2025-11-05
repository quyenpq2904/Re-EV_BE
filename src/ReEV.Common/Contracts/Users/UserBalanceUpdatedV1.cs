namespace ReEV.Common.Contracts.Users
{
    public record UserBalanceUpdatedV1(
        Guid UserId,
        float Balance,
        float LockedBalance
    );
}

