using SWIMS.Models.Logging;
using System.Collections.Generic;

namespace SWIMS.Models
{
    public class StoredProcess : IAudited
    {
        public int Id { get; set; }

        // e.g., "dbo.usp_GenerateReport"
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Preferred: use a named ConnectionStrings key from appsettings.json
        public string? ConnectionKey { get; set; }

        // Optional per-process direct connection (keeps module independent of the app's main connection):
        public string? DataSource { get; set; }
        public string? Database { get; set; }
        // If you must store a SQL login per process, store it encrypted.
        public string? DbUserEncrypted { get; set; }
        public string? DbPasswordEncrypted { get; set; }

        public ICollection<StoredProcessParam> Params { get; set; } = new List<StoredProcessParam>();
    }
}