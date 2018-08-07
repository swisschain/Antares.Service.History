using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using MoreLinq;

namespace Lykke.Service.History.Workflow.Projections
{
    public class ExecutionProjection
    {
        private const int BulkSize = 5000;

        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly IOrdersRepository _ordersRepository;

        private readonly ConcurrentBag<KeyValuePair<List<Order>, TaskCompletionSource<bool>>> _bag;

        public ExecutionProjection(IHistoryRecordsRepository historyRecordsRepository, IOrdersRepository ordersRepository)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _ordersRepository = ordersRepository;
            _bag = new ConcurrentBag<KeyValuePair<List<Order>, TaskCompletionSource<bool>>>();
        }

        //public async Task<CommandHandlingResult> Handle(ExecutionProcessedEvent @event)
        //{
        //    var orders = Mapper.Map<IEnumerable<Order>>(@event).ToList();

        //    if (!orders.Any())
        //        return CommandHandlingResult.Ok();

        //    var semaphore = new SemaphoreSlim(1, 1);

        //    _bag.Add(KeyValuePair.Create(orders, semaphore));

        //    await semaphore.WaitAsync(10000);

        //    return CommandHandlingResult.Ok();
        //}

        //public Task<bool> Test(object msg)
        //{
        //    var tcs = new TaskCompletionSource<bool>();
        //    tcs.SetResult(true);
        //    _bag.Add(KeyValuePair.Create(msg, tcs));

        //    return tcs.Task;
        //}

        //public async Task Start()
        //{
        //    while (true)
        //    {
        //        var list = new List<KeyValuePair<List<Order>, SemaphoreSlim>>();

        //        try
        //        {
        //            for (var i = 0; i < 100; i++)
        //            {
        //                if (_bag.TryTake(out var keyValuePair))
        //                    list.Add(keyValuePair);
        //            }

        //            var orders = list.SelectMany(x => x.Key).ToList();

        //            await _ordersRepository.UpsertBulkAsync(orders);

        //            var trades = orders.SelectMany(x => x.Trades);

        //            var batched = trades.Batch(BulkSize).ToList();

        //            foreach (var tradesBatch in batched)
        //            {
        //                await _historyRecordsRepository.TryInsertBulkAsync(tradesBatch);
        //            }
        //        }
        //        finally
        //        {
        //            foreach (var item in list)
        //                item.Value.Release();
        //        }

        //        await Task.Delay(1000);
        //    }
        //}
    }
}
