using Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataProcessor
{
	public class BalancerStrategy : IStrategy
	{
		private PriceRecord _latestMainPrice;
		public decimal BtcTargetShare { get; set; } = .5m;
		public decimal BtcShareMaxDiviation { get; set; } = .05m;

		[Column(TypeName = "decimal(16,8)")]
		public decimal Usdt { get; private set; } = 1000;

		[Column(TypeName = "decimal(16,8)")]
		public decimal Btc { get; private set; } = 0;

		public decimal PortfolioTotal
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
			if (Math.Abs(btcCurrentShareDeviation) < BtcShareMaxDiviation)
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
			var total = PortfolioTotal;
			var btcCurrentShare = btcValue / total;

			return btcCurrentShare;
		}

		private void Buy(PriceRecord price, decimal portfolioShareToBuy)
		{
			var portfolioUsdtValue = PortfolioTotal;
			var usdtNeeded = portfolioUsdtValue * -portfolioShareToBuy;

			Usdt -= usdtNeeded;
			Btc += usdtNeeded / price.Open;
		}

		private void Sell(PriceRecord price, decimal portfolioShareToSell)
		{
			var portfolioUsdtValue = PortfolioTotal;
			var usdtNeeded = portfolioUsdtValue * portfolioShareToSell;
			var btcNeeded = usdtNeeded / price.Open;

			Usdt += usdtNeeded;
			Btc -= btcNeeded;
		}
	}
}
