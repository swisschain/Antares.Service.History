using JetBrains.Annotations;

namespace Lykke.Service.History.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class HistorySettings
    {
        public DbSettings Db { get; set; }

        public CqrsSettings Cqrs { get; set; }
    }
}
