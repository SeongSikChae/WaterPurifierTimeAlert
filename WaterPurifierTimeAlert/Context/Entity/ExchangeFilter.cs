using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WaterPurifierTimeAlert.Context.Entity
{
	[Table("ExchangeFilter")]
	public sealed class ExchangeFilter
	{
		[Key, StringLength(20)]
		public string FilterName { get; set; } = null!;

		[Required]
		public DateTime LastExchnageDate { get; set; }
	}
}
