using Binance.Net.Objects.Models.Futures;
using Data;
using Strategies;

namespace DataProcessor
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var shares = new Dictionary<string, decimal>();
			shares.Add(GeneralConstants.USDT, 1);
			shares.Add(GeneralConstants.BTCUSDT, 1);
			//shares.Add("ETHUSDT", 2);
			//shares.Add("BNBUSDT", 2);
			//shares.Add("ADAUSDT", 2);

			var strategy = new BalancerPro(shares);
			RunStrategy(strategy);
			//await UpdatePricesFromExchangeAsync();
		}

		private static async Task UpdatePricesFromExchangeAsync()
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
			var coinsTotal = strategy.PortfolioCoinsTotal;
			var usdt = strategy.Usdt;
			var total = coinsTotal + usdt;

			Console.WriteLine($"{portfolio}, USDT: {usdt.ToString(GeneralConstants.UsdtFormat)}, Coins: {coinsTotal.ToString(GeneralConstants.UsdtFormat)} USDT, Total: {(total).ToString(GeneralConstants.UsdtFormat)}");
		}

		private static void DemonstrateTrade(object sender, TradeEventArgs eventArgs)
		{
			string date = eventArgs.DaTime.Date.ToShortDateString();
			string btcQty = eventArgs.Qty.ToString("##0.####");
			string coinPrice = eventArgs.Price.ToString("#####0.########");
			string total = eventArgs.Amount.ToString("#####0.");
			string usdt = eventArgs.USDT.ToString("#####0.");

			Console.WriteLine($"{date} {eventArgs.Direction} {btcQty} {eventArgs.Symbol} {coinPrice} each, USDT: {usdt}, Total: {total} USDT");
		}
	}
}
