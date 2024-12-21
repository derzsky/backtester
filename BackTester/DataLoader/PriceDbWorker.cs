using Data;

namespace DataProcessor
{
	public class PriceDbWorker
	{
		public async Task UpdateAllPRices(string quote)
		{
			var dataContext = new DatContext();

			var client = new BinanceClient();
			var symbols = await client.GetAllSymbols(quote);

			foreach(var sym in symbols)
			{
				await UpdatePrices(sym, dataContext, client, PriceRecord.TimeFrame.Day1);
			}
		}

		public async Task UpdatePrices(string symbol, PriceRecord.TimeFrame timeFrame = PriceRecord.TimeFrame.Day1)
		{
			var dataContext = new DatContext();
			var client = new BinanceClient();
			await UpdatePrices(symbol, dataContext, client, timeFrame);
		}

		private async Task UpdatePrices(string symbol, DatContext dataContext, BinanceClient client, PriceRecord.TimeFrame timeFrame, int limit = 1000)
		{
			var exchangePrices = await client.GetKlines(symbol, timeFrame, limit);

			var pricesInDb = dataContext.Prices.Where(p =>
			p.Symbol.ToUpper() == symbol.ToUpper()
			&& p.Frame == timeFrame).ToList();

			var newPrices = exchangePrices.Where(ep =>
			!pricesInDb.Any(dbp => dbp.IsSamePrice(ep))).ToList();

			if (!newPrices.Any())
				return;

			dataContext.AddRange(newPrices);
			dataContext.SaveChanges();
		}
	}
}
