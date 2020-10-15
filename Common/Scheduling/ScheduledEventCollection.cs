using QuantConnect.Interfaces;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using QuantConnect.Configuration;
using QuantConnect.Logging;
using QuantConnect.Util;
using Newtonsoft.Json;
using System.IO;
using System;

namespace QuantConnect.Scheduling
{   
    /// <summary>
    /// Provides access to ScheduledEvents. Serialize ScheduledEventParams for its recovering after system breakdown
    /// </summary>
    public class ScheduledEventCollection : INotifyCollectionChanged
    {
        private List<ScheduledEventParams> _scheduledEventParams = new List<ScheduledEventParams>();
        private readonly string json_path;
        private readonly SerializedIEnumerableScheduledEventJsonConverter serializedIEnumerableScheduledEventJsonConverter
            = new SerializedIEnumerableScheduledEventJsonConverter();
        private readonly JsonSerializer serializer = new JsonSerializer();

        /// <summary>
        /// Fires when schaduled event is added or removed
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Trading algorithm
        /// </summary>
        public IAlgorithm Algorithm { get; private set; }
  
        /// <summary>
        /// Current scheduled events
        /// </summary>
        public ConcurrentDictionary<ScheduledEvent, int> ScheduleEvents { get; set; }

        /// <summary>
        /// Load stored scheduled events params
        /// </summary>
        private void GetOldScheduledEventParams()
        {
            if (Algorithm.LiveMode)
            {
                using (var stream = File.Open(json_path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    JsonReader reader = new JsonTextReader(new StreamReader(stream));
                    var scheduledEventParams = serializedIEnumerableScheduledEventJsonConverter.ReadJson(reader, typeof(IEnumerable<ScheduledEventParams>), _scheduledEventParams, serializer);
                    reader.DisposeSafely();

                    if (scheduledEventParams != null)
                    {
                        _scheduledEventParams = (List<ScheduledEventParams>)scheduledEventParams;
                        foreach (var _event in _scheduledEventParams)
                        {
                            PlaceSheduledEvent(_event);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set scheduled events
        /// </summary>
        /// <param name="scheduleEvents"></param>
        public void SetScheduledEvents(ConcurrentDictionary<ScheduledEvent, int> scheduleEvents)
        {
            ScheduleEvents = scheduleEvents;
        }

        /// <summary>
        /// Create instance of ScheduledEventCollection
        /// </summary>
        /// <param name="algorithm"></param>
        public ScheduledEventCollection(IAlgorithm algorithm)
        {
            Algorithm = algorithm;
            CollectionChanged += ScheduleEventCollectionChanged;
            try
            {
                json_path = Config.Get("open_orders_position");
            }
            catch (Exception err)
            {

                Log.Error(err);
            }
            GetOldScheduledEventParams();
            
        }

        private void ScheduleEventCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
           
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ScheduledEventParams scheduledEventParams = (ScheduledEventParams)e.NewItems[0];
                PlaceSheduledEvent(scheduledEventParams);
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                ScheduledEventParams scheduledEventParams = (ScheduledEventParams)e.OldItems[0];
                ScheduledEvent scheduledEvent = GetScheduledEventByName(scheduledEventParams.Tag);
                if (scheduledEvent != null)
                {
                    Algorithm.Schedule.Remove(scheduledEvent);
                }
            }

            if (Algorithm.LiveMode)
            {
                using (var stream = File.Open(json_path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    JsonWriter writer = new JsonTextWriter(new StreamWriter(stream));
                    serializedIEnumerableScheduledEventJsonConverter.WriteJson(writer, _scheduledEventParams, serializer);
                    writer.Flush();
                    writer.DisposeSafely();
                }
            }
        }

        private void PlaceSheduledEvent(ScheduledEventParams scheduledEventParams)
        {
            Algorithm.Schedule.On(scheduledEventParams.Tag,
            Algorithm.Schedule.DateRules.On(scheduledEventParams.ExpiryTime.Date),
            Algorithm.Schedule.TimeRules.At(scheduledEventParams.ExpiryTime.TimeOfDay, TimeZones.Utc),
            () =>
            {
                Algorithm.MarketOrderWrapper(scheduledEventParams.Symbol,
                                            scheduledEventParams.Quantity,
                                            orderReason: scheduledEventParams.OrderReason,
                                            tag: scheduledEventParams.Tag);
            });
        }

        /// <summary>
        /// Add scheduledEventParams which is converted to ScheduledEvent and placed in SheduleManager
        /// </summary>
        /// <param name="scheduledEventParams"></param>
        public void Add(ScheduledEventParams scheduledEventParams)
        {
            _scheduledEventParams.Add(scheduledEventParams);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, scheduledEventParams));

        }

        /// <summary>
        /// Remove scheduledEventParams which is converted to ScheduledEvent and removed from SheduleManager
        /// </summary>
        /// <param name="scheduledEventTag"></param>
        public void Remove(string scheduledEventTag)
        {
            ScheduledEventParams scheduledEventParams = GetScheduledEventParams(scheduledEventTag);    
            if (scheduledEventParams != null)
            {
                _scheduledEventParams.Remove(scheduledEventParams);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, scheduledEventParams));
            }
        }

        /// <summary>
        /// CollectionChanged invokator
        /// </summary>
        /// <param name="changedEventArgs"></param>
        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs changedEventArgs)
        {
            CollectionChanged?.Invoke(this, changedEventArgs);
        }

        /// <summary>
        /// Get ScheduledEvent from ScheduleManager
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ScheduledEvent GetScheduledEventByName(string name)
        {
            foreach (var _event in ScheduleEvents.Keys)
            {
                if (_event.Name == name)
                {
                    return _event;
                }
            }
            return null;
        }

        /// <summary>
        ///  Get ScheduledEventParams by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ScheduledEventParams GetScheduledEventParams(string name)
        {
            foreach (var event_params in _scheduledEventParams)
            {
                if(event_params.Name == name)
                {
                    return event_params;
                }
            }
            return null;
        }
    }
}
