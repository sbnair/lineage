using System;
using System.IO;
using QuantConnect.Data;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Plots;
using System.Windows.Forms;
using QuantConnect.Data.Consolidators;


namespace QuantConnect.Algorithm.CSharp
{
    public class FakeNetworkStrategy : QCAlgorithm
    {


        private readonly string _symbol = "EURUSD";
        private Symbol symbol;
        private const int window_size = 125;

        private FakeNetwork _indicator;

        public override void Initialize()
        {
            SetStartDate(2008, 12, 25);
            SetEndDate(2020, 5, 1);
            SetCash(100000);

            //EnableAutomaticIndicatorWarmUp = true;
            AddSecurity(SecurityType.Forex, _symbol, Resolution.Hour, Market.IB, false, 1, true);

            symbol = Symbol(_symbol);

            SetBenchmark(symbol);

            _indicator = FN(symbol, window_size, 6, 1, SubscriptionManager, Resolution.Hour);

        }

        public override void OnData(Slice slice)
        {
            if (!_indicator.IsReady) return;

            var quantity = Portfolio[_symbol].Quantity;

            var signal = _indicator.Current;


            if (signal == (decimal)1)
            {

                if (quantity < 0)
                {
                    ClosePosition(symbol, 1000, OrderType.Market);
                    OpenPosition(symbol, 1000, 0.15, 0.05, TimeSpan.FromHours(120));
                }

                else if (quantity == 0)
                {
                    OpenPosition(symbol, 1000, 0.1, 0.05, TimeSpan.FromHours(120));
                }

            }

            else if (signal == (decimal)-1)
            {
                if (quantity > 0)
                {
                    ClosePosition(symbol, -1000, OrderType.Market);
                    OpenPosition(symbol, -1000, 0.1, 0.05, TimeSpan.FromHours(120));
                }

                else if (quantity == 0)
                {
                    OpenPosition(symbol, -1000, 0.1, 0.05, TimeSpan.FromHours(120));
                }

            }

            return;
        }

        public override void OnEndOfAlgorithm()
        {
            // Liquidate();

            //string file = "C:\\Users\\Виктор\\Documents\\trades_audusd.csv";


            //string delimiter = "  ";

            //if (!File.Exists(file))
            //{
            //    string header = "EntryTime" + delimiter + "EntryPrice" + delimiter + "ExitTime" + delimiter + "ExitPrice" + delimiter + "ProfitLoss" + Environment.NewLine;

            //    File.WriteAllText(file, header);
            //}



            double[] pnl = new double[TradeBuilder.ClosedTrades.Count];
            for (int i = 0; i < TradeBuilder.ClosedTrades.Count; i++)
            {
                pnl[i] = (double)TradeBuilder.ClosedTrades[i].ProfitLoss;
            }

            LineChart plot = new LineChart();
            plot.AddSeries(pnl, cumsum: true, label: _symbol);

            Application.Run(plot);


            //foreach (var trade in TradeBuilder.ClosedTrades)
            //{
            //string line = trade.EntryTime + delimiter + trade.EntryPrice + delimiter + trade.ExitTime + delimiter + trade.ExitPrice + delimiter + trade.ProfitLoss + Environment.NewLine;
            //File.AppendAllText(file, line);
            //    Log($"Symbol: {trade.Symbol} Quantity: {trade.Quantity} EntryTime: {trade.EntryTime} EntryPrice: {trade.EntryPrice} ExitTime: {trade.ExitTime} ExitPrice: {trade.ExitPrice} ProfitLoss: {trade.ProfitLoss}");
            //}

        }
    }
}
