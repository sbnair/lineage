using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace QuantConnect.Plots
{
    public partial class LineChart : Form
    {
        public PlotView plotView { get; set; }
        public double[] y { get; set; }
        public string label { get; set; }

        public LineChart()
        {
            plotView = new PlotView();
            plotView.Location = new Point(0, 0);
            plotView.Size = new Size(800, 600);

            plotView.Model = new PlotModel { Title = "Backtest Results" };

            plotView.Model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "index" });
            plotView.Model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "pnl" });

            this.y = y;
            this.label = label;

            InitializeComponent();
        }

        public void AddSeries(double[] y, bool cumsum, string label)
        {
            var series = new LineSeries { StrokeThickness = 1, MarkerSize = 1, Title = label };

            if (cumsum)
            {
                double s = 0.0;
                for (int i = 0; i < y.Length; i++)
                {
                    s += y[i];
                    series.Points.Add(new DataPoint((double)i, s));
                }
            }
            else
            {
                for (int i = 0; i < y.Length; i++)
                {
                    series.Points.Add(new DataPoint((double)i, y[i]));
                }
            }

            plotView.Model.Series.Add(series);

        }

        private void Chart_Load(object sender, EventArgs e)
        {
            
            this.Controls.Add(plotView);
        }
    }
}
