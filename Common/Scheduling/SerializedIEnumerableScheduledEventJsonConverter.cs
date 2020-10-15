/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using QuantConnect.Util;
using System.Collections.Generic;

namespace QuantConnect.Scheduling
{
    /// <summary>
    /// Defines how SerializedScheduledEventParams should be serialized to json
    /// </summary>
    public class SerializedIEnumerableScheduledEventJsonConverter : TypeChangeJsonConverter<IEnumerable<ScheduledEventParams>, IEnumerable<SerializedScheduledEventParams>>
    {
        private readonly string _algorithmId;

        /// <summary>
        /// True will populate TResult object returned by <see cref="Convert(IEnumerable{SerializedScheduledEventParams})"/> with json properties
        /// </summary>
        protected override bool PopulateProperties => false;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="algorithmId">The associated algorithm id, required when serializing</param>
        public SerializedIEnumerableScheduledEventJsonConverter(string algorithmId = null)
        {
            _algorithmId = algorithmId;
        }

        /// <summary>
        /// Returns true if the provided type can be converted
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IEnumerable<ScheduledEventParams>).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// Convert the input value to a value to be serialized
        /// </summary>
        /// <param name="values">The input values to be converted before serialization</param>
        /// <returns>A new instance of TResult that is to be serialized</returns>
        protected override IEnumerable<SerializedScheduledEventParams> Convert(IEnumerable<ScheduledEventParams> values)
        {
            List<SerializedScheduledEventParams> serializedScheduledEventParams = new List<SerializedScheduledEventParams>();
            foreach (var item in values)
            {
                serializedScheduledEventParams.Add(new SerializedScheduledEventParams(item));
            }
            return serializedScheduledEventParams;
        }

        /// <summary>
        /// Converts the input value to be deserialized
        /// </summary>
        /// <param name="values">The deserialized value that needs to be converted to <see cref="ScheduledEventParams"/></param>
        /// <returns>The converted value</returns>
        protected override IEnumerable<ScheduledEventParams> Convert(IEnumerable<SerializedScheduledEventParams> values)
        {
            List<ScheduledEventParams> scheduledEventParams = new List<ScheduledEventParams>();
            foreach (var item in values)
            {
                scheduledEventParams.Add(ScheduledEventParams.FromSerialized(item));
            }

            return scheduledEventParams;
        }
    }
}