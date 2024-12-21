using Data;

namespace DataProcessor
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var strategy = new BalancerStrategy();
			var runner = new StrategyRunner(strategy);

			var dataContext = new DatContext();
			var prices = dataContext.Prices
				.Where(p => p.Symbol == "BTCUSDT"
				&& p.Frame == PriceRecord.TimeFrame.Day1).ToList();

			runner.Run(prices);

			Console.WriteLine("Hello, World!");
		}
	}
}
