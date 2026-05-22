using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaterPurifierTimeAlert.Server.Context.Entity
{
    [Table("PushSubscription")]
    public sealed class PushSubscription
    {
        [Key, StringLength(500)]
        public string Endpoint { get; set; } = null!;

        [Required, StringLength(200)]
        public string P256dh { get; set; } = null!;

        [Required, StringLength(100)]
        public string Auth { get; set; } = null!;

        [Required, StringLength(256)]
        public string UserEmail { get; set; } = null!;
    }
}
