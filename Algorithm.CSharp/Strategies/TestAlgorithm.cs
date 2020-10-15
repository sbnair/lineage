
using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Indicators;
using System;

namespace QuantConnect.Algorithm.CSharp
{

    public class TestAlgorithm : QCAlgorithm
    {
        RelativeStrengthIndex rsi;
        public override void Initialize()
        {
            SetStartDate(2013, 10, 07);  //Set Start Date
            SetEndDate(2019, 10, 11);    //Set End Date
            SetCash(100000);             //Set Strategy Cash

            AddForex("EURUSD", Resolution.Hour);

            rsi = RSI("EURUSD", 14, MovingAverageType.Simple, Resolution.Daily);

        }

        public override void OnData(Slice data)
        {
            Console.WriteLine("ON DATA");
            if (!rsi.IsReady) return;

            var quantity = Portfolio["EURUSD"].Quantity;

            if (rsi > 70 && quantity == 0)
            {
                SetHoldings("EURUSD", 1);
            }
            else if (rsi < 30 && quantity > 0)
            {
                Liquidate();
            }

        }

    }
}
