using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaterPurifierTimeAlert.Server.Context.Entity
{
    [Table("ExchangeFilter")]
    public sealed class ExchangeFilter
    {
        [Key, StringLength(20)]
        public string FilterName { get; set; } = null!;

        [Required]
        public DateTime LastExchnageDate { get; set; }

        [NotMapped]
        public DateTime NextExchnageDate { get; set; }
    }
}
