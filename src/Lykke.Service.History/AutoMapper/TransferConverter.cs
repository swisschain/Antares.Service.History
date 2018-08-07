using System;
using System.Collections.Generic;
using AutoMapper;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;

namespace Lykke.Service.History.AutoMapper
{
    public class TransferConverter : ITypeConverter<CashTransferProcessedEvent, IEnumerable<Transfer>>
    {
        public IEnumerable<Transfer> Convert(CashTransferProcessedEvent source, IEnumerable<Transfer> destination, ResolutionContext context)
        {
            yield return new Transfer
            {
                Id = source.Id,
                WalletId = source.FromWalletId,
                Volume = -Math.Abs(source.Volume),
                Timestamp = source.Timestamp,
                AssetId = source.AssetId,
                FeeSize = source.FromWalletId == source.FeeSourceWalletId ? source.FeeSize : null
            };

            yield return new Transfer
            {
                Id = source.Id,
                WalletId = source.ToWalletId,
                Volume = Math.Abs(source.Volume),
                Timestamp = source.Timestamp,
                AssetId = source.AssetId,
                FeeSize = source.ToWalletId == source.FeeSourceWalletId ? source.FeeSize: null
            };
        }
    }
}
