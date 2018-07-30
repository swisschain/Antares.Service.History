using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.History.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public HistorySettings HistoryService { get; set; }
    }
}
