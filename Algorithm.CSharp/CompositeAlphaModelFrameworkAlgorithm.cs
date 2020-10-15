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

using System.Collections.Generic;
using System.Windows.Forms;
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Execution;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.Risk;
using QuantConnect.Algorithm.Framework.Selection;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Indicators;
using QuantConnect.Interfaces;
using QuantConnect.Plots;
using QuantConnect.Util;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Show cases how to use the <see cref="CompositeAlphaModel"/> to define
    /// </summary>
    public class CompositeAlphaModelFrameworkAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {

        private readonly List<string> _tickers = new List<string>();

        public override void Initialize()
        {
            SetStartDate(2008, 12, 25);
            SetEndDate(2020, 12, 1);
            SetCash(10000);

            _tickers.Add("AUDUSD");

            // even though we're using a framework algorithm, we can still add our securities
            // using the AddEquity/Forex/Crypto/ect methods and then pass them into a manual
            // universe selection model using Securities.Keys
            foreach (string ticker in _tickers)
                AddSecurity(SecurityType.Forex, ticker, Resolution.Hour, Market.IB, false, 1, true);

            // define a manual universe of all the securities we manually registered
            SetUniverseSelection(new ManualUniverseSelectionModel(Securities.Keys));

            // define alpha model as a composite of the rsi and ema cross models
            //SetAlpha(new CompositeAlphaModel(
            //    new RsiAlphaModel(),
            //    new EmaCrossAlphaModel()
            //));

            SetAlpha(new DifferentialAlpha(window_size: 99, window_timeframe: 6, resolution: Resolution.Hour));

            // default models for the rest
            SetPortfolioConstruction(new EqualWeightingPortfolioConstructionModel());
            SetExecution(new ImmediateExecutionModel());
            SetRiskManagement(new NullRiskManagementModel());
        }


        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public Language[] Languages { get; } = {Language.CSharp, Language.Python};

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "7"},
            {"Average Win", "0.01%"},
            {"Average Loss", "-0.40%"},
            {"Compounding Annual Return", "1114.772%"},
            {"Drawdown", "1.800%"},
            {"Expectancy", "-0.319"},
            {"Net Profit", "3.244%"},
            {"Sharpe Ratio", "23.478"},
            {"Probabilistic Sharpe Ratio", "80.383%"},
            {"Loss Rate", "33%"},
            {"Win Rate", "67%"},
            {"Profit-Loss Ratio", "0.02"},
            {"Alpha", "4.314"},
            {"Beta", "1.239"},
            {"Annual Standard Deviation", "0.285"},
            {"Annual Variance", "0.081"},
            {"Information Ratio", "47.452"},
            {"Tracking Error", "0.101"},
            {"Treynor Ratio", "5.409"},
            {"Total Fees", "$67.00"},
            {"Fitness Score", "0.501"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "148.636"},
            {"Return Over Maximum Drawdown", "1502.912"},
            {"Portfolio Turnover", "0.501"},
            {"Total Insights Generated", "2"},
            {"Total Insights Closed", "0"},
            {"Total Insights Analysis Completed", "0"},
            {"Long Insight Count", "2"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"Estimated Monthly Alpha Value", "$0"},
            {"Total Accumulated Estimated Alpha Value", "$0"},
            {"Mean Population Estimated Insight Value", "$0"},
            {"Mean Population Direction", "0%"},
            {"Mean Population Magnitude", "0%"},
            {"Rolling Averaged Population Direction", "0%"},
            {"Rolling Averaged Population Magnitude", "0%"},
            {"OrderListHash", "-28636839"}
        };

        public override void OnEndOfAlgorithm()
        {
            double[] pnl = new double[TradeBuilder.ClosedTrades.Count];
            for (int i = 0; i < TradeBuilder.ClosedTrades.Count; i++)
            {
                pnl[i] = (double)TradeBuilder.ClosedTrades[i].ProfitLoss;
            }

            LineChart plot = new LineChart();
            plot.AddSeries(pnl, cumsum: true, label: "AUDUSD");

            Application.Run(plot);
        }

    }

    public class DifferentialAlpha : AlphaModel
    {
        private readonly Dictionary<Symbol, SymbolData> _symbolDataBySymbol = new Dictionary<Symbol, SymbolData>();

        private int _window_size, _window_timeframe;
        private Resolution _resolution;

        public DifferentialAlpha(int window_size, int window_timeframe, Resolution resolution)
        {
            _window_size = window_size;
            _window_timeframe = window_timeframe;
            _resolution = resolution;

            Name = $"{nameof(DifferentialAlpha)}({_window_size},{_window_timeframe},{_resolution})";
        }

        /// <summary>
        /// Updates this alpha model with the latest data from the algorithm.
        /// This is called each time the algorithm receives data for subscribed securities
        /// </summary>
        /// <param name="algorithm">The algorithm instance</param>
        /// <param name="data">The new data available</param>
        /// <returns>The new insights generated</returns>
        public override IEnumerable<Insight> Update(QCAlgorithm algorithm, Slice data)
        {
            var insights = new List<Insight>();
            foreach (var kvp in _symbolDataBySymbol)
            {
                var symbol = kvp.Key;
                var diff = kvp.Value.DIFF;
                //var previousState = kvp.Value.State;
                //var state = GetState(rsi, previousState);

                if (diff.IsReady)
                {
                    var insightPeriod = _resolution.ToTimeSpan().Multiply(_window_timeframe);

                    var signal = diff.Current * (-1);

                    if (signal == (decimal)1)
                    {
                        insights.Add(Insight.Price(symbol, insightPeriod, InsightDirection.Up));
                    }
                    else if (signal == (decimal)-1)
                    {
                        insights.Add(Insight.Price(symbol, insightPeriod, InsightDirection.Down));
                    }

                    //switch (state)
                    //{
                    //    case State.TrippedLow:
                    //        insights.Add(Insight.Price(symbol, insightPeriod, InsightDirection.Up));
                    //        break;

                    //    case State.TrippedHigh:
                    //        insights.Add(Insight.Price(symbol, insightPeriod, InsightDirection.Down));
                    //        break;
                    //}
                }

                //kvp.Value.State = state;
            }

            return insights;
        }


        /// <summary>
        /// Cleans out old security data and initializes the RSI for any newly added securities.
        /// This functional also seeds any new indicators using a history request.
        /// </summary>
        /// <param name="algorithm">The algorithm instance that experienced the change in securities</param>
        /// <param name="changes">The security additions and removals from the algorithm</param>
        public override void OnSecuritiesChanged(QCAlgorithm algorithm, SecurityChanges changes)
        {
            // clean up data for removed securities
            if (changes.RemovedSecurities.Count > 0)
            {
                var removed = changes.RemovedSecurities.ToHashSet(x => x.Symbol);
                foreach (var subscription in algorithm.SubscriptionManager.Subscriptions)
                {
                    if (removed.Contains(subscription.Symbol))
                    {
                        _symbolDataBySymbol.Remove(subscription.Symbol);
                        subscription.Consolidators.Clear();
                    }
                }
            }

            // initialize data for added securities
            var addedSymbols = new List<Symbol>();
            foreach (var added in changes.AddedSecurities)
            {
                if (!_symbolDataBySymbol.ContainsKey(added.Symbol))
                {
                    Differential indicator = algorithm.DIFF(added.Symbol, _window_size, _window_timeframe, algorithm.SubscriptionManager, _resolution);
                    //var rsi = algorithm.RSI(added.Symbol, _period, MovingAverageType.Wilders, _resolution);
                    var symbolData = new SymbolData(added.Symbol, indicator);
                    _symbolDataBySymbol[added.Symbol] = symbolData;
                    addedSymbols.Add(symbolData.Symbol);
                }
            }

            if (addedSymbols.Count > 0)
            {
                // warmup our indicators by pushing history through the consolidators
                algorithm.History(addedSymbols, _window_size, _resolution)
                    .PushThrough(data =>
                    {
                        SymbolData symbolData;
                        if (_symbolDataBySymbol.TryGetValue(data.Symbol, out symbolData))
                        {
                            symbolData.DIFF.Update(data.EndTime, data.Value);
                        }
                    });
            }
        }

        /// <summary>
        /// Contains data specific to a symbol required by this model
        /// </summary>
        private class SymbolData
        {
            public Symbol Symbol { get; }
            //public State State { get; set; }
            public Differential DIFF { get; }

            public SymbolData(Symbol symbol, Differential diff)
            {
                Symbol = symbol;
                DIFF = diff;
                //State = State.Middle;
            }
        }
           
    

    }

}
