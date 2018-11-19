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
        /// Orders api interface
        /// </summary>
        IOrdersApi OrdersApi { get; }
        
        /// <summary>
        /// Trades api interface
        /// </summary>
        ITradesApi TradesApi { get; }
    }
}
