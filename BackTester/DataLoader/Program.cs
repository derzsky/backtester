using Data;

namespace DataProcessor
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			IStrategy strategy = new BalancerStrategy();

			var dataContext = new DatContext();
			var prices = dataContext.Prices
				.Where(p => p.Frame == PriceRecord.TimeFrame.Day1).ToList();

			strategy.RunFull(prices);

			var portfolio = strategy.DemonstratePortfolio();
			var total = strategy.PortfolioTotal;

			Console.WriteLine($"{portfolio}, Total: {total.ToString(GeneralConstants.UsdtFormat)}");

			Console.WriteLine("Hello, World!");
		}
	}
}
