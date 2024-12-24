

using Data;

namespace Strategies
{
	public interface IStrategy
	{
		public decimal PortfolioCoinsTotal { get; }
		public decimal Usdt { get; }

		public string DemonstratePortfolio();
		public void RunFull(List<PriceRecord> prices);

		//уведомлять когда произошла сделка
		public event TradeDelegate OnTrade;
	}

	public delegate void TradeDelegate(object sender, TradeEventArgs eventArgs);

	public class TradeEventArgs : EventArgs
	{
		public DateTime DaTime { get; set; }
		public TradeDirection Direction { get; set; }
		public decimal Price { get; set; }
		public decimal Qty { get; set; }
		public string Symbol { get; set; }
		public decimal USDT { get; set; }
		public decimal Amount
		{
			get
			{
				return Qty * Price;
			}
		}
	}

	public enum TradeDirection
	{
		Buy,
		Sell
	}
}
