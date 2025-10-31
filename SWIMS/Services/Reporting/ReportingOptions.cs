namespace SWIMS.Services.Reporting
{
    public class ReportingOptions
    {
        public string ReportServerUrl { get; set; } = default!; // e.g. http://ssrs-host/ReportServer
        public string ReportPathRoot { get; set; } = "/SWIMS_Report";

        // control the same-origin viewer path
        public bool UseReverseProxy { get; set; } = true;      // default ON to avoid mixed content
        public string ReverseProxyBasePath { get; set; } = "/ssrs";

        public UrlAccessOptions UrlAccess { get; set; } = new();
        public ServiceAccountOptions ServiceAccount { get; set; } = new();

        public class UrlAccessOptions
        {
            public bool UseEmbed { get; set; } = true;         // rs:Embed=true
            public bool HideToolbar { get; set; } = true;      // rc:Toolbar=false
            public string? Zoom { get; set; } = "PageWidth";   // rc:Zoom
        }

        public class ServiceAccountOptions
        {
            public string? Domain { get; set; }
            public string? Username { get; set; }
            public string? Password { get; set; }
        }
    }
}