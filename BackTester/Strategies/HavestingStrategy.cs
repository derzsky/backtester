using Data;

namespace Strategies
{
	public class HavestingStrategy : IStrategy
	{
		public decimal PortfolioTotal => throw new NotImplementedException();

		public event TradeDelegate OnTrade;

		public string DemonstratePortfolio()
		{
			throw new NotImplementedException();
		}

		public void RunFull(List<PriceRecord> prices)
		{
			throw new NotImplementedException();
		}
	}
}
