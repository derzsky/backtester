using Data;
using Microsoft.Identity.Client;
using System.Collections.Frozen;
using System.Security.AccessControl;
using System.Xml.Serialization;

namespace Strategies
{
	public class BalancerPro : IStrategy
	{
		private Dictionary<string, decimal> _portfolio = new();
		private Dictionary<string, decimal> _targetShares = new();
		private decimal _maxDeviation = .49m;
		private List<PriceRecord> _currentPrices = new();
		private Dictionary<string, decimal> _coinsInPortfolio =>
			_portfolio.Where(p => p.Key != GeneralConstants.USDT)
			.ToDictionary();

		public decimal PortfolioCoinsTotal
		{
			get
			{
				decimal total = 0;
				foreach (var coin in _coinsInPortfolio)
				{
					var price = _currentPrices.First(p => p.Symbol == coin.Key);
					total += coin.Value * price.Open;
				}

				return total;
			}
		}

		public decimal PortfolioTotal => PortfolioCoinsTotal + Usdt;

		public decimal Usdt => _portfolio[GeneralConstants.USDT];

		public event TradeDelegate OnTrade;

		public BalancerPro(Dictionary<string, decimal> shares)
		{
			_portfolio.Add(GeneralConstants.USDT, 10_000);

			var sharesSum = shares.Values.Sum();

			foreach (var item in shares)
			{
				_targetShares.Add(item.Key, item.Value / sharesSum);

				if (_portfolio.ContainsKey(item.Key))
					continue;

				_portfolio.Add(item.Key, 0);
			}
		}

		public string DemonstratePortfolio()
		{
			return $"{_portfolio.Count} Positions";
		}

		public void RunFull(List<PriceRecord> prices)
		{
			var pricesNeeded = prices
								.Where(p => _portfolio.Keys.Contains(p.Symbol)
								&& p.DateAndTime > new DateTime(2021, 5, 3))
								.ToList();

			var dates = pricesNeeded.Select(p => p.DateAndTime)
									.Order()
									.Distinct().ToList();

			foreach (var dat in dates)
			{
				var todayPrices = pricesNeeded.Where(p => p.DateAndTime == dat)
												.ToList();

				_currentPrices = todayPrices;

				Balance(todayPrices);
			}
		}

		private void Balance(IEnumerable<PriceRecord> todayPrices)
		{
			var currentDeviations = new Dictionary<string, decimal>();

			foreach (var item in _portfolio)
			{
				PriceRecord price;
				decimal value;

				price = todayPrices.FirstOrDefault(p => p.Symbol == item.Key);
				if (price == null && item.Key == GeneralConstants.USDT)
					value = Usdt;
				else
					value = price.Open * item.Value;

				var share = value / PortfolioTotal;
				var targetShare = _targetShares[item.Key];

				currentDeviations.Add(item.Key, share - targetShare);
			}

			if (!currentDeviations.Any(d => Math.Abs(d.Value) > _maxDeviation))
				return;

			foreach (var coin in _coinsInPortfolio)
			{
				var currDeviation = currentDeviations[coin.Key];

				var value = currDeviation * PortfolioTotal;
				var price = todayPrices.First(tp => tp.Symbol == coin.Key);
				var qty = Math.Abs(value) / price.Open;

				if (currDeviation > 0)
				{
					Sell(coin.Key, qty, price.Open);
					RaiseOnTrade(TradeDirection.Sell, qty, price.DateAndTime, price.Open, coin.Key);
				}
				else if (currDeviation < 0)
				{
					Buy(coin.Key, qty, price.Open);
					RaiseOnTrade(TradeDirection.Buy, qty, price.DateAndTime, price.Open, coin.Key);
				}

			}
		}

		private void Sell(string symbol, decimal qty, decimal price)
		{
			_portfolio[symbol] -= qty;
			_portfolio[GeneralConstants.USDT] += qty * price;
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
				Symbol = symbol,
				USDT = Usdt
			};

			OnTrade?.Invoke(this, eventArgs);
		}
	}
}
