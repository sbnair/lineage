using System;
using QuantConnect.Data.Consolidators;
using QuantConnect.Data.Market;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Indicators;

namespace QuantConnect.Algorithm.CSharp
{
    public class CandleSampler : QCAlgorithm
    {
        private readonly string _symbol = "EURUSD";
        public override void Initialize()
        {
            // backtest parameters
            SetStartDate(2009, 10, 4);
            SetEndDate(2019, 10, 11);

            // cash allocation
            SetCash(25000);

            AddSecurity(SecurityType.Forex, "EURUSD", Resolution.Hour, Market.IB, false, 1, true);

            //create a consolidator object; for quotebars; for a timespan of 6 hours
            var thirtyMinutes = new QuoteBarConsolidator(TimeSpan.FromHours(6));

            //bind event handler to data consolidated event.
            thirtyMinutes.DataConsolidated += OnHalfHour;

            //register the consolidator for data.
            SubscriptionManager.AddConsolidator(_symbol, thirtyMinutes);
        }

        //event handler for data!
        public void OnHalfHour(object sender, QuoteBar bar)
        {
            //Console.WriteLine(bar.Time + "    " + bar);
            Console.WriteLine(bar.Time);
            //Log(bar);

        }

        public override void OnData(Slice data)
        { }
    }
}