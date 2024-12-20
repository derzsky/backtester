using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
	public class PriceRecord
	{
		public int Id { get; set; }

		public string Symbol { get; set; }

		public DateTime DateAndTime { get; set; }
		public TimeFrame Frame { get; set; }


		[Column(TypeName = "decimal(16,8)")]
		public decimal Open { get; set; }

		[Column(TypeName = "decimal(16,8)")]
		public decimal High { get; set; }

		[Column(TypeName = "decimal(16,8)")]
		public decimal Low { get; set; }

		[Column(TypeName = "decimal(16,8)")]
		public decimal Close { get; set; }

		public bool IsSamePrice(PriceRecord that)
		{
			var sameSymbol = string.Equals(Symbol, that.Symbol, StringComparison.InvariantCultureIgnoreCase);

			var sameTimeFrame = Frame == that.Frame;

			var sameDateTime = DateAndTime == that.DateAndTime;

			return sameSymbol && sameTimeFrame && sameDateTime;
		}

		public enum TimeFrame
		{
			Minute1,
			Hour1,
			Day1,
			Week1,
			Month1
		}
	}
}
