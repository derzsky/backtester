using Data;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.ComponentModel.DataAnnotations;

namespace Strategies
{
	public class BuyLowSellDouble : IStrategy
	{
		private string[] _stableCoins = { "USDC", "BUSD", "FDUSD", "TUSD", "PAX", "USDD", "USDS", "HUSD", "DAI", "USDP", "EURI", "AUD", "EUR", "AEUR" };

		private Dictionary<string, decimal> _portfolio = new();
		private List<PriceRecord> _latestPrices = new();
		private List<string> _delistedCoins = new();
		private TimeSpan _acceptableTimeGap = TimeSpan.FromDays(440);

		public DateTime StartDate = new DateTime(2021, 05, 03);

		public decimal PortfolioCoinsTotal
		{
			get
			{
				decimal total = 0;
				foreach (var position in _portfolio)
				{
					var positinoPrice = _latestPrices.FirstOrDefault(p => p.Symbol == position.Key);

					if (positinoPrice is null)
					{
						_delistedCoins.Add(position.Key);
						continue;
					}

					total += position.Value * positinoPrice.Close;
				}

				return total;
			}
		}

		public decimal Usdt { get { return _portfolio[GeneralConstants.USDT]; } }

		public event TradeDelegate OnTrade;

		public BuyLowSellDouble()
		{
			_portfolio.Add(GeneralConstants.USDT, 10_000);
		}

		public string DemonstratePortfolio()
		{
			return $"{_portfolio.Count} Positions";
		}

		public void RunFull(List<PriceRecord> prices)
		{
			var stablecoinsPairs = _stableCoins.Select(s => $"{s}{GeneralConstants.USDT}").ToList();

			var relevantPrices = prices.Where(p => p.DateAndTime >= StartDate
			&& !stablecoinsPairs.Contains(p.Symbol))
				//.Where(p => p.Symbol == GeneralConstants.BTCUSDT)
				.ToList();

			BuyOnLow(relevantPrices, prices);

			var latestBtcPrice = prices.Where(p => p.Symbol == GeneralConstants.BTCUSDT)
				.MaxBy(p => p.DateAndTime);

			_latestPrices = prices.Where(p => p.DateAndTime == latestBtcPrice.DateAndTime).ToList();
		}

		private void BuyOnLow(List<PriceRecord> relevantPrices, List<PriceRecord> prices)
		{
			var amountForEachPosition = _portfolio[GeneralConstants.USDT] / 60;
			

			var dates = relevantPrices.Select(p => p.DateAndTime).Distinct().ToList();
			foreach (var dat in dates)
			{
				var datePrices = relevantPrices.Where(p => p.DateAndTime == dat).ToList();

				HandleBuys(dat, datePrices, prices, amountForEachPosition);

				if (Usdt < amountForEachPosition)
					break;
			}
		}

		private void HandleBuys(DateTime dat, List<PriceRecord> datePrices, List<PriceRecord> prices, decimal amountForEachPosition)
		{
			//находим обновлённый старый лоу
			var coveredLows = GetMinimalCoveredPrices(datePrices, prices);

			//смотрим, чтобы он был давно
			var oldEnoughLows = coveredLows
									.Where(p => dat - p.DateAndTime >= _acceptableTimeGap)
									.ToList();

			//кандидат это современная цена, которая обновила достаточно старый лоу
			//и такой монеты не должно быть куплено раньше
			var candidatesForBuy = datePrices.Where(dp => oldEnoughLows.Any(old => old.Symbol == dp.Symbol)
														&& !_portfolio.Keys.Any(key => key == dp.Symbol))
											.ToList();
			if (!candidatesForBuy.Any())
				return;

			//берём самый старый из них
			var priceToBuy = candidatesForBuy.MinBy(cfb => cfb.DateAndTime);

			var qty = amountForEachPosition / priceToBuy.Close;

			Buy(priceToBuy.Symbol, qty, priceToBuy.Close);
			RaiseOnTrade(TradeDirection.Buy, qty, priceToBuy.DateAndTime, priceToBuy.Close, priceToBuy.Symbol);
		}

		private void Buy(string sym, decimal amountToBuy, decimal PriceValue)
		{
			if (!_portfolio.ContainsKey(sym))
				_portfolio.Add(sym, 0);

			_portfolio[GeneralConstants.USDT] -= amountToBuy * PriceValue;
			_portfolio[sym] += amountToBuy;
		}

		private void RaiseOnTrade(TradeDirection direction, decimal qty, DateTime daTime, decimal priceValue, string symbol)
		{
			var eventArgs = new TradeEventArgs
			{
				DaTime = daTime,
				Direction = direction,
				Price = priceValue,
				Qty = qty,
				Symbol = symbol
			};

			OnTrade?.Invoke(this, eventArgs);
		}

		private List<PriceRecord> GetMinimalCoveredPrices(List<PriceRecord> datePrices, List<PriceRecord> prices)
		{
			var symbolsTrading = datePrices.Select(p => p.Symbol).Distinct().ToList();

			var minimalCoveredPrices = new List<PriceRecord>();
			foreach (var sym in symbolsTrading)
			{
				var currentPrice = datePrices.First(dp => dp.Symbol == sym);

				var olderPrices = prices.Where(p => p.Symbol == currentPrice.Symbol
											&& p.DateAndTime < currentPrice.DateAndTime)
										.ToList();

				var latestNotCoveredPrice = olderPrices
											.Where(op => op.Low < currentPrice.Low)
											.MaxBy(op => op.DateAndTime);
				DateTime? notCoveredDate = latestNotCoveredPrice?.DateAndTime;
				if (notCoveredDate is null)
					notCoveredDate = currentPrice.DateAndTime - _acceptableTimeGap;

				var minimalCoveredPrice = olderPrices
					.Where(p => p.Symbol == sym
							&& p.DateAndTime > notCoveredDate
							&& p.Low <= currentPrice.Open
							&& p.Low >= currentPrice.Low)
					.MinBy(p => p.Low);

				if (minimalCoveredPrice is null)
					continue;

				//минимальная цена может быть с тех пор перекрыта более новой и более низкой
				var pricesSinceTheMinimal = olderPrices
												.Where(p => p.DateAndTime > minimalCoveredPrice.DateAndTime
														&& p.DateAndTime < currentPrice.DateAndTime
														&& p.Low < minimalCoveredPrice.Low)
												.ToList();
				if (pricesSinceTheMinimal.Any())
					continue;

				minimalCoveredPrices.Add(minimalCoveredPrice);
			}

			return minimalCoveredPrices;
		}
	}
}
