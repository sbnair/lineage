using System;
using QuantConnect.Orders;

namespace QuantConnect.Scheduling
{
    /// <summary>
    /// ScheduledEvent transport data structure
    /// </summary>
    public class ScheduledEventParams
    {
        private string tag;
        private Symbol symbol;
        private DateTime expiry_time;
        private decimal quantity;
        private OrderReason reason;

        /// <summary>
        /// Gets an identifier for this event
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets the utc expiry time of the order
        /// </summary>
        public DateTime ExpiryTime { get; set; }

        /// <summary>
        /// Gets order tag
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Gets order ticker
        /// </summary>
        public Symbol Symbol { get; set; }

        /// <summary>
        /// Gets order size
        /// </summary>
        public decimal Quantity { get; set; }


        /// <summary>
        /// Gets order reason
        /// </summary>
        public OrderReason OrderReason { get; set; }

        /// <summary>
        /// Initialize ScheduledEventParams
        /// </summary>
        /// <param name="name">an identifier for scheduled event</param>
        /// <param name="expiry_time">utc expiry time of the order</param>
        /// <param name="symbol">order ticker</param>
        /// <param name="quantity">order size</param>
        /// <param name="orderReason">order reason</param>
        public ScheduledEventParams(string name, DateTime expiry_time, Symbol symbol, decimal quantity, OrderReason orderReason)
        {
            Name = name;
            ExpiryTime = expiry_time;
            Symbol = symbol;
            Quantity = quantity;
            OrderReason = orderReason;
            Tag = name;
        }

        /// <summary>
        /// Creates a new ScheduledEventParams instance from a SerializedScheduledEventParams instance
        /// </summary>
        /// <param name="serializedScheduledEventsParams"></param>
        /// <returns></returns>
        public static ScheduledEventParams FromSerialized(SerializedScheduledEventParams serializedScheduledEventsParams)
        {
            string name = serializedScheduledEventsParams.Name;
            string tag = serializedScheduledEventsParams.Tag;

            var sid = SecurityIdentifier.Parse(serializedScheduledEventsParams.Symbol);
            Symbol symbol = new Symbol(sid, sid.Symbol);

            DateTime expiry_time = QuantConnect.Time.UnixTimeStampToDateTime(serializedScheduledEventsParams.ExpiryTime);
            OrderReason orderReason = serializedScheduledEventsParams.OrderReason;

            decimal quantity = serializedScheduledEventsParams.Quantity;
            
            return new ScheduledEventParams(name, expiry_time, symbol, quantity, orderReason);
        }
    }
}