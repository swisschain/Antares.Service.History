using System.Threading.Tasks;
using Antares.Service.History.GrpcContract.Trades;
using Grpc.Core;
using GetTradesResponse = Antares.Service.History.GrpcContract.Trades.GetTradesResponse;

namespace Antares.Service.History.GrpcServices
{
    public class TradesService : Trades.TradesBase
    {
        public TradesService()
        {
            
        }

        public override Task<GetTradesResponse> GetTrades(GetTradesRequest request, ServerCallContext context)
        {
            return base.GetTrades(request, context);
        }

        public override Task<GetTradesResponse> GetTradesByDates(GetTradesByDatesRequest request, ServerCallContext context)
        {
            return base.GetTradesByDates(request, context);
        }

        public override Task<GetTradesResponse> GetTradesByOrderId(GetTradesByOrderIdRequest request, ServerCallContext context)
        {
            return base.GetTradesByOrderId(request, context);
        }
    }
}
