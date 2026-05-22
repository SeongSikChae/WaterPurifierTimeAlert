using System.Configuration.Annotation;

namespace WaterPurifierTimeAlert.Server
{
    public sealed class Configuration
    {
        [Property(PropertyType.STRING, required: true)]
        public string ContentRootPath { get; set; } = null!;

        [Property(PropertyType.STRING, DefaultValue = "ClientApp")]
        public string WebRootPath { get; set; } = null!;

        [Property(PropertyType.USHORT, DefaultValue = "80")]
        public ushort? WebHttpPort { get; set; }

        [Property(PropertyType.USHORT, DefaultValue = "443")]
        public ushort? WebHttpsPort { get; set; }

        [Property(PropertyType.STRING, required: true)]
        public string ServerCertificate { get; set; } = null!;

        [Property(PropertyType.STRING, required: true)]
        public string ServerCertificatePassword { get; set; } = null!;

        [Property(PropertyType.STRING, required: false)]
        public string? CertificateChain { get; set; }

        [Property(PropertyType.STRING, required: false)]
        public string? IncludeCipherSuites { get; set; }

        [Property(PropertyType.STRING, required: true)]
        public string DbPath { get; set; } = null!;

        [Property(PropertyType.STRING, required: false)]
        public string? VapidSubject { get; set; }

        [Property(PropertyType.STRING, required: false)]
        public string? VapidPublicKey { get; set; }

        [Property(PropertyType.STRING, required: false)]
        public string? VapidPrivateKey { get; set; }

        [Property(PropertyType.STRING, required: false, DefaultValue = "0 0 9 * * ?")]
        public string AlertScheduleExpression { get; set; } = null!;
    }
}
