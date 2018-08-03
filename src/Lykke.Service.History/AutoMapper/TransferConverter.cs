using System;
using System.Collections.Generic;
using AutoMapper;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.Core.Domain;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;

namespace Lykke.Service.History.AutoMapper
{
    public class TransferConverter : ITypeConverter<SaveTransferCommand, IEnumerable<Transfer>>
    {
        public IEnumerable<Transfer> Convert(SaveTransferCommand source, IEnumerable<Transfer> destination, ResolutionContext context)
        {
            yield return new Transfer
            {
                Id = source.Id,
                WalletId = source.FromWalletId,
                Volume = -Math.Abs(source.Volume),
                Timestamp = source.Timestamp,
                AssetId = source.AssetId,
                FeeSize = source.FromWalletId == source.FeeWalletId ? source.FeeSize.GetValueOrDefault() : 0
            };

            yield return new Transfer
            {
                Id = source.Id,
                WalletId = source.ToWalletId,
                Volume = Math.Abs(source.Volume),
                Timestamp = source.Timestamp,
                AssetId = source.AssetId,
                FeeSize = source.ToWalletId == source.FeeWalletId ? source.FeeSize.GetValueOrDefault() : 0
            };
        }
    }
}
