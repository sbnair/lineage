using QuantConnect.Orders.Serialization;
using System.Collections.Specialized;
using QuantConnect.Configuration;
using System.Collections.Generic;
using QuantConnect.Logging;
using QuantConnect.Util;
using Newtonsoft.Json;
using System.IO;
using System;

namespace QuantConnect.Orders
{
    /// <summary>
    /// Observable Collection for open position orders with serializing
    /// </summary>
    public class OpenPositionOrders : INotifyCollectionChanged
    {
        private readonly string json_path;
        private readonly SerializedIEnumerableOrderJsonConverter serialized_ienumerable_order_json_converter = new SerializedIEnumerableOrderJsonConverter();
        private readonly JsonSerializer serializer = new JsonSerializer();
        private readonly bool live_mode;

        /// <summary>
        /// Fires when collection was changed, e.g add or remove orders
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Create OpenPositionOrders
        /// </summary>
        /// <param name="live_mode"></param>
        public OpenPositionOrders(bool live_mode)
        {
            try
            {
                this.json_path = Config.Get("open_orders_position");
            }
            catch (Exception err)
            {
                Log.Error(err);
            }
            this.live_mode = live_mode;
            CollectionChanged += Orders_CollectionChanged;
            GetOldOrders();
        }

        /// <summary>
        /// Get old open positions orders from json file
        /// </summary>
        private void GetOldOrders()
        {
            if (live_mode)
            {
                using (var stream = File.Open(json_path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    JsonReader reader = new JsonTextReader(new StreamReader(stream));
                    var stored_orders = serialized_ienumerable_order_json_converter.ReadJson(reader, typeof(IEnumerable<Order>), Values, serializer);
                    reader.DisposeSafely();

                    if (stored_orders != null)
                    {
                        Values = (List<Order>)stored_orders;
                    }
                }

            }
        }

        /// <summary>
        /// Get open position orders
        /// </summary>
        public List<Order> Values { get; private set; } = new List<Order>();

        /// <summary>
        /// Add order to list
        /// </summary>
        /// <param name="order"></param>
        public void Add(Order order)
        {
            Values.Add(order);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, order));
        }


        /// <summary>
        /// Remove order from list
        /// </summary>
        /// <param name="order"></param>
        public void Remove(Order order)
        {
            Values.Remove(order);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, order));            
        }

        /// <summary>
        /// Fires when order list is changed
        /// </summary>
        /// <param name="changedEventArgs"></param>
        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs changedEventArgs)
        {
            CollectionChanged?.Invoke(this, changedEventArgs);
        }

        private void Orders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (live_mode)
            {
                using (var stream = File.Open(json_path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    JsonWriter writer = new JsonTextWriter(new StreamWriter(stream));
                    serialized_ienumerable_order_json_converter.WriteJson(writer, Values, serializer);
                    writer.Flush();
                    writer.DisposeSafely();
                }
            }
        }
    }
}