using Data;
using Strategies;

namespace DataProcessor
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var strategy = new BuyWhenLowPrice();
			RunStrategy(strategy);
		}

		private async Task UpdatePricesFromExchange()
		{
			var priceWorker = new PriceDbWorker();
			await priceWorker.UpdateAllPRices("USDT", PriceRecord.TimeFrame.Week1);
		}

		private static void RunStrategy(IStrategy strategy)
		{
			var dataContext = new DatContext();
			var prices = dataContext.Prices
				.Where(p => p.Frame == PriceRecord.TimeFrame.Week1
						&& p.Symbol.IndexOf("DOWN") == -1
						&& p.Symbol.IndexOf("UPUSDT") == -1).ToList();

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
			string coinPrice = eventArgs.Price.ToString("#####0.########");
			string total = eventArgs.Amount.ToString("#####0.");

			Console.WriteLine($"{date} {eventArgs.Direction} {btcQty} {eventArgs.Symbol} {coinPrice} each, Total: {total} USDT");
		}
	}
}
