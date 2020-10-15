using Newtonsoft.Json.Converters;
using QuantConnect.Orders;
using Newtonsoft.Json;


namespace QuantConnect.Scheduling
{
    /// <summary>
    ///  Data transfer object used for serializing an <see cref="ScheduledEventParams"/>
    /// </summary>
    public class SerializedScheduledEventParams
    {
        /// <summary>
        /// Gets an identifier for this event
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the utc expiry date of the order
        /// </summary>
        [JsonProperty("expiry_time", NullValueHandling = NullValueHandling.Ignore)]
        public double ExpiryTime { get; private set; }

        /// <summary>
        /// Gets order tag
        /// </summary>
        [JsonProperty("tag")]
        public string Tag { get; private set; }

        /// <summary>
        /// Gets order ticker
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; private set; }

        /// <summary>
        /// Gets order size
        /// </summary>
        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public decimal Quantity { get; private set; }


        /// <summary>
        /// Gets order reason
        /// </summary>
        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter), true)]
        public OrderReason OrderReason { get; private set; }


        /// <summary>
        /// Empty constructor required for JSON converter.
        /// </summary>
        private SerializedScheduledEventParams()
        {

        }

        /// <summary>
        /// Creates a new SerializedScheduledEventParams instance based on the provided ScheduledEventParams
        /// </summary>
        /// <param name="scheduledEventParams"></param>
        public SerializedScheduledEventParams(ScheduledEventParams scheduledEventParams)
        {
            Name = scheduledEventParams.Name;
            ExpiryTime = Time.DateTimeToUnixTimeStamp(scheduledEventParams.ExpiryTime);
            Tag = scheduledEventParams.Tag;
            Quantity = scheduledEventParams.Quantity;
            OrderReason = scheduledEventParams.OrderReason;
            Symbol = scheduledEventParams.Symbol.ID.ToString();
        }
    }
}