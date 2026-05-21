namespace WaterPurifierTimeAlert.Server.Entity
{
    public sealed class User
    {
        public string Email { get; set; } = null!;

        public long NotBefore { get; set; }

        public long NotAfter { get; set; }

        public string Thumbprint { get; set; } = null!;
    }
}
