using Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor
{
	public class BalancerStrategy
	{
		public decimal BtcTargetShare { get; set; } = .5m;
		public decimal BtcShareMaxDiviation { get; set; } = .1m;

		[Column(TypeName = "decimal(16,8)")]
		public decimal Usdt { get; private set; } = 1000;

		[Column(TypeName = "decimal(16,8)")]
		public decimal Btc { get; private set; } = 0;

		public decimal GetPortfolioTotal(PriceRecord btcPrice)
		{
			var btcValue = Btc * btcPrice.Open;
			var portfolioValue = btcValue + Usdt;

			return portfolioValue;
		}

		public void PlayIteration(PriceRecord price)
		{
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
			var total = GetPortfolioTotal(btcPrice);
			var btcCurrentShare = btcValue / total;

			return btcCurrentShare;
		}

		private void Buy(PriceRecord price, decimal portfolioShareToBuy)
		{
			var portfolioUsdtValue = GetPortfolioTotal(price);
			var usdtNeeded = portfolioUsdtValue * -portfolioShareToBuy;

			Usdt -= usdtNeeded;
			Btc += usdtNeeded / price.Open;
		}

		private void Sell(PriceRecord price, decimal portfolioShareToSell)
		{
			var portfolioUsdtValue = GetPortfolioTotal(price);
			var usdtNeeded = portfolioUsdtValue * portfolioShareToSell;
			var btcNeeded = usdtNeeded / price.Open;

			Usdt += usdtNeeded;
			Btc -= btcNeeded;
		}
	}
}
