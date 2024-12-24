using Data;

namespace Strategies
{
	public class BuyOnceAndHold : IStrategy
	{
		private string[] _stableCoins = { "USDC", "BUSD", "FDUSD", "TUSD", "PAX", "USDD", "USDS", "HUSD", "DAI", "USDP", "EURI" };

		private Dictionary<string, decimal> _portfolio = new();
		private List<PriceRecord> _latestPrices = new();
		private List<string> _delistedCoins = new();

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

		public BuyOnceAndHold()
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

			var relevantPrices = prices.Where(p => p.DateAndTime == StartDate
			&& !stablecoinsPairs.Contains(p.Symbol)).ToList();

			InitialBuy(relevantPrices);

			var latestBtcPrice = prices.Where(p => p.Symbol == "BTCUSDT")
				.MaxBy(p => p.DateAndTime);

			_latestPrices = prices.Where(p => p.DateAndTime == latestBtcPrice.DateAndTime).ToList();
		}

		private void InitialBuy(List<PriceRecord> relevantPrices)
		{
			var symbols = relevantPrices.Select(p => p.Symbol).Distinct().ToList();

			var volumeForEachPosition = _portfolio[GeneralConstants.USDT] / symbols.Count;

			foreach (var sym in symbols)
			{
				var initialPrice = relevantPrices.Where(p => p.Symbol == sym)
					.MinBy(p => p.DateAndTime);

				var amountToBuy = volumeForEachPosition / initialPrice.Close;

				Buy(sym, amountToBuy, initialPrice);

				RaiseOnTrade(TradeDirection.Buy, amountToBuy, initialPrice.DateAndTime, initialPrice.Close, initialPrice.Symbol);
			}

		}

		private void Buy(string sym, decimal amountToBuy, PriceRecord price)
		{
			if (!_portfolio.ContainsKey(sym))
				_portfolio.Add(sym, 0);

			_portfolio[GeneralConstants.USDT] -= amountToBuy * price.Close;
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
	}
}
