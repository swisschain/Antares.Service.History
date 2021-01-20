using System;
using System.Linq;
using System.Threading.Tasks;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.GrpcContract.Common;
using Antares.Service.History.GrpcContract.History;
using Grpc.Core;
using HistoryType = Antares.Service.History.Core.Domain.Enums.HistoryType;

namespace Antares.Service.History.GrpcServices
{
    public class HistoryService : GrpcContract.History.History.HistoryBase
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;

        public HistoryService(IHistoryRecordsRepository historyRecordsRepository)
        {
            _historyRecordsRepository = historyRecordsRepository;
        }

        public override async Task<HistoryGetHistoryResponse> GetHistory(HistoryGetHistoryRequest request, ServerCallContext context)
        {
            var type = request.Type.ToArray();

            if (type.Length == 0)
                type = Enum.GetValues(typeof(GrpcContract.Common.HistoryType)).Cast<GrpcContract.Common.HistoryType>().ToArray();

            var mappedType = type.Select(x => x switch
            {
                GrpcContract.Common.HistoryType.CashIn => Antares.Service.History.Core.Domain.Enums.HistoryType.CashIn,
                GrpcContract.Common.HistoryType.CashOut => Antares.Service.History.Core.Domain.Enums.HistoryType.CashOut,
                GrpcContract.Common.HistoryType.Trade => Antares.Service.History.Core.Domain.Enums.HistoryType.Trade,
                GrpcContract.Common.HistoryType.OrderEvent => Antares.Service.History.Core.Domain.Enums.HistoryType.OrderEvent,

                _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
            }).ToArray();

            var data = await _historyRecordsRepository.GetByWalletAsync(
                walletId, mappedType,
                offset,
                limit,
                assetPairId,
                assetId,
                from,
                to);

            return new HistoryGetHistoryResponse()
            {
                Items = { },
                Pagination = new PaginatedInt64Response()
                {
                    Count = ,
                    Cursor = ,
                    Order = 
                }
            };
        }

        public override Task<GetHistoryItemResponse> GetHistoryItem(GetHistoryItemRequest request, ServerCallContext context)
        {
            return base.GetHistoryItem(request, context);
        }
    }
}
