

using Data;

namespace DataProcessor
{
	public interface IStrategy
	{
		public decimal PortfolioTotal { get; }

		public string DemonstratePortfolio();
		public void RunFull(List<PriceRecord> prices);
	}
}
