using Lykke.HttpClientGenerator;

namespace Lykke.Service.History.Client
{
    /// <summary>
    ///     History API aggregating interface.
    /// </summary>
    public class HistoryClient : IHistoryClient
    {
        /// <summary>C-tor</summary>
        public HistoryClient(IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<IHistoryApi>();
        }
        // Note: Add similar Api properties for each new service controller

        /// <summary>Inerface to History Api.</summary>
        public IHistoryApi Api { get; private set; }
    }
}
