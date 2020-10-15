using System;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;


namespace QuantConnect.Algorithm.CSharp
{

    public class RSIAlgorithm : QCAlgorithm
    {
        decimal lastOpenPrice = 0;
        decimal lastClosePrice = 0;

        ExponentialMovingAverage EMAFast;
        ExponentialMovingAverage EMASlow;


        DateTime startDate = new DateTime(2017, 7, 1);
        DateTime endDate = new DateTime(2017, 7, 18);


        public override void Initialize()
        {
            SetStartDate(startDate);
            SetEndDate(endDate);

            SetCash(100000);


            AddForex("EURUSD", Resolution.Hour);



            Chart stockPlot = new Chart("Trade Plot");
            Series assetOpenPrice = new Series("Open", SeriesType.Scatter, 0);
            Series assetClosePrice = new Series("Close", SeriesType.Scatter, 0);
            Series fastMA = new Series("FastMA", SeriesType.Line, 0);
            Series slowMA = new Series("SlowMA", SeriesType.Line, 0);

            EMAFast = EMA("EURUSD", 9);
            EMASlow = EMA("EURUSD", 90);

            stockPlot.AddSeries(assetOpenPrice);
            stockPlot.AddSeries(assetClosePrice);
            stockPlot.AddSeries(fastMA);
            stockPlot.AddSeries(slowMA);
            AddChart(stockPlot);

        }


        public void OnData(QuoteBars data)
        {

            if (!Portfolio.HoldStock)
            {
                SetHoldings("EURUSD", 10);
            }

            lastOpenPrice = data["EURUSD"].Open;
            lastClosePrice = data["EURUSD"].Close;
            Plot("Trade Plot", "Open", lastOpenPrice);
            Plot("Trade Plot", "Close", lastClosePrice);

            if (EMASlow.IsReady)
            {
                Plot("Trade Plot", "FastMA", EMAFast);
                Plot("Trade Plot", "SlowMA", EMASlow);

            }
        }

    }
}