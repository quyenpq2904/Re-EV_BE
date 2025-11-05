namespace ReEV.Common.Contracts.Users
{
    public record UserUpsertedV1(
        Guid UserId,
        string Email,
        string? FullName,
        string? PhoneNumber,
        string? AvatarUrl,
        float Balance = 0,
        float LockedBalance = 0
    );
}
