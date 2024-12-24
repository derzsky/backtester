using Binance.Net.Clients;
using Binance.Net.Enums;
using Data;

namespace DataProcessor
{
	public class BinanceClient
	{
		public async Task<List<string>> GetAllSymbols(string quoteSymbol)
		{
			List<string> result = new();

			var restClient = new BinanceRestClient();
			var callResult = await restClient.SpotApi.ExchangeData.GetExchangeInfoAsync();

			if (!callResult.Success)
				return result;

			var dataSymbols = callResult.Data.Symbols;

			var exchangeSymbolsFiltered = dataSymbols.Where(ds => ds.QuoteAsset == quoteSymbol
																&& ds.Status == SymbolStatus.Trading).ToList();

			result = exchangeSymbolsFiltered.Select(esf => esf.Name).ToList();

			return result;
		}

		public async Task<List<PriceRecord>> GetKlines(string symbol, PriceRecord.TimeFrame timeFrame, int limit)
		{
			List<PriceRecord> result = new();

			var klineInterval = ConvertToInterval(timeFrame);

			var restClient = new BinanceRestClient();
			var callResult = await restClient.SpotApi.ExchangeData.GetKlinesAsync(symbol, klineInterval, limit: limit);

			if (!callResult.Success)
				return result;

			result = callResult.Data.Select(kline =>
											new PriceRecord
											{
												Symbol = symbol,
												Frame = timeFrame,
												DateAndTime = kline.OpenTime,
												Open = kline.OpenPrice,
												High = kline.HighPrice,
												Low = kline.LowPrice,
												Close = kline.ClosePrice
											}).ToList();

			return result;
		}

		private KlineInterval ConvertToInterval(PriceRecord.TimeFrame timeFrame)
		{
			return timeFrame switch
			{
				PriceRecord.TimeFrame.Minute1 => KlineInterval.OneMinute,
				PriceRecord.TimeFrame.Hour1 => KlineInterval.OneHour,
				PriceRecord.TimeFrame.Day1 => KlineInterval.OneDay,
				PriceRecord.TimeFrame.Week1 => KlineInterval.OneWeek,
				PriceRecord.TimeFrame.Month1 => KlineInterval.OneMonth,

				_ => KlineInterval.OneDay
			};
		}
	}
}
