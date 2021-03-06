﻿using System;
using Antares.Job.History.RabbitSubscribers.Models.Enums;
using ProtoBuf;

namespace Antares.Job.History.RabbitSubscribers.Models
{
    /// <summary>
    /// User trade item
    /// </summary>
    [ProtoContract]
    public class TradeModel
    {
        [ProtoMember(1, IsRequired = true)]
        public Guid Id { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public Guid WalletId { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public string AssetPairId { get; set; }

        [ProtoMember(4, IsRequired = true)]
        public string BaseAssetId { get; set; }

        [ProtoMember(5, IsRequired = true)]
        public decimal BaseVolume { get; set; }

        [ProtoMember(6, IsRequired = true)]
        public decimal Price { get; set; }

        [ProtoMember(7, IsRequired = true)]
        public DateTime Timestamp { get; set; }

        [ProtoMember(8, IsRequired = true)]
        public string QuotingAssetId { get; set; }

        [ProtoMember(9, IsRequired = true)]
        public decimal QuotingVolume { get; set; }

        [ProtoMember(10, IsRequired = true)]
        public int Index { get; set; }

        [ProtoMember(11, IsRequired = true)]
        public TradeRole Role { get; set; }

        [ProtoMember(12, IsRequired = false)]
        public decimal? FeeSize { get; set; }

        [ProtoMember(13, IsRequired = false)]
        public string FeeAssetId { get; set; }

        [ProtoMember(14, IsRequired = true)]
        public Guid OppositeWalletId { get; set; }
    }
}
