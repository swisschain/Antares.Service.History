using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.History.Contracts.Cqrs
{
    /// <summary>
    /// History service bounded context
    /// </summary>
    public static class HistoryBoundedContext
    {
        /// <summary>
        /// Bounded context name
        /// </summary>
        public static string Name = "history";
    }
}
