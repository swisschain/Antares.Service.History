using JetBrains.Annotations;

namespace Lykke.Service.History.Client
{
    /// <summary>
    ///     History client interface.
    /// </summary>
    [PublicAPI]
    public interface IHistoryClient
    {
        /// <summary>Application Api interface</summary>
        IHistoryApi Api { get; }
    }
}
