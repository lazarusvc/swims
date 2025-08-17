namespace SWIMS.Models
{
    public sealed class StoredProcOptions
    {
        public int DefaultCommandTimeoutSeconds { get; set; } = 60;
        public ManualConnectionOptions ManualConnection { get; set; } = new();
    }

    public sealed class ManualConnectionOptions
    {
        public bool Encrypt { get; set; } = true;
        public bool TrustServerCertificate { get; set; } = true;
        public bool MultipleActiveResultSets { get; set; } = true;
        public int ConnectTimeout { get; set; } = 15;
    }
}
