using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor
{
	public class StrategyRunner
	{
		BalancerStrategy _strat;

		public StrategyRunner(BalancerStrategy strategy)
		{
			_strat = strategy;
		}
		public void Run(List<PriceRecord> prices)
		{
			var portfolio = _strat.DemonstratePortfolio();
			var total = _strat.GetPortfolioTotal(prices.First());

			Console.WriteLine($"{portfolio}, Total: {total.ToString(GeneralConstants.UsdtFormat)}");


			foreach (var pric in prices)
			{
				_strat.PlayIteration(pric);
			}

			portfolio = _strat.DemonstratePortfolio();
			total = _strat.GetPortfolioTotal(prices.Last());

			Console.WriteLine($"{portfolio}, Total: {total.ToString(GeneralConstants.UsdtFormat)}");
		}
	}
}
