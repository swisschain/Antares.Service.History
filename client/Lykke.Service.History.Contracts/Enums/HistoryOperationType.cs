using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.History.Contracts.Enums
{
    /// <summary>
    /// Operation type
    /// </summary>
    public enum HistoryOperationType
    {
        None,
        Card,
        Bank,
        Crypto,
        Hft,
        Forward
    }
}
