using Data;
using Strategies;

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

			strategy.OnTrade += DemonstrateTrade;

			strategy.RunFull(prices);

			var portfolio = strategy.DemonstratePortfolio();
			var total = strategy.PortfolioTotal;

			Console.WriteLine($"{portfolio}, Total: {total.ToString(GeneralConstants.UsdtFormat)}");

			Console.WriteLine("Hello, World!");
		}

		private static void DemonstrateTrade(object sender, TradeEventArgs eventArgs)
		{
			Console.WriteLine($"{eventArgs.DaTime.Date.ToShortDateString()}, {eventArgs.Direction}, {eventArgs.Qty.ToString("##0.####")} BTC for {eventArgs.Price.ToString("#####0.##")} UDST each, Total: {eventArgs.Amount.ToString("#####0.##")} USDT");
		}
	}
}
