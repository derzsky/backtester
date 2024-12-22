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
		}

		private static void DemonstrateTrade(object sender, TradeEventArgs eventArgs)
		{
			string date = eventArgs.DaTime.Date.ToShortDateString();
			string btcQty = eventArgs.Qty.ToString("##0.####");
			string btcPrice = eventArgs.Price.ToString("#####0.");
			string total = eventArgs.Amount.ToString("#####0.");

			Console.WriteLine($"{date} {eventArgs.Direction} {btcQty} BTC for {btcPrice} USDT each, Total: {total} USDT");
		}
	}
}
