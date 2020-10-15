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
    public class Differential : BarIndicator, IIndicatorWarmUpPeriodProvider
    {
        [DllImport(@"\..\..\..\TestExperiments\libs\CompLib_dll.dll", 
            EntryPoint = "differential_strategy", 
            CallingConvention = CallingConvention.Cdecl)]
        static extern int Differential_strategy(double[] data, int data_size, int model_order, int polynom_order, double reg);


        private readonly int _window_size;
        private RollingWindow<decimal> _window;
        private QuoteBarConsolidator _sampler;


        //string delimiter;
        //string file;


        /// <summary>
        /// Initializes a new instance of the <see cref="Differential"/> class using the specified name and period.
        /// </summary> 
        /// <param name="name">The name of this indicator</param>
        /// <param name="window_size">Quantity of elements in roling window</param>
        /// <param name="time_frame_func">function for time_frame privides start time for candle and time step</param>
        /// <param name="subs_manager">SubcscriptionManaget of Algorithm</param>
        /// <param name="symbol">Contract symbol</param>
        public Differential(string name, int window_size, Func<DateTime, CalendarInfo> time_frame_func, SubscriptionManager subs_manager, Symbol symbol)
            : base(name)
        {

            _window_size = window_size;
            ABSR = new ABSReturn("ABSReturn", 1);
            _window = new RollingWindow<decimal>(window_size);
            _sampler = new QuoteBarConsolidator(time_frame_func);
            _sampler.DataConsolidated += OnCandle;
            subs_manager.AddConsolidator(symbol, _sampler);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Differential"/> class using the specified name and period.
        /// </summary> 
        /// <param name="name">The name of this indicator</param>
        /// <param name="window_size">Quantity of elements in roling window</param>
        /// <param name="time_frame">number of data points for candle</param>
        /// <param name="subs_manager">SubcscriptionManaget of Algorithm</param>
        /// <param name="symbol">Contract symbol</param>
        public Differential(string name, int window_size, int time_frame, SubscriptionManager subs_manager, Symbol symbol)
    : base(name)
        {
            _window_size = window_size;
            ABSR = new ABSReturn("ABSReturn", 1);
            _window = new RollingWindow<decimal>(window_size);
            _sampler = new QuoteBarConsolidator(time_frame);
            _sampler.DataConsolidated += OnCandle;
            subs_manager.AddConsolidator(symbol, _sampler);


            //file = "C:\\Users\\Виктор\\Documents\\quotes.csv";


            //delimiter = "  ";

            //if (!File.Exists(file))
            //{
            //    string header = "Time" + delimiter + "bid_o" + delimiter + "bid_h" +
            //         delimiter + "bid_l" + delimiter + "bid_c" +
            //         delimiter + "ask_o" + delimiter + "ask_h" +
            //         delimiter + "ask_l" + delimiter + "ask_c" + Environment.NewLine;

            //    File.WriteAllText(file, header);
            //}



        }

        private void OnCandle(object sender, QuoteBar e)
        {
            //Console.WriteLine(e.Time);
            //var mid_price = (e.Bid.Close + e.Ask.Close) / (decimal)2.0;
            //Console.WriteLine(e.Time + "    " +  e.Price);
            //Console.WriteLine("                                               ");
            //Console.WriteLine(e.Bid.Close + "        " + e.Ask.Close + "     " + e.Time);
            //Console.WriteLine("                                               ");
            //Console.WriteLine("                                               ");
            //Console.WriteLine(e.Bid.Close + "     " + e.Ask.Close + "    " + e.Time);



            //string line = e.Time + delimiter + e.Bid.Open + delimiter + e.Bid.High +
            //    delimiter + e.Bid.Low + delimiter + e.Bid.Close +
            //    delimiter + e.Ask.Open + delimiter + e.Ask.High +
            //    delimiter + e.Ask.Low + delimiter + e.Ask.Close + Environment.NewLine;
            //File.AppendAllText(file, line);


            ABSR.Update(e.Time, e.Price);
            if (ABSR.IsReady)
            {
                //Console.WriteLine("---------------------OnCandle Begin---------------------------");
                //Console.WriteLine(ABSR + "   " + e.Price + "    " + e.Time);
                //Console.WriteLine("---------------------OnCandle End---------------------------");

                if (ABSR.Samples != 1)
                {
                    _window.Add(ABSR);
                };

                //if (_window.IsReady)
                //{
                //    var returns = _window.ToDoubleArray();
                //    Array.Reverse(returns);


                //    Console.WriteLine("---------------------OnCandle---------------------------");
                //    Console.WriteLine("______________________________________________________  ");
                //    Console.WriteLine("______________________________________________________  " + e.Time);
                //    Console.Write(string.Join(" ", returns));
                //    Console.WriteLine("--------------------End-----------------------------------");

                //}


            }
        }

        /// <summary>
        /// The ABSR indicator instance being used
        /// </summary>
        public ABSReturn ABSR { get; }

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
                var returns = _window.ToDoubleArray();
               
                Array.Reverse(returns);
                //Console.Write(string.Join(" ", returns));


               int signal = Differential_strategy(returns, returns.Length, 3, 5, 0.0);

                //Console.WriteLine(input.Time);
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
            ABSR.Reset();
            _window.Reset();
            base.Reset();
        }
    }
}