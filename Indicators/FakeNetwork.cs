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
*/

using System;
using System.IO;
using System.Runtime.InteropServices;
using QuantConnect.Data.Market;
using QuantConnect.Data.Consolidators;
using QuantConnect.Data;
using System.Threading.Tasks;


namespace QuantConnect.Indicators
{

    /// <summary>
    /// This indicator computes the differential of SimpleExponentialDistribution. 
    /// </summary>
    public class FakeNetwork : BarIndicator, IIndicatorWarmUpPeriodProvider
    {
        [DllImport(@"\..\..\..\TestExperiments\libs\CompLib_dll.dll",
            EntryPoint = "fake_network_strategy",
            CallingConvention = CallingConvention.Cdecl)]
        static extern int Fake_network_strategy(double[] bid_close, double[] ask_close, int data_size, double eps, double lb, double ub, int h);

        private readonly double _lb = -15.5;
        private readonly double _ub = 15.5;
        private readonly int _h;
        private readonly int _window_size;
        private RollingWindow<QuoteBar> _window;
        private QuoteBarConsolidator _sampler;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeNetwork"/> class using the specified name and period.
        /// </summary> 
        /// <param name="name">The name of this indicator</param>
        /// <param name="window_size">Quantity of elements in roling window</param>
        /// <param name="h">Matrix size
        /// <param name="time_frame_func">function for time_frame privides start time for candle and time step</param>
        /// <param name="subs_manager">SubcscriptionManaget of Algorithm</param>
        /// <param name="symbol">Contract symbol</param>
        public FakeNetwork(string name, int window_size, int h, Func<DateTime, CalendarInfo> time_frame_func, SubscriptionManager subs_manager, Symbol symbol)
            : base(name)
        {
            _h = h;
            _window_size = window_size;
            _window = new RollingWindow<QuoteBar>(_window_size);
            _sampler = new QuoteBarConsolidator(time_frame_func);
            _sampler.DataConsolidated += OnCandle;
            subs_manager.AddConsolidator(symbol, _sampler);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeNetwork"/> class using the specified name and period.
        /// </summary> 
        /// <param name="name">The name of this indicator</param>
        /// <param name="window_size">Quantity of elements in roling window</param>
        /// <param name="h">Matrix size
        /// <param name="time_frame">number of data points for candle</param>
        /// <param name="subs_manager">SubcscriptionManaget of Algorithm</param>
        /// <param name="symbol">Contract symbol</param>
        public FakeNetwork(string name, int window_size, int h, int time_frame, SubscriptionManager subs_manager, Symbol symbol)
    : base(name)
        {
            _h = h;
            _window_size = window_size;
            _window = new RollingWindow<QuoteBar>(_window_size);
            _sampler = new QuoteBarConsolidator(time_frame);
            _sampler.DataConsolidated += OnCandle;
            subs_manager.AddConsolidator(symbol, _sampler);

        }

        private void OnCandle(object sender, QuoteBar e)
        {
            _window.Add(e);
        }


        /// <summary>
        /// Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady => _window.IsReady;

        /// <summary>
        /// Required period, in data points, for the indicator to be ready and fully initialized.
        /// </summary>
        public int WarmUpPeriod => _window_size;

        /// <summary>
        /// Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>A new value for this indicator</returns>
        protected override decimal ComputeNextValue(IBaseDataBar input)
        {
            if (IsReady)
            {
                var bid_close = new double[_window_size];
                var ask_close = new double[_window_size];
                for (int i = 0; i < _window.Size; i++)
                {
                    bid_close[i] = (double)_window[i].Bid.Close;
                    ask_close[i] = (double)_window[i].Ask.Close;
                }
                double eps = bid_close[_window.Size - 1] - ask_close[_window.Size - 1];


                int signal = Fake_network_strategy(bid_close, ask_close, _window.Size, eps, _lb, _ub, _h);

                return (decimal)signal;
            }
            else
            {
                return 0m;
            }

        }

        /// <summary>
        /// Resets this indicator to its initial state
        /// </summary>
        public override void Reset()
        {
            _window.Reset();
            base.Reset();
        }
    }
}