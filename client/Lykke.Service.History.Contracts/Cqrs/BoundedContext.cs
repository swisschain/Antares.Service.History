using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.History.Contracts.Cqrs
{
    /// <summary>
    /// History service bounded context
    /// </summary>
    public static class BoundedContext
    {
        /// <summary>
        /// Context name
        /// </summary>
        public static readonly string Name = "history";
    }
}
