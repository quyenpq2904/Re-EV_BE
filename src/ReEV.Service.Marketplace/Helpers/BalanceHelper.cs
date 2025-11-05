using ReEV.Common.Contracts.Users;
using ReEV.Service.Marketplace.Models;
using ReEV.Service.Marketplace.Services;

namespace ReEV.Service.Marketplace.Helpers
{
    public static class BalanceHelper
    {
        /// <summary>
        /// Publish event để đồng bộ Balance và LockedBalance với AuthService
        /// </summary>
        public static async Task PublishBalanceUpdateEventAsync(
            User user,
            RabbitMQPublisher publisher,
            ILogger logger)
        {
            try
            {
                var balanceEvent = new UserBalanceUpdatedV1(
                    user.Id,
                    user.Balance,
                    user.LockedBalance
                );

                await publisher.PublishUserBalanceUpdatedAsync(balanceEvent);
                logger.LogInformation(
                    "User balance update event published successfully. UserId: {UserId}, Balance: {Balance}, LockedBalance: {LockedBalance}",
                    user.Id, user.Balance, user.LockedBalance);
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw để không làm fail request
                // Event sẽ được retry hoặc xử lý sau
                logger.LogError(ex, "Failed to publish user balance update event. UserId: {UserId}", user.Id);
            }
        }
    }
}

