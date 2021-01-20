using System.Threading.Tasks;
using Antares.Service.History.GrpcContract.Orders;
using Grpc.Core;

namespace Antares.Service.History.GrpcServices
{
    public class OrderService : Orders.OrdersBase
    {
        public OrderService()
        {
            
        }

        public override Task<GetOrderListResponse> GetActiveOrders(GetActiveOrdersRequest request, ServerCallContext context)
        {
            return base.GetActiveOrders(request, context);
        }

        public override Task<GetOrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
        {
            return base.GetOrder(request, context);
        }

        public override Task<GetOrderListResponse> GetOrderList(GetOrderListRequest request, ServerCallContext context)
        {
            return base.GetOrderList(request, context);
        }

        public override Task<GetOrderListResponse> GetOrdersByDates(GetOrdersByDatesRequest request, ServerCallContext context)
        {
            return base.GetOrdersByDates(request, context);
        }
    }
}
