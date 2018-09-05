using JetBrains.Annotations;

namespace Lykke.Service.History.Client
{
    /// <summary>
    ///     History client interface.
    /// </summary>
    [PublicAPI]
    public interface IHistoryClient
    {
        /// <summary>
        /// History api interface (trade \ cashin \ cashout \ transfer \ order event)
        /// </summary>
        IHistoryApi HistoryApi { get; }

        /// <summary>
        /// 
        /// </summary>
        IOrdersApi OrdersApi { get; }
    }
}
