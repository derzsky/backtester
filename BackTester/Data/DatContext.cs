using Data.Hidden;
using Microsoft.EntityFrameworkCore;

namespace Data
{
	public class DatContext : DbContext
	{
		public DatContext()
		{

		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(Constants.ConnectionString);
		}

		public DbSet<PriceRecord> Prices { get; set; }
	}
}
