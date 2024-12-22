

using Data;

namespace Strategies
{
	public interface IStrategy
	{
		public decimal PortfolioTotal { get; }

		public string DemonstratePortfolio();
		public void RunFull(List<PriceRecord> prices);

		//уведомлять когда произошла сделка
	}
}
