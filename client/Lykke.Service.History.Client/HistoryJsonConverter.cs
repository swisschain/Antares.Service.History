using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.History.Contracts;
using Lykke.Service.History.Contracts.Enums;
using Lykke.Service.History.Contracts.History;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.History.Client
{
    /// <inheritdoc />
    public class HistoryJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return typeof(BaseHistoryModel).IsAssignableFrom(objectType);
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var target = Create(jsonObject);
            if (target == null)
                return null;
            serializer.Populate(jsonObject.CreateReader(), target);
            return target;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private static BaseHistoryModel Create(JObject jsonObject)
        {
            if (Enum.TryParse<HistoryType>(jsonObject["Type"].ToString(), out var type))
            {
                switch (type)
                {
                    case HistoryType.CashIn:
                        return new CashinModel();
                    case HistoryType.CashOut:
                        return new CashoutModel();
                    case HistoryType.Transfer:
                        return new TransferModel();
                    case HistoryType.Trade:
                        return new TradeModel();
                    case HistoryType.OrderEvent:
                        return new OrderEventModel();
                }
            }

            return null;
        }
    }
}
