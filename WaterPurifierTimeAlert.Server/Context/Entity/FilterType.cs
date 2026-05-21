using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaterPurifierTimeAlert.Server.Context.Entity
{
    [Table("FilterType")]
    public sealed class FilterType
    {
        [Key, StringLength(20)]
        public string Name { get; set; } = null!;

        [Required, StringLength(10)]
        public string ExpireTime { get; set; } = null!;
    }
}
