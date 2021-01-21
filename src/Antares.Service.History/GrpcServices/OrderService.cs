using System;
using System.Linq;
using System.Threading.Tasks;
using Antares.Service.History.Core.Domain.Orders;
using Antares.Service.History.GrpcContract.Common;
using Antares.Service.History.GrpcContract.Orders;
using Antares.Service.History.GrpcServices.Mappers;
using Grpc.Core;

namespace Antares.Service.History.GrpcServices
{
    public class OrderService : Orders.OrdersBase
    {
        private readonly IOrdersRepository _ordersRepository;

        public OrderService(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
        }

        public override async Task<GetOrderListResponse> GetActiveOrders(GetActiveOrdersRequest request, ServerCallContext context)
        {
            var pagination = GrpcMapper.EnsurePagination(request.Pagination);
            var data = await _ordersRepository.GetOrdersAsync(
                Guid.Parse(request.WalletId),
                new[] { Antares.Service.History.Core.Domain.Enums.OrderType.Limit,
                    Antares.Service.History.Core.Domain.Enums.OrderType.StopLimit },
                new[] { Antares.Service.History.Core.Domain.Enums.OrderStatus.Placed,
                    Antares.Service.History.Core.Domain.Enums.OrderStatus.PartiallyMatched,
                    Antares.Service.History.Core.Domain.Enums.OrderStatus.Pending },
                request.AssetPairId,
                pagination.Offset,
                pagination.Limit);

            var items = data.Select(x => GrpcMapper.MapOrder(x)).ToArray();

            return new GetOrderListResponse()
            {
                Items = { items },
                Pagination = new PaginatedInt32Response()
                {
                    Count = items.Count(),
                    Offset = pagination.Offset + items.Length,
                }
            };
        }

        public override async Task<GetOrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
        {
            var order = await _ordersRepository.GetAsync(Guid.Parse(request.Id));

            return new GetOrderResponse()
            {
                Item = GrpcMapper.MapOrder(order)
            };
        }

        public override async Task<GetOrderListResponse> GetOrderList(GetOrderListRequest request, ServerCallContext context)
        {
            var status = request.Status.ToArray();

            if (status.Length == 0)
                status = System.Enum.GetValues(typeof(GrpcContract.Common.OrderStatus)).Cast<GrpcContract.Common.OrderStatus>().ToArray();

            var type = request.Type.ToArray();

            if (type.Length == 0)
                type = System.Enum.GetValues(typeof(GrpcContract.Orders.OrderType)).Cast<GrpcContract.Orders.OrderType>().ToArray();

            var mappedType = type.Select(x => x switch
            {
                GrpcContract.Orders.OrderType.UnknownType => Antares.Service.History.Core.Domain.Enums.OrderType.Unknown,
                GrpcContract.Orders.OrderType.Market => Antares.Service.History.Core.Domain.Enums.OrderType.Market,
                GrpcContract.Orders.OrderType.Limit => Antares.Service.History.Core.Domain.Enums.OrderType.Limit,
                GrpcContract.Orders.OrderType.StopLimit => Antares.Service.History.Core.Domain.Enums.OrderType.StopLimit,
                _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
            }).ToArray();

            var mappedStatus = status.Select(x => x switch
            {
                GrpcContract.Common.OrderStatus.UnknownOrder => Antares.Service.History.Core.Domain.Enums.OrderStatus.Unknown,
                GrpcContract.Common.OrderStatus.Placed => Antares.Service.History.Core.Domain.Enums.OrderStatus.Placed,
                GrpcContract.Common.OrderStatus.PartiallyMatched => Antares.Service.History.Core.Domain.Enums.OrderStatus.PartiallyMatched,
                GrpcContract.Common.OrderStatus.Matched => Antares.Service.History.Core.Domain.Enums.OrderStatus.Matched,
                GrpcContract.Common.OrderStatus.Pending => Antares.Service.History.Core.Domain.Enums.OrderStatus.Pending,
                GrpcContract.Common.OrderStatus.Cancelled => Antares.Service.History.Core.Domain.Enums.OrderStatus.Cancelled,
                GrpcContract.Common.OrderStatus.Replaced => Antares.Service.History.Core.Domain.Enums.OrderStatus.Replaced,
                GrpcContract.Common.OrderStatus.Rejected => Antares.Service.History.Core.Domain.Enums.OrderStatus.Rejected,
                _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
            }).ToArray();

            var pagination = GrpcMapper.EnsurePagination(request.Pagination);

            var data = await _ordersRepository.GetOrdersAsync(
                Guid.Parse(request.WalletId),
                mappedType,
                mappedStatus,
                request.AssetPairId,
                pagination.Offset,
                pagination.Limit);

            var items = data.Select(GrpcMapper.MapOrder).ToArray();

            return new GetOrderListResponse()
            {
                Items = { items },
                Pagination = new PaginatedInt32Response()
                {
                    Count = items.Length,
                    Offset = pagination.Offset + items.Length
                }
            };
        }

        public override async Task<GetOrderListResponse> GetOrdersByDates(GetOrdersByDatesRequest request, ServerCallContext context)
        {
            var pagination = GrpcMapper.EnsurePagination(request.Pagination);
            var data = await _ordersRepository.GetOrdersByDatesAsync(
                request.From.ToDateTime(),
                request.To.ToDateTime(),
                pagination.Offset,
                pagination.Limit);

            var items = data.Select(GrpcMapper.MapOrder).ToArray();

            return new GetOrderListResponse()
            {
                Items = { items },
                Pagination = new PaginatedInt32Response()
                {
                    Count = items.Length,
                    Offset = pagination.Offset + items.Length
                }
            };
        }
    }
}
