using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.History.Core.Domain
{
    public enum HistoryState
    {
        InProgress,
        Finished,
        Canceled,
        Failed
    }
}
