using ScottPlot;
using ScottPlot.WPF;

namespace Visuals
{
	public class StrategyViewModel
	{
		public WpfPlot WindowPlot { get; set; }
		private Plot _mainPlot { get { return WindowPlot.Plot; } }

		public void DrawPlot()
		{
			//var generated = Generate.RandomOHLCs(30);
			var prices = GenerateOHLC();//
			_mainPlot.Add.OHLC(prices);
			_mainPlot.Axes.DateTimeTicksBottom();

			WindowPlot.Refresh();
		}

		private List<OHLC> GenerateOHLC()
		{
			List<OHLC> list = new()
			{
				new OHLC{
					Open = 2,
					High = 8,
					Low = 1,
					Close = 1,
					DateTime = DateTime.Now.Date,
					TimeSpan = new TimeSpan(1, 0, 0, 0)
				},
				new OHLC{
					Open = 10,
					High = 32,
					Low = 8,
					Close = 16,
					DateTime = DateTime.Now.Date.AddDays(-1),
					TimeSpan = new TimeSpan(1, 0, 0, 0)
				},
			};

			return list;
		}

		private void DemoCode()
		{
			ScottPlot.Plot myPlot = new();

			// start with original data
			double[] xs = Generate.Consecutive(100);
			double[] ys = Generate.NoisyExponential(100);

			// log-scale the data and account for negative values
			double[] logYs = ys.Select(Math.Log10).ToArray();

			// add log-scaled data to the plot
			var sp = myPlot.Add.Scatter(xs, logYs);
			sp.LineWidth = 0;

			// create a minor tick generator that places log-distributed minor ticks
			ScottPlot.TickGenerators.LogMinorTickGenerator minorTickGen = new();

			// create a numeric tick generator that uses our custom minor tick generator
			ScottPlot.TickGenerators.NumericAutomatic tickGen = new();
			tickGen.MinorTickGenerator = minorTickGen;

			// create a custom tick formatter to set the label text for each tick
			static string LogTickLabelFormatter(double y) => $"{Math.Pow(10, y):N0}";

			// tell our major tick generator to only show major ticks that are whole integers
			tickGen.IntegerTicksOnly = true;

			// tell our custom tick generator to use our new label formatter
			tickGen.LabelFormatter = LogTickLabelFormatter;

			// tell the left axis to use our custom tick generator
			myPlot.Axes.Left.TickGenerator = tickGen;

			// show grid lines for minor ticks
			myPlot.Grid.MajorLineColor = Colors.Black.WithOpacity(.15);
			myPlot.Grid.MinorLineColor = Colors.Black.WithOpacity(.05);
			myPlot.Grid.MinorLineWidth = 1;

			myPlot.SavePng("demo.png", 400, 300);
		}
	}
}
