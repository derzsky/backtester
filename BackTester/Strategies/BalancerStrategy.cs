using Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Strategies
{
	public class BalancerStrategy : IStrategy
	{
		private PriceRecord _latestMainPrice;

		public event TradeDelegate OnTrade;

		public decimal BtcTargetShare { get; set; } = .5m;
		public decimal BtcShareMaxDeviation { get; set; } = .20m;

		[Column(TypeName = "decimal(16,8)")]
		public decimal Usdt { get; private set; } = 1000;

		[Column(TypeName = "decimal(16,8)")]
		public decimal Btc { get; private set; } = 0;

		public decimal PortfolioCoinsTotal
		{
			get
			{
				if (_latestMainPrice == null)
					return 0;

				var btcValue = Btc * _latestMainPrice.Open;
				var portfolioValue = btcValue + Usdt;

				return portfolioValue;
			}
		}

		public void RunFull(List<PriceRecord> prices)
		{
			var btcPrices = prices.Where(p => p.Symbol == "BTCUSDT")
									.OrderBy(p => p.DateAndTime).ToList();

			foreach (var pric in btcPrices)
			{
				PlayIteration(pric);
			}
		}

		public void PlayIteration(PriceRecord price)
		{
			_latestMainPrice = price;

			var btcCurrentShare = GetBtcCurrentShare(price);
			var btcCurrentShareDeviation = btcCurrentShare - BtcTargetShare;
			if (Math.Abs(btcCurrentShareDeviation) < BtcShareMaxDeviation)
				return;

			if (btcCurrentShareDeviation <= 0)
			{
				Buy(price, btcCurrentShareDeviation);
				return;
			}

			if (btcCurrentShareDeviation > 0)
				Sell(price, btcCurrentShareDeviation);

		}

		public string DemonstratePortfolio()
		{
			return $"USDT: {Usdt.ToString(GeneralConstants.UsdtFormat)}, BTC: {Btc.ToString(GeneralConstants.BtcFormat)}";
		}

		private decimal GetBtcCurrentShare(PriceRecord btcPrice)
		{
			var btcValue = Btc * btcPrice.Open;
			var total = PortfolioCoinsTotal;
			var btcCurrentShare = btcValue / total;

			return btcCurrentShare;
		}

		private void Buy(PriceRecord price, decimal portfolioShareToBuy)
		{
			var portfolioUsdtValue = PortfolioCoinsTotal;
			var usdtNeeded = portfolioUsdtValue * -portfolioShareToBuy;
			var qty = usdtNeeded / price.Open;

			Usdt -= usdtNeeded;
			Btc += qty;

			RaiseOnTrade(TradeDirection.Buy, qty, price.DateAndTime, price.Open, price.Symbol);
		}

		private void Sell(PriceRecord price, decimal portfolioShareToSell)
		{
			var portfolioUsdtValue = PortfolioCoinsTotal;
			var usdtNeeded = portfolioUsdtValue * portfolioShareToSell;
			var btcNeeded = usdtNeeded / price.Open;

			Usdt += usdtNeeded;
			Btc -= btcNeeded;

			RaiseOnTrade(TradeDirection.Sell, btcNeeded, price.DateAndTime, price.Open, price.Symbol);
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
