using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.History.Core.Domain.Enums;

namespace Lykke.Service.History.Core.Domain.Operations
{
    public interface IOperationsRepository
    {
        Task<Operation> Get(Guid id);

        Task<bool> TryInsertAsync(Operation operation);
    }
}
